using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System.Text.Json.Serialization;
using UrbanLink_CRUD.Models.Data;
using UrbanLink_CRUD.Services.RoutingLogic;
using UrbanLink_CRUD.Models.Domain;
using UrbanLink_CRUD.Models.Domain.Fare;
using UrbanLink_CRUD.Models.Domain.Incident;
using UrbanLink_CRUD.Models.Domain.Incident.FireBase;
using UrbanLink_CRUD.Models.Domain.Journey;
using UrbanLink_CRUD.Services.RoutingLogic; // 1. Updated Namespace
using UrbanLink_CRUD.Services.Workers;

var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// 1. REGISTER SERVICES (builder.Services.Add...)
//    All service registrations MUST happen BEFORE calling builder.Build()
// =========================================================================

// Controllers & JSON Options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Ignore cycles to prevent infinite loops during serialization
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy.WithOrigins("http://localhost:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

// Configure JWT Authentication
// --- CORS: allow the React admin UI (Vite dev server) to call this API ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AdminUI", policy => policy
        .WithOrigins(
            "http://localhost:5173",
            "http://127.0.0.1:5173")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()); // needed for the SignalR hub (/transithub)
});

// 2. Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ClockSkew = TimeSpan.Zero
    };
});

// Configure OpenAPI / Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "UrbanLink API", Version = "v1" });

    // Define the Security Scheme
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n" +
                      "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                      "Example: 'Bearer eyJhbGciOiJIUzI1Ni...'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Apply the Security Requirement globally 
    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document),
            new List<string>()
        }
    });
});

// Database Context
builder.Services.AddDbContext<ULDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection")));

// Custom Dependency Injection Services
builder.Services.AddSingleton<ITransitRoutingService, TransitRoutingService>();


// =========================================================================
// 2. BUILD THE APPLICATION (Locks builder.Services)
// =========================================================================
// Thise line enables the pattern so it can be injected into any controller or service
builder.Services.AddScoped<IFareStrategyFactory, FareStrategyFactory>();
builder.Services.AddSingleton<IRoutingStrategyFactory, RoutingStrategyFactory>();
builder.Services.AddSingleton<ITransitRoutingService, TransitRoutingService>();
builder.Services.AddSingleton<IJourneyFactory, JourneyFactory>();

//------Registering IncidentReport------
// Register the specific strategies
builder.Services.AddScoped<IIncidentStrategy, DelayIncidentStrategy>();
// builder.Services.AddScoped<IIncidentStrategy, MedicalIncidentStrategy>(); // Add others as you build them

// Register the resolver itself
builder.Services.AddScoped<IncidentStrategyResolver>();
//------End--------

// --- REGISTERING THE SERVICE ---
// 2. Updated DI Registration to use the Interface
builder.Services.AddSingleton<ITransitRoutingService, TransitRoutingService>();


// Register the background worker
builder.Services.AddHostedService<TicketExpirationWorker>();

builder.Services.AddSignalR(); // for SignalR


//-------Firebase Configuration----------
// 1. Initialize Firebase Admin SDK
var firebaseKeyPath = Path.Combine(builder.Environment.ContentRootPath, "firebase_admin_sdk.json");

// Ensure it only initializes once
if (FirebaseApp.DefaultInstance == null)
{
    FirebaseApp.Create(new AppOptions()
    {
        Credential = GoogleCredential.FromFile(firebaseKeyPath)
    });
}

// 2. Register the Notification Service
builder.Services.AddScoped<IFirebaseNotificationService, FirebaseNotificationService>();
//---------------End-------------------

var app = builder.Build();


// =========================================================================
// 3. STARTUP TASKS (Graph Initialization & Database Seeding)
// =========================================================================

// Initialize Graph on Startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ULDBContext>();
        var routingService = services.GetRequiredService<ITransitRoutingService>();

        // Fetch segments, eagerly loading each route's schedules
        var dbSegments = context.RouteSegments
            .Include(s => s.Route)
                .ThenInclude(r => r.Schedules)
            .ToList();

        var graphEdges = new List<GraphEdge>();

        // Dynamic offset calculation per route
        foreach (var routeGroup in dbSegments.GroupBy(s => s.RouteId))
        {
            var routeSegments = routeGroup.ToList();

            var currentSegment = routeSegments.FirstOrDefault(s =>
                !routeSegments.Any(other => other.DestinationStopId == s.OriginStopId));

            int currentOffset = 0;

            while (currentSegment != null)
            {
                graphEdges.Add(new GraphEdge(
                    currentSegment.OriginStopId,
                    currentSegment.DestinationStopId,
                    currentSegment.RouteId,
                    currentSegment.TravelTimeMinutes,
                    currentSegment.Route.BaseFare,
                    currentOffset,
                    currentSegment.Route.Schedules.ToList()
                ));

                currentOffset += currentSegment.TravelTimeMinutes;
                currentSegment = routeSegments.FirstOrDefault(s => s.OriginStopId == currentSegment.DestinationStopId);
            }
        }

        var hubIds = context.Stops
            .Where(s => s.IsTransferHub)
            .Select(s => s.Id)
            .ToList();

        routingService.InitializeGraph(graphEdges, hubIds);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred initializing the transit graph: {ex.Message}");
    }
}

// Run Database Seeder
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ULDBContext>();

        DbInitializer.SeedAdminUser(context);
        DbInitializer.SeedBuses(context);
        DbInitializer.SeedTransitProviders(context);
        DbInitializer.SeedTransitSystem(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}


// =========================================================================
// 4. HTTP REQUEST PIPELINE MIDDLEWARE
// =========================================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "UrbanLink API v1");
    });
}

// CORS must be executed before Authentication and Authorization
app.UseCors("AllowReactApp");

// Authentication MUST strictly come before Authorization

// Disabled UseHttpsRedirection to allow access for the Mobile application
// app.UseHttpsRedirection();

// CORS must come BEFORE authentication so the browser's preflight (OPTIONS)
// request from the admin UI is answered with the right headers.
app.UseCors("AdminUI");

// These two MUST appear in this exact order, Authentication first, then Authorization. Otherwise, you will get 401 Unauthorized errors.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<TransitHub>("/transithub"); // <-- Maps the WebSocket route endpoint

app.Run();