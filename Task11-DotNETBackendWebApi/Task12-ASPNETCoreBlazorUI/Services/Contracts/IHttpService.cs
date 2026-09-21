using Task12_ASPNETCoreBlazorUI.Models;

namespace Task12_ASPNETCoreBlazorUI.Services.Contracts;

public interface IHttpService
{
    /// <summary>
    /// Gets a list of items of type T from the specified request URI.
    /// </summary>
    /// <typeparam name="T">The type of items to retrieve.</typeparam>
    /// <param name="requestUri">The URI of the request.</param>
    /// <returns>A list of items of type T.</returns>
    Task<List<T>> GetListAsync<T>(string requestUri);

    /// <summary>
    /// Gets an item of type T from the specified request URI.
    /// </summary>
    /// <typeparam name="T">The type of the item to retrieve.</typeparam>
    /// <param name="requestUri">The URI of the request.</param>
    /// <returns>The item of type T, or null if not found.</returns>
    Task<T?> GetAsync<T>(string requestUri);

    /// <summary>
    /// Sends a POST request to the specified request URI with the provided body of type T.
    /// </summary>
    /// <typeparam name="T">The type of the item to create.</typeparam>
    /// <param name="requestUri">The URI of the request.</param>
    /// <param name="body">The body of the request.</param>
    /// <returns>An ApiResponseDto indicating the result of the operation.</returns>
    Task<ApiResponseDto> PostAsync<T>(string requestUri, T body);

    /// <summary>
    /// Sends a PUT request to the specified request URI with the provided body of type T.
    /// </summary>
    /// <typeparam name="T">The type of the item to update.</typeparam>
    /// <param name="requestUri">The URI of the request.</param>
    /// <param name="body">The body of the request.</param>
    /// <returns>An ApiResponseDto indicating the result of the operation.</returns>
    Task<ApiResponseDto> PutAsync<T>(string requestUri, T body);

    /// <summary>
    /// Sends a DELETE request to the specified request URI.
    /// </summary>
    /// <param name="requestUri">The URI of the request.</param>
    /// <returns>An ApiResponseDto indicating the result of the operation.</returns>
    Task<ApiResponseDto> DeleteAsync(string requestUri);
}
