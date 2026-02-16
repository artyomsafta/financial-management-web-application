using Task11_DotNETBackendWebApi.Helpers.Enums;

namespace Task11_DotNETBackendWebApi.Data.Entities;

public class Wallet
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Balance { get; set; } = 0m;
    public string BaseCurrency { get; set; } = nameof(Currencies.UAH);
    public bool IsDeleted { get; set; } = false;

    public Guid UserId { get; set; }
    public User User { get; set; }

    public List<FinancialOperation> FinancialOperations { get; set; } = new();
}
