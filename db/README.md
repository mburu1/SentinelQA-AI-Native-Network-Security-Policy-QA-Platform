# SentinelQA Database Scripts

## Execution order (PostgreSQL)
1. `postgresql/migrations/*.sql` (numeric order)
2. `postgresql/functions/*.sql`
3. `postgresql/indexes/*.sql`
4. `postgresql/seeds/*.sql`

Docker Compose applies these automatically on first boot via
`infrastructure/docker/postgres-init.sh`.

## Dev-only credential scheme
Seeded users carry `password_hash = 'dev-only-sha256:' || sha256(password)`.
The API accepts this scheme ONLY when `ASPNETCORE_ENVIRONMENT=Development`.
Production uses the real KDF hash written by the Identity module.
Default dev password: `SentinelQA-Dev-2026!`

## MongoDB
Run with `mongosh` against the `sentinelqa` database:
1. `mongodb/collections/*.js`
2. `mongodb/indexes/*.js`
3. `mongodb/seed/*.js`