using Laba.Shared.Domain.Entities;
using Laba.Shared.Domain.ValueObjects;

namespace Laba.API.Abstract.Interfaces.RepositoryInterfaces;

public interface IUserRepository
{
    public Task<UserEntity?> Get(Guid userId);
    public Task<UserEntity?> Get(EmailValueObject email);
    public Task<bool> Exists(EmailValueObject email);
    public Task<bool> Exists(string username);

    public Task CreateUser(UserEntity userEntity);
}