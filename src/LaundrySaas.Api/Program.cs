using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using LaundrySaas.Application;
using LaundrySaas.Infrastructure;
using LaundrySaas.Infrastructure.Authentication;
using LaundrySaas.Infrastructure.Persistence;
using LaundrySaas.Infrastructure.Seeding;
using LaundrySaas.Api.Middleware;
using LaundrySaas.Api.Authorization;
using LaundrySaas.Api.Endpoints.AuthTenants;
using LaundrySaas.Api.Endpoints.Billing;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

// Register Application & Infrastructure Services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Configure JWT options & Authentication
var jwtSettings = new JwtSettings();
builder.Configuration.GetSection(JwtSettings.SectionName).Bind(jwtSettings);

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
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
    };
});

// Register Permission-based Authorization
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddAuthorization();

// Register MongoDB Client & Database
var mongoConnectionString = builder.Configuration.GetSection("MongoDbSettings:ConnectionString").Value
                            ?? "mongodb://localhost:27017";
var mongoDatabaseName = builder.Configuration.GetSection("MongoDbSettings:DatabaseName").Value
                        ?? "possaas_mongo";

builder.Services.AddSingleton<IMongoClient>(new MongoClient(mongoConnectionString));
builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoDatabaseName);
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<TenantResolverMiddleware>();

app.MapTenantEndpoints();
app.MapPlanEndpoints();

// Database Migration & Seed
await using var scope = app.Services.CreateAsyncScope();

var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

await db.Database.MigrateAsync();

await SeedData.SeedAsync(db);

// Run the app
await app.RunAsync();
