using Clutch.Web.Domain;
using Microsoft.EntityFrameworkCore;

namespace Clutch.Web.Services;

public interface ITeamService
{
    Task<Team?> GetByIdAsync(int id);
    Task<Team?> GetByJoinCodeAsync(string joinCode);
    Task<Team?> GetByHackathonAndUserAsync(int hackathonId, string userId);
    Task<Team> CreateTeamAsync(int hackathonId, string name, string leadUserId);
    Task<bool> JoinTeamAsync(string joinCode, string userId);
    Task<bool> LeaveTeamAsync(int teamId, string userId);
    Task<bool> RemoveMemberAsync(int teamId, string memberUserId, string actorId);
    Task<bool> TransferLeadAsync(int teamId, string currentLeadId, string newLeadId);
    Task<Submission?> SaveSubmissionAsync(int teamId, string title, string description, string repoUrl, string? demoUrl, int? trackId, string actorId);
}

public class TeamService : ITeamService
{
    private readonly Data.AppDbContext _db;

    public TeamService(Data.AppDbContext db)
    {
        _db = db;
    }

    public async Task<Team?> GetByIdAsync(int id)
    {
        return await _db.Teams
            .Include(t => t.Hackathon).ThenInclude(h => h.Tracks)
            .Include(t => t.Members).ThenInclude(m => m.User)
            .Include(t => t.Lead)
            .Include(t => t.Submission).ThenInclude(s => s!.Track)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Team?> GetByJoinCodeAsync(string joinCode)
    {
        return await _db.Teams
            .Include(t => t.Hackathon)
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.JoinCode == joinCode);
    }

    public async Task<Team?> GetByHackathonAndUserAsync(int hackathonId, string userId)
    {
        return await _db.Teams
            .Include(t => t.Hackathon)
            .Include(t => t.Members).ThenInclude(m => m.User)
            .Include(t => t.Submission).ThenInclude(s => s!.Track)
            .FirstOrDefaultAsync(t => t.HackathonId == hackathonId && t.Members.Any(m => m.UserId == userId));
    }

    public async Task<Team> CreateTeamAsync(int hackathonId, string name, string leadUserId)
    {
        var hackathon = await _db.Hackathons.FindAsync(hackathonId)
            ?? throw new InvalidOperationException("Hackathon not found.");

        if (hackathon.Status != HackathonStatus.Published || DateTime.UtcNow > hackathon.RegistrationDeadline)
        {
            throw new InvalidOperationException("Registration is closed for this hackathon.");
        }

        // Check if name is already taken in this hackathon
        var nameTaken = await _db.Teams.AnyAsync(t => t.HackathonId == hackathonId && t.Name.ToLower() == name.Trim().ToLower());
        if (nameTaken)
        {
            throw new InvalidOperationException($"Team name '{name}' is already taken in this hackathon.");
        }

        // Check if user is already in another team for this hackathon
        var alreadyRegistered = await _db.TeamMembers.AnyAsync(m => m.Team.HackathonId == hackathonId && m.UserId == leadUserId);
        if (alreadyRegistered)
        {
            throw new InvalidOperationException("You are already registered with a team in this hackathon.");
        }

        var joinCode = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        var team = new Team
        {
            HackathonId = hackathonId,
            Name = name.Trim(),
            JoinCode = joinCode,
            LeadId = leadUserId,
            CreatedAt = DateTime.UtcNow,
            Members = [new TeamMember { UserId = leadUserId, JoinedAt = DateTime.UtcNow }]
        };

        _db.Teams.Add(team);
        await _db.SaveChangesAsync();
        return team;
    }

    public async Task<bool> JoinTeamAsync(string joinCode, string userId)
    {
        var team = await _db.Teams
            .Include(t => t.Hackathon)
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.JoinCode == joinCode);

        if (team is null) return false;
        if (team.Hackathon.Status != HackathonStatus.Published || DateTime.UtcNow > team.Hackathon.RegistrationDeadline)
        {
            return false;
        }

        if (team.Members.Count >= team.Hackathon.MaxTeamSize) return false;
        if (team.Members.Any(m => m.UserId == userId)) return false;

        // Check if user is already in another team in this hackathon
        var alreadyInTeam = await _db.TeamMembers
            .AnyAsync(m => m.Team.HackathonId == team.HackathonId && m.UserId == userId);
        if (alreadyInTeam) return false;

        team.Members.Add(new TeamMember { TeamId = team.Id, UserId = userId, JoinedAt = DateTime.UtcNow });
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> LeaveTeamAsync(int teamId, string userId)
    {
        var team = await _db.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team is null) return false;
        var member = team.Members.FirstOrDefault(m => m.UserId == userId);
        if (member is null) return false;
        if (team.LeadId == userId && team.Members.Count > 1) return false; // Must transfer lead before leaving

        _db.TeamMembers.Remove(member);
        if (team.Members.Count <= 1)
        {
            _db.Teams.Remove(team); // If last member leaves, delete team
        }
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveMemberAsync(int teamId, string memberUserId, string actorId)
    {
        var team = await _db.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team is null) return false;
        if (team.LeadId != actorId) return false; // Only lead can remove members
        if (memberUserId == team.LeadId) return false; // Lead cannot remove self

        var member = team.Members.FirstOrDefault(m => m.UserId == memberUserId);
        if (member is null) return false;

        _db.TeamMembers.Remove(member);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> TransferLeadAsync(int teamId, string currentLeadId, string newLeadId)
    {
        var team = await _db.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == teamId && t.LeadId == currentLeadId);

        if (team is null) return false;
        if (!team.Members.Any(m => m.UserId == newLeadId)) return false;

        team.LeadId = newLeadId;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<Submission?> SaveSubmissionAsync(int teamId, string title, string description, string repoUrl, string? demoUrl, int? trackId, string actorId)
    {
        var team = await _db.Teams
            .Include(t => t.Hackathon)
            .Include(t => t.Submission)
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team is null) return null;
        if (team.LeadId != actorId) return null; // Only lead can submit/update
        if (team.Hackathon.Status != HackathonStatus.Published || DateTime.UtcNow > team.Hackathon.SubmissionDeadline)
        {
            return null; // Submission window closed
        }

        if (team.Submission is null)
        {
            team.Submission = new Submission
            {
                TeamId = teamId,
                Title = title.Trim(),
                Description = description.Trim(),
                RepoUrl = repoUrl.Trim(),
                DemoUrl = demoUrl?.Trim(),
                TrackId = trackId,
                SubmittedAt = DateTime.UtcNow
            };
            _db.Submissions.Add(team.Submission);
        }
        else
        {
            team.Submission.Title = title.Trim();
            team.Submission.Description = description.Trim();
            team.Submission.RepoUrl = repoUrl.Trim();
            team.Submission.DemoUrl = demoUrl?.Trim();
            team.Submission.TrackId = trackId;
            team.Submission.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return team.Submission;
    }
}
