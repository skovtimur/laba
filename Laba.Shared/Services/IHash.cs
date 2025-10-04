namespace Laba.Shared.Services;

public interface IHash
{
    public string HashPassword(string password);
    public string HashPassword(string password, byte[] salt);
    public byte[] GenerateSalt();
}