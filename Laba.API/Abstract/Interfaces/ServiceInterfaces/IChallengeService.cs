using Laba.Shared.Domain.Models;
using Laba.Shared.Domain.ValueObjects;

namespace Laba.API.Abstract.Interfaces.ServiceInterfaces;

public interface IChallengeService
{
    public Task<string?> GetNonce(EmailValueObject email);
    public Task<bool> Exists(EmailValueObject email);
    public Task<ChallengeModel> CreateNonce(EmailValueObject email, HashedPasswordValueObject hashedPassword);
    public Task RemoveNonce(EmailValueObject email);
}