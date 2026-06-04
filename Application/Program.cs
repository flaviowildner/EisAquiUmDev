using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Application.Data;
using Application.Services;

var builder = WebApplication.CreateBuilder(args);
var railwayPort = Environment.GetEnvironmentVariable("PORT");

if (!string.IsNullOrWhiteSpace(railwayPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{railwayPort}");
}

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// DbContext com InMemory
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("AuthDb"));

// Servico de autenticacao
builder.Services.AddScoped<IAuthService, AuthService>();

// HttpClientFactory para validar tokens Google
builder.Services.AddHttpClient();

// Configurar JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JwtSettings:SecretKey nao configurado");
var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true
    };
});

builder.Services.AddAuthorization();

// CORS para comunicacao com frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        var origins = new List<string> { "http://localhost:3000", "http://localhost:5000", "http://localhost:5173" };
        var frontendUrl = builder.Configuration["Frontend:Url"];

        if (!string.IsNullOrWhiteSpace(frontendUrl))
        {
            origins.Add(frontendUrl);
        }

        policy.WithOrigins(origins.Distinct().ToArray())
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Usar CORS
app.UseCors("AllowLocalhost");

// Usar autenticacao e autorizacao
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
