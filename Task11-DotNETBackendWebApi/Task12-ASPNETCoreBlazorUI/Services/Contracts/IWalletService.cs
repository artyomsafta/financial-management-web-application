using Shared.Models.DTOs;

namespace Task12_ASPNETCoreBlazorUI.Services.Contracts;

public interface IWalletService
{
    Task<List<WalletDto>> GetListAsync();
}
