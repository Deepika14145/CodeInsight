using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeInsight.Infrastructure.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Repositories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Url = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Owner = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Language = table.Column<string>(type: "TEXT", nullable: true),
                    Stars = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Repositories", x => x.Id));

            migrationBuilder.CreateTable(
                name: "AnalysisReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RepositoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TotalFilesAnalyzed = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalIssuesFound = table.Column<int>(type: "INTEGER", nullable: false),
                    HighComplexityCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LongMethodCount = table.Column<int>(type: "INTEGER", nullable: false),
                    DuplicateBlockCount = table.Column<int>(type: "INTEGER", nullable: false),
                    DeepNestingCount = table.Column<int>(type: "INTEGER", nullable: false),
                    MaintainabilityScore = table.Column<double>(type: "REAL", nullable: false),
                    ComplexityScore = table.Column<double>(type: "REAL", nullable: false),
                    CodeQualityScore = table.Column<double>(type: "REAL", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    ProgressPercent = table.Column<int>(type: "INTEGER", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnalysisReports_Repositories_RepositoryId",
                        column: x => x.RepositoryId,
                        principalTable: "Repositories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CodeIssues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReportId = table.Column<int>(type: "INTEGER", nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 260, nullable: false),
                    IssueType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Severity = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Suggestion = table.Column<string>(type: "TEXT", nullable: false),
                    MethodName = table.Column<string>(type: "TEXT", nullable: true),
                    LineStart = table.Column<int>(type: "INTEGER", nullable: true),
                    LineEnd = table.Column<int>(type: "INTEGER", nullable: true),
                    MetricValue = table.Column<int>(type: "INTEGER", nullable: true),
                    CodeSnippet = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeIssues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CodeIssues_AnalysisReports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "AnalysisReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex("IX_AnalysisReports_RepositoryId", "AnalysisReports", "RepositoryId");
            migrationBuilder.CreateIndex("IX_CodeIssues_ReportId", "CodeIssues", "ReportId");
            migrationBuilder.CreateIndex("IX_Repositories_Url", "Repositories", "Url", unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("CodeIssues");
            migrationBuilder.DropTable("AnalysisReports");
            migrationBuilder.DropTable("Repositories");
        }
    }
}
