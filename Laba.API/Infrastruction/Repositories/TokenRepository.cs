using Dapper;
using Laba.API.Abstract.Interfaces.RepositoryInterfaces;
using Laba.Shared.Domain.Entities;

namespace Laba.API.Infrastruction.Repositories;

public class TokenRepository(NpsqlConnectionFactory factory) : ITokenRepository
{
    public async Task<TokenEntity?> Get(string token)
    {
        using var connection = factory.CreateConnection();
        var query = $@"SELECT refresh_token AS {nameof(TokenEntity.RefreshToken)},
             user_id AS {nameof(TokenEntity.UserId)} 
             FROM tokens WHERE refresh_token = @Token";

        var entity = await connection.QuerySingleOrDefaultAsync<TokenEntity>(query, new { Token = token });
        return entity;
    }


    public async Task<bool> ExistsByUserId(Guid userId)
    {
        using var connection = factory.CreateConnection();
        var query = $@"SELECT refresh_token AS {nameof(TokenEntity.RefreshToken)},
             user_id AS {nameof(TokenEntity.UserId)} 
             FROM tokens WHERE user_id = @UserId";

        var entity = await connection.QuerySingleOrDefaultAsync<TokenEntity>(query, new { UserId = userId });
        return entity != null;
    }

    public async Task AddToken(TokenEntity token)
    {
        using var connection = factory.CreateConnection();

        var query = "INSERT INTO tokens VALUES (@UserId, @RefreshToken)";
        await connection.ExecuteAsync(query,
            new { UserId = token.UserId, RefreshToken = token.RefreshToken });
    }

    public async Task UpdateToken(Guid userId, string newRefreshToken)
    {
        using var connection = factory.CreateConnection();

        var query = "UPDATE tokens SET refresh_token = @RefreshToken WHERE user_id = @UserId";
        await connection.ExecuteAsync(query, new { UserId = userId, RefreshToken = newRefreshToken });
    }
}