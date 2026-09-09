# SentinelQA Web (Angular 20)

Enterprise UI for the SentinelQA network security policy & QA platform.

## Stack
- Angular 20 (standalone components, signals, new control flow, lazy routes)
- TypeScript strict mode, SCSS design system (enterprise dark theme)
- RxJS + `HttpClient` with JWT auth interceptor (single-flight refresh), ProblemDetails error normalization, correlation IDs
- RBAC guards (`roleGuard`) + `*sqHasRole` directive
- Playwright for E2E tests

## Prerequisites
- Node.js ≥ 20.19 (LTS 22 recommended)
- Backend API running on `https://localhost:7043` (see backend README)

## Run locally
```bash
cd frontend/sentinelqa-web
npm ci
npm start            # http://localhost:4200 (API proxied via proxy.conf.json)
```

## Tests
```bash
npx playwright install chromium
npm run e2e          # headless
npm run e2e:ui       # interactive UI mode
```

## Architecture map
```
src/app
├── core/        # models, auth, guards, interceptors, API services (no UI)
├── shared/      # data-table, status-badge, pagination, pipes, validators
├── layout/      # shell + sidebar + topbar
└── features/    # lazy-loaded bounded contexts (policies, change-requests, …)
```

## Security notes
- Access token kept **in memory only**; refresh token persisted for silent re-auth.
- Audit/no-cache endpoints rely on server `Cache-Control: no-store`.
- AI Copilot output is explicitly marked advisory — never authoritative.