using Microsoft.EntityFrameworkCore;
using WareFlow.Core.Enums;
using WareFlow.Infrastructure.Data;

namespace WareFlow.API.Services;

public class DemandForecastService
{
    private readonly ApplicationDbContext _context;

    public DemandForecastService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DemandForecast?> ForecastAsync(int productId)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null) return null;

        // Ventas de los últimos 30 días
        var since = DateTime.UtcNow.AddDays(-30);
        var sales = await _context.InventoryMovements
            .Where(m => m.ProductId == productId
                     && m.Type == MovementType.Sale
                     && m.MovementDate >= since)
            .ToListAsync();

        // Si no hay ventas, no se puede predecir
        if (!sales.Any())
        {
            return new DemandForecast
            {
                ProductId = productId,
                ProductName = product.Name,
                CurrentStock = product.Stock,
                MinimumStock = product.MinimumStock,
                AverageDailySales = 0,
                DaysUntilMinimum = null,
                RecommendedReorderDate = null,
                RecommendedReorderQuantity = 0,
                Confidence = 0,
                Message = "Sin datos de ventas en los últimos 30 días"
            };
        }

        // Promedio diario
        var totalSold = sales.Sum(s => s.Quantity);
        var averageDailySales = (decimal)totalSold / 30m;

        // Días hasta llegar al mínimo
        var stockAvailable = product.Stock - product.MinimumStock;
        int? daysUntilMinimum = null;
        DateTime? reorderDate = null;

        if (averageDailySales > 0 && stockAvailable > 0)
        {
            var days = (int)Math.Floor(stockAvailable / averageDailySales);
            daysUntilMinimum = days;
            reorderDate = DateTime.UtcNow.AddDays(days);
        }
        else if (stockAvailable <= 0)
        {
            daysUntilMinimum = 0;
            reorderDate = DateTime.UtcNow;
        }

        // Cantidad recomendada: cubrir 30 días de ventas
        var recommendedQuantity = (int)Math.Ceiling(averageDailySales * 30);

        // Confianza: basada en cantidad de movimientos
        // 0 movimientos = 0, 30+ movimientos = 1
        var confidence = Math.Min(1.0m, sales.Count / 30m);

        return new DemandForecast
        {
            ProductId = productId,
            ProductName = product.Name,
            CurrentStock = product.Stock,
            MinimumStock = product.MinimumStock,
            AverageDailySales = Math.Round(averageDailySales, 2),
            DaysUntilMinimum = daysUntilMinimum,
            RecommendedReorderDate = reorderDate,
            RecommendedReorderQuantity = recommendedQuantity,
            Confidence = Math.Round(confidence, 2),
            Message = null
        };
    }
}

public class DemandForecast
{
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public int CurrentStock { get; set; }
    public int MinimumStock { get; set; }
    public decimal AverageDailySales { get; set; }
    public int? DaysUntilMinimum { get; set; }
    public DateTime? RecommendedReorderDate { get; set; }
    public int RecommendedReorderQuantity { get; set; }
    public decimal Confidence { get; set; }
    public string? Message { get; set; }
}
