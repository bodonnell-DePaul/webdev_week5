namespace Api.Models;

public sealed record ErrorResponse(string Error, string Description);

public sealed record TokenResponse(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    string Scope,
    string Subject,
    string DisplayName);

public sealed record ApiStatus(string Name, string Instructions, string RequiredScope);
