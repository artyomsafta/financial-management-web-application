using Task11_DotNETBackendWebApi.Models;
using Task11_DotNETBackendWebApi.Models.DTOs;

namespace Task11_DotNETBackendWebApi.Services.Contracts;

public interface IFinancialOperationService
{
    Task<IEnumerable<FinancialOperationDto>> GetAllAsync();
    Task<FinancialOperationDto?> GetByIdAsync(Guid id);
    Task<FinancialOperationDto> CreateAsync(FinancialOperationRequest request);
    Task<bool> UpdateAsync(Guid id, FinancialOperationRequest request);
    Task<bool> SoftDeleteAsync(Guid id);

    Task<ReportDto> GetDailyReportAsync(DateTime date);
    Task<ReportDto> GetPeriodReportAsync(DateTime start, DateTime end);
}
