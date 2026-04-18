using Microsoft.AspNetCore.Mvc;
using CodeInsight.Core.DTOs;
using CodeInsight.Core.Interfaces;

namespace CodeInsight.API.Controllers;

[ApiController]
[Route("api")]
[Produces("application/json")]
public class AnalysisController : ControllerBase
{
    private readonly IAnalysisService _analysisService;
    private readonly ILogger<AnalysisController> _logger;

    public AnalysisController(IAnalysisService analysisService, ILogger<AnalysisController> logger)
    {
        _analysisService = analysisService;
        _logger = logger;
    }

    /// <summary>Submit a GitHub repository for analysis.</summary>
    [HttpPost("analyze")]
    [ProducesResponseType(typeof(AnalysisReportSummaryDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Analyze([FromBody] AnalyzeRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _logger.LogInformation("Analysis requested for {Url}", request.RepositoryUrl);
        var result = await _analysisService.StartAnalysisAsync(request);
        return Accepted(result);
    }

    /// <summary>Get all analysis reports (paginated).</summary>
    [HttpGet("reports")]
    [ProducesResponseType(typeof(PagedResult<AnalysisReportSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReports([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _analysisService.GetAllReportsAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>Get a detailed analysis report by ID.</summary>
    [HttpGet("reports/{id:int}")]
    [ProducesResponseType(typeof(AnalysisReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReport(int id)
    {
        var report = await _analysisService.GetReportAsync(id);
        if (report == null) return NotFound(new { message = $"Report {id} not found." });
        return Ok(report);
    }

    /// <summary>Get paginated issues for a report.</summary>
    [HttpGet("issues/{reportId:int}")]
    [ProducesResponseType(typeof(PagedResult<CodeIssueDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetIssues(int reportId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _analysisService.GetIssuesAsync(reportId, page, pageSize);
        return Ok(result);
    }
}
