using Bogus;
using Shared.Models;
using Shared.Models.DTOs;
using Task12_ASPNETCoreBlazorUI.Models;
using Task12_ASPNETCoreBlazorUI.Services.Contracts;

namespace Task12_ASPNETCoreBlazorUI.Services.MockServices;

public class MockWalletService : IWalletService
{
    private readonly List<WalletDto> _wallets;

    public MockWalletService()
    {
        var currencies = new[]
{
            new CurrencyListDto { Id = 0, Code = "USD", },
            new CurrencyListDto { Id = 1, Code = "EUR", },
            new CurrencyListDto { Id = 2, Code = "GBP", },
            new CurrencyListDto { Id = 3, Code = "JPY", },
            new CurrencyListDto { Id = 4, Code = "AUD", },
            new CurrencyListDto { Id = 5, Code = "CAD", },
        };

        var userFaker = new Faker<UserDto>("en")
            .RuleFor(u => u.Id, f => Guid.NewGuid())
            .RuleFor(u => u.Username, f => f.Name.FirstName())
            .RuleFor(u => u.Role, f => f.PickRandom<UserRoles>());

        var walletFaker = new Faker<WalletDto>("en")
            .RuleFor(w => w.Id, f => Guid.NewGuid())
            .RuleFor(w => w.BaseCurrency, f => f.PickRandom(currencies))
            .RuleFor(w => w.User, f => userFaker.Generate())
            .RuleFor(w => w.Name, (f, w) => $"{w.User.Username}\'s wallet");

        _wallets = walletFaker.Generate(60);
    }

    public Task<List<WalletDto>> GetListAsync()
    {
        return Task.FromResult(_wallets
            .OrderBy(w => w.User.Username)
            .ToList());
    }

    public Task<WalletDto> GetByIdAsync(Guid id)
    {
        var wallet = _wallets.FirstOrDefault(w => w.Id == id);
        return Task.FromResult(wallet ?? new WalletDto());
    }

    public Task<ApiResponseDto> CreateAsync(WalletDto model)
    {
        var newWallet = new WalletDto
        {
            Id = Guid.NewGuid(),
            Name = model.Name,
            BaseCurrency = new CurrencyListDto()
            {
                Id = 0,
                Code = model.BaseCurrency.Code
            },
            User = new UserDto()
            {
                Id = Guid.NewGuid(),
                Username = model.User.Username,
                Role = UserRoles.User
            } 
        };
        _wallets.Add(newWallet);

        return Task.FromResult(new ApiResponseDto
        {
            IsSuccess = true,
            Message = "Wallet successfully created (Mock)"
        });
    }

    public Task<ApiResponseDto> UpdateAsync(Guid id, WalletDto model)
    {
        var existingWallet = _wallets.FirstOrDefault(w => w.Id == id);
        if (existingWallet != null)
        {
            existingWallet.Name = model.Name;
            existingWallet.BaseCurrency = model.BaseCurrency;
        }

        return Task.FromResult(new ApiResponseDto
        {
            IsSuccess = true,
            Message = "Wallet successfully updated (Mock)"
        });
    }

    public Task<ApiResponseDto> DeleteAsync(Guid id)
    {
        var wallet = _wallets.FirstOrDefault(w => w.Id == id);
        if (wallet != null)
        {
            _wallets.Remove(wallet);
        }

        return Task.FromResult(new ApiResponseDto
        {
            IsSuccess = true,
            Message = "Wallet successfully deleted (Mock)"
        });
    }
}
