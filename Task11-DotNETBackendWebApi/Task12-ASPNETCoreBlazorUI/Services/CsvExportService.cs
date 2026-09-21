using Shared.Models.DTOs;
using System.Text;

namespace Task12_ASPNETCoreBlazorUI.Services;

public class CsvExportService
{
    public byte[] GenerateOperationsCsv(IEnumerable<FinancialOperationDto> operations)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Operation date;Wallet;Type;Currency;Amount");    

        foreach (var op in operations)
        {
            var date = op.Date.ToString("yyyy-MM-dd HH:mm:ss");
            var wallet = EscapeCsvField(op.Wallet?.Name);
            var type = EscapeCsvField(op.Type?.Name);
            var currency = EscapeCsvField(op.Currency?.Code);
            var amount = op.Amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

            sb.AppendLine($"{date};{wallet};{type};{currency};{amount}");
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    private string EscapeCsvField(string? field)
    {
        if (string.IsNullOrEmpty(field)) return "";
        if (field.Contains(';') || field.Contains('"') || field.Contains('\n'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return field;
    }
}
