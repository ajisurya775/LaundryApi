using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using LaundrySaas.Application.Abstractions;
using LaundrySaas.Infrastructure.MultiTenancy;
using LaundrySaas.Api.Middleware;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using LaundrySaas.Infrastructure.Data;
using LaundrySaas.Infrastructure.Seeding;
using LaundrySaas.Infrastructure.Authentication;
using LaundrySaas.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, TenantProvider>();
builder.Services.AddScoped<IBranchProvider, BranchProvider>();

// Configure JWT options & Authentication
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
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
builder.Services.AddAuthorization();

// Register Token and FirebaseAuth services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddSingleton<IFirebaseAuthService, FirebaseAuthService>();

// Register PostgreSQL DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

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

// Database Migration & Seed
await using var scope = app.Services.CreateAsyncScope();

var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

await db.Database.MigrateAsync();

await SeedData.SeedAsync(db);

// Run the app
await app.RunAsync();
