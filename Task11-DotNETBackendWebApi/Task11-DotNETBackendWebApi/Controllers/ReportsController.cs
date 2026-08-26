using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Task11_DotNETBackendWebApi.Services.Contracts;

namespace Task11_DotNETBackendWebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("daily")]
    public async Task<ActionResult> GetDailyReportAsync([FromQuery] DateTime date)
    {
        var result = await _reportService.GetDailyReportAsync(date);
        return Ok(result);      
    }

    [HttpGet("period")]
    public async Task<ActionResult> GetPeriodReportAsync([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var result = await _reportService.GetPeriodReportAsync(startDate, endDate);
        return Ok(result);
    }
}
