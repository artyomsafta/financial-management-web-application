using Task11_DotNETBackendWebApi.Helpers.Enums;

namespace Task11_DotNETBackendWebApi.Data.Entities;

public class FinancialOperation
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string CurrentCurrency { get; set; } = nameof(Currencies.UAH);
    public string TransactionComment { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public bool IsDeleted { get; set; } = false;

    public Guid FinancialTypeId { get; set; }
    public FinancialType Type { get; set; }

    public Guid WalletId { get; set; }
    public Wallet Wallet { get; set; }
}
