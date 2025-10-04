using System.Security.Cryptography;
using Laba.API.Abstract.Interfaces.ServiceInterfaces;
using Laba.Shared.Domain.Models;
using Laba.Shared.Domain.ValueObjects;
using Microsoft.Extensions.Caching.Distributed;

namespace Laba.API.Services;

public class ChallengeService(IDistributedCache cache) : IChallengeService
{
    public async Task<string?> GetNonce(EmailValueObject email) => await cache.GetStringAsync("nonce:" + email.Email);
    public async Task<bool> Exists(EmailValueObject email) => string.IsNullOrEmpty(await GetNonce(email)) == false;

    public async Task<ChallengeModel> CreateNonce(EmailValueObject email,
        HashedPasswordValueObject hashedPassword)
    {
        var nonce = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));

        await cache.SetStringAsync("nonce:" + email.Email, nonce, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
        });

        var parts = hashedPassword.Password.Split('-');
        var saltHex = parts[1];

        return new ChallengeModel(Nonce: nonce, SaltHex: saltHex);
    }

    public async Task RemoveNonce(EmailValueObject email)
    {
        await cache.RemoveAsync("nonce:" + email.Email);
    }
}