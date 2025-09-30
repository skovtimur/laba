using Laba.API.Abstract.Interfaces.ServiceInterfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Laba.API.Services;

public class GuessPasswordAttemptServer(IMemoryCache cache) : IAttemptServer
{
    public async Task Decrement(string key)
    {
        var value = await cache.GetOrCreateAsync(key, factory =>
        {
            factory.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3);
            factory.Value = 3;

            return Task.FromResult(3);
        });
        cache.Set(key, value - 1);
    }

    public bool IsBlocked(string key)
    {
        if (cache.TryGetValue(key, out int value) == false)
            return false;

        return value <= 0;
    }
}