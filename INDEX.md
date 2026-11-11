# INDEX.md — File map

> Antigravity updates this file whenever a file is added, moved, renamed or removed. One line per file: what it is for.

## Root
| Path | Purpose |
|---|---|
| Clutch.sln | Visual Studio / .NET solution containing Web and Tests |
| AGENTS.md | Master instructions for the AI agent (local) |
| INDEX.md | This file map |
| PROJECT_LOG.md | Chronological work log (local) |
| ROADMAP.md | Phases and task checklist (local) |
| README.md | Human-facing project intro and setup |
| .env.example | Required environment variables (no secrets) |
| .gitignore | Git ignore rules for build artifacts, secrets, and temp files |
| Dockerfile / docker-compose.yml | Container build and local stack (app + SQL Server) |

## docs/
| Path | Purpose |
|---|---|
| docs/PRD.md | Product requirements, roles, user stories |
| docs/ARCHITECTURE.md | Layers, folders, services, routes, SignalR hubs |
| docs/DB_SCHEMA.md | Tables, columns, relations, indexes |
| docs/DESIGN_SYSTEM.md | Modern design tokens, components, screen specs, copy rules |
| docs/TEST_PLAN.md | Test strategy, cases per feature |
| docs/DEPLOYMENT.md | Deploy steps, env vars, rollback |
| docs/PROMPTS.md | Ready-to-paste Antigravity prompts per phase |

## design/
| Path | Purpose |
|---|---|
| design/clutch-mark.svg, design/clutch-wordmark.svg | Logo mark (favicon) and wordmark |
| docs/BRAND.md | Name meaning, mark, brand line, voice, usage rules |
| design/theme.css | Design tokens + base components (copy to wwwroot/css) |

## .agent/
| Path | Purpose |
|---|---|
| .agent/rules/*.md | Always-on rules (coding, security, design, logging) |
| .agent/workflows/*.md | Reusable commands (/new-feature, /run-tests, /end-session) |

## .github/
| Path | Purpose |
|---|---|
| .github/workflows/ci.yml | Build + test on every push and PR |

## src/ (created during Phase 2+)
| Path | Purpose |
|---|---|
| src/Clutch.Web/Clutch.Web.csproj, Program.cs | Web project definition and pipeline configuration |
| src/Clutch.Web/appsettings.json | Base configuration (logging, connection strings) |
| src/Clutch.Web/Properties/launchSettings.json | Local development launch profiles (ports 5240 / 7140) |
| src/Clutch.Web/Domain/Enums.cs | Hackathon status and user role definitions |
| src/Clutch.Web/Domain/ApplicationUser.cs | Extended Identity user entity |
| src/Clutch.Web/Domain/Entities.cs | Core domain entities (Hackathon, Team, Submission, Score, etc.) |
| src/Clutch.Web/Data/AppDbContext.cs | EF Core DbContext with relational configurations and indexes |
| src/Clutch.Web/Data/DbSeeder.cs | Seed initial roles and administrator |
| src/Clutch.Web/Migrations/ | EF Core database migration files |
| src/Clutch.Web/Services/IHackathonService.cs | Hackathon domain service interface and implementation |
| src/Clutch.Web/Services/ITeamService.cs | Team management service interface and implementation |
| src/Clutch.Web/Services/IScoringService.cs | Weighted scoring and leaderboard calculation service |
| src/Clutch.Web/Services/INotificationService.cs | In-app notification service |
| src/Clutch.Web/Services/IEmailSender.cs | Templated SMTP and mock email delivery service |
| src/Clutch.Web/Services/IAuditService.cs | System audit logging and security event tracking service |
| src/Clutch.Web/Hubs/LeaderboardHub.cs | SignalR live scoreboard real-time broadcasting hub |
| src/Clutch.Web/Controllers/HomeController.cs | Main preview and navigation controller |
| src/Clutch.Web/Controllers/AccountController.cs | Identity authentication controller (Login, Register, Logout) |
| src/Clutch.Web/Controllers/HackathonsController.cs | Hackathon CRUD, status transitions, and announcements controller |
| src/Clutch.Web/Controllers/TeamsController.cs | Squad creation, join code membership, and squad management controller |
| src/Clutch.Web/Controllers/SubmissionsController.cs | Project submission and track selection controller |
| src/Clutch.Web/Controllers/JudgingController.cs | Judge assignments, squad evaluations, and rubric scoring controller |
| src/Clutch.Web/Controllers/LeaderboardController.cs | Real-time calculated scoreboard and results reveal controller |
| src/Clutch.Web/Controllers/ExportController.cs | CSV data export for teams, participants, and scoring sheets |
| src/Clutch.Web/Controllers/AdminController.cs | Administration panel and security audit log inspection controller |
| src/Clutch.Web/Models/AccountViewModels.cs | ViewModels for user authentication |
| src/Clutch.Web/Models/HackathonViewModels.cs | ViewModels for hackathon listing, details, and creation |
| src/Clutch.Web/Models/TeamViewModels.cs | ViewModels for squad creation, join codes, and roster |
| src/Clutch.Web/Models/SubmissionViewModels.cs | ViewModels for project code and demo submissions |
| src/Clutch.Web/Models/JudgingViewModels.cs | ViewModels for rubric scoring, judge assignments, and scoreboard rows |
| src/Clutch.Web/Models/ErrorViewModel.cs | Model for global error page |
| src/Clutch.Web/Models/MockData.cs | Fake data for design phase |
| src/Clutch.Web/Views/Shared/_Layout.cshtml | Site shell: header, nav, footer, toast |
| src/Clutch.Web/Views/Shared/_NotificationBell.cshtml | Bell button + notifications drawer |
| src/Clutch.Web/Views/Shared/_Toast.cshtml | Toast container |
| src/Clutch.Web/Views/Shared/_EmptyState.cshtml | Reusable empty state |
| src/Clutch.Web/Views/Shared/_StatusSticker.cshtml | Status sticker badge |
| src/Clutch.Web/Views/Shared/Error.cshtml | Friendly global error view |
| src/Clutch.Web/Views/Account/*.cshtml | Authentication views: Login, Register, AccessDenied |
| src/Clutch.Web/Views/Hackathons/*.cshtml | Hackathon views: Index, Create, Detail |
| src/Clutch.Web/Views/Teams/*.cshtml | Squad management views: Create, Join, Detail, ResultCard |
| src/Clutch.Web/Views/Submissions/*.cshtml | Project submission editing view |
| src/Clutch.Web/Views/Judging/*.cshtml | Judging views: Index, HackathonTeams, ScoreTeam |
| src/Clutch.Web/Views/Leaderboard/*.cshtml | Scoreboard views: Index, Hidden |
| src/Clutch.Web/Views/Admin/*.cshtml | Platform administration and audit trail views |
| src/Clutch.Web/Views/Home/*.cshtml | Mock pages: Index, Event, Leaderboard, Team, Judge, Organizer, Admin |
| src/Clutch.Web/wwwroot/css/theme.css | Design tokens + components |
| src/Clutch.Web/wwwroot/css/site.css | Page-level layout |
| src/Clutch.Web/wwwroot/js/site.js | Toast, leaderboard re-rank animation, judge total preview |

## tests/
| Path | Purpose |
|---|---|
| tests/Clutch.Tests/Clutch.Tests.csproj | Unit and integration test project (xUnit, FluentAssertions) |
| tests/Clutch.Tests/SetupTests.cs | Verification test suite for setup, mock data, and preview routes |
| tests/Clutch.Tests/FoundationTests.cs | Domain service logic, weighted scoring math, and policy tests |
| tests/Clutch.Tests/CoreFeatureTests.cs | Core feature tests for teams, submissions, deadlines, and hackathons |
| tests/Clutch.Tests/JudgingTests.cs | Rubric evaluation, lock immutability, conflict prevention, and scoring tests |
| tests/Clutch.Tests/ExtrasTests.cs | CSV exports, audit logging, email sender, and SignalR hub tests |

_Antigravity: add rows here as the solution grows._
