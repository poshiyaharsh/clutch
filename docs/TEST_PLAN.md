# TEST_PLAN

## Strategy
- Unit tests (xUnit + FluentAssertions) for services, with EF Core SQLite in-memory or InMemory provider.
- Integration tests (WebApplicationFactory) for auth, roles, and main flows.
- Manual UI checklist at 375px, 768px, 1280px.

## Must-have cases
### Registration
- Create team: duplicate name in same hackathon rejected; same name in another hackathon allowed.
- Registration after deadline rejected.
- User already in a team of this hackathon cannot create/join another.
- Join with invalid code rejected; join full team rejected.
- Lead transfer and member removal permissions.
### Submission
- Only team members can edit; locked after deadline; URL validation.
### Judging
- Non-assigned judge gets 403. Points above MaxPoints rejected.
- Scores editable until lock; after lock rejected.
- Judge who is in a team can't score that team.
### Scoring
- Weighted total correct for 1, 2, 3 judges.
- Missing scores handled (team shows "pending", not 0).
- Tie-break order verified.
### Authorization (IDOR)
- Organizer A cannot edit Organizer B's hackathon. Participant cannot open judge routes.
### Security
- POST without antiforgery token rejected. Rate limit on login. Security headers present.
### Live
- Hub broadcasts new ranking after score save (integration test with test client).

## Exit criteria
All tests green in CI, >= 80% coverage on Services/, no open high-severity bugs.
