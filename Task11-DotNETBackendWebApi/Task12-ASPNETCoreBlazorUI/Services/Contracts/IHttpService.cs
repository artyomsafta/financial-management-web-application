using Task12_ASPNETCoreBlazorUI.Models;

namespace Task12_ASPNETCoreBlazorUI.Services.Contracts;

public interface IHttpService
{
    Task<List<T>> GetListAsync<T>(string requestUri);
    Task<T?> GetByIdAsync<T>(string requestUri, Guid id);
    Task<ApiResponseDto> PostAsync<T>(string requestUri, T body);
    Task<ApiResponseDto> PutAsync<T>(string requestUri, T body);
    Task<ApiResponseDto> DeleteAsync(string requestUri);
}
