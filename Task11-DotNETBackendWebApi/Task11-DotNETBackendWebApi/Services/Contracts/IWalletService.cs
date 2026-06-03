using Task11_DotNETBackendWebApi.Models;
using Task11_DotNETBackendWebApi.Models.DTOs;

namespace Task11_DotNETBackendWebApi.Services.Contracts;

public interface IWalletService
{
    Task<IEnumerable<WalletDto>> GetListAsync();
    Task<WalletDto> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(CreateWalletRequest request);
    Task<bool> UpdateAsync(Guid id, UpdateWalletRequest request);
    Task<bool> DeleteAsync(Guid id);
}
