using Shared.Models.DTOs;
using Task12_ASPNETCoreBlazorUI.Models;

namespace Task12_ASPNETCoreBlazorUI.Services.Contracts;

public interface IWalletService
{
    Task<List<WalletDto>> GetListAsync();
    Task<WalletDto> GetByIdAsync(Guid id);
    Task<ApiResponseDto> CreateAsync(WalletDto model);
    Task<ApiResponseDto> UpdateAsync(Guid id, WalletDto model);
    Task<ApiResponseDto> DeleteAsync(Guid id);
}
