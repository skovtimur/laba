using AutoMapper;
using Dapper;
using Laba.API.Abstract.Interfaces.RepositoryInterfaces;
using Laba.API.Infrastruction.ModelsFromDb;
using Laba.Shared.Domain.Entities;
using Laba.Shared.Domain.ValueObjects;

namespace Laba.API.Infrastruction.Repositories;

public class UserRepository(NpsqlConnectionFactory factory, IMapper mapper) : IUserRepository
{
    public async Task<UserEntity?> Get(Guid userId)
    {
        using var connection = factory.CreateConnection();
        var query = @$"SELECT id AS {nameof(UserModelFromDb.Id)}, 
                    username AS {nameof(UserModelFromDb.Username)}, 
                    email AS {nameof(UserModelFromDb.Email)},     
                    hashed_password AS {nameof(UserModelFromDb.HashedPassword)},
                    created_at AS {nameof(UserModelFromDb.CreatedAt)} 
                    FROM users WHERE id = @Id";

        var result = await connection.QueryAsync<UserModelFromDb>(query, new { Id = userId });
        var user = result.SingleOrDefault();
        return user == null ? null : mapper.Map<UserEntity>(user);
    }

    public async Task<UserEntity?> Get(EmailValueObject email)
    {
        using var connection = factory.CreateConnection();
        var query = @$"SELECT id AS {nameof(UserModelFromDb.Id)}, 
                    username AS {nameof(UserModelFromDb.Username)}, 
                    email AS {nameof(UserModelFromDb.Email)},     
                    hashed_password AS {nameof(UserModelFromDb.HashedPassword)},
                    created_at AS {nameof(UserModelFromDb.CreatedAt)} 
                    FROM users WHERE email = @Email";

        var result = await connection.QueryAsync<UserModelFromDb>(query, new { Email = email.Email });
        var user = result.SingleOrDefault();
        return user == null ? null : mapper.Map<UserEntity>(user);
    }

    public async Task<bool> Exists(EmailValueObject email)
    {
        var user = await Get(email);
        return user != null;
    }

    public async Task<bool> Exists(string username)
    {
        using var connection = factory.CreateConnection();

        var query = @$"SELECT id AS {nameof(UserModelFromDb.Id)}, 
                    username AS {nameof(UserModelFromDb.Username)}, 
                    email AS {nameof(UserModelFromDb.Email)},     
                    hashed_password AS {nameof(UserModelFromDb.HashedPassword)},
                    created_at AS {nameof(UserModelFromDb.CreatedAt)} 
                    FROM users WHERE username = @Username";

        var user = await connection.QueryAsync<UserModelFromDb>(query, new { Username = username });
        return user.Any();
    }

    public async Task CreateUser(UserEntity userEntity)
    {
        using var connection = factory.CreateConnection();
        var query = "INSERT INTO users VALUES (@Id, @Username, @Email, @HashedPassword, @CreatedAt)";

        await connection.ExecuteAsync(query,
            new
            {
                Id = userEntity.Id,
                Username = userEntity.Username,
                Email = userEntity.Email.Email,
                HashedPassword = userEntity.HashedPassword.Password,
                CreatedAt = userEntity.CreatedAt
            });
    }
}