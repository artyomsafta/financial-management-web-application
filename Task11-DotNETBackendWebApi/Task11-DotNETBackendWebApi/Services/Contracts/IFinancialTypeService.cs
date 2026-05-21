using Task11_DotNETBackendWebApi.Models;
using Task11_DotNETBackendWebApi.Models.DTOs;

namespace Task11_DotNETBackendWebApi.Services.Contracts;

public interface IFinancialTypeService
{
    Task<IEnumerable<FinancialTypeDto>> GetListAsync();
    Task<Result<FinancialTypeDto>> GetByIdAsync(Guid id);
    Task<Result<Guid>> CreateAsync(FinancialTypeRequest request);
    Task<Result> UpdateAsync(Guid id, FinancialTypeRequest request);
    Task<Result> DeleteAsync(Guid id);
}
