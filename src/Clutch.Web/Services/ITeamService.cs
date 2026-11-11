using Clutch.Web.Domain;
using Microsoft.EntityFrameworkCore;

namespace Clutch.Web.Services;

public interface ITeamService
{
    Task<Team?> GetByIdAsync(int id);
    Task<Team?> GetByJoinCodeAsync(string joinCode);
    Task<Team> CreateTeamAsync(int hackathonId, string name, string leadUserId);
    Task<bool> JoinTeamAsync(string joinCode, string userId);
    Task<bool> LeaveTeamAsync(int teamId, string userId);
    Task<bool> TransferLeadAsync(int teamId, string currentLeadId, string newLeadId);
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
            .Include(t => t.Members).ThenInclude(m => m.User)
            .Include(t => t.Lead)
            .Include(t => t.Submission)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Team?> GetByJoinCodeAsync(string joinCode)
    {
        return await _db.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.JoinCode == joinCode);
    }

    public async Task<Team> CreateTeamAsync(int hackathonId, string name, string leadUserId)
    {
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
}
