using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Api.Security;

public sealed class JwtTokenService
{
    public const int AccessTokenLifetimeSeconds = 900;
    private const string Issuer = "webdev-week5-demo-auth";
    private const string Audience = "national-parks-api";
    private readonly byte[] signingKey = RandomNumberGenerator.GetBytes(32);

    public string CreateAccessToken(StoredAuthorizationCode code)
    {
        var now = DateTimeOffset.UtcNow;
        var header = new Dictionary<string, object>
        {
            ["alg"] = "HS256",
            ["typ"] = "JWT"
        };
        var payload = new Dictionary<string, object>
        {
            ["iss"] = Issuer,
            ["aud"] = Audience,
            ["sub"] = code.Subject,
            ["name"] = code.DisplayName,
            ["scope"] = code.Scope,
            ["iat"] = now.ToUnixTimeSeconds(),
            ["exp"] = now.AddSeconds(AccessTokenLifetimeSeconds).ToUnixTimeSeconds()
        };

        var encodedHeader = Base64Url.Encode(JsonSerializer.SerializeToUtf8Bytes(header));
        var encodedPayload = Base64Url.Encode(JsonSerializer.SerializeToUtf8Bytes(payload));
        var signature = Sign($"{encodedHeader}.{encodedPayload}");
        return $"{encodedHeader}.{encodedPayload}.{signature}";
    }

    public ClaimsPrincipal ValidateAccessToken(string token)
    {
        var parts = token.Split('.');
        if (parts.Length != 3)
        {
            throw new InvalidOperationException("Malformed token.");
        }

        var expectedSignature = Sign($"{parts[0]}.{parts[1]}");
        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.ASCII.GetBytes(expectedSignature),
                Encoding.ASCII.GetBytes(parts[2])))
        {
            throw new InvalidOperationException("Invalid token signature.");
        }

        var header = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(Base64Url.DecodeText(parts[0])) ?? [];
        if (!header.TryGetValue("alg", out var alg) || alg.GetString() != "HS256")
        {
            throw new InvalidOperationException("Invalid token algorithm.");
        }

        var payload = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(Base64Url.DecodeText(parts[1])) ?? [];
        RequireString(payload, "iss", Issuer);
        RequireString(payload, "aud", Audience);

        var exp = payload.TryGetValue("exp", out var expElement) ? expElement.GetInt64() : 0;
        if (DateTimeOffset.FromUnixTimeSeconds(exp) <= DateTimeOffset.UtcNow)
        {
            throw new InvalidOperationException("Token expired.");
        }

        var identity = new ClaimsIdentity(DemoBearerAuthenticationHandler.SchemeName);
        AddClaim(identity, ClaimTypes.NameIdentifier, payload, "sub");
        AddClaim(identity, ClaimTypes.Name, payload, "name");
        AddClaim(identity, "scope", payload, "scope");
        return new ClaimsPrincipal(identity);
    }

    private string Sign(string unsignedToken)
    {
        using var hmac = new HMACSHA256(signingKey);
        return Base64Url.Encode(hmac.ComputeHash(Encoding.ASCII.GetBytes(unsignedToken)));
    }

    private static void RequireString(Dictionary<string, JsonElement> payload, string key, string expected)
    {
        if (!payload.TryGetValue(key, out var value) || value.GetString() != expected)
        {
            throw new InvalidOperationException($"Invalid {key} claim.");
        }
    }

    private static void AddClaim(ClaimsIdentity identity, string claimType, Dictionary<string, JsonElement> payload, string key)
    {
        if (payload.TryGetValue(key, out var value) && value.GetString() is { } text)
        {
            identity.AddClaim(new Claim(claimType, text));
        }
    }
}
