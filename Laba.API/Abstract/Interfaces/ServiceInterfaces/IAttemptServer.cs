namespace Laba.API.Abstract.Interfaces.ServiceInterfaces;

public interface IAttemptServer
{
    public Task Decrement(string key);
    public bool IsBlocked(string key);
}