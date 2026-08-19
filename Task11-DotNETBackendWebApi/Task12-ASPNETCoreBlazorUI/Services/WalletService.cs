using Shared.Models.DTOs;
using Task12_ASPNETCoreBlazorUI.Services.Contracts;

namespace Task12_ASPNETCoreBlazorUI.Services;

public class WalletService : IWalletService
{
    private readonly IHttpService _httpService;
    private static readonly string _walletsUri = "Wallets";
    private static readonly string _walletsListUri = "Wallets/list";

    public WalletService(IHttpService httpService)
    {
        _httpService = httpService;
    }

    public async Task<List<WalletDto>> GetListAsync()
    {
        return await _httpService.GetListAsync<WalletDto>($"{_walletsListUri}");
    }
}
