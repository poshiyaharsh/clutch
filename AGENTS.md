# AGENTS.md — Clutch (Hackathon Management Portal v2)

> Antigravity: read this file FIRST in every session. Then read ROADMAP.md, INDEX.md and the last 5 entries of PROJECT_LOG.md.

## 1. Project
Clutch is a role-based hackathon portal: events, team registration, project submissions, rubric judging, live leaderboard.
It is the improved rebuild of the MVP at rushisorathiya89/Event-Hackathon-Management-Mini-Portal.
Roles: Admin, Organizer, Participant, Judge.

## 2. Stack (locked — do not change without asking)
- ASP.NET Core 8 MVC (Razor Views), C# 12
- ASP.NET Core Identity (cookie auth, role + policy authorization)
- EF Core 8 + SQL Server (Docker for dev). Migrations are applied by a CLI command / deploy step, NOT silently at startup in Production.
- Bootstrap 5 + custom theme (design/genz-theme.css), jQuery Validation
- SignalR for live leaderboard
- xUnit + FluentAssertions + WebApplicationFactory for tests
- GitHub Actions CI, Docker for deploy

## 3. Architecture rules
- Controllers stay thin. Business logic lives in `Services/` behind interfaces (`IHackathonService`, `ITeamService`, `IScoringService`, ...).
- Never expose EF entities to views. Use ViewModels / DTOs.
- All state-changing actions: POST + `[ValidateAntiForgeryToken]` + authorization attribute.
- Authorization via policies (`RequireAdmin`, `RequireJudgeForHackathon`, ...), never role-string checks scattered in views.
- No secrets in git. Use user-secrets locally and env vars in Docker. Keep `.env.example` updated.
- Every new table/column = migration + update `docs/DB_SCHEMA.md`.
- Every new route = update `docs/API.md` section inside ARCHITECTURE.md.

## 4. Design rules
- Follow `docs/DESIGN_SYSTEM.md` and `design/genz-theme.css`. Do not introduce new colors, fonts or radii outside the tokens.
- Mobile first. Keyboard focus visible. Respect `prefers-reduced-motion`.
- Copy: plain verbs, sentence case, buttons say what they do ("Create team", not "Submit"). Errors say what went wrong and how to fix it. Empty states invite an action.

## 5. Working protocol (every task)
1. State the plan in 3–7 bullets before editing code.
2. Work on ONE phase at a time (see ROADMAP.md). Do not start the next phase unless asked.
3. Write or update tests with the feature.
4. Run `dotnet build` and `dotnet test`. Fix failures before finishing.
5. Update, in this order:
   - `PROJECT_LOG.md` (append an entry — template inside that file)
   - `INDEX.md` (add/rename/remove files and one-line purposes)
   - `ROADMAP.md` (tick checkboxes, never untick without a note)
   - Relevant `docs/*.md`
6. End with a short summary: what changed, how to verify, what is next.

## 6. Definition of done
- Builds with 0 warnings introduced
- Tests added and passing
- Authorization verified for each role
- UI checked at 375px and 1280px
- Log, index, roadmap, docs updated

## 7. Do not
- Do not rewrite working files wholesale to make a small change.
- Do not add NuGet packages without noting them in PROJECT_LOG.md.
- Do not store plain-text passwords, tokens or connection strings in the repo.
- Do not delete user data in migrations without a backup note.
