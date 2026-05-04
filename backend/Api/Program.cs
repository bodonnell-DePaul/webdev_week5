using Api.Models;
using Api.Security;
using Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddSingleton<OAuthCodeStore>();
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddSingleton<INationalParkService, NationalParkService>();
builder.Services.AddAuthentication(DemoBearerAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, DemoBearerAuthenticationHandler>(
        DemoBearerAuthenticationHandler.SchemeName,
        options => { });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DatasetReader", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
            context.User.FindAll("scope")
                .Any(claim => claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Contains(OAuthOptions.DatasetScope, StringComparer.Ordinal)));
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("ReactApp");
app.UseAuthentication();
app.UseAuthorization();

var oauth = app.MapGroup("/oauth").WithTags("OAuth demo");

oauth.MapGet("/authorize", (
    string response_type,
    string client_id,
    string redirect_uri,
    string code_challenge,
    string code_challenge_method,
    string? scope,
    string? state,
    OAuthCodeStore codeStore) =>
{
    if (response_type != "code")
    {
        return Results.BadRequest(new ErrorResponse("unsupported_response_type", "Only authorization code flow is supported."));
    }

    if (!OAuthOptions.IsValidClient(client_id) || !OAuthOptions.IsValidRedirectUri(redirect_uri))
    {
        return Results.BadRequest(new ErrorResponse("invalid_client", "Unknown client or redirect URI."));
    }

    if (code_challenge_method != "S256" || string.IsNullOrWhiteSpace(code_challenge))
    {
        return Results.BadRequest(new ErrorResponse("invalid_request", "PKCE S256 code challenge is required."));
    }

    var requestedScope = string.IsNullOrWhiteSpace(scope) ? OAuthOptions.DatasetScope : scope;
    if (!OAuthOptions.IsAllowedScope(requestedScope))
    {
        return Results.BadRequest(new ErrorResponse("invalid_scope", "The requested scope is not available."));
    }

    var code = codeStore.CreateCode(new AuthorizationCodeRequest(
        client_id,
        redirect_uri,
        requestedScope,
        code_challenge,
        OAuthOptions.DemoSubject,
        OAuthOptions.DemoDisplayName));

    return Results.Redirect(BuildRedirectUri(redirect_uri, code, state));
});

oauth.MapPost("/token", async (HttpRequest request, OAuthCodeStore codeStore, JwtTokenService tokens) =>
{
    if (!request.HasFormContentType)
    {
        return Results.BadRequest(new ErrorResponse("invalid_request", "Token requests must use form-url-encoded content."));
    }

    var form = await request.ReadFormAsync();
    var grantType = form["grant_type"].ToString();
    var code = form["code"].ToString();
    var redirectUri = form["redirect_uri"].ToString();
    var clientId = form["client_id"].ToString();
    var verifier = form["code_verifier"].ToString();

    if (grantType != "authorization_code")
    {
        return Results.BadRequest(new ErrorResponse("unsupported_grant_type", "Only authorization_code is supported."));
    }

    if (!codeStore.TryConsumeCode(code, clientId, redirectUri, verifier, out var authCode))
    {
        return Results.BadRequest(new ErrorResponse("invalid_grant", "Authorization code or PKCE verifier is invalid."));
    }

    var token = tokens.CreateAccessToken(authCode);
    return Results.Ok(new TokenResponse(
        token,
        "Bearer",
        JwtTokenService.AccessTokenLifetimeSeconds,
        authCode.Scope,
        authCode.Subject,
        authCode.DisplayName));
});

app.MapGet("/api/status", () => new ApiStatus(
        "National Parks OAuth Dataset API",
        "Use /oauth/authorize with PKCE, then call protected /api/parks routes with the bearer token.",
        OAuthOptions.DatasetScope))
    .WithTags("Public");

var parks = app.MapGroup("/api/parks")
    .RequireAuthorization("DatasetReader")
    .WithTags("Protected dataset");

parks.MapGet("/", (INationalParkService service) => Results.Ok(service.GetAll()));

parks.MapGet("/{id:int}", (int id, INationalParkService service) =>
    service.GetById(id) is { } park ? Results.Ok(park) : Results.NotFound(new ErrorResponse("not_found", "Park not found.")));

parks.MapGet("/search", (string? state, string? q, INationalParkService service) =>
{
    if (state is { Length: > 2 })
    {
        return Results.BadRequest(new ErrorResponse("invalid_state", "Use a two-letter state code."));
    }

    return Results.Ok(service.Search(state, q));
});

app.Run();

static string BuildRedirectUri(string redirectUri, string code, string? state)
{
    var query = new List<string> { $"code={Uri.EscapeDataString(code)}" };
    if (!string.IsNullOrWhiteSpace(state))
    {
        query.Add($"state={Uri.EscapeDataString(state)}");
    }

    var separator = redirectUri.Contains('?', StringComparison.Ordinal) ? '&' : '?';
    return $"{redirectUri}{separator}{string.Join('&', query)}";
}
