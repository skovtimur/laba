using System.Text;
using System.Text.Json;
using HealthChecks.UI.Client;
using Laba.API.Abstract.Interfaces.RepositoryInterfaces;
using Laba.API.Abstract.Interfaces.ServiceInterfaces;
using Laba.API.Infrastruction;
using Laba.API.Infrastruction.Repositories;
using Laba.API.Mapping;
using Laba.API.Options;
using Laba.API.Services;
using Laba.Shared.Domain.Models;
using Laba.Shared.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var jwtOptions = builder.Configuration.GetRequiredSection("Jwt").Get<JwtOptions>()!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(x =>
{
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtOptions.Issuer,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidAlgorithms =
            new List<string> { jwtOptions.AlgorithmForAccessToken, jwtOptions.AlgorithmForRefreshToken },

        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.AccessTokenSecretKey)),
        LifetimeValidator = (notBefore, expiresDate, _, _) =>
            notBefore < DateTime.UtcNow && expiresDate > DateTime.UtcNow
    };
});
builder.Services.AddAuthorization();

string postgresConnectionString = builder.Configuration.GetConnectionString("Postgres") ??
                                  throw new NullReferenceException(
                                      "Postgres connection string is empty");
builder.Services.AddHealthChecks()
    .AddNpgSql(postgresConnectionString)
    .AddRedis("localhost:6379");
builder.Services
    .AddHealthChecksUI(setupSettings: setup => { setup.AddHealthCheckEndpoint("My API Health", "/health"); })
    .AddInMemoryStorage();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetRequiredSection("Jwt"));

builder.Services.AddAutoMapper(x => { x.AddProfile<MainMapperProfile>(); });
builder.Services.AddRouting();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policyBuilder =>
    {
        policyBuilder
            .WithOrigins(
                "http://localhost:6060",
                "https://localhost:6060"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost";
    options.InstanceName = "local";
});
builder.Services.AddSwaggerGen(setup =>
{
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "JWT Authentication",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",

        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    setup.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

    setup.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

builder.Services.AddSingleton<IHash, HashService>();
builder.Services.AddSingleton<IHashVerify, HashService>();
builder.Services.AddSingleton(new NpsqlConnectionFactory(postgresConnectionString));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IChallengeService, ChallengeService>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();
builder.Services.AddScoped<IAttemptServer, GuessPasswordAttemptServer>();
builder.Services.AddScoped<IJwtService, JwtService>();

var app = builder.Build();

app.UseRouting();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else if (app.Environment.IsProduction())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.UseHealthChecksUI(options => options.UIPath = "/health-ui");
app.MapControllers();

app.Run();