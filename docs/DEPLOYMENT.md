# DEPLOYMENT

## Targets (pick one)
- Render / Railway / Azure App Service using the Dockerfile.
- Database: Azure SQL, or SQL Server container on a VM, or managed SQL Server provider.

## Steps
1. Set env vars from `.env.example` in the host (never commit secrets).
2. Build image: `docker build -t clutch .`
3. Run migrations as a release step: `dotnet ef database update` (or a migration bundle: `dotnet ef migrations bundle`).
4. Start app; confirm `/health` returns 200.
5. Seed admin runs once from SeedAdmin__* env vars; change password after first login.

## Checklist
- [ ] HTTPS enforced, HSTS on
- [ ] Production error page (no stack traces)
- [ ] SMTP configured and test email sent
- [ ] Backups for the database
- [ ] CORS closed (same-origin only)
- [ ] SignalR works behind the proxy (WebSockets enabled)

## Rollback
Redeploy previous image tag. For schema issues, apply the previous migration only if it is backward compatible; otherwise restore backup.
