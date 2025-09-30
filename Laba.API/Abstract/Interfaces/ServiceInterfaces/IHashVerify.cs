namespace Laba.API.Abstract.Interfaces.ServiceInterfaces;

public interface IHashVerify
{
    public bool AreBothSame(string str, string hashedString);
}