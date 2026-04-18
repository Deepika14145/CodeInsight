using Microsoft.EntityFrameworkCore;
using Serilog;
using CodeInsight.Infrastructure.Data;
using CodeInsight.Infrastructure.Repositories;
using CodeInsight.Infrastructure.Services;
using CodeInsight.Core.Interfaces;
using CodeInsight.Analysis.Engine;
using CodeInsight.API.Services;
using CodeInsight.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/codeinsight-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=codeinsight.db"));

// Repositories
builder.Services.AddScoped<IRepositoryRepository, RepositoryRepository>();
builder.Services.AddScoped<IAnalysisReportRepository, AnalysisReportRepository>();
builder.Services.AddScoped<ICodeIssueRepository, CodeIssueRepository>();

// Services
builder.Services.AddScoped<IAnalysisService, AnalysisService>();
builder.Services.AddScoped<IGitHubService, GitHubService>();
builder.Services.AddScoped<ICodeAnalyzer, RoslynCodeAnalyzer>();
builder.Services.AddScoped<RepositoryAnalysisOrchestrator>();

// Caching
builder.Services.AddMemoryCache();

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "CodeInsight API",
        Version = "v1",
        Description = "Technical Debt Analyzer — Powered by Roslyn"
    });
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});

// CORS for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Auto-create DB on startup (SQLite - runs before any requests)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    Log.Information("Database ready at {Path}", db.Database.GetDbConnection().DataSource);
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CodeInsight API v1"));
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();
