using Microsoft.EntityFrameworkCore;
using WareFlow.Core.Models;
using WareFlow.Infrastructure.Data;

namespace WareFlow.API.Services;

/// <summary>
/// Fase IA.2: detección de anomalías en órdenes.
/// Calcula un score 0-100 comparando la orden contra el historial (Z-score de
/// monto y cantidad) y cae a heurísticas básicas cuando hay menos de 10 órdenes.
/// Randomized PCA (ML.NET) queda como TODO: aún no se invoca el modelo.
/// </summary>
public class AnomalyDetectionService
{
    private readonly ApplicationDbContext _context;

    public AnomalyDetectionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(decimal score, string reason)> CalculateAsync(Order order)
    {
        // Cargar historial de ordenes (ultimas 100, excluyendo esta)
        var history = await _context.Orders
            .Where(o => o.Id != order.Id)
            .OrderByDescending(o => o.OrderDate)
            .Take(100)
            .Select(o => new
            {
                o.TotalAmount,
                ItemCount = o.OrderItems.Count,
                TotalQuantity = o.OrderItems.Sum(oi => oi.Quantity),
                o.OrderDate
            })
            .ToListAsync();

        // Si hay pocas ordenes, usar heuristicas basicas
        if (history.Count < 10)
        {
            return (CalculateHeuristic(order), "Heuristico (pocas ordenes historicas)");
        }

        // Calcular estadisticas del historial
        var avgTotal = (decimal)history.Average(h => (double)h.TotalAmount);
        var stdTotal = (decimal)Math.Sqrt(history.Average(h => Math.Pow((double)h.TotalAmount - (double)avgTotal, 2)));
        var avgQuantity = (double)history.Average(h => h.TotalQuantity);
        var stdQuantity = Math.Sqrt(history.Average(h => Math.Pow(h.TotalQuantity - avgQuantity, 2)));

        // Z-scores para esta orden
        var currentTotal = order.TotalAmount;
        var currentQuantity = order.OrderItems.Sum(oi => oi.Quantity);

        var zTotal = stdTotal > 0 ? (double)(currentTotal - avgTotal) / (double)stdTotal : 0;
        var zQuantity = stdQuantity > 0 ? (currentQuantity - avgQuantity) / stdQuantity : 0;

        // Score basado en los z-scores (max 100)
        var maxZ = Math.Max(Math.Abs(zTotal), Math.Abs(zQuantity));
        var score = Math.Min(100, maxZ * 25);  // z=1 -> 25, z=2 -> 50, z=4 -> 100

        // Razon
        var reasons = new List<string>();
        if (Math.Abs(zTotal) > 1.5) reasons.Add($"monto {(zTotal > 0 ? "alto" : "bajo")} (z={zTotal:F1})");
        if (Math.Abs(zQuantity) > 1.5) reasons.Add($"cantidad {(zQuantity > 0 ? "alta" : "baja")} (z={zQuantity:F1})");
        if (order.OrderDate.Hour < 6 || order.OrderDate.Hour > 22) reasons.Add("fuera de horario");

        var reason = reasons.Any() ? string.Join(", ", reasons) : "Normal";

        return ((decimal)Math.Round(score, 2), reason);
    }

    private decimal CalculateHeuristic(Order order)
    {
        var score = 0m;
        var quantity = order.OrderItems.Sum(oi => oi.Quantity);

        if (order.TotalAmount > 1000000) score += 30;
        if (order.TotalAmount > 5000000) score += 30;
        if (quantity > 10) score += 20;
        if (quantity > 50) score += 20;

        return Math.Min(100, score);
    }
}
