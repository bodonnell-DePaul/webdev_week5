namespace Api.Security;

public static class OAuthOptions
{
    public const string ClientId = "react-demo-client";
    public const string DatasetScope = "national-parks.read";
    public const string DemoSubject = "student-123";
    public const string DemoDisplayName = "Course Demo Student";

    private static readonly HashSet<string> RedirectUris = new(StringComparer.Ordinal)
    {
        "http://localhost:5173/callback",
        "http://127.0.0.1:5173/callback"
    };

    public static bool IsValidClient(string clientId) => clientId == ClientId;

    public static bool IsValidRedirectUri(string redirectUri) => RedirectUris.Contains(redirectUri);

    public static bool IsAllowedScope(string scope) =>
        scope.Split(' ', StringSplitOptions.RemoveEmptyEntries).All(value => value == DatasetScope);
}
