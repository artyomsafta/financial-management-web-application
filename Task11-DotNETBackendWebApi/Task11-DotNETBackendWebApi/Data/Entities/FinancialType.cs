namespace Task11_DotNETBackendWebApi.Data.Entities;

public class FinancialType
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public List<FinancialOperation> FinancialOperations { get; set; } = new();
}
