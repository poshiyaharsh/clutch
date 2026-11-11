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
| src/Clutch.Web/Clutch.Web.csproj, Program.cs | Minimal web project so the mock pages run (Phase 2 extends these) |
| src/Clutch.Web/appsettings.json | Base configuration (logging, connection strings) |
| src/Clutch.Web/Properties/launchSettings.json | Local development launch profiles (ports 5240 / 7140) |
| src/Clutch.Web/Controllers/HomeController.cs | Phase 3 preview controller (mock pages) |
| src/Clutch.Web/Models/MockData.cs | Fake data for design phase. Delete in Phase 5 |
| src/Clutch.Web/Views/Shared/_Layout.cshtml | Site shell: header, nav, footer, toast |
| src/Clutch.Web/Views/Shared/_NotificationBell.cshtml | Bell button + notifications drawer |
| src/Clutch.Web/Views/Shared/_Toast.cshtml | Toast container |
| src/Clutch.Web/Views/Shared/_EmptyState.cshtml | Reusable empty state |
| src/Clutch.Web/Views/Shared/_StatusSticker.cshtml | Status sticker badge |
| src/Clutch.Web/Views/Home/*.cshtml | Mock pages: Index, Event, Leaderboard, Team, Judge, Organizer |
| src/Clutch.Web/wwwroot/css/theme.css | Design tokens + components |
| src/Clutch.Web/wwwroot/css/site.css | Page-level layout |
| src/Clutch.Web/wwwroot/js/site.js | Toast, leaderboard re-rank animation, judge total preview |

## tests/
| Path | Purpose |
|---|---|
| tests/Clutch.Tests/Clutch.Tests.csproj | Unit and integration test project (xUnit, FluentAssertions) |
| tests/Clutch.Tests/SetupTests.cs | Verification test suite for setup and mock data |

_Antigravity: add rows here as the solution grows._
