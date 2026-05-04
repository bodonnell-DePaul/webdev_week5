using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Api.Security;

public sealed class DemoBearerAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    JwtTokenService tokens)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "DemoBearer";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authorization = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authorization) ||
            !AuthenticationHeaderValue.TryParse(authorization, out var header) ||
            !"Bearer".Equals(header.Scheme, StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(header.Parameter))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        try
        {
            ClaimsPrincipal principal = tokens.ValidateAccessToken(header.Parameter);
            var ticket = new AuthenticationTicket(principal, SchemeName);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch (InvalidOperationException ex)
        {
            return Task.FromResult(AuthenticateResult.Fail(ex.Message));
        }
        catch (FormatException)
        {
            return Task.FromResult(AuthenticateResult.Fail("Token encoding is invalid."));
        }
        catch (JsonException)
        {
            return Task.FromResult(AuthenticateResult.Fail("Token payload is invalid."));
        }
    }
}
