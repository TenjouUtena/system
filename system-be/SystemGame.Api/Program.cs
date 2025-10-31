using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System.Text;
using SystemGame.Api.Contexts;
using SystemGame.Api.Data.Entities;
using SystemGame.Api.Models;
using SystemGame.Api.Services;
using SystemGame.Api.Services.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Database
var connectionString = builder.Configuration.GetConnectionString("PostgreSql");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Identity
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // User settings
    options.User.RequireUniqueEmail = true;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication
var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Google OAuth
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        var googleSettings = builder.Configuration.GetSection("OAuth:Google");
        options.ClientId = googleSettings["ClientId"] ?? "";
        options.ClientSecret = googleSettings["ClientSecret"] ?? "";
    });

// Redis
var redisConfig = builder.Configuration.GetConnectionString("Redis") 
    ?? builder.Configuration["Redis:Configuration"]!;
builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
{
    var config = ConfigurationOptions.Parse(redisConfig);
    config.AbortOnConnectFail = false;
    config.ConnectRetry = 3;
    config.ConnectTimeout = 5000;
    return ConnectionMultiplexer.Connect(config);
});
builder.Services.AddScoped<RedisService>();

// Services
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<GalaxyGeneratorService>();
builder.Services.AddScoped<PlanetGridGeneratorService>();
builder.Services.AddScoped<SimulationService>();
builder.Services.AddScoped<SpaceshipService>();

// Agent services (Phase 6)
builder.Services.AddSingleton<SystemGame.Api.Services.Agents.AgentBehaviorService>();
builder.Services.AddScoped<SystemGame.Api.Services.Agents.AgentExecutionService>();

// Combat services (Phase 8)
builder.Services.AddScoped<CombatService>();
builder.Services.AddScoped<NpcSpawnService>();

// Game balance configuration (Phase 10)
builder.Services.AddSingleton<GameConfigService>();
builder.Services.Configure<GameConfig>(builder.Configuration.GetSection("GameConfig"));

// SignalR
builder.Services.AddSignalR();

// Background services
builder.Services.AddHostedService<GameSimulationHostedService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5001", "http://localhost:3000", "http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Controllers
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "System Game API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<GameHub>("/hubs/game");

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    
    context.Database.Migrate();
    
    // Seed roles
    if (!await roleManager.RoleExistsAsync("Player"))
    {
        await roleManager.CreateAsync(new IdentityRole("Player"));
    }
}

app.Run();
