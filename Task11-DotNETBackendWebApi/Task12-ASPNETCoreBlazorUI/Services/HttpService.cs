using Task12_ASPNETCoreBlazorUI.Services.Contracts;
using Task12_ASPNETCoreBlazorUI.Models;

namespace Task12_ASPNETCoreBlazorUI.Services;

public class HttpService : IHttpService
{
    private readonly IAuthService _authService;

    public HttpService(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<List<T>> GetListAsync<T>(string requestUri)
    {
        var client = await _authService.CreateAuthenticatedClientAsync();
        return await client.GetFromJsonAsync<List<T>>(requestUri) ?? new List<T>();
    }

    public async Task<T?> GetByIdAsync<T>(string requestUri, Guid id)
    {
        var client = await _authService.CreateAuthenticatedClientAsync();
        return await client.GetFromJsonAsync<T>($"{requestUri}/{id}");
    }

    public Task<ApiResponseDto> PostAsync<T>(string requestUri, T body)
    {
        return ExecuteHttpRequestAsync(client => client.PostAsJsonAsync(requestUri, body));
    }

    public Task<ApiResponseDto> PutAsync<T>(string requestUri, T body)
    {
        return ExecuteHttpRequestAsync(client => client.PutAsJsonAsync(requestUri, body));
    }

    public Task<ApiResponseDto> DeleteAsync(string requestUri)
    {
        return ExecuteHttpRequestAsync(client => client.DeleteAsync(requestUri));
    }

    private async Task<ApiResponseDto> ExecuteHttpRequestAsync(
        Func<HttpClient, Task<HttpResponseMessage>> httpRequest
    )
    {
        var client = await _authService.CreateAuthenticatedClientAsync();
        var response = await httpRequest(client);

        if (response.IsSuccessStatusCode)
        {
            return new ApiResponseDto
            {
                IsSuccess = true,
                Message = "Operation completed successfully!"
            };
        }
        else
        {
            var errorText = await response.Content.ReadAsStringAsync();

            return new ApiResponseDto
            {
                IsSuccess = false,
                Message = !string.IsNullOrWhiteSpace(errorText) ? errorText : "Server error"
            };
        }
    }
}
