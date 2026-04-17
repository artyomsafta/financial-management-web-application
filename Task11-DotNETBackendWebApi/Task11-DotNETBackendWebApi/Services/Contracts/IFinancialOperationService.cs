using Task11_DotNETBackendWebApi.Models;
using Task11_DotNETBackendWebApi.Models.DTOs;

namespace Task11_DotNETBackendWebApi.Services.Contracts;

public interface IFinancialOperationService
{
    Task<IEnumerable<FinancialOperationDto>> GetListAsync();
    Task<Result<FinancialOperationDto>> GetByIdAsync(Guid id);
    Task<Result<FinancialOperationDto>> CreateAsync(FinancialOperationRequest request);
    Task<Result> UpdateAsync(Guid id, FinancialOperationRequest request);
    Task<Result> DeleteAsync(Guid id);
}
