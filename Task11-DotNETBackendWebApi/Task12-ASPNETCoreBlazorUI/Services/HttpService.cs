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

    public async Task<List<T>> GetAsync<T>(string requestUri)
    {
        var client = await _authService.CreateAuthenticatedClientAsync();
        return await client.GetFromJsonAsync<List<T>>(requestUri) ?? new List<T>();
    }

    public Task<ApiResponseDto> PostAsync<T>(string requestUri, T body, string entityType)
    {
        return ExecuteHttpRequestAsync(
            client => client.PostAsJsonAsync(requestUri, body),
            $"{entityType} created successfully!"
        );
    }

    public Task<ApiResponseDto> PutAsync<T>(string requestUri, T body, string entityType)
    {
        return ExecuteHttpRequestAsync(
            client => client.PutAsJsonAsync(requestUri, body),
            $"{entityType} updated successfully!"
        );
    }

    public Task<ApiResponseDto> DeleteAsync(string requestUri, string entityType)
    {
        return ExecuteHttpRequestAsync(
            client => client.DeleteAsync(requestUri),
            $"{entityType} deleted successfully!"
        );
    }

    private async Task<ApiResponseDto> ExecuteHttpRequestAsync(
        Func<HttpClient, Task<HttpResponseMessage>> httpRequest,
        string successMessage
    )
    {
        var client = await _authService.CreateAuthenticatedClientAsync();
        var response = await httpRequest(client);

        if (response.IsSuccessStatusCode)
        {
            return new ApiResponseDto
            {
                IsSuccess = true,
                Message = successMessage
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
