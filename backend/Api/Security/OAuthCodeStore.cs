using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace Api.Security;

public sealed class OAuthCodeStore
{
    private static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(5);
    private readonly ConcurrentDictionary<string, StoredAuthorizationCode> codes = new(StringComparer.Ordinal);

    public string CreateCode(AuthorizationCodeRequest request)
    {
        PruneExpiredCodes();
        var code = Base64Url.Encode(RandomNumberGenerator.GetBytes(32));
        var storedCode = new StoredAuthorizationCode(
            request.ClientId,
            request.RedirectUri,
            request.Scope,
            request.CodeChallenge,
            request.Subject,
            request.DisplayName,
            DateTimeOffset.UtcNow.Add(CodeLifetime));

        codes[code] = storedCode;
        return code;
    }

    public bool TryConsumeCode(
        string code,
        string clientId,
        string redirectUri,
        string verifier,
        out StoredAuthorizationCode storedCode)
    {
        storedCode = default!;
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(verifier))
        {
            return false;
        }

        if (!codes.TryRemove(code, out var candidate))
        {
            return false;
        }

        if (candidate.ExpiresAt < DateTimeOffset.UtcNow ||
            candidate.ClientId != clientId ||
            candidate.RedirectUri != redirectUri)
        {
            return false;
        }

        var computedChallenge = Base64Url.Encode(SHA256.HashData(System.Text.Encoding.ASCII.GetBytes(verifier)));
        if (!CryptographicOperations.FixedTimeEquals(
                System.Text.Encoding.ASCII.GetBytes(candidate.CodeChallenge),
                System.Text.Encoding.ASCII.GetBytes(computedChallenge)))
        {
            return false;
        }

        storedCode = candidate;
        return true;
    }

    private void PruneExpiredCodes()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var pair in codes.Where(pair => pair.Value.ExpiresAt < now))
        {
            codes.TryRemove(pair.Key, out _);
        }
    }
}
