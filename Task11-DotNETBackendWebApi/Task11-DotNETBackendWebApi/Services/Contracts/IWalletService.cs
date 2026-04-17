using Task11_DotNETBackendWebApi.Models;
using Task11_DotNETBackendWebApi.Models.DTOs;

namespace Task11_DotNETBackendWebApi.Services.Contracts;

public interface IWalletService
{
    Task<IEnumerable<WalletDto>> GetListAsync();
    Task<Result<WalletDto>> GetByIdAsync(Guid id);
    Task<Result<WalletDto>> CreateAsync(WalletRequest request);
    Task<Result> UpdateAsync(Guid id, WalletRequest request);
    Task<Result> DeleteAsync(Guid id);
}
