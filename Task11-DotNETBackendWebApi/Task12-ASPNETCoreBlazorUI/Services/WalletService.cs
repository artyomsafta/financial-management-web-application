using Shared.Models;
using Shared.Models.DTOs;
using Task12_ASPNETCoreBlazorUI.Models;
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

    public async Task<WalletDto> GetByIdAsync(Guid id)
    {
        return await _httpService.GetByIdAsync<WalletDto>($"{_walletsUri}", id);
    }

    public async Task<ApiResponseDto> CreateAsync(WalletDto model)
    {
        var createRequest = new CreateWalletRequest
        {
            UserId = model.User.Id,
            Name = model.Name,
            BaseCurrency = model.BaseCurrency.Code
        };

        return await _httpService.PostAsync(_walletsUri, createRequest);
    }

    public async Task<ApiResponseDto> UpdateAsync(Guid id, WalletDto model)
    {
        var updateRequest = new UpdateWalletRequest
        {
            Name = model.Name,
            BaseCurrency = model.BaseCurrency.Code
        };

        return await _httpService.PutAsync($"{_walletsUri}/{id}", updateRequest);
    }

    public async Task<ApiResponseDto> DeleteAsync(Guid id)
    {
        return await _httpService.DeleteAsync($"{_walletsUri}/{id}");
    }
}
