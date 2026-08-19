using Shared.Models.DTOs;

namespace Task12_ASPNETCoreBlazorUI.Services.Contracts;

public interface IFinancialTypeService
{
    Task<List<FinancialTypeDto>> GetListAsync();
}
