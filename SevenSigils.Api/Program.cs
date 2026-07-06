using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Serilog;
using SevenSigils.Api.Validation;
using SevenSigils.Application.Admin;
using SevenSigils.Application.Auth;
using SevenSigils.Application.Catalog;
using SevenSigils.Application.Services;
using SevenSigils.Domain.Abstractions;
using SevenSigils.Api.HealthChecks;
using SevenSigils.Infrastructure.Options;
using SevenSigils.Infrastructure.Repositories;
using SevenSigils.Infrastructure.Security;
using SevenSigils.Infrastructure.Seeding;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.Configure<BlazonDataOptions>(builder.Configuration.GetSection(BlazonDataOptions.SectionName));
builder.Services.Configure<MongoDbOptions>(builder.Configuration.GetSection(MongoDbOptions.SectionName));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MongoDbOptions>>().Value;
    return new MongoClient(opts.ConnectionString);
});

builder.Services.AddSingleton<IBlazonRepository, MongoDbBlazonRepository>();
builder.Services.AddSingleton<IUserRepository, MongoDbUserRepository>();
builder.Services.AddTransient<BlazonSeeder>();
builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddSingleton<IAccessTokenGenerator, JwtAccessTokenGenerator>();
builder.Services.AddSingleton<IRandomProvider, CryptoRandomProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IQuizQuestionService, QuizQuestionService>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<IAdminBlazonService, AdminBlazonService>();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("quiz", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 60;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 10;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks()
    .AddCheck<MongoDbHealthCheck>("mongodb", tags: ["ready"]);

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(builder.Configuration["Cors:FrontendOrigin"] ?? "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // HTTPS non exigé uniquement en dev local (pas de certificat TLS sur localhost).
        // En prod, le TLS est terminé par le reverse proxy nginx.
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

var app = builder.Build();

// Fail-fast : hors Development, on refuse de démarrer avec la clé placeholder,
// une clé vide ou une clé trop courte (HS256 exige au moins 256 bits = 32 octets).
// Lecture via le DI (config finale) et non builder.Configuration : les surcharges
// de WebApplicationFactory (tests) ne sont appliquées qu'au moment du Build().
var effectiveJwtKey = app.Services
    .GetRequiredService<Microsoft.Extensions.Options.IOptions<JwtOptions>>().Value.Key;
if (!app.Environment.IsDevelopment()
    && (string.IsNullOrWhiteSpace(effectiveJwtKey)
        || effectiveJwtKey.StartsWith("CHANGE_ME", StringComparison.Ordinal)
        || Encoding.UTF8.GetByteCount(effectiveJwtKey) < 32))
{
    throw new InvalidOperationException(
        "Jwt:Key must be a strong secret of at least 32 bytes outside Development. " +
        "Set the JWT_KEY environment variable (see docker-compose.yml).");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseCors("Frontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

if (app.Configuration.GetValue<bool?>("MongoDb:SeedOnStartup") != false)
{
    var seeder = app.Services.GetRequiredService<BlazonSeeder>();
    await seeder.SeedAsync();
}

app.Run();

public partial class Program;
