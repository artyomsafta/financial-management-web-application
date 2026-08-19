using Shared.Models.DTOs;
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
}
