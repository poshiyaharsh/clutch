using Clutch.Web.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Clutch.Web.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Hackathon> Hackathons => Set<Hackathon>();
    public DbSet<Track> Tracks => Set<Track>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<Criterion> Criteria => Set<Criterion>();
    public DbSet<JudgeAssignment> JudgeAssignments => Set<JudgeAssignment>();
    public DbSet<Score> Scores => Set<Score>();
    public DbSet<JudgingLock> JudgingLocks => Set<JudgingLock>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Hackathon
        builder.Entity<Hackathon>(b =>
        {
            b.HasIndex(h => h.Slug).IsUnique();
            b.HasIndex(h => new { h.Status, h.StartsAt });
            b.Property(h => h.Title).HasMaxLength(200).IsRequired();
            b.Property(h => h.Slug).HasMaxLength(220).IsRequired();
            b.Property(h => h.Description).IsRequired();
            b.HasOne(h => h.Organizer)
                .WithMany(u => u.OrganizedHackathons)
                .HasForeignKey(h => h.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Track
        builder.Entity<Track>(b =>
        {
            b.Property(t => t.Name).HasMaxLength(100).IsRequired();
            b.HasOne(t => t.Hackathon)
                .WithMany(h => h.Tracks)
                .HasForeignKey(t => t.HackathonId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Team
        builder.Entity<Team>(b =>
        {
            b.HasIndex(t => new { t.HackathonId, t.Name }).IsUnique();
            b.HasIndex(t => t.JoinCode).IsUnique();
            b.Property(t => t.Name).HasMaxLength(100).IsRequired();
            b.Property(t => t.JoinCode).HasMaxLength(16).IsRequired();
            b.HasOne(t => t.Hackathon)
                .WithMany(h => h.Teams)
                .HasForeignKey(t => t.HackathonId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(t => t.Lead)
                .WithMany(u => u.LedTeams)
                .HasForeignKey(t => t.LeadId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // TeamMember
        builder.Entity<TeamMember>(b =>
        {
            b.HasIndex(tm => new { tm.TeamId, tm.UserId }).IsUnique();
            b.HasOne(tm => tm.Team)
                .WithMany(t => t.Members)
                .HasForeignKey(tm => tm.TeamId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(tm => tm.User)
                .WithMany(u => u.TeamMemberships)
                .HasForeignKey(tm => tm.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Submission
        builder.Entity<Submission>(b =>
        {
            b.HasIndex(s => s.TeamId).IsUnique();
            b.Property(s => s.Title).HasMaxLength(200).IsRequired();
            b.Property(s => s.RepoUrl).HasMaxLength(500).IsRequired();
            b.HasOne(s => s.Team)
                .WithOne(t => t.Submission)
                .HasForeignKey<Submission>(s => s.TeamId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(s => s.Track)
                .WithMany(tr => tr.Submissions)
                .HasForeignKey(s => s.TrackId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Criterion
        builder.Entity<Criterion>(b =>
        {
            b.Property(c => c.Name).HasMaxLength(100).IsRequired();
            b.HasOne(c => c.Hackathon)
                .WithMany(h => h.Criteria)
                .HasForeignKey(c => c.HackathonId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // JudgeAssignment
        builder.Entity<JudgeAssignment>(b =>
        {
            b.HasIndex(ja => new { ja.HackathonId, ja.JudgeId }).IsUnique();
            b.HasOne(ja => ja.Hackathon)
                .WithMany(h => h.JudgeAssignments)
                .HasForeignKey(ja => ja.HackathonId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(ja => ja.Judge)
                .WithMany(u => u.JudgeAssignments)
                .HasForeignKey(ja => ja.JudgeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Score
        builder.Entity<Score>(b =>
        {
            b.HasIndex(s => new { s.TeamId, s.CriterionId, s.JudgeId }).IsUnique();
            b.HasIndex(s => s.TeamId);
            b.HasIndex(s => s.JudgeId);
            b.HasIndex(s => s.CriterionId);

            b.HasOne(s => s.Team)
                .WithMany(t => t.Scores)
                .HasForeignKey(s => s.TeamId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(s => s.Criterion)
                .WithMany(c => c.Scores)
                .HasForeignKey(s => s.CriterionId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(s => s.Judge)
                .WithMany(u => u.GivenScores)
                .HasForeignKey(s => s.JudgeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // JudgingLock
        builder.Entity<JudgingLock>(b =>
        {
            b.HasKey(jl => jl.HackathonId);
            b.HasOne(jl => jl.Hackathon)
                .WithOne(h => h.JudgingLock)
                .HasForeignKey<JudgingLock>(jl => jl.HackathonId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(jl => jl.LockedBy)
                .WithMany()
                .HasForeignKey(jl => jl.LockedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Announcement
        builder.Entity<Announcement>(b =>
        {
            b.Property(a => a.Title).HasMaxLength(200).IsRequired();
            b.HasIndex(a => new { a.HackathonId, a.CreatedAt });
            b.HasOne(a => a.Hackathon)
                .WithMany(h => h.Announcements)
                .HasForeignKey(a => a.HackathonId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(a => a.CreatedBy)
                .WithMany()
                .HasForeignKey(a => a.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Notification
        builder.Entity<Notification>(b =>
        {
            b.HasIndex(n => new { n.UserId, n.IsRead });
            b.Property(n => n.Title).HasMaxLength(200).IsRequired();
            b.HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AuditLog
        builder.Entity<AuditLog>(b =>
        {
            b.HasIndex(a => a.CreatedAt);
            b.Property(a => a.Action).HasMaxLength(100).IsRequired();
            b.Property(a => a.EntityType).HasMaxLength(100).IsRequired();
            b.HasOne(a => a.Actor)
                .WithMany()
                .HasForeignKey(a => a.ActorId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
