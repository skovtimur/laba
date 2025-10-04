using Laba.API.Services;
using Laba.Shared.Domain.ValueObjects;
using Laba.Shared.Services;

namespace Laba.API.Tests;

public class HashTest
{
    [Theory]
    [InlineData("test")]
    [InlineData("Hello-wordl")]
    [InlineData("kjadsflkadjsf2oijfo2f3")]
    public void Verify_Password_Are_Right(string str)
    {
        var hashedString = HashService.HashPassword(str);

        Assert.NotEmpty(hashedString);
        Assert.NotEqual(hashedString, str);

        var isEqual = HashService.IsPasswordRight(str, new HashedPasswordValueObject { Password = hashedString });
        Assert.True(isEqual);
    }

    [Theory]
    [InlineData("test", "test2")]
    [InlineData("Hello-wordl", "123")]
    [InlineData("kjadsflkadjsf2oijfo2f3", "alskdfasdlkfjsalkd")]
    public void Verify_Passwords_Are_Not_Right(string str, string otherStr)
    {
        var hashedString1 = HashService.HashPassword(str);
        var hashedString2 = HashService.HashPassword(otherStr);

        Assert.NotEmpty(hashedString1);
        Assert.NotEmpty(hashedString2);
        Assert.NotEqual(hashedString1, hashedString2);

        Assert.False(HashService.IsPasswordRight(otherStr, new HashedPasswordValueObject { Password = hashedString1 }));
        Assert.False(HashService.IsPasswordRight(str, new HashedPasswordValueObject { Password = hashedString2 }));
    }


    [Theory]
    [InlineData("test")]
    [InlineData("Hello-wordl")]
    [InlineData("kjadsflkadjsf2oijfo2f3")]
    public void Verify_Hashes_Are_Same(string str)
    {
        var salt = HashService.GenerateSalt();
        var hashedString1 = HashService.HashPassword(str, salt);
        var hashedString2 = HashService.HashPassword(str, salt);

        Assert.NotEmpty(hashedString1);
        Assert.NotEmpty(hashedString2);

        Assert.True(HashService.AreHashesSame(
            Convert.FromHexString(hashedString1.Split("-")[0]),
            Convert.FromHexString(hashedString1.Split("-")[0])));
    }

    private HashService HashService => _service;
    private readonly HashService _service = new();
}