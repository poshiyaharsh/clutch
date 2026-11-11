using Clutch.Web.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace Clutch.Tests;

public class SetupTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SetupTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void MockData_ReturnsInitialMockHackathons()
    {
        // Act
        var events = MockData.Events;

        // Assert
        events.Should().NotBeNull();
        events.Should().NotBeEmpty();
        events.Should().Contain(e => e.Title.Contains("Hackathon"));
    }

    [Fact]
    public void MockData_ReturnsValidLeaderboard()
    {
        // Act
        var leaderboard = MockData.Leaderboard;

        // Assert
        leaderboard.Should().NotBeNull();
        leaderboard.Should().NotBeEmpty();
        leaderboard.Should().BeInDescendingOrder(t => t.Score);
    }

    [Fact]
    public async Task HomePage_ReturnsSuccessAndCorrectContentType()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
    }
}
