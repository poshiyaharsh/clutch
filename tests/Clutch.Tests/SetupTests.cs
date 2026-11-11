using Clutch.Web.Models;

namespace Clutch.Tests;

public class SetupTests
{
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
}
