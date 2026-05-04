# Week 5 OAuth Example Application

This repository contains a classroom example that applies the Week 5 security/OAuth notes and incorporates topics from earlier weeks. The application uses a React frontend and a .NET 10 backend API.

## What it demonstrates

- **React frontend:** components, props, state, effects, forms, conditional rendering, and guarded pages.
- **.NET 10 backend:** Minimal APIs, endpoint groups, CORS, OpenAPI, service dependency injection, validation, and protected routes.
- **OAuth/OIDC concepts:** OAuth roles, authorization code flow, PKCE, access tokens, bearer authorization, scopes, and guarded resource APIs.
- **Open dataset:** a small educational subset based on public U.S. National Park Service facts about national parks.

## Project structure

```text
backend/Api      .NET 10 protected resource API and local demo OAuth authorization server
frontend         React + TypeScript + Vite single-page application
DataPresistence.md  Week 4 persistence notes retained from the original repository
```

## Run the backend

```bash
cd backend/Api
dotnet restore
dotnet run --launch-profile http
```

The API runs at `http://localhost:5015` by default. OpenAPI is available in development at `http://localhost:5015/openapi/v1.json`.

## Run the frontend

```bash
cd frontend
npm install
npm run dev
```

The React app runs at `http://localhost:5173`.

## Try the OAuth flow

1. Open `http://localhost:5173`.
2. Select **Sign in with OAuth**.
3. The frontend creates a PKCE verifier/challenge and redirects to `/oauth/authorize` on the backend.
4. The backend simulates a trusted authorization server for the course demo and redirects back with an authorization code.
5. The frontend exchanges that code at `/oauth/token` for a short-lived bearer token.
6. Guarded pages use the token to call protected routes such as `/api/parks` and `/api/parks/search`.

The demo grants the `national-parks.read` scope. The protected dataset endpoints reject requests that do not include a valid bearer token with that scope.

## Important security note

This is an educational OAuth/PKCE demonstration that keeps all pieces local so it can run without external provider secrets. A production application should use a trusted OAuth/OIDC provider, validate issuer/audience/signatures against that provider, use HTTPS, and choose secure token storage appropriate for the app.
