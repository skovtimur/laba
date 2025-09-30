using System.Security.Cryptography;
using Laba.API.Abstract.Interfaces.ServiceInterfaces;

namespace Laba.API.Services;

public class HashService : IHash, IHashVerify
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100000;
    private readonly HashAlgorithmName AlgorithmName = HashAlgorithmName.SHA512;

    public string Hash(string str)
    {
        // Как работает:
        // Хеш фукнция берет значения и salt, после превращает это все в уникальную строку
        // Salt - рандомные числа
        // Хэш функция может сравнить 2 хэша.

        //Rfc2898DeriveBytes реализует функцию формирования ключа на основе пароля (PBKDF2) посредством генератора псевдослучайных чисел HMACSHA1.
        var salt = GenerateSalt();
        var hash = Rfc2898DeriveBytes.Pbkdf2(password: str,
            salt: salt,
            iterations: Iterations,
            hashAlgorithm: AlgorithmName,
            outputLength: HashSize);

        //Hex String - шестнадцатеричная строка
        return $"{Convert.ToHexString(hash)}-{Convert.ToHexString(salt)}";
    }

    public bool AreBothSame(string str, string hashedString)
    {
        string[] parts = hashedString.Split('-');
        byte[] hash = Convert.FromHexString(parts[0]);
        byte[] salt = Convert.FromHexString(parts[1]);

        var inputHash = Rfc2898DeriveBytes.Pbkdf2(password: str,
            salt: salt,
            iterations: Iterations,
            hashAlgorithm: AlgorithmName,
            outputLength: HashSize);

        return CryptographicOperations.FixedTimeEquals(hash, inputHash);
    }

    private byte[] GenerateSalt()
    {
        return RandomNumberGenerator.GetBytes(SaltSize);
    }
}