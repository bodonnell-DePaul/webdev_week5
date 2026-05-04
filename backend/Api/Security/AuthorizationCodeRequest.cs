namespace Api.Security;

public sealed record AuthorizationCodeRequest(
    string ClientId,
    string RedirectUri,
    string Scope,
    string CodeChallenge,
    string Subject,
    string DisplayName);
