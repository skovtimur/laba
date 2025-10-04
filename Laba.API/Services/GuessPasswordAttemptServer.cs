using Laba.API.Abstract.Interfaces.ServiceInterfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace Laba.API.Services;

public class GuessPasswordAttemptServer(IDistributedCache cache) : IAttemptServer
{
    public async Task Decrement(string key)
    {
        var value = await cache.GetStringAsync(key);

        if (string.IsNullOrEmpty(value) == false
            && int.TryParse(value, out int integerValue))
        {
            await cache.SetStringAsync(key, (integerValue - 1).ToString());
            return;
        }
        await cache.SetStringAsync(key, 3.ToString());
    }

    public async Task<bool> IsBlocked(string key)
    {
        var value = await cache.GetStringAsync(key);

        if (string.IsNullOrEmpty(value) || int.TryParse(value, out int integerValue))
            return false;

        return integerValue <= 0;
    }
}