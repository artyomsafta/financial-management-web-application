using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Localization;
using MudBlazor.Services;
using System.Globalization;
using Task12_ASPNETCoreBlazorUI.Components;
using Task12_ASPNETCoreBlazorUI.Services;
using Task12_ASPNETCoreBlazorUI.Services.Contracts;
using Task12_ASPNETCoreBlazorUI.Services.MockServices;

namespace Task12_ASPNETCoreBlazorUI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        builder.Services.AddMudServices();

        builder.Services.AddScoped<TokenStore>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IHttpService, HttpService>();

        //=== Real services for actual API calls (uncomment for actual API usage)
        /*
        builder.Services.AddScoped<IFinancialOperationService, FinancialOperationService>();
        builder.Services.AddScoped<IFinancialTypeService, FinancialTypeService>();
        builder.Services.AddScoped<IWalletService, WalletService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IReportService, ReportService>();
        */

        //=== Mock services for testing purposes
        ///*
        builder.Services.AddScoped<IFinancialOperationService, MockFinancialOperationService>();
        builder.Services.AddScoped<IFinancialTypeService, MockFinancialTypeService>();
        builder.Services.AddScoped<IWalletService, MockWalletService>();
        builder.Services.AddScoped<IUserService, MockUserService>();
        builder.Services.AddScoped<IReportService, MockReportService>();
        //*/

        builder.Services.AddHttpClient("Api", client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!);
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        builder.Services.AddAuthorizationCore();
        builder.Services.AddCascadingAuthenticationState();

        builder.Services.AddScoped<CustomAuthenticationStateProvider>();
        builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
            sp.GetRequiredService<CustomAuthenticationStateProvider>());

        builder.Services.AddLocalization();

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        var supportedCultures = new[]
        {
            new CultureInfo("uk-UA"),
            new CultureInfo("uk"),
            new CultureInfo("ru-RU"),
            new CultureInfo("ru"),
            new CultureInfo("en-US"),
            new CultureInfo("en")
        };

        app.UseRequestLocalization(new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture("uk-UA"),
            SupportedCultures = supportedCultures,
            SupportedUICultures = supportedCultures
        });

        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        app.UseHttpsRedirection();

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}
