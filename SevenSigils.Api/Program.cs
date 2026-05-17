using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Serilog;
using SevenSigils.Api.Validation;
using SevenSigils.Application.Auth;
using SevenSigils.Application.Services;
using SevenSigils.Domain.Abstractions;
using SevenSigils.Infrastructure.Options;
using SevenSigils.Infrastructure.Repositories;
using SevenSigils.Infrastructure.Security;
using SevenSigils.Infrastructure.Seeding;
using System.Text;

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

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

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
        options.RequireHttpsMetadata = false;
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseCors("Frontend");
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
