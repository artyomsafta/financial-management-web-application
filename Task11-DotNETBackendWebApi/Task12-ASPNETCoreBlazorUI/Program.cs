using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using Task12_ASPNETCoreBlazorUI.Components;
using Task12_ASPNETCoreBlazorUI.Services;
using Task12_ASPNETCoreBlazorUI.Services.Contracts;

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

        builder.Services.AddScoped<IFinancialOperationService, FinancialOperationService>();
        builder.Services.AddScoped<IFinancialTypeService, FinancialTypeService>();
        builder.Services.AddScoped<IWalletService, WalletService>();
        builder.Services.AddScoped<IUserService, UserService>();

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

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        app.UseHttpsRedirection();

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}
