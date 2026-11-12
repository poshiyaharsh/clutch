using Clutch.Web.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Clutch.Web.Data;

public static class DbSeeder
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var adminEmail = configuration["SeedAdmin:Email"] ?? "admin@clutch.local";
        var adminPassword = configuration["SeedAdmin:Password"];
        if (string.IsNullOrWhiteSpace(adminPassword) || adminPassword.StartsWith("REPLACE_WITH_", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "SeedAdmin:Password must be configured via environment variable or dotnet user-secrets before running --seed.");
        }

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                DisplayName = "Clutch Admin",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, Roles.Admin);
            }
        }
    }

    public static async Task SeedDemoDataAsync(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        await SeedRolesAndAdminAsync(serviceProvider, configuration);

        var demoPassword = configuration["SeedDemo:Password"];
        if (string.IsNullOrWhiteSpace(demoPassword) || demoPassword.StartsWith("REPLACE_WITH_", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "SeedDemo:Password must be configured via environment variable or dotnet user-secrets before running --seed-demo.");
        }

        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        async Task<ApplicationUser> EnsureUserAsync(string email, string displayName, string org, string role)
        {
            var existing = await userManager.FindByEmailAsync(email);
            if (existing is not null)
            {
                return existing;
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                DisplayName = displayName,
                CollegeOrCompany = org,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, demoPassword);
            if (!result.Succeeded)
            {
                var err = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to seed user {email}: {err}");
            }

            await userManager.AddToRoleAsync(user, role);
            return user;
        }

        var organizer = await EnsureUserAsync("organizer@clutch.local", "Elena Vance", "Clutch Foundation", Roles.Organizer);
        var judge1 = await EnsureUserAsync("judge1@clutch.local", "Dr. Aris Thorne", "MIT CSAIL", Roles.Judge);
        var judge2 = await EnsureUserAsync("judge2@clutch.local", "Maya Lin", "Vercel Engineering", Roles.Judge);
        var alex = await EnsureUserAsync("alex@clutch.local", "Alex Chen", "MIT", Roles.Participant);
        var sara = await EnsureUserAsync("sara@clutch.local", "Sara Rodriguez", "UC Berkeley", Roles.Participant);
        var priya = await EnsureUserAsync("priya@clutch.local", "Priya Nair", "Georgia Tech", Roles.Participant);
        var marcus = await EnsureUserAsync("marcus@clutch.local", "Marcus Bell", "Carnegie Mellon", Roles.Participant);

        const string demoSlug = "codemind-ai-2026";
        if (await db.Hackathons.AnyAsync(h => h.Slug == demoSlug))
        {
            return;
        }

        var now = DateTime.UtcNow;
        var hackathon = new Hackathon
        {
            Title = "CodeMind Global AI Hackathon",
            Slug = demoSlug,
            Description = "Build high-reliability developer tools, autonomous workflows, and climate software over 48 hours.",
            StartsAt = now.AddDays(-1),
            EndsAt = now.AddDays(2),
            RegistrationDeadline = now.AddDays(1),
            SubmissionDeadline = now.AddDays(2),
            MaxTeamSize = 4,
            Status = HackathonStatus.Published,
            HideLeaderboardUntilFinal = false,
            OrganizerId = organizer.Id,
            CreatedAt = now.AddDays(-10)
        };

        db.Hackathons.Add(hackathon);
        await db.SaveChangesAsync();

        var trackAgents = new Track { HackathonId = hackathon.Id, Name = "AI Agents", Description = "Autonomous tooling and reasoning pipelines" };
        var trackDevTools = new Track { HackathonId = hackathon.Id, Name = "Developer Tools", Description = "IDE, CLI, and observability products" };
        var trackClimate = new Track { HackathonId = hackathon.Id, Name = "Climate Tech", Description = "Grid analytics and carbon reduction software" };
        db.Tracks.AddRange(trackAgents, trackDevTools, trackClimate);

        var critExec = new Criterion { HackathonId = hackathon.Id, Name = "Technical Execution", Description = "Code quality, reliability, and architecture", MaxPoints = 10, WeightPercent = 35, SortOrder = 1 };
        var critInnov = new Criterion { HackathonId = hackathon.Id, Name = "Innovation & Impact", Description = "Originality and real-world problem fit", MaxPoints = 10, WeightPercent = 30, SortOrder = 2 };
        var critCraft = new Criterion { HackathonId = hackathon.Id, Name = "Product Craft", Description = "UX clarity, accessibility, and polish", MaxPoints = 10, WeightPercent = 20, SortOrder = 3 };
        var critDemo = new Criterion { HackathonId = hackathon.Id, Name = "Live Demo", Description = "Clarity of live walkthrough and problem explanation", MaxPoints = 10, WeightPercent = 15, SortOrder = 4 };
        db.Criteria.AddRange(critExec, critInnov, critCraft, critDemo);

        db.JudgeAssignments.AddRange(
            new JudgeAssignment { HackathonId = hackathon.Id, JudgeId = judge1.Id },
            new JudgeAssignment { HackathonId = hackathon.Id, JudgeId = judge2.Id });

        db.Announcements.Add(new Announcement
        {
            HackathonId = hackathon.Id,
            Title = "Submissions open — rubric weights locked",
            Body = "Ensure your GitHub repository includes setup instructions and a live demo link before the deadline.",
            CreatedById = organizer.Id,
            CreatedAt = now.AddHours(-6)
        });

        await db.SaveChangesAsync();

        var team1 = new Team
        {
            HackathonId = hackathon.Id,
            Name = "Null Pointers",
            JoinCode = "K7Q2M9XD",
            LeadId = alex.Id,
            Members =
            [
                new TeamMember { UserId = alex.Id },
                new TeamMember { UserId = sara.Id }
            ]
        };
        var team2 = new Team
        {
            HackathonId = hackathon.Id,
            Name = "Byte Builders",
            JoinCode = "B4L8P2RT",
            LeadId = priya.Id,
            Members = [new TeamMember { UserId = priya.Id }]
        };
        var team3 = new Team
        {
            HackathonId = hackathon.Id,
            Name = "Signal Crafters",
            JoinCode = "S9N3W5VY",
            LeadId = marcus.Id,
            Members = [new TeamMember { UserId = marcus.Id }]
        };
        db.Teams.AddRange(team1, team2, team3);
        await db.SaveChangesAsync();

        db.Submissions.AddRange(
            new Submission
            {
                TeamId = team1.Id,
                TrackId = trackAgents.Id,
                Title = "PromptPilot",
                Description = "Deterministic regression testing and trace diffing for multi-step AI agent pipelines.",
                RepoUrl = "https://github.com/clutch-demo/promptpilot",
                DemoUrl = "https://promptpilot.demo.local",
                SubmittedAt = now.AddHours(-3)
            },
            new Submission
            {
                TeamId = team2.Id,
                TrackId = trackClimate.Id,
                Title = "GreenGrid",
                Description = "Real-time regional carbon intensity scheduler for CI/CD runners and batch compute jobs.",
                RepoUrl = "https://github.com/clutch-demo/greengrid",
                DemoUrl = "https://greengrid.demo.local",
                SubmittedAt = now.AddHours(-2)
            },
            new Submission
            {
                TeamId = team3.Id,
                TrackId = trackDevTools.Id,
                Title = "TracePulse",
                Description = "Zero-config OpenTelemetry flamegraph viewer built directly into local dev terminals.",
                RepoUrl = "https://github.com/clutch-demo/tracepulse",
                SubmittedAt = now.AddHours(-1)
            });

        db.Scores.AddRange(
            new Score { TeamId = team1.Id, CriterionId = critExec.Id, JudgeId = judge1.Id, Points = 9, Comment = "Clean architecture and test suite." },
            new Score { TeamId = team1.Id, CriterionId = critInnov.Id, JudgeId = judge1.Id, Points = 9, Comment = "Addresses real reliability pain." },
            new Score { TeamId = team1.Id, CriterionId = critCraft.Id, JudgeId = judge1.Id, Points = 10, Comment = "Sharp, accessible UI." },
            new Score { TeamId = team1.Id, CriterionId = critDemo.Id, JudgeId = judge1.Id, Points = 9, Comment = "Clear walkthrough." },
            new Score { TeamId = team2.Id, CriterionId = critExec.Id, JudgeId = judge1.Id, Points = 9, Comment = "Solid telemetry integration." },
            new Score { TeamId = team2.Id, CriterionId = critInnov.Id, JudgeId = judge1.Id, Points = 8 },
            new Score { TeamId = team2.Id, CriterionId = critCraft.Id, JudgeId = judge1.Id, Points = 8 },
            new Score { TeamId = team2.Id, CriterionId = critDemo.Id, JudgeId = judge1.Id, Points = 9 },
            new Score { TeamId = team3.Id, CriterionId = critExec.Id, JudgeId = judge2.Id, Points = 8 },
            new Score { TeamId = team3.Id, CriterionId = critInnov.Id, JudgeId = judge2.Id, Points = 8 },
            new Score { TeamId = team3.Id, CriterionId = critCraft.Id, JudgeId = judge2.Id, Points = 7 },
            new Score { TeamId = team3.Id, CriterionId = critDemo.Id, JudgeId = judge2.Id, Points = 8 });

        await db.SaveChangesAsync();
    }
}
