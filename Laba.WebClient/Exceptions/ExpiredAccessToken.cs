namespace Laba.WebClient.Exceptions;

public class ExpiredAccessToken : UnauthorizedAccessException
{
    public override string Message => "The user have expired access token.";
}