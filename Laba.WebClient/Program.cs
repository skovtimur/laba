using Laba.Shared.Domain.Models;
using Laba.Shared.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Laba.WebClient;
using Laba.WebClient.Abstract.ServiceInterfaces;
using Laba.WebClient.Services;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorizationCore();

builder.Logging.SetMinimumLevel(LogLevel.Debug);

builder.Services.AddMudServices();
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddTransient(_ => new HttpClient
{
    BaseAddress = new Uri("http://localhost:8080")
});

builder.Services.AddSingleton<IHash, HashService>();
builder.Services.AddSingleton<IHashVerify, HashService>();
builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<IAuthService, AuthenticationService>();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();

var app = builder.Build();
await app.RunAsync();