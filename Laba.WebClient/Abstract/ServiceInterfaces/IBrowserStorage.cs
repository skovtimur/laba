namespace Laba.WebClient.Abstract.ServiceInterfaces;

public interface IBrowserStorage
{
    public Task<string?> GetAccessToken();
    public Task<string?> GetRefreshToken();

    public Task<TData?> Get<TData>(string key);
    public Task Save<TData>(string key, TData value);
    public Task Remove(string key);
}