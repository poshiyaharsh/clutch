using Clutch.Web.Controllers;
using Clutch.Web.Data;
using Clutch.Web.Domain;
using Clutch.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Reflection;

namespace Clutch.Tests;

public class SecurityAndHardeningTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SecurityAndHardeningTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private AppDbContext CreateInMemoryDb(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Responses_IncludeSecurityHeaders()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        response.Headers.Contains("X-Content-Type-Options").Should().BeTrue();
        response.Headers.GetValues("X-Content-Type-Options").First().Should().Be("nosniff");

        response.Headers.Contains("X-Frame-Options").Should().BeTrue();
        response.Headers.GetValues("X-Frame-Options").First().Should().Be("DENY");

        response.Headers.Contains("Referrer-Policy").Should().BeTrue();
        response.Headers.GetValues("Referrer-Policy").First().Should().Be("strict-origin-when-cross-origin");
    }

    [Theory]
    [InlineData("/hackathons/create")]
    [InlineData("/admin")]
    [InlineData("/judging")]
    [InlineData("/teams/create?hackathonId=1")]
    public async Task ProtectedEndpoints_RedirectUnauthenticatedUsersToLogin(string url)
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act
        var response = await client.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("/Account/Login");
    }

    [Fact]
    public void AllPostActions_HaveValidateAntiForgeryTokenAttribute()
    {
        // Arrange
        var assembly = typeof(HomeController).Assembly;
        var controllerTypes = assembly.GetTypes()
            .Where(t => typeof(Controller).IsAssignableFrom(t) && !t.IsAbstract);

        var missingAntiCsrf = new List<string>();

        // Act
        foreach (var controller in controllerTypes)
        {
            var postActions = controller.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(m => m.GetCustomAttributes(typeof(HttpPostAttribute), true).Any());

            foreach (var action in postActions)
            {
                var hasTokenAttr = action.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), true).Any() ||
                                   action.GetCustomAttributes(typeof(AutoValidateAntiforgeryTokenAttribute), true).Any();

                if (!hasTokenAttr)
                {
                    missingAntiCsrf.Add($"{controller.Name}.{action.Name}");
                }
            }
        }

        // Assert
        missingAntiCsrf.Should().BeEmpty("all state-changing POST actions must enforce ValidateAntiForgeryToken");
    }

    [Fact]
    public async Task Security_PreventIDOR_OnTeamLeaderTransfer()
    {
        // Arrange
        using var db = CreateInMemoryDb(nameof(Security_PreventIDOR_OnTeamLeaderTransfer));
        var team = new Team
        {
            Id = 1,
            HackathonId = 1,
            Name = "Protected Squad",
            JoinCode = "PROT1234",
            LeadId = "user-real-lead",
            Members =
            [
                new TeamMember { TeamId = 1, UserId = "user-real-lead" },
                new TeamMember { TeamId = 1, UserId = "user-target" }
            ]
        };
        db.Teams.Add(team);
        await db.SaveChangesAsync();

        var teamService = new TeamService(db);

        // Act: Attacker "user-attacker" tries to reassign leadership of team 1
        var result = await teamService.TransferLeadAsync(1, "user-attacker", "user-attacker");

        // Assert: Denied
        result.Should().BeFalse();
        var unchanged = await db.Teams.FindAsync(1);
        unchanged!.LeadId.Should().Be("user-real-lead");
    }

    [Fact]
    public async Task Security_PreventIDOR_OnTeamMemberRemoval()
    {
        // Arrange
        using var db = CreateInMemoryDb(nameof(Security_PreventIDOR_OnTeamMemberRemoval));
        var team = new Team
        {
            Id = 2,
            HackathonId = 1,
            Name = "Squad Two",
            JoinCode = "SQUAD222",
            LeadId = "user-owner",
            Members =
            [
                new TeamMember { TeamId = 2, UserId = "user-owner" },
                new TeamMember { TeamId = 2, UserId = "user-victim" }
            ]
        };
        db.Teams.Add(team);
        await db.SaveChangesAsync();

        var teamService = new TeamService(db);

        // Act: Non-lead "user-intruder" attempts to kick out "user-victim"
        var result = await teamService.RemoveMemberAsync(2, "user-victim", "user-intruder");

        // Assert: Denied
        result.Should().BeFalse();
        var members = await db.TeamMembers.Where(m => m.TeamId == 2).ToListAsync();
        members.Should().HaveCount(2);
    }
}
