# Clutch

Hackathon portal for events, teams, submissions, judging and a live leaderboard.
Rebuild of the CodeMind AI Hackathon mini portal, with Identity auth, rubric scoring, live updates and a modern vibrant design.

## Run locally
1. Install .NET 8 SDK and Docker.
2. `cp .env.example .env` and edit values.
3. `docker compose up -d db`
4. `dotnet ef database update --project src/Clutch.Web`
5. `dotnet run --project src/Clutch.Web`

## Docs
Start with docs/PRD.md and docs/ARCHITECTURE.md.
