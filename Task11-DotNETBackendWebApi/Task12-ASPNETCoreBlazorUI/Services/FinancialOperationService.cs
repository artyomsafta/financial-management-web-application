using Shared.Models;
using Shared.Models.DTOs;
using Task12_ASPNETCoreBlazorUI.Models;
using Task12_ASPNETCoreBlazorUI.Services.Contracts;

namespace Task12_ASPNETCoreBlazorUI.Services;

public class FinancialOperationService : IFinancialOperationService
{
    private readonly IHttpService _httpService;
    private static readonly string _finOpUri = "FinancialOperations";
    private static readonly string _finOpListUri = "FinancialOperations/list";

    public FinancialOperationService(IHttpService httpService)
    {
        _httpService = httpService;
    }

    public async Task<List<FinancialOperationDto>> GetListAsync()
    {
        return await _httpService.GetListAsync<FinancialOperationDto>($"{_finOpListUri}");
    }

    public async Task<FinancialOperationDto> GetByIdAsync(Guid id)
    {
        return await _httpService.GetAsync<FinancialOperationDto>($"{_finOpUri}/{id}");
    }

    public async Task<ApiResponseDto> CreateAsync(FinancialOperationDto model)
    {
        var createRequest = new CreateFinOperationRequest
        {
            TypeId = model.Type.Id,
            WalletId = model.Wallet.Id,
            Amount = model.Amount,
            Date = model.Date,
            Currency = model.Currency.Code,
            Note = model.Note
        };

        return await _httpService.PostAsync(_finOpUri, createRequest);
    }

    public async Task<ApiResponseDto> UpdateAsync(Guid id, FinancialOperationDto model)
    {
        var updateRequest = new UpdateFinOperationRequest
        {
            TypeId = model.Type.Id,
            Amount = model.Amount,
            Date = model.Date,
            Currency = model.Currency.Code,
            Note = model.Note
        };

       return await _httpService.PutAsync($"{_finOpUri}/{id}", updateRequest);
    }

    public async Task<ApiResponseDto> DeleteAsync(Guid id)
    {
        return await _httpService.DeleteAsync($"{_finOpUri}/{id}");
    }
}
