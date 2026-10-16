using WareFlow.Core.Enums;

namespace WareFlow.Core.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public OrderStatus Status { get; set; }
        // Los precios del catálogo incluyen IVA (19%): TotalAmount es el total con
        // IVA; NetAmount y TaxAmount son su desglose tributario (DTE).
        public decimal TotalAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public string Notes { get; set; } = string.Empty;
        
        public User User { get; set; } = null!;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
