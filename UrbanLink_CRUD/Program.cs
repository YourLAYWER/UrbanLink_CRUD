using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System.Text.Json.Serialization;
using UrbanLink_CRUD.Models.Data;
using static UrbanLink_CRUD.Services.Routing_Logic.TransitSystemService;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Controllers AND configure JSON Options (Done once)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Ignore cycles to prevent infinite loops during serialization
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// 2. Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    // Set JWT as the default authentication scheme
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

// Configure OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "UrbanLink API", Version = "v1" });

    // 1. Define the Security Scheme
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

    // 2. Apply the Security Requirement globally 
    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            // Pass the document context to the reference
            new OpenApiSecuritySchemeReference("Bearer", document),
            new List<string>() // Must be an IList<string>, not a string array
        }
    }); // Added the missing semicolon here
});

// Tell the application about your Database Context
builder.Services.AddDbContext<ULDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection")));

// --- REGISTERING THE SERVICE ---
builder.Services.AddSingleton<TransitRoutingService>();

var app = builder.Build();

// --- INITIALIZING THE GRAPH ON STARTUP ---
// We create a temporary scope just to fetch the initial data from SQL Server
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ULDBContext>();
        var routingService = services.GetRequiredService<TransitRoutingService>();

        // 1. Fetch from DB and immediately map to the lightweight algorithm record
        var graphEdges = context.RouteSegments
        .Select(dbSegment => new GraphEdge(
            dbSegment.OriginStopId,
            dbSegment.DestinationStopId,
            dbSegment.RouteId,
            dbSegment.TravelTimeMinutes,
            dbSegment.Route.BaseFare // Pulls the flat rate directly from the Route table
        ))
        .ToList();
        var hubIds = context.Stops
            .Where(s => s.IsTransferHub)
            .Select(s => s.Id)
            .ToList();

        // 2. Feed the safe records into the Singleton engine
        routingService.InitializeGraph(graphEdges, hubIds);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred initializing the transit graph: {ex.Message}");
    }
}

// --- THIS BLOCK RUNS THE SEEDER ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ULDBContext>();

        // Optional but recommended: Ensure the database exists and migrations are applied
        // context.Database.Migrate(); 

        // Call your seeding method
        DbInitializer.SeedAdminUser(context);
    }
    catch (Exception ex)
    {
        // Log the error if seeding fails (e.g., database isn't running)
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}
// -----------------------------------------

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Generate the Swagger JSON document
    app.UseSwagger();

    // Serve the Swagger UI web page
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "UrbanLink API v1");
    });
}


// Disabled UseHttpsRedirection to allow access for the Mobile application
// app.UseHttpsRedirection();

// These two MUST appear in this exact order, Authentication first, then Authorization. Otherwise, you will get 401 Unauthorized errors.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();