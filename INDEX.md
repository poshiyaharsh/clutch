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
| .dockerignore | Excludes build artifacts, secrets, and local meta files from Docker build context |
| Dockerfile / docker-compose.yml | Multi-stage production container (`final` + `migrator`) and Compose stack (app + SQL Server) |

## docs/
| Path | Purpose |
|---|---|
| docs/PRD.md | Product requirements, roles, user stories |
| docs/ARCHITECTURE.md | Layers, folders, services, routes, SignalR hubs |
| docs/DB_SCHEMA.md | Tables, columns, relations, indexes |
| docs/DESIGN_SYSTEM.md | Modern design tokens, dual-theme palettes, WCAG contrast table, Bootstrap mappings |
| docs/BRAND.md | Brand identity guidelines: mark geometry, wordmark, clear space, asset inventory, and _Logo.cshtml |
| docs/CONTRAST_REPORT.md | Automated WCAG 2.1 AA color contrast compliance report across light and dark tokens |
| docs/TEST_PLAN.md | Test strategy, cases per feature |
| docs/DEPLOYMENT.md | Deploy steps, env vars, rollback |
| docs/screenshots/ | Live UI screenshots (`home-desktop.png`, `leaderboard-desktop.png`, `judge-desktop.png`, `organizer-desktop.png`) |
| docs/PROMPTS.md | Ready-to-paste Antigravity prompts per phase |

## tools/
| Path | Purpose |
|---|---|
| tools/contrast-check/check_contrast.py | Automated WCAG 2.1 AA relative luminance and contrast ratio verification script |

## .agent/
| Path | Purpose |
|---|---|
| .agent/rules/*.md | Always-on rules (coding, security, design, logging) |
| .agent/workflows/*.md | Reusable commands (/new-feature, /run-tests, /end-session) |

## .github/
| Path | Purpose |
|---|---|
| .github/workflows/ci.yml | Build, test, WCAG contrast check, and Docker verification on every push and PR |
| .github/workflows/deploy.yml | Build and publish production web and migrator images to GHCR |

## src/
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
| src/Clutch.Web/Models/MockData.cs | Mock data for preview routes |
| src/Clutch.Web/Views/Shared/_Layout.cshtml | Modern product shell: navbar, search, theme switcher, command menu |
| src/Clutch.Web/Views/Shared/_Logo.cshtml | Scalable brand logo component with full wordmark and mark-only variants |
| src/Clutch.Web/Views/Shared/_Badge.cshtml | Modern pill and status badge component |
| src/Clutch.Web/Views/Shared/_NotificationBell.cshtml | Notification popover with Lucide bell icon |
| src/Clutch.Web/Views/Shared/_ThemeToggle.cshtml | Light / Dark / System segmented theme switcher |
| src/Clutch.Web/Views/Shared/_CommandMenu.cshtml | Ctrl+K / Cmd+K quick command search dialog |
| src/Clutch.Web/Views/Shared/_Toast.cshtml | Toast notification container |
| src/Clutch.Web/Views/Shared/_EmptyState.cshtml | Reusable clean empty state |
| src/Clutch.Web/Views/Shared/Error.cshtml | Friendly global error view |
| src/Clutch.Web/Views/Account/*.cshtml | Authentication views: Login, Register, AccessDenied |
| src/Clutch.Web/Views/Hackathons/*.cshtml | Hackathon views: Index, Create, Detail |
| src/Clutch.Web/Views/Teams/*.cshtml | Squad management views: Create, Join, Detail, ResultCard |
| src/Clutch.Web/Views/Submissions/*.cshtml | Project submission editing view |
| src/Clutch.Web/Views/Judging/*.cshtml | Judging views: Index, HackathonTeams, ScoreTeam |
| src/Clutch.Web/Views/Leaderboard/*.cshtml | Scoreboard views: Index, Hidden |
| src/Clutch.Web/Views/Admin/*.cshtml | Platform administration and audit trail views |
| src/Clutch.Web/Views/Home/*.cshtml | Modern views: Index, Event, Leaderboard, Team, Judge, Organizer, Admin |
| src/Clutch.Web/wwwroot/css/clutch-theme.css | Calm high-end product design tokens and components |
| src/Clutch.Web/wwwroot/css/site.css | Page-level layout, slim navbar, command palette, dashboard shell |
| src/Clutch.Web/wwwroot/img/icons.svg | Lucide vector icon SVG sprite (no emoji) |
| src/Clutch.Web/wwwroot/img/brand/ | Brand assets: vector marks, wordmarks, icons, web manifest, and open graph image |
| src/Clutch.Web/wwwroot/js/site.js | Theme persistence, command menu, live FLIP leaderboard, judge scoring |

## tests/
| Path | Purpose |
|---|---|
| tests/Clutch.Tests/Clutch.Tests.csproj | Test project definition |
| tests/Clutch.Tests/SetupTests.cs | Verification test for solution wiring and test runner |
| tests/Clutch.Tests/FoundationTests.cs | EF Core DbContext, user authentication, and entity relation tests |
| tests/Clutch.Tests/CoreFeatureTests.cs | Core hackathon, team, and submission service integration tests |
| tests/Clutch.Tests/JudgingTests.cs | Multi-judge rubric scoring, conflict-of-interest, and locking tests |
| tests/Clutch.Tests/ExtrasTests.cs | SignalR live leaderboard, CSV export, email service, and audit log tests |
| tests/Clutch.Tests/SecurityAndHardeningTests.cs | Security headers, rate limiting, and CSRF protection tests |
| tests/Clutch.Tests/DeploymentTests.cs | Health endpoint, CLI role/admin seeding, and demo data idempotency tests |
