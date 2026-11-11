using Clutch.Web.Data;
using Clutch.Web.Domain;
using Clutch.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace Clutch.Tests;

public class JudgingTests
{
    private AppDbContext CreateInMemoryDb(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task ScoringService_PreventsJudgeFromScoringOwnTeam()
    {
        // Arrange
        using var db = CreateInMemoryDb(nameof(ScoringService_PreventsJudgeFromScoringOwnTeam));
        var hackathon = new Hackathon { Id = 1, Title = "Hack1", Slug = "hack-1", OrganizerId = "org-1" };
        var criterion = new Criterion { Id = 1, HackathonId = 1, Name = "Quality", MaxPoints = 10, WeightPercent = 100 };
        var team = new Team
        {
            Id = 1,
            HackathonId = 1,
            Name = "Conflict Team",
            JoinCode = "CONF1234",
            LeadId = "judge-alice", // Judge Alice is squad lead
            Members = [new TeamMember { TeamId = 1, UserId = "judge-alice" }]
        };
        db.Hackathons.Add(hackathon);
        db.Criteria.Add(criterion);
        db.Teams.Add(team);
        await db.SaveChangesAsync();

        var service = new ScoringService(db);

        // Act & Assert
        var act = async () => await service.SubmitScoreAsync(1, 1, "judge-alice", 10, "Great job by myself");
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*cannot score this squad*");
    }

    [Fact]
    public async Task ScoringService_PreventsScoringWhenJudgingLocked()
    {
        // Arrange
        using var db = CreateInMemoryDb(nameof(ScoringService_PreventsScoringWhenJudgingLocked));
        var hackathon = new Hackathon { Id = 2, Title = "Hack2", Slug = "hack-2", OrganizerId = "org-1" };
        var criterion = new Criterion { Id = 2, HackathonId = 2, Name = "Impact", MaxPoints = 10, WeightPercent = 100 };
        var team = new Team
        {
            Id = 2,
            HackathonId = 2,
            Name = "Team Two",
            JoinCode = "TWO12345",
            LeadId = "user-2",
            Members = [new TeamMember { TeamId = 2, UserId = "user-2" }]
        };
        db.Hackathons.Add(hackathon);
        db.Criteria.Add(criterion);
        db.Teams.Add(team);
        db.JudgingLocks.Add(new JudgingLock { HackathonId = 2, LockedById = "admin-1", LockedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var service = new ScoringService(db);

        // Act & Assert
        var act = async () => await service.SubmitScoreAsync(2, 2, "judge-bob", 9, "Too late");
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*cannot score this squad*");
    }

    [Fact]
    public async Task ScoringService_EnforcesPointsWithinCriterionMaxPoints()
    {
        // Arrange
        using var db = CreateInMemoryDb(nameof(ScoringService_EnforcesPointsWithinCriterionMaxPoints));
        var hackathon = new Hackathon { Id = 3, Title = "Hack3", Slug = "hack-3", OrganizerId = "org-1" };
        var criterion = new Criterion { Id = 3, HackathonId = 3, Name = "Design", MaxPoints = 10, WeightPercent = 100 };
        var team = new Team
        {
            Id = 3,
            HackathonId = 3,
            Name = "Team Three",
            JoinCode = "THREE123",
            LeadId = "user-3",
            Members = [new TeamMember { TeamId = 3, UserId = "user-3" }]
        };
        db.Hackathons.Add(hackathon);
        db.Criteria.Add(criterion);
        db.Teams.Add(team);
        await db.SaveChangesAsync();

        var service = new ScoringService(db);

        // Act & Assert: Score over max points (15 > 10)
        var actOver = async () => await service.SubmitScoreAsync(3, 3, "judge-charlie", 15, "Too high");
        await actOver.Should().ThrowAsync<ArgumentOutOfRangeException>();

        // Act & Assert: Score below zero (-1 < 0)
        var actUnder = async () => await service.SubmitScoreAsync(3, 3, "judge-charlie", -1, "Too low");
        await actUnder.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task ScoringService_AveragesScoresAcrossMultipleJudgesAccurately()
    {
        // Arrange
        using var db = CreateInMemoryDb(nameof(ScoringService_AveragesScoresAcrossMultipleJudgesAccurately));
        var hackathon = new Hackathon { Id = 4, Title = "Hack4", Slug = "hack-4", OrganizerId = "org-1" };
        var criterion = new Criterion { Id = 4, HackathonId = 4, Name = "Technical Merit", MaxPoints = 10, WeightPercent = 100 };
        var team = new Team
        {
            Id = 4,
            HackathonId = 4,
            Name = "DevSquad",
            JoinCode = "DEV12345",
            LeadId = "user-dev",
            Members = [new TeamMember { TeamId = 4, UserId = "user-dev" }]
        };
        db.Hackathons.Add(hackathon);
        db.Criteria.Add(criterion);
        db.Teams.Add(team);

        // Judge 1 gives 6/10, Judge 2 gives 8/10, Judge 3 gives 10/10 -> Avg = 8/10 -> (8/10)*100 = 80.0
        db.Scores.Add(new Score { TeamId = 4, CriterionId = 4, JudgeId = "j1", Points = 6 });
        db.Scores.Add(new Score { TeamId = 4, CriterionId = 4, JudgeId = "j2", Points = 8 });
        db.Scores.Add(new Score { TeamId = 4, CriterionId = 4, JudgeId = "j3", Points = 10 });
        await db.SaveChangesAsync();

        var service = new ScoringService(db);

        // Act
        var leaderboard = await service.CalculateLeaderboardAsync(4);

        // Assert
        leaderboard.Should().HaveCount(1);
        leaderboard[0].TotalScore.Should().Be(80.0m);
    }

    [Fact]
    public async Task ScoringService_AssignsJudgeAndAvoidsDuplicateAssignments()
    {
        // Arrange
        using var db = CreateInMemoryDb(nameof(ScoringService_AssignsJudgeAndAvoidsDuplicateAssignments));
        db.Users.Add(new ApplicationUser { Id = "judge-user-1", Email = "judge@clutch.local", UserName = "judge@clutch.local" });
        await db.SaveChangesAsync();

        var service = new ScoringService(db);

        // Act
        var result1 = await service.AssignJudgeAsync(10, "judge@clutch.local");
        var result2 = await service.AssignJudgeAsync(10, "judge@clutch.local"); // Duplicate call

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();

        var assignments = await db.JudgeAssignments.Where(j => j.HackathonId == 10).ToListAsync();
        assignments.Should().HaveCount(1);
    }
}
