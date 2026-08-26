using Shared.Models;
using Shared.Models.DTOs;

namespace Task11_DotNETBackendWebApi.Services.Contracts;

public interface IFinancialTypeService
{
    Task<IEnumerable<FinancialTypeDto>> GetListAsync();
    Task<FinancialTypeDto> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(FinancialTypeRequest request);
    Task<bool> UpdateAsync(Guid id, FinancialTypeRequest request);
    Task<bool> DeleteAsync(Guid id);
}
