using WareFlow.Core.Enums;

namespace WareFlow.Core.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int MinimumStock { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }

        // Vector semántico del producto (768 floats de gemini-embedding-2)
        // serializado como JSON. Con 66 productos no hace falta una vector DB:
        // la similitud coseno se calcula en memoria.
        public string? Embedding { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();
    }
}
