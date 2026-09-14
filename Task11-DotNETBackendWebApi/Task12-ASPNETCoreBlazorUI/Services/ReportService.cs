using Shared.Models.DTOs;
using Task12_ASPNETCoreBlazorUI.Services.Contracts;

namespace Task12_ASPNETCoreBlazorUI.Services;

public class ReportService : IReportService
{
    private readonly IAuthService _authService;
    private static readonly string _dailyReportsUri = "Reports/daily";
    private static readonly string _periodReportsUri = "Reports/period";

    public ReportService(IAuthService authService)
    {
        _authService = authService;
    }

    public Task<ReportDto> GetDailyReportAsync(DateTime date)
    {
        var uri = $"{_dailyReportsUri}?date={DateToString(date)}";
        return GetReportAsync(uri);
    }

    public Task<ReportDto> GetPeriodReportAsync(DateTime startDate, DateTime endDate)
    {
        var uri = $"{_periodReportsUri}?startDate={DateToString(startDate)}&endDate={DateToString(endDate)}";
        return GetReportAsync(uri);
    }

    private async Task<ReportDto> GetReportAsync(string uri)
    {
        var client = await _authService.CreateAuthenticatedClientAsync();
        var response = await client.GetAsync(uri);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<ReportDto>();
        }
        else
        {
            var errorText = await response.Content.ReadAsStringAsync();
            throw new Exception(!string.IsNullOrWhiteSpace(errorText) ? errorText : "Server error");
        }
    }

    private static string DateToString(DateTime date)
    {
        return date.ToString("yyyy-MM-dd");
    }
}
