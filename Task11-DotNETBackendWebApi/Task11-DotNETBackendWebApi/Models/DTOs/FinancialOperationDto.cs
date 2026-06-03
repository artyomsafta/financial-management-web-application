namespace Task11_DotNETBackendWebApi.Models.DTOs;

public class FinancialOperationDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public CurrencyListDto Currency { get; set; }
    public string Comment { get; set; } = "";
    public string Note { get; set; } = "";
    public FinancialTypeListDto Type { get; set; }
    public WalletListDto Wallet { get; set; }
}
