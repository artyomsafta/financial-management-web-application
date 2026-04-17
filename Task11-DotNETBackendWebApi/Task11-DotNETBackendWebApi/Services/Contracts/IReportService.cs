using Task11_DotNETBackendWebApi.Models;
using Task11_DotNETBackendWebApi.Models.DTOs;

namespace Task11_DotNETBackendWebApi.Services.Contracts;

public interface IReportService
{
    Task<Result<ReportDto>> GetDailyReportAsync(DateTime date);
    Task<Result<ReportDto>> GetPeriodReportAsync(DateTime start, DateTime end);
}
