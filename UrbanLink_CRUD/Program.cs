using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System.Text.Json.Serialization;
using UrbanLink_CRUD.Models.Data;
using UrbanLink_CRUD.Models.Domain;
using UrbanLink_CRUD.Models.Domain.Fare;
using UrbanLink_CRUD.Models.Domain.Incident;
using UrbanLink_CRUD.Models.Domain.Incident.FireBase;
using UrbanLink_CRUD.Models.Domain.Journey;
using UrbanLink_CRUD.Services.RoutingLogic;
using UrbanLink_CRUD.Services.Workers;

var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// 1. REGISTER SERVICES
// =========================================================================

// Controllers & JSON Options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// CORS Policies
var corsPolicyName = "AllowReactApp";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",  // Vite dev server
                "https://localhost:5173",
                "http://127.0.0.1:5173",
                "http://localhost:3000",  // React App default
                "https://localhost:3000"
               )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Required for SignalR WebSockets and cookies
    });
});

// Configure JWT Authentication
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

    c.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer"),
            new List<string>()
        }
    });
});

// Database Context
builder.Services.AddDbContext<ULDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection")));

// Dependency Injection Registrations
builder.Services.AddScoped<IFareStrategyFactory, FareStrategyFactory>();
builder.Services.AddSingleton<IRoutingStrategyFactory, RoutingStrategyFactory>();
builder.Services.AddSingleton<ITransitRoutingService, TransitRoutingService>();
builder.Services.AddSingleton<IJourneyFactory, JourneyFactory>();

// Incident Strategies
builder.Services.AddScoped<IIncidentStrategy, DelayIncidentStrategy>();
builder.Services.AddScoped<IncidentStrategyResolver>();

// Register Background Services & SignalR
builder.Services.AddHostedService<TicketExpirationWorker>();
builder.Services.AddSignalR();

// Firebase Admin SDK Configuration (Safely Wrapped)
var firebaseKeyPath = Path.Combine(builder.Environment.ContentRootPath, "firebase_admin_sdk.json");
if (File.Exists(firebaseKeyPath))
{
    if (FirebaseApp.DefaultInstance == null)
    {
        FirebaseApp.Create(new AppOptions()
        {
            Credential = GoogleCredential.FromFile(firebaseKeyPath)
        });
    }
}
else
{
    Console.WriteLine("[Warning] firebase_admin_sdk.json not found. Skipping Firebase Admin SDK initialization for local execution.");
}

builder.Services.AddScoped<IFirebaseNotificationService, FirebaseNotificationService>();

// =========================================================================
// 2. BUILD THE APPLICATION
// =========================================================================
var app = builder.Build();

// =========================================================================
// 3. STARTUP TASKS (Graph Initialization & Database Seeding)
// =========================================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ULDBContext>();

        // Seed transit tables
        DbInitializer.SeedAdminUser(context);
        DbInitializer.SeedBuses(context);
        DbInitializer.SeedTransitProviders(context);
        DbInitializer.SeedTransitSystem(context);
        DbInitializer.SeedDemoOperations(context);

        var routingService = services.GetRequiredService<ITransitRoutingService>();

        var dbSegments = context.RouteSegments
            .Include(s => s.Route)
                .ThenInclude(r => r.Schedules)
            .ToList();

        var graphEdges = new List<GraphEdge>();

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
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the graph or seeding the database.");
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

// CORS MUST strictly come before Authentication & Authorization
app.UseCors(corsPolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<TransitHub>("/transithub");

app.Run();