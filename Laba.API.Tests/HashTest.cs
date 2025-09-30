using Laba.API.Services;

namespace Laba.API.Tests;

public class HashTest
{
    [Theory]
    [InlineData("test")]
    [InlineData("Hello-wordl")]
    [InlineData("kjadsflkadjsf2oijfo2f3")]
    public void HashIsCorrect(string str)
    {
        var hashedString = HashService.Hash(str);

        Assert.NotEmpty(hashedString);
        Assert.NotEqual(hashedString, str);

        var isEqual = HashService.AreBothSame(str, hashedString);
        Assert.True(isEqual);
    }

    [Theory]
    [InlineData("test", "test2")]
    [InlineData("Hello-wordl", "123")]
    [InlineData("kjadsflkadjsf2oijfo2f3", "alskdfasdlkfjsalkd")]
    public void HashIncorrect(string str, string otherStr)
    {
        var hashedString1 = HashService.Hash(str);
        var hashedString2 = HashService.Hash(otherStr);

        Assert.NotEmpty(hashedString1);
        Assert.NotEmpty(hashedString2);
        Assert.NotEqual(hashedString1, hashedString2);

        Assert.False(HashService.AreBothSame(otherStr, hashedString1));
        Assert.False(HashService.AreBothSame(str, hashedString2));
    }

    private HashService HashService => _service;
    private readonly HashService _service = new();
}