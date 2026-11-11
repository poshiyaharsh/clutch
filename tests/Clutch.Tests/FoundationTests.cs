using Clutch.Web.Data;
using Clutch.Web.Domain;
using Clutch.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Clutch.Tests;

public class FoundationTests
{
    private AppDbContext CreateInMemoryDb(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task ScoringService_CalculatesWeightedScoresAndBreaksTiesCorrectly()
    {
        // Arrange
        using var db = CreateInMemoryDb(nameof(ScoringService_CalculatesWeightedScoresAndBreaksTiesCorrectly));
        var hackathon = new Hackathon
        {
            Id = 1,
            Title = "AI Sprint",
            Slug = "ai-sprint",
            OrganizerId = "org-1",
            Status = HackathonStatus.Published
        };
        db.Hackathons.Add(hackathon);

        // 2 criteria: Innovation (weight 60%, max 10), Execution (weight 40%, max 10)
        var crit1 = new Criterion { Id = 1, HackathonId = 1, Name = "Innovation", MaxPoints = 10, WeightPercent = 60, SortOrder = 1 };
        var crit2 = new Criterion { Id = 2, HackathonId = 1, Name = "Execution", MaxPoints = 10, WeightPercent = 40, SortOrder = 2 };
        db.Criteria.AddRange(crit1, crit2);

        // 2 teams
        var teamA = new Team { Id = 1, HackathonId = 1, Name = "Team Alpha", JoinCode = "ALPHA001", LeadId = "user-1" };
        var teamB = new Team { Id = 2, HackathonId = 1, Name = "Team Beta", JoinCode = "BETA0002", LeadId = "user-2" };
        db.Teams.AddRange(teamA, teamB);

        // Team Alpha: Innovation = 10 (score = (10/10)*60 = 60), Execution = 5 (score = (5/10)*40 = 20) -> Total = 80
        db.Scores.Add(new Score { TeamId = 1, CriterionId = 1, JudgeId = "judge-1", Points = 10 });
        db.Scores.Add(new Score { TeamId = 1, CriterionId = 2, JudgeId = "judge-1", Points = 5 });

        // Team Beta: Innovation = 5 (score = (5/10)*60 = 30), Execution = 10 (score = (10/10)*40 = 40) -> Total = 70
        db.Scores.Add(new Score { TeamId = 2, CriterionId = 1, JudgeId = "judge-1", Points = 5 });
        db.Scores.Add(new Score { TeamId = 2, CriterionId = 2, JudgeId = "judge-1", Points = 10 });

        await db.SaveChangesAsync();

        var scoringService = new ScoringService(db);

        // Act
        var leaderboard = await scoringService.CalculateLeaderboardAsync(1);

        // Assert
        leaderboard.Should().HaveCount(2);
        leaderboard[0].TeamId.Should().Be(1);
        leaderboard[0].Rank.Should().Be(1);
        leaderboard[0].TotalScore.Should().Be(80m);
        leaderboard[1].TeamId.Should().Be(2);
        leaderboard[1].Rank.Should().Be(2);
        leaderboard[1].TotalScore.Should().Be(70m);
    }

    [Fact]
    public async Task TeamService_PreventsJoiningWhenTeamFull()
    {
        // Arrange
        using var db = CreateInMemoryDb(nameof(TeamService_PreventsJoiningWhenTeamFull));
        var hackathon = new Hackathon
        {
            Id = 1,
            Title = "Solo Hack",
            Slug = "solo-hack",
            MaxTeamSize = 1,
            OrganizerId = "org-1"
        };
        db.Hackathons.Add(hackathon);

        var team = new Team
        {
            Id = 10,
            HackathonId = 1,
            Name = "Full Team",
            JoinCode = "FULL1234",
            LeadId = "user-lead",
            Members = [new TeamMember { TeamId = 10, UserId = "user-lead" }]
        };
        db.Teams.Add(team);
        await db.SaveChangesAsync();

        var teamService = new TeamService(db);

        // Act
        var joinResult = await teamService.JoinTeamAsync("FULL1234", "user-intruder");

        // Assert
        joinResult.Should().BeFalse();
    }

    [Fact]
    public async Task TeamService_TransfersLeadCorrectly()
    {
        // Arrange
        using var db = CreateInMemoryDb(nameof(TeamService_TransfersLeadCorrectly));
        var team = new Team
        {
            Id = 20,
            HackathonId = 1,
            Name = "Duo Team",
            JoinCode = "DUO12345",
            LeadId = "user-lead",
            Members =
            [
                new TeamMember { TeamId = 20, UserId = "user-lead" },
                new TeamMember { TeamId = 20, UserId = "user-second" }
            ]
        };
        db.Teams.Add(team);
        await db.SaveChangesAsync();

        var teamService = new TeamService(db);

        // Act
        var transferResult = await teamService.TransferLeadAsync(20, "user-lead", "user-second");

        // Assert
        transferResult.Should().BeTrue();
        var updated = await db.Teams.FindAsync(20);
        updated!.LeadId.Should().Be("user-second");
    }

    [Fact]
    public async Task HackathonService_CreatesAndRetrievesBySlug()
    {
        // Arrange
        using var db = CreateInMemoryDb(nameof(HackathonService_CreatesAndRetrievesBySlug));
        var service = new HackathonService(db);

        var hackathon = new Hackathon
        {
            Title = "Cloud Challenge",
            Slug = "cloud-challenge",
            Description = "Build scalable cloud services.",
            StartsAt = DateTime.UtcNow,
            EndsAt = DateTime.UtcNow.AddDays(2),
            RegistrationDeadline = DateTime.UtcNow.AddDays(1),
            SubmissionDeadline = DateTime.UtcNow.AddDays(2),
            Status = HackathonStatus.Published
        };

        // Act
        await service.CreateAsync(hackathon, "organizer-99");
        var retrieved = await service.GetBySlugAsync("cloud-challenge");

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Title.Should().Be("Cloud Challenge");
        retrieved.OrganizerId.Should().Be("organizer-99");
    }

    [Theory]
    [InlineData("RequireAdmin", new[] { Roles.Admin })]
    [InlineData("RequireOrganizer", new[] { Roles.Admin, Roles.Organizer })]
    [InlineData("RequireJudge", new[] { Roles.Admin, Roles.Judge })]
    [InlineData("RequireParticipant", new[] { Roles.Participant })]
    public async Task AuthorizationPolicies_ConfiguredWithCorrectRoles(string policyName, string[] expectedRoles)
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAdmin", policy => policy.RequireRole(Roles.Admin));
            options.AddPolicy("RequireOrganizer", policy => policy.RequireRole(Roles.Admin, Roles.Organizer));
            options.AddPolicy("RequireJudge", policy => policy.RequireRole(Roles.Admin, Roles.Judge));
            options.AddPolicy("RequireParticipant", policy => policy.RequireRole(Roles.Participant));
        });

        var provider = services.BuildServiceProvider();
        var policyProvider = provider.GetRequiredService<IAuthorizationPolicyProvider>();

        // Act
        var policy = await policyProvider.GetPolicyAsync(policyName);

        // Assert
        policy.Should().NotBeNull();
        var roleRequirement = policy!.Requirements
            .OfType<Microsoft.AspNetCore.Authorization.Infrastructure.RolesAuthorizationRequirement>()
            .FirstOrDefault();

        roleRequirement.Should().NotBeNull();
        roleRequirement!.AllowedRoles.Should().BeEquivalentTo(expectedRoles);
    }
}
