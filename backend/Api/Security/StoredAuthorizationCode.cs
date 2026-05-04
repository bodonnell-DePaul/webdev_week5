namespace Api.Security;

public sealed record StoredAuthorizationCode(
    string ClientId,
    string RedirectUri,
    string Scope,
    string CodeChallenge,
    string Subject,
    string DisplayName,
    DateTimeOffset ExpiresAt);
