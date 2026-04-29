# Auth Refresh + CSRF + Tenant Select

## Cookie policy

- `rt` (refresh token cookie)
  - `HttpOnly=true`, `Path=/auth`, host-only (no `Domain`), `Max-Age/Expires` 30 days.
  - `Secure=true` in production; in development it can be `false` only when running over HTTP.
  - `SameSite` is environment-configured (`Lax` by default, `None` only for real cross-site + secure).
  - Server stores only `SHA-256(refreshToken + pepper)`.
- `__Host-af` / `af` (framework antiforgery cookie)
  - Set by ASP.NET Core antiforgery (`IAntiforgery.GetAndStoreTokens`).
  - `HttpOnly=true`, `Path=/`, header `X-XSRF-TOKEN`.
  - Uses `__Host-af` when secure policy is enforced, otherwise `af`.
- `XSRF-TOKEN` (SPA readable request token)
  - Set by API from `tokens.RequestToken`.
  - `HttpOnly=false`, `Path=/` and same `SameSite/Secure` policy as auth cookies.

## Refresh rotation and reuse detection

- Refresh tokens are one-time-use.
- `POST /auth/refresh` revokes the current token and rotates to a new token/cookie.
- Reuse detection:
  - Reuse within `20s` grace window returns `200` (concurrency-safe path) without invalidating the family.
  - Reuse after `20s` is treated as compromise: revoke whole token family (`SessionId`) and return `401`.

## CORS by environment

- Policy allows credentials and explicit origins from config.
- Allowed headers: `Authorization`, `Content-Type`, `X-XSRF-TOKEN`.
- Allowed methods: `POST`, `OPTIONS`.

## Rate limiting

- `POST /auth/login`: by IP + user key (email), fixed window (8 req/min).
- `POST /auth/refresh`: by `SessionId` token family, fixed window (45 req/min).
- `POST /auth/logout`: soft by IP, fixed window (20 req/min).
- On limit breach the API returns `429` with `ProblemDetails`.

`appsettings.Development.json`

- `AllowedOrigins`: `http://localhost:5173`
- `CookieSecure`: `false` (if local HTTP) or `true` (if local HTTPS)
- `SameSite`: `Lax` (proxy/same-site) or `None` (real cross-site)

`appsettings.Production.json`

- `AllowedOrigins`: `https://app.tudominio.cl`
- `CookieSecure`: `true`
- `SameSite`: `Lax` by default (`None` only for real cross-site)

## CSRF flow (ASP.NET Core antiforgery)

- Auth endpoints contract: `POST /auth/login`, `POST /auth/refresh`, `POST /auth/logout` are cookie-based and must not send `Authorization` header.
- Login bootstrap (`POST /auth/login`): emits antiforgery cookie (`__Host-af`/`af`) and `XSRF-TOKEN` via `GetAndStoreTokens`.
- Protected auth actions:
  - `POST /auth/refresh` validates antiforgery (`ValidateRequestAsync`).
  - `POST /auth/logout` validates antiforgery (`ValidateRequestAsync`).
- Hardening: antiforgery issue/validation is executed against an anonymous principal for auth endpoints to avoid claims-principal mismatch.
- Antiforgery validation failures return `400` with `ProblemDetails`.

## HTTP responses

| Endpoint | Success | CSRF fail | Unauthorized | Forbidden | Rate limit |
|---|---:|---:|---:|---:|---:|
| `POST /auth/login` | `200` | n/a | `401` | n/a | `429` |
| `POST /auth/refresh` | `200` | `400` | `401` | n/a | `429` |
| `POST /auth/logout` | `204` | `400` | n/a (idempotent) | n/a | `429` |
| `POST /api/tenant/select` | `200` | n/a | `401` (no bearer) | `403` | n/a |

## SPA client requirement

- The SPA must send `X-XSRF-TOKEN` header on `/auth/refresh` and `/auth/logout`.
- Calls to `/auth/*` must use `credentials: "include"`.
- Calls to `/auth/login`, `/auth/refresh`, and `/auth/logout` must NOT send `Authorization` header.
- `/api/tenant/select` remains bearer-token based (it may also use credentials include, but it does not depend on refresh cookies).
