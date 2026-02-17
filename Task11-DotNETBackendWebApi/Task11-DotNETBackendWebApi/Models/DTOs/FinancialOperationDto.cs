namespace Task11_DotNETBackendWebApi.Models.DTOs;

public class FinancialOperationDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string CurrentCurrency { get; set; } = string.Empty;
    public string TransactionComment { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public Guid TypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public Guid WalletId { get; set; }
    public string WalletName { get; set; } = string.Empty;
}
