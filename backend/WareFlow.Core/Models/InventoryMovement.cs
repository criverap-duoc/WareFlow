using WareFlow.Core.Enums;

namespace WareFlow.Core.Models
{
    public class InventoryMovement
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public MovementType Type { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime MovementDate { get; set; } = DateTime.UtcNow;
        public string? UserId { get; set; }
        public int? OrderId { get; set; }
        
        public Product Product { get; set; } = null!;
        public User? User { get; set; }
        public Order? Order { get; set; }
    }
}
