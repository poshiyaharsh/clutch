# PROMPTS — paste into Antigravity, one phase at a time

> Always start a session with: "Read AGENTS.md, ROADMAP.md, INDEX.md and the last 5 entries of PROJECT_LOG.md. Then wait."

## Phase 1 — Analysis
"Read docs/PRD.md. Review it against the v1 README gaps. List any missing user stories, edge cases and risks. Update docs/PRD.md, tick Phase 1 in ROADMAP.md, add a PROJECT_LOG.md entry. Do not write code."

## Phase 2 — Setup
"Create solution Clutch with src/Clutch.Web (ASP.NET Core 8 MVC) and tests/Clutch.Tests (xUnit). Add the packages listed in AGENTS.md. Wire docker-compose SQL Server, user-secrets and .env. Copy design/genz-theme.css to wwwroot/css and load it in _Layout with the fonts. Make CI pass. Update INDEX.md, ROADMAP.md, PROJECT_LOG.md."

## Phase 3 — Design
"Using docs/DESIGN_SYSTEM.md, build _Layout (nav, footer, notification bell placeholder, toast partial, empty-state partial) and static mock Razor pages for Home, Event detail, Team page, Judge scoring, Leaderboard, Organizer dashboard. Use fake data. Check 375px and 1280px. Finalize docs/DB_SCHEMA.md. Update log, index, roadmap."

## Phase 4 — Foundation
"Implement entities from docs/DB_SCHEMA.md, AppDbContext with configurations, first migration, Identity with the 4 roles, seeded admin from env vars, authorization policies from docs/ARCHITECTURE.md, service interfaces, DI, global error page, Serilog. Add tests for policies. Update docs, log, index, roadmap."

## Phase 5 — Core features
"Implement hackathon CRUD with status transitions, registration deadline, tracks, team creation with unique name and join code, join/leave/remove/transfer lead, project submission, in-app notifications and announcements. Replace mock data in views. Add unit and integration tests from docs/TEST_PLAN.md. Update docs, log, index, roadmap."

## Phase 6 — Judging
"Implement judge assignment, rubric criteria with weights summing to 100, per-criterion score entry, IScoringService (weighted total, multi-judge average, tie-break), judging lock, and the leaderboard page. Add tests for scoring math and authorization. Update docs, log, index, roadmap."

## Phase 7 — Extras
"Add the SignalR LeaderboardHub with live updates and polling fallback, SMTP email notifications behind IEmailSender, CSV export, result card or PDF certificate, and an audit log viewer. Implement the leaderboard tick animation from DESIGN_SYSTEM.md. Update docs, log, index, roadmap."

## Phase 8 — Testing and hardening
"Run the full TEST_PLAN.md. Add missing tests. Do a security pass: CSRF on all POSTs, IDOR checks on every id route, rate limiting on auth, security headers, input validation. Do an accessibility and responsive pass. Fix N+1 queries and add missing indexes. Report findings in PROJECT_LOG.md."

## Phase 9 — Deployment
"Finalize Dockerfile and docker-compose.prod.yml, add a /health endpoint, add a deploy job to CI, write production config notes in docs/DEPLOYMENT.md, add seed demo data script and final README with screenshots. Update log, index, roadmap."

## Handy one-liners
- "Continue the current phase from ROADMAP.md, next unchecked item only."
- "Review the last PROJECT_LOG.md entry and verify it against the code. Fix drift in INDEX.md."
- "Run /end-session."
