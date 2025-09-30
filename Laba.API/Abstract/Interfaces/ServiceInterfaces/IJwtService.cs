using Laba.Shared.Domain.Models;

namespace Laba.API.Abstract.Interfaces.ServiceInterfaces;

public interface IJwtService
{
    public Task<JwtTokens> GenerateNewTokens(Guid userId, string userName);
}