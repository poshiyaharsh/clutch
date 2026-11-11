namespace Clutch.Web.Models;

// Phase 3 mock data. Replace with services in Phase 5. Antigravity: delete this file when real data is wired.
public record EventCard(int Id, string Title, string Blurb, string Status, string StatusClass, string DatesText, int Teams);
public record TeamRow(int Rank, string Name, string Track, double Score, bool Pending);
public record Criterion(string Name, string Hint, int Max, int Weight);

public static class MockData
{
    public static readonly List<EventCard> Events = new()
    {
        new(1, "CodeMind AI Hackathon", "24 hours to build something with AI that people would actually use.", "Open", "open", "Dec 5–6", 42),
        new(2, "Campus Climate Jam", "Build tools that cut waste on campus. Teams of up to 4.", "Judging", "judging", "Nov 8–9", 31),
        new(3, "Midnight Mobile Sprint", "Ship a mobile prototype between dusk and dawn.", "Draft", "draft", "Jan 17", 0),
    };

    public static readonly List<TeamRow> Leaderboard = new()
    {
        new(1, "Null Pointers", "Health", 92.4, false),
        new(2, "Byte Me", "Education", 88.1, false),
        new(3, "Git Happens", "Open track", 85.7, false),
        new(4, "Ctrl+Alt+Elite", "Health", 81.3, false),
        new(5, "Segfault Squad", "Education", 77.9, false),
        new(6, "Rubber Duckies", "Open track", 0, true),
    };

    public static readonly List<Criterion> Rubric = new()
    {
        new("Idea", "Is the problem real and the idea fresh?", 10, 25),
        new("Execution", "Does it work during the demo?", 10, 30),
        new("Design", "Is it clear and pleasant to use?", 10, 20),
        new("Impact", "Who benefits, and how much?", 10, 25),
    };
}
