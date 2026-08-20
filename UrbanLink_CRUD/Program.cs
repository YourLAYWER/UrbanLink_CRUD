//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using Microsoft.OpenApi;
//using System.Text;
//using System.Text.Json.Serialization;
//using UrbanLink_CRUD.Models.Data;
//using static UrbanLink_CRUD.Services.Routing_Logic.TransitSystemService;

//var builder = WebApplication.CreateBuilder(args);

//// 1. Add Controllers AND configure JSON Options (Done once)
//builder.Services.AddControllers()
//    .AddJsonOptions(options =>
//    {
//        // Ignore cycles to prevent infinite loops during serialization
//        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
//    });

//// 2. Configure JWT Authentication
//var jwtSettings = builder.Configuration.GetSection("Jwt");
//var secretKey = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

//builder.Services.AddAuthentication(options =>
//{
//    // Set JWT as the default authentication scheme
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(options =>
//{
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        ValidIssuer = jwtSettings["Issuer"],
//        ValidAudience = jwtSettings["Audience"],
//        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
//        ClockSkew = TimeSpan.Zero
//    };
//});

//// Configure OpenAPI
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "UrbanLink API", Version = "v1" });

//    // 1. Define the Security Scheme
//    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//    {
//        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n" +
//                      "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
//                      "Example: 'Bearer eyJhbGciOiJIUzI1Ni...'",
//        Name = "Authorization",
//        In = ParameterLocation.Header,
//        Type = SecuritySchemeType.ApiKey,
//        Scheme = "Bearer"
//    });

//    // 2. Apply the Security Requirement globally 
//    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
//    {
//        {
//            // Pass the document context to the reference
//            new OpenApiSecuritySchemeReference("Bearer", document),
//            new List<string>() // Must be an IList<string>, not a string array
//        }
//    }); // Added the missing semicolon here
//});

//// Tell the application about your Database Context
//builder.Services.AddDbContext<ULDBContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection")));

//// --- REGISTERING THE SERVICE ---
//builder.Services.AddSingleton<TransitRoutingService>();

//var app = builder.Build();

//// --- INITIALIZING THE GRAPH ON STARTUP ---
//// We create a temporary scope just to fetch the initial data from SQL Server
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    try
//    {
//        var context = services.GetRequiredService<ULDBContext>();
//        var routingService = services.GetRequiredService<TransitRoutingService>();

//        // 1. Fetch from DB and immediately map to the lightweight algorithm record
//        var graphEdges = context.RouteSegments
//        .Select(dbSegment => new GraphEdge(
//            dbSegment.OriginStopId,
//            dbSegment.DestinationStopId,
//            dbSegment.RouteId,
//            dbSegment.TravelTimeMinutes,
//        dbSegment.Route.BaseFare // Pulls the flat rate directly from the Route table
//        ))
//        .ToList();
//        var hubIds = context.Stops
//            .Where(s => s.IsTransferHub)
//            .Select(s => s.Id)
//            .ToList();

//        // 2. Feed the safe records into the Singleton engine
//        routingService.InitializeGraph(graphEdges, hubIds);
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine($"An error occurred initializing the transit graph: {ex.Message}");
//    }
//}

//// --- THIS BLOCK RUNS THE SEEDER ---
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    try
//    {
//        var context = services.GetRequiredService<ULDBContext>();

//        // Optional but recommended: Ensure the database exists and migrations are applied
//        // context.Database.Migrate(); 

//        // Call your seeding method
//        DbInitializer.SeedAdminUser(context);
//    }
//    catch (Exception ex)
//    {
//        // Log the error if seeding fails (e.g., database isn't running)
//        var logger = services.GetRequiredService<ILogger<Program>>();
//        logger.LogError(ex, "An error occurred while seeding the database.");
//    }
//}
//// -----------------------------------------

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    // Generate the Swagger JSON document
//    app.UseSwagger();

//    // Serve the Swagger UI web page
//    app.UseSwaggerUI(c =>
//    {
//        c.SwaggerEndpoint("/swagger/v1/swagger.json", "UrbanLink API v1");
//    });
//}


//// Disabled UseHttpsRedirection to allow access for the Mobile application
//// app.UseHttpsRedirection();

//// These two MUST appear in this exact order, Authentication first, then Authorization. Otherwise, you will get 401 Unauthorized errors.
//app.UseAuthentication();
//app.UseAuthorization();

//app.MapControllers();

//app.Run();

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi;
using System.Text;
using System.Text.Json.Serialization;
using UrbanLink_CRUD.Models.Data;
using UrbanLink_CRUD.Services.RoutingLogic; // 1. Updated Namespace
using UrbanLink_CRUD.Services.Workers;

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
            new List<string>()
        }
    });
});

// Tell the application about your Database Context
builder.Services.AddDbContext<ULDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection")));

// --- REGISTERING THE SERVICE ---
// 2. Updated DI Registration to use the Interface
builder.Services.AddSingleton<ITransitRoutingService, TransitRoutingService>();


// Register the background worker
builder.Services.AddHostedService<TicketExpirationWorker>();


var app = builder.Build();

// --- INITIALIZING THE GRAPH ON STARTUP ---
// We create a temporary scope just to fetch the initial data from SQL Server
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ULDBContext>();

        // 1. SEED TRANSIT TABLES FIRST
        DbInitializer.SeedAdminUser(context);
        DbInitializer.SeedTransitProviders(context);
        DbInitializer.SeedTransitSystem(context);

        var routingService = services.GetRequiredService<ITransitRoutingService>();

        // 2. Fetch the segments from the database into memory so we can run our offset math
        // Make sure to Include(s => s.Route) so we have access to the FirstDepartureTime, etc.
        var dbSegments = context.RouteSegments
            .Include(s => s.Route)
            .ToList();

        var graphEdges = new List<GraphEdge>();

        // 3. DYNAMIC OFFSET CALCULATION
        // Group the segments by Route (e.g., all segments for Route 1, then all for Route 2)
        foreach (var routeGroup in dbSegments.GroupBy(s => s.RouteId))
        {
            var routeSegments = routeGroup.ToList();

            // Find the "Start" of the route (the segment where its Origin is NOT the Destination of any other segment)
            var currentSegment = routeSegments.FirstOrDefault(s =>
                !routeSegments.Any(other => other.DestinationStopId == s.OriginStopId));

            int currentOffset = 0; // Starts at 0 for the first stop!

            // Trace the route from start to finish
            while (currentSegment != null)
            {
                // Build the updated GraphEdge with all 9 parameters
                graphEdges.Add(new GraphEdge(
                    currentSegment.OriginStopId,
                    currentSegment.DestinationStopId,
                    currentSegment.RouteId,
                    currentSegment.TravelTimeMinutes,
                    currentSegment.Route.BaseFare,

                    // Pass in the schedule from the Route table
                    currentSegment.Route.FirstDepartureTime,
                    currentSegment.Route.LastDepartureTime,
                    currentSegment.Route.FrequencyMinutes,

                    // Inject our dynamically calculated offset!
                    currentOffset
                ));

                // Increase the offset by the time it took to travel this segment
                currentOffset += currentSegment.TravelTimeMinutes;

                // Move to the next segment in the chain (where Origin == Current Destination)
                currentSegment = routeSegments.FirstOrDefault(s => s.OriginStopId == currentSegment.DestinationStopId);
            }
        }

        // 4. Get the hubs
        var hubIds = context.Stops
            .Where(s => s.IsTransferHub)
            .Select(s => s.Id)
            .ToList();

        // 5. Feed the dynamically mapped records into the Singleton engine
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

        DbInitializer.SeedTickets(context);

        DbInitializer.SeedTransitProviders(context);
        DbInitializer.SeedTransitSystem(context); // Call the new seeding method for transit data

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