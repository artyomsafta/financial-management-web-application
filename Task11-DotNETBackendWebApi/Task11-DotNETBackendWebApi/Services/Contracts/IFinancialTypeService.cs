using Task11_DotNETBackendWebApi.Models;
using Task11_DotNETBackendWebApi.Models.DTOs;

namespace Task11_DotNETBackendWebApi.Services.Contracts;

public interface IFinancialTypeService
{
    Task<IEnumerable<FinancialTypeDto>> GetAllAsync();
    Task<FinancialTypeDto?> GetByIdAsync(Guid id);
    Task<FinancialTypeDto> CreateAsync(FinancialTypeRequest request);
    Task<bool> UpdateAsync(Guid id, FinancialTypeRequest request);
    Task<bool> SoftDeleteAsync(Guid id);
}
