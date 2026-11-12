# DEPLOYMENT

## Supported Targets
- **Docker Compose (Self-hosted / VM)**: Full stack (`web` + `db` SQL Server 2022) defined in `docker-compose.yml`.
- **Container Platforms (Render / Railway / Azure Container Apps / Fly.io)**: Deploy using the multi-stage `Dockerfile` (`target: final`, port `8080`) paired with Azure SQL or managed SQL Server.
- **GitHub Container Registry (GHCR)**: Automated build & push workflow in `.github/workflows/deploy.yml` produces both the runtime web image and a standalone `migrator` bundle image.

---

## Configuration & Environment Variables
Copy `.env.example` to `.env` (never commit `.env` or secrets to git) and configure:

| Variable | Purpose |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | Set to `Production` |
| `ASPNETCORE_FORWARDEDHEADERS_ENABLED` | Set to `true` when running behind TLS-terminating reverse proxies |
| `MSSQL_SA_PASSWORD` | Strong SQL Server `sa` password for Docker Compose `db` container |
| `ConnectionStrings__DefaultConnection` | SQL Server connection string (`TrustServerCertificate=True` if self-signed) |
| `SeedAdmin__Email` / `SeedAdmin__Password` | Admin credentials used exclusively by `--seed` |
| `SeedDemo__Password` | Password assigned to sample demo accounts when running `--seed-demo` |
| `Smtp__Host`, `Port`, `User`, `Password`, `From` | Optional SMTP relay for notification emails |

For local development outside Docker, use `dotnet user-secrets` (`src/Clutch.Web/Clutch.Web.csproj` already defines `<UserSecretsId>`).

---

## Release & Deployment Steps

1. **Build container image**:
   ```bash
   docker build -t clutch:latest .
   ```
   Or build and start the full stack with Docker Compose:
   ```bash
   cp .env.example .env
   # Edit .env with strong passwords
   docker compose up -d --build
   ```

2. **Apply EF Core migrations as an explicit release step** (migrations never run silently on startup):
   - Via CLI:
     ```bash
     dotnet ef database update --project src/Clutch.Web
     ```
   - Or via self-contained migration bundle (`migrator` stage in `Dockerfile`):
     ```bash
     docker build --target migrator -t clutch-migrator .
     docker run --rm clutch-migrator /app/efbundle --connection "$ConnectionStrings__DefaultConnection"
     ```

3. **Seed roles and initial Admin account** (one-time explicit command):
   ```bash
   dotnet run --project src/Clutch.Web -- --seed
   # Or inside the deployed container:
   docker compose exec web dotnet Clutch.Web.dll --seed
   ```
   For staging/demo environments, sample events, tracks, teams, rubric criteria, and scores can be seeded with:
   ```bash
   docker compose exec web dotnet Clutch.Web.dll --seed-demo
   ```

4. **Verify health endpoints**:
   - Liveness: `GET /health` $\to$ HTTP `200` (`{"status":"healthy","service":"clutch",...}`)
   - Readiness (verifies SQL Server connectivity): `GET /health/ready` $\to$ HTTP `200` (`{"status":"ready","database":"connected",...}`) or `503` if database is unreachable.

---

## Production Checklist
- [x] `ForwardedHeaders` configured (`X-Forwarded-For`, `X-Forwarded-Proto`) for reverse-proxy TLS termination
- [x] HTTPS enforced and HSTS enabled in non-Development environments
- [x] Friendly error page (`/Home/Error`) without leaked stack traces
- [x] Security headers middleware (`X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, `Permissions-Policy`)
- [x] Non-root runtime user (`$APP_UID`) and `.dockerignore` excluding secrets and artifacts
- [x] Health checks (`/health` and `/health/ready`) wired into `Dockerfile` and `docker-compose.yml`
- [ ] Configure automated SQL Server backups and SMTP relay in your production host
- [ ] Ensure WebSockets are enabled on the reverse proxy for SignalR (`/hubs/leaderboard`)

---

## Rollback
1. Redeploy the previous container image tag from GHCR (`ghcr.io/<owner>/<repo>:sha-<prev>`).
2. If a schema migration caused an issue, revert only backward-compatible migrations via `dotnet ef database update <PreviousMigration>` or restore the pre-release database snapshot.
