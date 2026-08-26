using Shared.Models;
using Shared.Models.DTOs;

namespace Task11_DotNETBackendWebApi.Services.Contracts;

public interface IFinancialOperationService
{
    Task<IEnumerable<FinancialOperationDto>> GetListAsync();
    Task<FinancialOperationDto> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(CreateFinOperationRequest request);
    Task<bool> UpdateAsync(Guid id, UpdateFinOperationRequest request);
    Task<bool> DeleteAsync(Guid id);
}
