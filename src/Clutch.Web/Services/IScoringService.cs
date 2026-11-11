using Clutch.Web.Domain;
using Microsoft.EntityFrameworkCore;

namespace Clutch.Web.Services;

public record TeamScoreResult(
    int TeamId,
    string TeamName,
    string? TrackName,
    decimal TotalScore,
    decimal FirstCriterionScore,
    DateTime? SubmittedAt,
    int Rank
);

public interface IScoringService
{
    Task SubmitScoreAsync(int teamId, int criterionId, string judgeId, int points, string? comment);
    Task<IReadOnlyList<TeamScoreResult>> CalculateLeaderboardAsync(int hackathonId);
    Task<bool> LockJudgingAsync(int hackathonId, string lockedById);
    Task<bool> IsJudgingLockedAsync(int hackathonId);
    Task<bool> CanJudgeTeamAsync(int teamId, string judgeId);
    Task<IReadOnlyList<Hackathon>> GetAssignedHackathonsForJudgeAsync(string judgeId);
    Task<IReadOnlyList<Team>> GetTeamsForJudgingAsync(int hackathonId);
    Task<IReadOnlyList<Criterion>> GetCriteriaAsync(int hackathonId);
    Task<IReadOnlyList<Score>> GetTeamScoresForJudgeAsync(int teamId, string judgeId);
    Task<bool> AssignJudgeAsync(int hackathonId, string judgeEmail);
    Task<Criterion> AddCriterionAsync(int hackathonId, string name, string? description, int maxPoints, int weightPercent);
}

public class ScoringService : IScoringService
{
    private readonly Data.AppDbContext _db;

    public ScoringService(Data.AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> IsJudgingLockedAsync(int hackathonId)
    {
        return await _db.JudgingLocks.AnyAsync(j => j.HackathonId == hackathonId);
    }

    public async Task<bool> CanJudgeTeamAsync(int teamId, string judgeId)
    {
        var team = await _db.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team is null) return false;

        // Block if judging is already locked
        if (await IsJudgingLockedAsync(team.HackathonId)) return false;

        // Block conflict of interest: judge cannot score their own team
        if (team.LeadId == judgeId || team.Members.Any(m => m.UserId == judgeId))
        {
            return false;
        }

        return true;
    }

    public async Task SubmitScoreAsync(int teamId, int criterionId, string judgeId, int points, string? comment)
    {
        var canJudge = await CanJudgeTeamAsync(teamId, judgeId);
        if (!canJudge)
        {
            throw new InvalidOperationException("You cannot score this squad (judging may be locked or you are a member of this squad).");
        }

        var criterion = await _db.Criteria.FindAsync(criterionId)
            ?? throw new InvalidOperationException("Rubric criterion not found.");

        if (points < 0 || points > criterion.MaxPoints)
        {
            throw new ArgumentOutOfRangeException(nameof(points), $"Points must be between 0 and {criterion.MaxPoints}.");
        }

        var existing = await _db.Scores
            .FirstOrDefaultAsync(s => s.TeamId == teamId && s.CriterionId == criterionId && s.JudgeId == judgeId);

        if (existing is not null)
        {
            existing.Points = points;
            existing.Comment = comment;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _db.Scores.Add(new Score
            {
                TeamId = teamId,
                CriterionId = criterionId,
                JudgeId = judgeId,
                Points = points,
                Comment = comment,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<Hackathon>> GetAssignedHackathonsForJudgeAsync(string judgeId)
    {
        return await _db.JudgeAssignments
            .Where(j => j.JudgeId == judgeId)
            .Select(j => j.Hackathon)
            .Include(h => h.Teams)
            .Include(h => h.JudgingLock)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Team>> GetTeamsForJudgingAsync(int hackathonId)
    {
        return await _db.Teams
            .Where(t => t.HackathonId == hackathonId)
            .Include(t => t.Submission).ThenInclude(s => s!.Track)
            .Include(t => t.Scores)
            .Include(t => t.Members)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Criterion>> GetCriteriaAsync(int hackathonId)
    {
        return await _db.Criteria
            .Where(c => c.HackathonId == hackathonId)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Score>> GetTeamScoresForJudgeAsync(int teamId, string judgeId)
    {
        return await _db.Scores
            .Where(s => s.TeamId == teamId && s.JudgeId == judgeId)
            .ToListAsync();
    }

    public async Task<bool> AssignJudgeAsync(int hackathonId, string judgeEmail)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == judgeEmail.Trim());
        if (user is null) return false;

        var alreadyAssigned = await _db.JudgeAssignments
            .AnyAsync(j => j.HackathonId == hackathonId && j.JudgeId == user.Id);
        if (alreadyAssigned) return true;

        _db.JudgeAssignments.Add(new JudgeAssignment
        {
            HackathonId = hackathonId,
            JudgeId = user.Id,
            AssignedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<Criterion> AddCriterionAsync(int hackathonId, string name, string? description, int maxPoints, int weightPercent)
    {
        var currentCount = await _db.Criteria.CountAsync(c => c.HackathonId == hackathonId);
        var criterion = new Criterion
        {
            HackathonId = hackathonId,
            Name = name.Trim(),
            Description = description?.Trim(),
            MaxPoints = maxPoints,
            WeightPercent = weightPercent,
            SortOrder = currentCount + 1
        };

        _db.Criteria.Add(criterion);
        await _db.SaveChangesAsync();
        return criterion;
    }

    public async Task<IReadOnlyList<TeamScoreResult>> CalculateLeaderboardAsync(int hackathonId)
    {
        var criteria = await _db.Criteria
            .Where(c => c.HackathonId == hackathonId)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

        var teams = await _db.Teams
            .Include(t => t.Submission).ThenInclude(s => s!.Track)
            .Include(t => t.Scores)
            .Where(t => t.HackathonId == hackathonId)
            .ToListAsync();

        var firstCriterion = criteria.FirstOrDefault();
        var results = new List<TeamScoreResult>();

        foreach (var team in teams)
        {
            decimal totalScore = 0m;
            decimal firstCriterionScore = 0m;

            foreach (var criterion in criteria)
            {
                var criterionScores = team.Scores.Where(s => s.CriterionId == criterion.Id).ToList();
                if (criterionScores.Count > 0 && criterion.MaxPoints > 0)
                {
                    decimal avgPoints = (decimal)criterionScores.Average(s => s.Points);
                    decimal criterionContribution = (avgPoints / criterion.MaxPoints) * criterion.WeightPercent;
                    totalScore += criterionContribution;

                    if (firstCriterion != null && criterion.Id == firstCriterion.Id)
                    {
                        firstCriterionScore = avgPoints;
                    }
                }
            }

            results.Add(new TeamScoreResult(
                team.Id,
                team.Name,
                team.Submission?.Track?.Name,
                Math.Round(totalScore, 2),
                Math.Round(firstCriterionScore, 2),
                team.Submission?.SubmittedAt,
                0
            ));
        }

        // Sort: (1) TotalScore desc, (2) First criterion score desc, (3) SubmittedAt asc
        var sorted = results
            .OrderByDescending(r => r.TotalScore)
            .ThenByDescending(r => r.FirstCriterionScore)
            .ThenBy(r => r.SubmittedAt ?? DateTime.MaxValue)
            .Select((r, index) => r with { Rank = index + 1 })
            .ToList();

        return sorted;
    }

    public async Task<bool> LockJudgingAsync(int hackathonId, string lockedById)
    {
        var existing = await _db.JudgingLocks.FindAsync(hackathonId);
        if (existing is not null) return true;

        _db.JudgingLocks.Add(new JudgingLock
        {
            HackathonId = hackathonId,
            LockedById = lockedById,
            LockedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return true;
    }
}
