using Serilog.Context;
using System.Text.RegularExpressions;

namespace Task11_DotNETBackendWebApi.Infrastructure.Logging;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = Guid.NewGuid().ToString();

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            var requestBody = await ReadRequestBody(context.Request);
            _logger.LogInformation("HTTP Request: {Method} {Url} | Body: {Body}",
                context.Request.Method, context.Request.Path, requestBody);

            var originalBodyStream = context.Response.Body;
            using var responseBodyMemoryStream = new MemoryStream();
            context.Response.Body = responseBodyMemoryStream;

            await _next(context);

            if (context.Response.StatusCode >= 400)
            {
                var responseBody = await ReadResponseBody(context.Response);
                _logger.LogWarning("HTTP Error Response: {StatusCode} | Body: {Body}",
                    context.Response.StatusCode, responseBody);
            }
            else
            {
                _logger.LogInformation("HTTP Response: {StatusCode}", context.Response.StatusCode);
            }

            await responseBodyMemoryStream.CopyToAsync(originalBodyStream);
        }
    }

    private async Task<string> ReadRequestBody(HttpRequest request)
    {
        request.EnableBuffering();
        using var reader = new StreamReader(request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;
        return MaskSensitiveData(body);
    }

    private async Task<string> ReadResponseBody(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(response.Body).ReadToEndAsync();
        response.Body.Seek(0, SeekOrigin.Begin);
        return MaskSensitiveData(body);
    }

    private string MaskSensitiveData(string body)
    {
        if (string.IsNullOrEmpty(body))
        {
            return body;
        }

        var maskedBody = body;

        maskedBody = Regex.Replace(maskedBody, @"(""password""\s*:\s*"")([^""]+)("")", "$1***$3", RegexOptions.IgnoreCase);
        maskedBody = Regex.Replace(maskedBody, @"(""token""\s*:\s*"")([^""]+)("")", "$1***$3", RegexOptions.IgnoreCase);

        return maskedBody;
    }
}
