using Shared.Models;
using Shared.Models.DTOs;
using Task12_ASPNETCoreBlazorUI.Models;
using Task12_ASPNETCoreBlazorUI.Services.Contracts;

namespace Task12_ASPNETCoreBlazorUI.Services;

public class FinancialTypeService : IFinancialTypeService
{
    private readonly IHttpService _httpService;
    private static readonly string _finTypesUri = "FinancialTypes";
    private static readonly string _finTypesListUri = "FinancialTypes/list";

    public FinancialTypeService(IHttpService httpService)
    {
        _httpService = httpService;
    }

    public async Task<List<FinancialTypeDto>> GetListAsync()
    {
        return await _httpService.GetListAsync<FinancialTypeDto>($"{_finTypesListUri}");
    }

    public async Task<FinancialTypeDto> GetByIdAsync(Guid id)
    {
        return await _httpService.GetByIdAsync<FinancialTypeDto>($"{_finTypesUri}", id);
    }

    public async Task<ApiResponseDto> CreateAsync(FinancialTypeDto model)
    {
        var createRequest = new FinancialTypeRequest
        {
            Name = model.Name,
            Description = model.Description,
            IsIncome = model.IsIncome
        };

        return await _httpService.PostAsync(_finTypesUri, createRequest);
    }

    public async Task<ApiResponseDto> UpdateAsync(Guid id, FinancialTypeDto model)
    {
        var updateRequest = new FinancialTypeRequest
        {
            Name = model.Name,
            Description = model.Description,
            IsIncome = model.IsIncome
        };

        return await _httpService.PutAsync($"{_finTypesUri}/{id}", updateRequest);
    }

    public async Task<ApiResponseDto> DeleteAsync(Guid id)
    {
        return await _httpService.DeleteAsync($"{_finTypesUri}/{id}");
    }
}
