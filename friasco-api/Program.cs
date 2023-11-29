using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using friasco_api.Data;
using friasco_api.Data.Repositories;
using friasco_api.Enums;
using friasco_api.Helpers;
using friasco_api.Helpers.AuthPolicies;
using friasco_api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.Sqlite;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IDapperWrapper, DapperWrapper>();
builder.Services.AddScoped<IBCryptWrapper, BCryptWrapper>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddHttpContextAccessor();

// AutoMapper Profiles
builder.Services.AddAutoMapper(typeof(MapperProfile).Assembly);

// Database connection string + Add DataContext + Inject connection method into DataContext constructor
var connectionString = builder.Configuration.GetConnectionString("FriascoDatabase");
builder.Services.AddScoped<IDataContext, DataContext>(serviceProvider =>
{
    return new DataContext(() => new SqliteConnection(connectionString));
});

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(x =>
    {
        x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT Authentication
var jwtSettings = new JwtSettings
{
    Key = Environment.GetEnvironmentVariable("JWT_KEY"),
    Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
    Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
};
builder.Services.AddSingleton(jwtSettings);
var tokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ClockSkew = TimeSpan.Zero,
    ValidateIssuerSigningKey = true,
    ValidIssuer = jwtSettings.Issuer,
    ValidAudience = jwtSettings.Audience,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
    RoleClaimType = ClaimTypes.Role
};
builder.Services.AddSingleton(tokenValidationParameters);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
        options.TokenValidationParameters = tokenValidationParameters
    );

// Policy Authorization
builder.Services.AddSingleton<IAuthorizationHandler, AdminOrSelfHandler>();
builder.Services.AddAuthorization(options =>
{
    // User Policy
    options.AddPolicy(AuthPolicyEnum.User, policy => policy.RequireRole(
        nameof(UserRoleEnum.User),
        nameof(UserRoleEnum.Admin),
        nameof(UserRoleEnum.SuperAdmin)
    ));
    // Admin Policy
    options.AddPolicy(AuthPolicyEnum.Admin, policy => policy.RequireRole(
        nameof(UserRoleEnum.Admin),
        nameof(UserRoleEnum.SuperAdmin)
    ));
    // SuperAdmin Policy
    options.AddPolicy(AuthPolicyEnum.SuperAdmin, policy => policy.RequireRole(
        nameof(UserRoleEnum.SuperAdmin)
    ));
    // AdminOrSelf Policy
    options.AddPolicy(AuthPolicyEnum.AdminOrSelf, policy => policy.Requirements.Add(new AdminOrSelfRequirement()));
});

// Swagger page
builder.Services.AddSwaggerGen(x =>
{
    x.SwaggerDoc("v1", new() { Title = "FriascoApi", Version = "v1" });
    x.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. 
            \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.  
            \r\n\r\nExample: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    x.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

// Ensure database and tables exist
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<IDataContext>();
    await context.InitDatabase();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Initialize any test users needed for dev purposes
await UserInitializer.CreateTestUsers(app);

app.UseHttpsRedirection();

app.UseCors(x => x
    .WithOrigins("http://localhost:8001", "localhost:8001", "localhost:61355")
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ErrorHandlerMiddleware>();

app.MapControllers();

app.Run();

public partial class Program { }
