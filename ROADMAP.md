# ROADMAP.md

> Antigravity ticks boxes as work completes. Work one phase at a time.

## Phase 1 — Analysis & PRD
- [ ] Review old repo features and gaps (see docs/PRD.md §2)
- [ ] Confirm MVP vs v2 scope
- [ ] Finalize user stories and acceptance criteria

## Phase 2 — Setup
- [ ] Create solution `Clutch` (Web, Tests)
- [ ] Add packages (EF Core SqlServer, Identity, SignalR client lib, xUnit, FluentAssertions)
- [ ] docker-compose with SQL Server, `.env` wiring, user-secrets
- [ ] Copy design/genz-theme.css to wwwroot/css, add fonts
- [ ] CI passing on empty solution

## Phase 3 — Design
- [ ] Finalize ERD in docs/DB_SCHEMA.md
- [ ] Build layout (`_Layout`, nav, footer, toast, empty-state partials)
- [ ] Static mock pages: Home, Event detail, Team page, Judge scoring, Leaderboard, Admin dashboard
- [ ] Accessibility pass on tokens (contrast, focus)

## Phase 4 — Foundation
- [ ] Entities + DbContext + first migration
- [ ] Identity with roles; seeded Admin from env vars
- [ ] Policies and authorization filters
- [ ] Service layer skeleton + DI
- [ ] Global error page, logging, validation helpers

## Phase 5 — Core features
- [ ] Hackathon CRUD (Draft/Published/Closed/Cancelled), deadlines, tracks
- [ ] Participant registration + team creation, unique team name, join code, size limits
- [ ] Team member management (join, leave, remove, transfer lead)
- [ ] Project submission (title, description, repo URL, demo URL, track)
- [ ] Notifications (in-app) + announcements

## Phase 6 — Judging
- [ ] Judge assignment per hackathon
- [ ] Rubric criteria with weights
- [ ] Score entry per judge per team per criterion; edit until locked
- [ ] Scoring service: weighted total, multi-judge average, tie-break rules
- [ ] Admin finalize/lock results

## Phase 7 — Extras
- [ ] SignalR live leaderboard
- [ ] Email notifications (SMTP, templated)
- [ ] CSV export (teams, scores)
- [ ] Certificates (PDF) or shareable result card
- [ ] Audit log viewer

## Phase 8 — Testing & hardening
- [ ] Unit tests for services (scoring, registration rules)
- [ ] Integration tests (auth, roles, happy paths)
- [ ] Security pass (CSRF, validation, rate limiting, headers, IDOR checks)
- [ ] Responsive + a11y pass
- [ ] Performance pass (indexes, N+1)

## Phase 9 — Deployment
- [ ] Production Dockerfile + compose
- [ ] CI/CD deploy workflow
- [ ] Production config, migrations step, seed script
- [ ] Final README, demo data, screenshots
