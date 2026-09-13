using Shared.Models.DTOs;

namespace Task12_ASPNETCoreBlazorUI.Services.Contracts;

public interface IReportService
{
    Task<ReportDto> GetPeriodReportAsync(DateTime startDate, DateTime endDate);
    Task<ReportDto> GetDailyReportAsync(DateTime date);
}
