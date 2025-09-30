using Laba.Shared.Domain.Entities;

namespace Laba.API.Abstract.Interfaces.RepositoryInterfaces;

public interface ITokenRepository
{
    public Task<TokenEntity?> Get(string token);
    public Task<bool> ExistsByUserId(Guid userId);

    public Task AddToken(TokenEntity token);
    public Task UpdateToken(Guid userId, string newRefreshToken);
}