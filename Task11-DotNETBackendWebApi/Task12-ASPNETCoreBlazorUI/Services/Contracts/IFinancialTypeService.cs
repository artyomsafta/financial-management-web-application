using Shared.Models.DTOs;
using Task12_ASPNETCoreBlazorUI.Models;

namespace Task12_ASPNETCoreBlazorUI.Services.Contracts;

public interface IFinancialTypeService
{
    Task<List<FinancialTypeDto>> GetListAsync();
    Task<FinancialTypeDto> GetByIdAsync(Guid id);
    Task<ApiResponseDto> CreateAsync(FinancialTypeDto model);
    Task<ApiResponseDto> UpdateAsync(Guid id, FinancialTypeDto model);
    Task<ApiResponseDto> DeleteAsync(Guid id);
}
