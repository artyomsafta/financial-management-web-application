using Shared.Models.DTOs;
using Task12_ASPNETCoreBlazorUI.Services.Contracts;

namespace Task12_ASPNETCoreBlazorUI.Services;

public class DtoReadService
{
    private static readonly string _baseUri = "api/v1/";
    private static readonly string _financialOperationsUri = "FinancialOperations";
    private static readonly string _finOperationsListUri = "FinancialOperations/list";
    private static readonly string _finTypesListUri = "FinancialTypes/list";
    private static readonly string _usersListUri = "Users/list";
    private static readonly string _walletsListUri = "Wallets/list";

    private readonly IHttpService _httpService;

    public DtoReadService(IHttpService httpService)
    {
        _httpService = httpService;
    }

    public async Task<FinancialOperationDto> GetFinancialOperationAsync(Guid id)
    {
        return await _httpService.GetByIdAsync<FinancialOperationDto>($"{_baseUri}{_financialOperationsUri}", id);
    }

    public async Task<List<FinancialOperationDto>> GetFinancialOperationsListAsync()
    {
        return await _httpService.GetListAsync<FinancialOperationDto>($"{_baseUri}{_finOperationsListUri}");
    }

    public async Task<List<FinancialTypeDto>> GetFinancialTypesListAsync()
    {
        return await _httpService.GetListAsync<FinancialTypeDto>($"{_baseUri}{_finTypesListUri}");
    }

    public async Task<List<UserDto>> GetUsersListAsync()
    {
        return await _httpService.GetListAsync<UserDto>($"{_baseUri}{_usersListUri}");
    }

    public async Task<List<WalletDto>> GetWalletsListAsync()
    {
        return await _httpService.GetListAsync<WalletDto>($"{_baseUri}{_walletsListUri}");
    }
}
