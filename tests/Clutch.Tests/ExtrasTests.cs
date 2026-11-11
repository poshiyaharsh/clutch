using Clutch.Web.Controllers;
using Clutch.Web.Data;
using Clutch.Web.Domain;
using Clutch.Web.Hubs;
using Clutch.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text;

namespace Clutch.Tests;

public class ExtrasTests
{
    private AppDbContext CreateInMemoryDb(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task ExportController_GeneratesValidTeamsCsv()
    {
        // Arrange
        using var db = CreateInMemoryDb(nameof(ExportController_GeneratesValidTeamsCsv));
        var hackathon = new Hackathon { Id = 1, Title = "AI Clash", Slug = "ai-clash", OrganizerId = "org-1" };
        var team = new Team
        {
            Id = 1,
            HackathonId = 1,
            Name = "Quantum Coders",
            JoinCode = "QUANTUM1",
            LeadId = "user-1",
            Members = [new TeamMember { TeamId = 1, UserId = "user-1" }],
            Submission = new Submission
            {
                Title = "Quantum Solver",
                RepoUrl = "https://github.com/quantum/solver",
                SubmittedAt = DateTime.UtcNow
            }
        };
        db.Hackathons.Add(hackathon);
        db.Teams.Add(team);
        await db.SaveChangesAsync();

        var hackathonService = new HackathonService(db);
        var scoringService = new ScoringService(db);
        var teamService = new TeamService(db);
        var controller = new ExportController(hackathonService, scoringService, teamService);

        // Act
        var result = await controller.ExportTeamsCsv(1);

        // Assert
        result.Should().BeOfType<FileContentResult>();
        var fileResult = (FileContentResult)result;
        fileResult.ContentType.Should().Be("text/csv");
        fileResult.FileDownloadName.Should().Contain("ai-clash-teams");

        var csvText = Encoding.UTF8.GetString(fileResult.FileContents);
        csvText.Should().Contain("Quantum Coders");
        csvText.Should().Contain("QUANTUM1");
        csvText.Should().Contain("Quantum Solver");
    }

    [Fact]
    public async Task ExportController_GeneratesValidScoresCsv()
    {
        // Arrange
        using var db = CreateInMemoryDb(nameof(ExportController_GeneratesValidScoresCsv));
        var hackathon = new Hackathon { Id = 2, Title = "Cloud Sprint", Slug = "cloud-sprint", OrganizerId = "org-1" };
        var crit = new Criterion { Id = 1, HackathonId = 2, Name = "Execution", MaxPoints = 10, WeightPercent = 100 };
        var team = new Team
        {
            Id = 2,
            HackathonId = 2,
            Name = "Cloud Team",
            JoinCode = "CLOUD123",
            LeadId = "user-2",
            Scores = [new Score { CriterionId = 1, JudgeId = "j1", Points = 9 }]
        };
        db.Hackathons.Add(hackathon);
        db.Criteria.Add(crit);
        db.Teams.Add(team);
        await db.SaveChangesAsync();

        var hackathonService = new HackathonService(db);
        var scoringService = new ScoringService(db);
        var teamService = new TeamService(db);
        var controller = new ExportController(hackathonService, scoringService, teamService);

        // Act
        var result = await controller.ExportScoresCsv(2);

        // Assert
        result.Should().BeOfType<FileContentResult>();
        var fileResult = (FileContentResult)result;
        fileResult.ContentType.Should().Be("text/csv");

        var csvText = Encoding.UTF8.GetString(fileResult.FileContents);
        csvText.Should().Contain("Rank,TeamId,TeamName");
        csvText.Should().Contain("Cloud Team");
        csvText.Should().Contain("90.00"); // 9/10 * 100 = 90
    }

    [Fact]
    public async Task AuditService_LogsAndRetrievesRecentActions()
    {
        // Arrange
        using var db = CreateInMemoryDb(nameof(AuditService_LogsAndRetrievesRecentActions));
        var service = new AuditService(db);

        // Act
        await service.LogAsync("admin-99", "HackathonPublished", "Hackathon", "101", "{\"slug\":\"ai-sprint\"}");
        await service.LogAsync("admin-99", "JudgingLocked", "Hackathon", "101", "{\"locked\":true}");

        var recentLogs = await service.GetRecentLogsAsync(10);

        // Assert
        recentLogs.Should().HaveCount(2);
        recentLogs[0].Action.Should().Be("JudgingLocked");
        recentLogs[1].Action.Should().Be("HackathonPublished");
    }

    [Fact]
    public async Task SmtpEmailSender_DoesNotThrowWhenSmtpNotConfigured()
    {
        // Arrange
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        var logger = NullLogger<SmtpEmailSender>.Instance;
        var sender = new SmtpEmailSender(config, logger);

        // Act & Assert
        var act = async () => await sender.SendEmailAsync("test@user.com", "Test Subject", "<p>Hello</p>");
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void LeaderboardHub_GeneratesCorrectGroupName()
    {
        // Act
        var groupName = LeaderboardHub.GetGroupName(42);

        // Assert
        groupName.Should().Be("hackathon_42");
    }
}
