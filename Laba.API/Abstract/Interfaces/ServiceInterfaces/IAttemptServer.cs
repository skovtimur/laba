namespace Laba.API.Abstract.Interfaces.ServiceInterfaces;

public interface IAttemptServer
{
    public Task Decrement(string key);
    public Task<bool> IsBlocked(string key);
}