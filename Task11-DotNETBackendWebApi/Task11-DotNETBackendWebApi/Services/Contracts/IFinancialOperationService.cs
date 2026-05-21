using Task11_DotNETBackendWebApi.Models;
using Task11_DotNETBackendWebApi.Models.DTOs;

namespace Task11_DotNETBackendWebApi.Services.Contracts;

public interface IFinancialOperationService
{
    Task<IEnumerable<FinancialOperationDto>> GetListAsync();
    Task<Result<FinancialOperationDto>> GetByIdAsync(Guid id);
    Task<Result<Guid>> CreateAsync(CreateFinOperationRequest request);
    Task<Result> UpdateAsync(Guid id, UpdateFinOperationRequest request);
    Task<Result> DeleteAsync(Guid id);
}
