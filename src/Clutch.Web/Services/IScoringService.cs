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
}

public class ScoringService : IScoringService
{
    private readonly Data.AppDbContext _db;

    public ScoringService(Data.AppDbContext db)
    {
        _db = db;
    }

    public async Task SubmitScoreAsync(int teamId, int criterionId, string judgeId, int points, string? comment)
    {
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
                0 // Rank assigned below
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
