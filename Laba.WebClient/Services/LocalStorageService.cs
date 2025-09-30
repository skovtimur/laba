using System.Text.Json;
using Laba.WebClient.Abstract.ServiceInterfaces;
using Microsoft.JSInterop;

namespace Laba.WebClient.Services;

public class LocalStorageService(IJSRuntime jsRuntime) : IBrowserStorage
{
    public const string AccessTokenPropertyName = "accessToken";
    public const string RefreshTokenPropertyName = "refreshToken";

    public async Task<string?> GetAccessToken() => await Get<string>(AccessTokenPropertyName);
    public async Task<string?> GetRefreshToken() => await Get<string>(RefreshTokenPropertyName);

    public async Task<TData?> Get<TData>(string key)
    {
        var value = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
        return string.IsNullOrEmpty(value) ? default : Deserialize<TData>(value);
    }

    public async Task Save<TData>(string key, TData value)
    {
        var valueAsString = Serialize(value);
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", key, valueAsString);
    }

    public async Task Remove(string key)
    {
        await jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
    }

    public string Serialize<TData>(TData value) => JsonSerializer.Serialize(value);
    public TData? Deserialize<TData>(string valueAsString) => JsonSerializer.Deserialize<TData>(valueAsString);
}