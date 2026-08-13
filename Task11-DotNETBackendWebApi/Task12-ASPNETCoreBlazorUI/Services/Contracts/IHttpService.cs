using Task12_ASPNETCoreBlazorUI.Models;

namespace Task12_ASPNETCoreBlazorUI.Services.Contracts;

public interface IHttpService
{
    Task<List<T>> GetAsync<T>(string requestUri);
    Task<ApiResponseDto> PostAsync<T>(string requestUri, T body, string entityType);
    Task<ApiResponseDto> PutAsync<T>(string requestUri, T body, string entityType);
    Task<ApiResponseDto> DeleteAsync(string requestUri, string entityType);
}
