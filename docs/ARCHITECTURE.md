# ARCHITECTURE

## Solution layout
```
Clutch.sln
src/
  Clutch.Web/
    Controllers/      Account, Home, Hackathons, Teams, Submissions, Judging, Leaderboard, Admin, Notifications
    Hubs/             LeaderboardHub.cs
    Services/         Interfaces + implementations (business logic)
    Data/             AppDbContext, Configurations/, Migrations/, Seed/
    Domain/           Entities, Enums
    Models/           ViewModels, DTOs
    Authorization/    Policies, Requirements, Handlers
    Views/            Razor views per controller + Shared/
    wwwroot/          css (theme.css), js, fonts
tests/
  Clutch.Tests/     Unit + integration tests
```

## Layers
Controller -> Service -> DbContext. Controllers map ViewModels <-> service DTOs. Services hold rules (deadlines, team size, scoring).

## Key services
- IHackathonService: CRUD, status transitions, publish/close/cancel.
- ITeamService: create, join by code, leave, remove, transfer lead.
- ISubmissionService: upsert, deadline lock.
- IScoringService: save scores, compute totals, rank, tie-break, lock.
- INotificationService: create in-app + queue email.
- IEmailSender: SMTP implementation behind an interface.
- IAuditService: write audit entries for admin/organizer/judge actions.

## Authorization policies
- RequireAdmin
- RequireOrganizer (Admin or Organizer)
- OrganizerOwnsHackathon (resource-based)
- JudgeAssignedToHackathon (resource-based)
- TeamLeadOnly / TeamMemberOnly

## Status rules
Hackathon: Draft -> Published -> Closed (judging) -> Finalized. Cancelled from Draft or Published.

## Routes (high level)
| Area | Routes |
|---|---|
| Public | `/`, `/events`, `/events/{id}` |
| Account | `/account/login`, `/register`, `/logout` |
| Teams | `/events/{id}/teams/new`, `/teams/{id}`, `/teams/join` |
| Submission | `/teams/{id}/submission` |
| Judging | `/judge`, `/judge/{hackathonId}`, `/judge/{hackathonId}/teams/{teamId}` |
| Leaderboard | `/events/{id}/leaderboard` |
| Organizer | `/organizer/events`, `/organizer/events/{id}/rubric`, `/judges`, `/announcements` |
| Admin | `/admin/users`, `/admin/audit` |
| Notifications | `/notifications`, POST `/notifications/read` |
| Operations | `GET /health` (liveness), `GET /health/ready` (DB readiness) |

_Antigravity keeps this table updated as routes are added._

## SignalR
`LeaderboardHub` group per hackathon (`hackathon-{id}`). After any score save/lock, `IScoringService` publishes the new ranking to the group. Client falls back to polling every 15s if the socket drops.

## Cross-cutting
Serilog (console + file), global exception page, FluentValidation or DataAnnotations (pick one in Phase 4 and note in log), response headers middleware, rate limiter on auth endpoints.
