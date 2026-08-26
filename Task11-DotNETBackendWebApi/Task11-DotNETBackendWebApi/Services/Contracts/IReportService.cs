using Shared.Models.DTOs;

namespace Task11_DotNETBackendWebApi.Services.Contracts;

public interface IReportService
{
    Task<ReportDto> GetDailyReportAsync(DateTime date);
    Task<ReportDto> GetPeriodReportAsync(DateTime start, DateTime end);
}
