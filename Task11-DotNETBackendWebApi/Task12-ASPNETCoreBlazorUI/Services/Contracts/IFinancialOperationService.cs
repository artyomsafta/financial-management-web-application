using Shared.Models;
using Shared.Models.DTOs;
using Task12_ASPNETCoreBlazorUI.Models;

namespace Task12_ASPNETCoreBlazorUI.Services.Contracts;

public interface IFinancialOperationService
{
    Task<List<FinancialOperationDto>> GetListAsync();
    Task<FinancialOperationDto> GetByIdAsync(Guid id);
    Task<ApiResponseDto> CreateAsync(FinancialOperationDto model);
    Task<ApiResponseDto> UpdateAsync(Guid id, FinancialOperationDto model);
    Task<ApiResponseDto> DeleteAsync(Guid id);
}
