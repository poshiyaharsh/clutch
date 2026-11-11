# PROJECT_LOG.md

> Append-only. Newest entry at the BOTTOM. Antigravity adds one entry after every task.

## Entry template
```
### YYYY-MM-DD HH:MM — Phase X — <short title>
- Goal:
- Done:
- Files changed:
- Decisions / why:
- Packages added:
- Tests: <added / passing count>
- Problems / TODO:
- Next:
```

---

### Kickoff — Phase 0 — Kit created
- Goal: Prepare control files, docs and design tokens for Clutch.
- Done: AGENTS.md, INDEX.md, ROADMAP.md, docs/*, design/genz-theme.css, Docker + CI skeleton.
- Decisions: Stack locked to ASP.NET Core 8 MVC + Identity + EF Core + SQL Server + SignalR.
- Next: Phase 1 — Analysis & PRD review.

### Phase 3 starter — Mock layout and pages added
- Done: _Layout, notification bell drawer, toast, empty-state and status-sticker partials; mock pages for Home, Event, Leaderboard (with simulated live re-rank), Team, Judge (live total), Organizer.
- Not yet verified: kit was written without a .NET SDK, so run `dotnet build` first and fix any compile issues.
- Next: Phase 2 setup (create solution/csproj/Program.cs), then run the mock pages and do /design-check.

### Rename — HackDrop to Clutch
- Done: renamed the product, namespaces, folders, csproj, Docker/CI paths, DB name (ClutchDb), email domain and CSS/JS prefixes (hd- to cl-, hdToast to clToast).
- Next: run `dotnet build` to confirm nothing was missed, then continue with Phase 2.

### Brand — Logo mark, wordmark and brand line added
- Done: design/clutch-mark.svg, design/clutch-wordmark.svg, wwwroot/favicon.svg, docs/BRAND.md; layout now shows the mark and favicon; footer uses the brand line.
- Decision: recommended line "Show up. Clutch it." (user to confirm).

### 2026-11-11 11:40 — Git remote setup & .gitignore hardening
- Goal: Connect repo to remote GitHub repository, secure .gitignore against sensitive and build artifact leaks, and establish Conventional Commits.
- Done: Expanded .gitignore with rules for build artifacts, secrets, environment files, IDE configs, databases, OS files; initialized git on branch `main`; connected remote origin `https://github.com/poshiyaharsh/clutch.git`; verified build (`dotnet build` 0 errors, 0 warnings).
- Files changed: `.gitignore`, `PROJECT_LOG.md`.
- Decisions / why: Exclude all binary build outputs (`bin/`, `obj/`, `TestResults/`), `.env*` files (except `.env.example`), secrets, local configs, and temporary files from version control to prevent accidental leaks.
- Packages added: None.
- Tests: N/A (build passes with 0 warnings, 0 errors).
- Problems / TODO: Remote repository is empty; ready for initial commit and push.
- Next: Create initial conventional commit and push to remote.
