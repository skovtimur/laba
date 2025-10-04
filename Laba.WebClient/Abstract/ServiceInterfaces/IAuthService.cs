using Laba.Shared.Domain.Dtos;
using Laba.Shared.Domain.Models;
using Laba.Shared.Requests;

namespace Laba.WebClient.Abstract.ServiceInterfaces;

public interface IAuthService
{
    public Task<UserDtoToView> GetInfoAboutUser();
    public Task Register(RegisterRequest request);
    public Task Login(LoginRequest request);
    public Task Logout();
    public Task UpdateTokens(string oldRefreshToken);

    public Task<ChallengeModel> StartHandshake(string email);
    public Task EndHandshake(ChallengeModel challenge, string email, string password);
}