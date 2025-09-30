using Laba.Shared.Domain.Dtos;
using Laba.Shared.Requests;

namespace Laba.WebClient.Abstract.ServiceInterfaces;

public interface IAuthService
{
    public Task<UserDtoToView> GetInfoAboutUser();
    public Task Register(RegisterRequest request);
    public Task Login(LoginRequest request);
    public Task Logout();
}