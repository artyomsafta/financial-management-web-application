using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Task11_DotNETBackendWebApi.Data;
using Task11_DotNETBackendWebApi.Models.DTOs;
using Task11_DotNETBackendWebApi.Services;
using Task11_DotNETBackendWebApi.Services.Contracts;

namespace Task11_DotNETBackendWebApiUnitTests;

[TestClass]
public class OperationCreateMethodUnitTests
{
    private DbContextOptions<AppDbContext> _options;
    private AppDbContext _context;
    private Mock<IUserContext> _userContextMock;
    private Mock<ICurrencyRatesService> _ratesServiceMock;
    private Mock<ILogger<FinancialOperationService>> _loggerMock;
    private FinancialOperationService _operationService;

    [TestInitialize]
    public void Setup()
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(_options);
        _userContextMock = new Mock<IUserContext>();

        _ratesServiceMock = new Mock<ICurrencyRatesService>();
        _ratesServiceMock
            .Setup(s => s.GetRateAsync(It.Is<string>(c => c == "USD"), It.IsAny<DateTime>()))
            .ReturnsAsync(new CurrencyRateResult
            {
                PurchaseRate = 42.70m,
                SaleRate = 43.30m
            });

        _loggerMock = new Mock<ILogger<FinancialOperationService>>();
        _operationService = new FinancialOperationService(_context, _userContextMock.Object, _ratesServiceMock.Object, _loggerMock.Object);

        this.SeedMockDb();
    }

    private void SeedMockDb()
    {
        using (var context = new AppDbContext(_options))
        {
        }
    }

    [TestMethod]
    public void TestMethod1()
    {
    }
}
