using Microsoft.EntityFrameworkCore;
using Task11_DotNETBackendWebApi.Data;
using Task11_DotNETBackendWebApi.Models.DTOs;
using Task11_DotNETBackendWebApi.Services.Contracts;

namespace Task11_DotNETBackendWebApi.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _context;
    private readonly IUserContext _userContext;
    private readonly ILogger<ReportService> _logger;

    public ReportService(AppDbContext context, IUserContext userContext, ILogger<ReportService> logger)
    {
        _context = context;
        _userContext = userContext;
        _logger = logger;
    }

    public async Task<ReportDto> GetDailyReportAsync(DateTime date)
    {
        return await GetPeriodReportAsync(date, date);
    }

    public async Task<ReportDto> GetPeriodReportAsync(DateTime start, DateTime end)
    {
        if (start > end)
        {
            throw new ArgumentException("The start date must be earlier or equal to the end date.");
        }

        var startDate = start.Date;
        var endDate = end.Date.AddDays(1).AddTicks(-1);

        var query = _context.FinancialOperations.AsQueryable();

        if (!_userContext.IsAdmin)
        {
            query = query.Where(w => w.Wallet.UserId == _userContext.UserId);
        }

        var operations = await query
                .AsNoTracking()
                .Include(o => o.Type)
                .Include(o => o.Wallet)
                .Where(o => o.Date >= startDate && o.Date <= endDate)
                .ToListAsync();

        var totalIncome = operations
                .Where(o => o.Type.IsIncome is true)
                .Sum(o => o.Amount);

        var totalExpenses = operations
                .Where(o => o.Type.IsIncome is false)
                .Sum(o => o.Amount);

        return new ReportDto
        {
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            NetResult = totalIncome - totalExpenses,
            Operations = operations
                .Select(o => new FinancialOperationDto
                {
                    Id = o.Id,
                    Amount = o.Amount,
                    Date = o.Date,
                    Currency = o.Currency,
                    Comment = o.Comment,
                    Note = o.Note,
                    TypeId = o.FinancialTypeId,
                    TypeName = o.Type.Name,
                    WalletId = o.WalletId,
                    WalletName = o.Wallet.Name
                })
                .OrderByDescending(o => o.Date)
                .ToList()
        };
    }
}
