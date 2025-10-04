using Laba.Shared.Domain.ValueObjects;

namespace Laba.Shared.Services;

public interface IHashVerify
{
    public bool AreStringAndHashSame(string str, string hashedString, byte[] salt);
    public bool IsPasswordRight(string password, HashedPasswordValueObject hashedPassword);
    public bool AreHashesSame(byte[] firstHash, byte[] secondHash);
}