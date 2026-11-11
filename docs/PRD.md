# PRD — Clutch

## 1. Summary
A portal where organizers run hackathons, participants form teams and submit projects, and judges score with a rubric while everyone watches a live leaderboard.

## 2. Gaps in the v1 MVP (what v2 fixes)
| v1 | v2 |
|---|---|
| Session auth; Admin set by editing DB | ASP.NET Core Identity, seeded admin, policies |
| Single score per judge | Weighted rubric, multi-judge average, lock/finalize |
| No project submission | Submission with repo/demo links and track |
| No join codes or invites | Team join codes, size limits, lead transfer |
| In-app notifications only | In-app + email + announcements |
| No tests / CI / Docker | xUnit, GitHub Actions, Docker |
| Static leaderboard | SignalR live leaderboard |
| No export | CSV export, result card/certificate |
| Auto-migrate on startup | Explicit migration step |

## 3. Roles
- **Admin:** everything, manages users and roles.
- **Organizer:** creates and manages own hackathons, tracks, rubric, judges, announcements.
- **Participant:** browses events, creates/joins teams, submits project, sees results.
- **Judge:** sees assigned hackathons, scores teams, sees leaderboard.

## 4. User stories and acceptance criteria
### Events
- As an organizer I create a hackathon (title, description, dates, registration deadline, max team size, tracks) in Draft, then Publish it.
  - AC: Draft hidden from participants. Published visible. Closed/Cancelled show a banner and block registration.
### Registration
- As a participant I create a team with a unique name inside a hackathon.
  - AC: duplicate name in same hackathon rejected with a clear message. Registration blocked after deadline. One team per participant per hackathon.
- As a participant I join a team with a join code.
  - AC: blocked if team full. Lead can remove members and transfer lead.
### Submission
- As a team lead I submit/edit a project until the submission deadline.
  - AC: title, description, repo URL, demo URL (optional), track. Locked after deadline.
### Judging
- As a judge I score each team on each rubric criterion (0–max).
  - AC: only assigned judges. Scores editable until the organizer locks judging. Can't score own team (if judge also participant).
- As an organizer I define rubric criteria with weights that sum to 100.
### Leaderboard
- As anyone with access I see ranked teams, updating live.
  - AC: weighted total averaged across judges. Ties broken by (1) highest score on first criterion, (2) earliest submission time. Hidden until organizer reveals, if "hide until final" is on.
### Notifications
- As a user I see a bell with unread count and a list; important events also email me.

## 5. Non-functional
- Mobile first (375px), WCAG AA contrast, keyboard accessible.
- Pages under 2s on typical hardware. Indexes on all foreign keys and leaderboard queries.
- Security: CSRF, server-side validation, IDOR checks on every id, rate limit on login/register, security headers.

## 6. Out of scope (v2)
Payments, video calls, mobile app, multi-tenant orgs.
