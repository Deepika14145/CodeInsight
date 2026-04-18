using Microsoft.EntityFrameworkCore;
using CodeInsight.Core.Entities;

namespace CodeInsight.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Fallback for design-time tools
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlite("Data Source=codeinsight.db");
    }

    public DbSet<Core.Entities.Repository> Repositories => Set<Core.Entities.Repository>();
    public DbSet<AnalysisReport> AnalysisReports => Set<AnalysisReport>();
    public DbSet<CodeIssue> CodeIssues => Set<CodeIssue>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Core.Entities.Repository>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Name).HasMaxLength(200).IsRequired();
            e.Property(r => r.Url).HasMaxLength(500).IsRequired();
            e.HasIndex(r => r.Url).IsUnique();
        });

        modelBuilder.Entity<AnalysisReport>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Status).HasMaxLength(50);
            e.HasOne(r => r.Repository)
             .WithMany(repo => repo.Reports)
             .HasForeignKey(r => r.RepositoryId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CodeIssue>(e =>
        {
            e.HasKey(i => i.Id);
            e.Property(i => i.FilePath).HasMaxLength(1000);
            e.Property(i => i.FileName).HasMaxLength(260);
            e.Property(i => i.IssueType).HasMaxLength(100);
            e.Property(i => i.Severity).HasMaxLength(50);
            e.Property(i => i.CodeSnippet).HasMaxLength(2000);
            e.HasOne(i => i.Report)
             .WithMany(r => r.Issues)
             .HasForeignKey(i => i.ReportId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
