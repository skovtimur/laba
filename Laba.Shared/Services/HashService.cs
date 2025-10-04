using System.Security.Cryptography;
using Laba.Shared.Domain.Models;
using Laba.Shared.Domain.ValueObjects;

namespace Laba.Shared.Services;

public class HashService : IHash, IHashVerify
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 1_000;
    private readonly HashAlgorithmName _algorithmName = HashAlgorithmName.SHA256;

    public string HashPassword(string password)
    {
        var salt = GenerateSalt();
        return HashPassword(password, salt);
    }

    public string HashPassword(string password, byte[] salt)
    {
        // Как работает:
        // Хеш фукнция берет значения и salt, после превращает это все в уникальную строку
        // Salt - рандомные числа
        // Хэш функция может сравнить 2 хэша.

        //Rfc2898DeriveBytes реализует функцию формирования ключа на основе пароля (PBKDF2) посредством генератора псевдослучайных чисел HMACSHA1.
        var hash = Rfc2898DeriveBytes.Pbkdf2(password: password,
            salt: salt,
            iterations: Iterations,
            hashAlgorithm: _algorithmName,
            outputLength: HashSize);

        //Hex String - шестнадцатеричная строка
        return $"{Convert.ToHexString(hash)}-{Convert.ToHexString(salt)}";
    }

    public bool IsPasswordRight(string password, HashedPasswordValueObject hashedPassword)
    {
        string[] parts = hashedPassword.Password.Split('-');
        string hashedString = parts[0];
        byte[] salt = Convert.FromHexString(parts[1]);

        return AreStringAndHashSame(password, hashedString, salt);
    }

    public bool AreStringAndHashSame(string str, string hashedString, byte[] salt)
    {
        byte[] hash = Convert.FromHexString(hashedString);

        var inputHash = Rfc2898DeriveBytes.Pbkdf2(password: str,
            salt: salt,
            iterations: Iterations,
            hashAlgorithm: _algorithmName,
            outputLength: HashSize);

        return CryptographicOperations.FixedTimeEquals(hash, inputHash);
    }


    public bool AreHashesSame(byte[] firstHash, byte[] secondHash)
    {
        return CryptographicOperations.FixedTimeEquals(firstHash, secondHash);
    }

    public byte[] GenerateSalt()
    {
        return RandomNumberGenerator.GetBytes(SaltSize);
    }
}