using Laba.Shared.Domain.Models;
using Laba.WebClient.Abstract.ServiceInterfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace Laba.WebClient;

public class BaseComponent : ComponentBase
{
    [Inject] public HttpClient HttpClient { get; set; } = default!;
    [Inject] public AuthenticationStateProvider AuthProvider { get; set; } = default!;
    [Inject] public IJSRuntime JsRuntime { get; set; } = default!;
    [Inject] public NavigationManager NavManager { get; set; } = default!;
    [Inject] public IAuthService AuthService { get; set; } = default!;

    private JwtAuthStateProvider JwtAuthStateProvider => (JwtAuthStateProvider)AuthProvider;

    protected async Task WriteLog(string message)
    {
        await JsRuntime.InvokeVoidAsync("console.log", message);
    }

    protected async Task WriteLog(params object[] messages)
    {
        await JsRuntime.InvokeVoidAsync("console.log", messages);
    }

    protected async Task WriteWarn(string message)
    {
        await JsRuntime.InvokeVoidAsync("console.warn", message);
    }

    protected async Task WriteWarn(params object[] messages)
    {
        await JsRuntime.InvokeVoidAsync("console.warn", messages);
    }

    protected async Task WriteError(string message)
    {
        await JsRuntime.InvokeVoidAsync("console.error", message);
    }

    protected async Task WriteError(params object[] messages)
    {
        await JsRuntime.InvokeVoidAsync("console.error", messages);
    }

    protected void NavTo(string path, bool refresh = false)
    {
        NavManager.NavigateTo(path, forceLoad: refresh);
    }
}