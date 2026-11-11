using Clutch.Web.Data;
using Clutch.Web.Domain;
using Clutch.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace Clutch.Tests;

public class CoreFeatureTests
{
    private AppDbContext CreateInMemoryDb(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task TeamService_PreventsDuplicateTeamNameInSameHackathon()
    {
        // Arrange
        using var db = CreateInMemoryDb(nameof(TeamService_PreventsDuplicateTeamNameInSameHackathon));
        var hackathon = new Hackathon
        {
            Id = 1,
            Title = "Hack Sprint",
            Slug = "hack-sprint",
            Status = HackathonStatus.Published,
            RegistrationDeadline = DateTime.UtcNow.AddDays(2),
            OrganizerId = "org-1"
        };
        db.Hackathons.Add(hackathon);
        await db.SaveChangesAsync();

        var teamService = new TeamService(db);
        await teamService.CreateTeamAsync(1, "ByteForce", "user-1");

        // Act & Assert
        var act = async () => await teamService.CreateTeamAsync(1, "ByteForce", "user-2");
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already taken*");
    }

    [Fact]
    public async Task TeamService_PreventsUserRegisteringTwiceInSameHackathon()
    {
        // Arrange
        using var db = CreateInMemoryDb(nameof(TeamService_PreventsUserRegisteringTwiceInSameHackathon));
        var hackathon = new Hackathon
        {
            Id = 2,
            Title = "Dual Hack",
            Slug = "dual-hack",
            Status = HackathonStatus.Published,
            RegistrationDeadline = DateTime.UtcNow.AddDays(2),
            OrganizerId = "org-1"
        };
        db.Hackathons.Add(hackathon);
        await db.SaveChangesAsync();

        var teamService = new TeamService(db);
        await teamService.CreateTeamAsync(2, "Alpha", "user-greedy");

        // Act & Assert
        var act = async () => await teamService.CreateTeamAsync(2, "Beta", "user-greedy");
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already registered*");
    }

    [Fact]
    public async Task TeamService_SavesAndUpdatesProjectSubmission()
    {
        // Arrange
        using var db = CreateInMemoryDb(nameof(TeamService_SavesAndUpdatesProjectSubmission));
        var hackathon = new Hackathon
        {
            Id = 3,
            Title = "Shipathon",
            Slug = "shipathon",
            Status = HackathonStatus.Published,
            RegistrationDeadline = DateTime.UtcNow.AddDays(1),
            SubmissionDeadline = DateTime.UtcNow.AddDays(3),
            OrganizerId = "org-1"
        };
        db.Hackathons.Add(hackathon);

        var team = new Team
        {
            Id = 10,
            HackathonId = 3,
            Name = "Builders",
            JoinCode = "BUILD123",
            LeadId = "user-builder",
            Members = [new TeamMember { TeamId = 10, UserId = "user-builder" }]
        };
        db.Teams.Add(team);
        await db.SaveChangesAsync();

        var teamService = new TeamService(db);

        // Act - First submission
        var sub1 = await teamService.SaveSubmissionAsync(
            10,
            "Autonomous Agents",
            "Agents that solve tasks",
            "https://github.com/test/repo",
            "https://demo.app",
            null,
            "user-builder"
        );

        // Assert
        sub1.Should().NotBeNull();
        sub1!.Title.Should().Be("Autonomous Agents");

        // Act - Update submission
        var sub2 = await teamService.SaveSubmissionAsync(
            10,
            "Autonomous Agents v2",
            "Improved agent swarm",
            "https://github.com/test/repo-v2",
            "https://demo2.app",
            null,
            "user-builder"
        );

        // Assert
        sub2.Should().NotBeNull();
        sub2!.Title.Should().Be("Autonomous Agents v2");
        sub2.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task TeamService_BlocksSubmissionAfterDeadline()
    {
        // Arrange
        using var db = CreateInMemoryDb(nameof(TeamService_BlocksSubmissionAfterDeadline));
        var hackathon = new Hackathon
        {
            Id = 4,
            Title = "Expired Hack",
            Slug = "expired-hack",
            Status = HackathonStatus.Published,
            SubmissionDeadline = DateTime.UtcNow.AddMinutes(-5), // Past deadline
            OrganizerId = "org-1"
        };
        db.Hackathons.Add(hackathon);

        var team = new Team
        {
            Id = 20,
            HackathonId = 4,
            Name = "Late Team",
            JoinCode = "LATE1234",
            LeadId = "user-late",
            Members = [new TeamMember { TeamId = 20, UserId = "user-late" }]
        };
        db.Teams.Add(team);
        await db.SaveChangesAsync();

        var teamService = new TeamService(db);

        // Act
        var result = await teamService.SaveSubmissionAsync(
            20,
            "Late Project",
            "Submitted too late",
            "https://github.com/late/repo",
            null,
            null,
            "user-late"
        );

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task TeamService_AllowsLeadToRemoveMember_AndBlocksNonLead()
    {
        // Arrange
        using var db = CreateInMemoryDb(nameof(TeamService_AllowsLeadToRemoveMember_AndBlocksNonLead));
        var team = new Team
        {
            Id = 30,
            HackathonId = 1,
            Name = "Collab Team",
            JoinCode = "COLLAB01",
            LeadId = "user-captain",
            Members =
            [
                new TeamMember { TeamId = 30, UserId = "user-captain" },
                new TeamMember { TeamId = 30, UserId = "user-slacker" }
            ]
        };
        db.Teams.Add(team);
        await db.SaveChangesAsync();

        var teamService = new TeamService(db);

        // Act - Non-lead attempts removal
        var unauthorizedAttempt = await teamService.RemoveMemberAsync(30, "user-slacker", "user-intruder");
        unauthorizedAttempt.Should().BeFalse();

        // Act - Captain removes slacker
        var authorizedAttempt = await teamService.RemoveMemberAsync(30, "user-slacker", "user-captain");
        authorizedAttempt.Should().BeTrue();

        var remaining = await db.TeamMembers.Where(m => m.TeamId == 30).ToListAsync();
        remaining.Should().HaveCount(1);
        remaining[0].UserId.Should().Be("user-captain");
    }

    [Fact]
    public async Task HackathonService_CreatesHackathonWithTracks_AndPostsAnnouncements()
    {
        // Arrange
        using var db = CreateInMemoryDb(nameof(HackathonService_CreatesHackathonWithTracks_AndPostsAnnouncements));
        db.Users.Add(new ApplicationUser { Id = "org-vitalik", UserName = "vitalik@ethereum.org", DisplayName = "Vitalik Buterin" });
        await db.SaveChangesAsync();

        var hackathonService = new HackathonService(db);

        var hackathon = new Hackathon
        {
            Title = "Web3 Nexus",
            Slug = "web3-nexus",
            Description = "Decentralized future",
            StartsAt = DateTime.UtcNow,
            EndsAt = DateTime.UtcNow.AddDays(3),
            RegistrationDeadline = DateTime.UtcNow.AddDays(1),
            SubmissionDeadline = DateTime.UtcNow.AddDays(3),
            Status = HackathonStatus.Published
        };

        // Act 1: Create with tracks
        var created = await hackathonService.CreateWithTracksAsync(
            hackathon,
            ["DeFi", "Infra", "Identity"],
            "org-vitalik"
        );

        created.Tracks.Should().HaveCount(3);
        created.Tracks.Should().Contain(t => t.Name == "DeFi");

        // Act 2: Post announcement
        var announcement = await hackathonService.AddAnnouncementAsync(
            created.Id,
            "Mentors are in room 4B",
            "Drop by for architecture feedback",
            "org-vitalik"
        );

        announcement.Should().NotBeNull();
        announcement.Title.Should().Be("Mentors are in room 4B");

        var detailed = await hackathonService.GetDetailBySlugAsync("web3-nexus");
        detailed.Should().NotBeNull();
        detailed!.Announcements.Should().HaveCount(1);
        detailed.Tracks.Should().HaveCount(3);
    }
}
