using System.Security.Claims;

namespace Laba.Shared.Extensions;

public static class ClaimExtensions
{
    public static bool TryGetUserId(this ClaimsPrincipal principal, out Guid userId)
    {
        var claims = principal.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

        if (claims != null
            && Guid.TryParse(claims.Value, out var id))
        {
            userId = id;
            return true;
        }

        userId = Guid.Empty;
        return false;
    }
}