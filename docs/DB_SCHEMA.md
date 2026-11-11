# DB_SCHEMA

> Update with every migration. PK = Id (int identity unless noted). All tables have CreatedAt (UTC).

## Identity
AspNetUsers (extended): DisplayName, AvatarUrl?, College/Company?, CreatedAt. Roles: Admin, Organizer, Participant, Judge.

## Tables
**Hackathon**: Id, Title, Slug (unique), Description, BannerUrl?, StartsAt, EndsAt, RegistrationDeadline, SubmissionDeadline, MaxTeamSize, Status (Draft/Published/Closed/Finalized/Cancelled), HideLeaderboardUntilFinal (bool), OrganizerId (FK User).

**Track**: Id, HackathonId (FK), Name, Description?

**Team**: Id, HackathonId (FK), Name, JoinCode (unique, 8 chars), LeadId (FK User), CreatedAt. Unique (HackathonId, Name).

**TeamMember**: Id, TeamId (FK), UserId (FK), JoinedAt. Unique (TeamId, UserId). Business rule: one team per user per hackathon.

**Submission**: Id, TeamId (FK, unique), TrackId? (FK), Title, Description, RepoUrl, DemoUrl?, SubmittedAt, UpdatedAt.

**Criterion**: Id, HackathonId (FK), Name, Description?, MaxPoints (int), WeightPercent (int), SortOrder. Rule: weights of a hackathon sum to 100.

**JudgeAssignment**: Id, HackathonId (FK), JudgeId (FK User). Unique (HackathonId, JudgeId).

**Score**: Id, TeamId (FK), CriterionId (FK), JudgeId (FK User), Points (int), Comment?, UpdatedAt. Unique (TeamId, CriterionId, JudgeId).

**JudgingLock**: HackathonId (PK/FK), LockedAt, LockedById.

**Announcement**: Id, HackathonId (FK), Title, Body, CreatedById, CreatedAt.

**Notification**: Id, UserId (FK), Title, Body, Url?, IsRead, CreatedAt.

**AuditLog**: Id, ActorId, Action, EntityType, EntityId, Details (json), CreatedAt.

## Indexes
- Team(HackathonId, Name) unique, Team(JoinCode) unique
- Score(TeamId), Score(JudgeId), Score(CriterionId)
- Notification(UserId, IsRead)
- Hackathon(Status, StartsAt)

## Ranking formula
For each team: for each criterion, average points across judges -> (avg / MaxPoints) * WeightPercent. Sum = total out of 100. Tie-break: score on first criterion, then earliest SubmittedAt.
