using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WareFlow.Core.Models;
using WareFlow.Core.Enums;
using WareFlow.Infrastructure.Data;
using System.Security.Claims;

namespace WareFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public OrdersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/orders
    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole("Admin");

        var query = _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.User)
            .AsQueryable();

        // Si no es admin, solo ve sus propias órdenes
        if (!isAdmin)
        {
            query = query.Where(o => o.UserId == userId);
        }

        var orders = await query
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new
            {
                o.Id,
                o.OrderNumber,
                o.OrderDate,
                o.Status,
                o.TotalAmount,
                o.Notes,
                User = new
                {
                    o.User.Id,
                    o.User.FirstName,
                    o.User.LastName,
                    o.User.Email
                },
                Items = o.OrderItems.Select(oi => new
                {
                    oi.Id,
                    oi.ProductId,
                    ProductName = oi.Product.Name,
                    oi.Quantity,
                    oi.UnitPrice,
                    oi.Subtotal
                })
            })
            .ToListAsync();

        return Ok(orders);
    }

    // GET: api/orders/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole("Admin");

        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.User)
            .Where(o => o.Id == id)
            .Select(o => new
            {
                o.Id,
                o.OrderNumber,
                o.OrderDate,
                o.Status,
                o.TotalAmount,
                o.Notes,
                User = new
                {
                    o.User.Id,
                    o.User.FirstName,
                    o.User.LastName,
                    o.User.Email
                },
                Items = o.OrderItems.Select(oi => new
                {
                    oi.Id,
                    oi.ProductId,
                    ProductName = oi.Product.Name,
                    ProductSKU = oi.Product.SKU,
                    oi.Quantity,
                    oi.UnitPrice,
                    oi.Subtotal
                })
            })
            .FirstOrDefaultAsync();

        if (order == null)
            return NotFound(new { message = "Orden no encontrada" });

        // Verificar permisos
        if (!isAdmin && order.User.Id != userId)
            return Forbid();

        return Ok(order);
    }

    // POST: api/orders
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto orderDto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Usuario no autenticado" });

            // Validar items
            if (orderDto.Items == null || !orderDto.Items.Any())
                return BadRequest(new { message = "La orden debe tener al menos un item" });

            // Verificar stock y calcular total
            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in orderDto.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null)
                    return BadRequest(new { message = $"Producto con ID {item.ProductId} no encontrado" });

                if (!product.IsActive)
                    return BadRequest(new { message = $"Producto {product.Name} no está activo" });

                if (product.Stock < item.Quantity)
                    return BadRequest(new { message = $"Stock insuficiente para {product.Name}. Disponible: {product.Stock}, Solicitado: {item.Quantity}" });

                // Reservar stock (disminuir)
                product.Stock -= item.Quantity;

                var orderItem = new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                };
                orderItems.Add(orderItem);
                totalAmount += orderItem.Subtotal;

                // Registrar movimiento de inventario
                var movement = new InventoryMovement
                {
                    ProductId = item.ProductId,
                    Type = MovementType.Sale,
                    Quantity = item.Quantity,
                    Reason = $"Venta - Orden #{orderDto.OrderNumber ?? "Pendiente"}",
                    MovementDate = DateTime.UtcNow,
                    UserId = userId
                };
                _context.InventoryMovements.Add(movement);
            }

            // Crear la orden
            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                TotalAmount = totalAmount,
                Notes = orderDto.Notes ?? string.Empty,
                OrderItems = orderItems
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Cargar la orden completa para respuesta
            var createdOrder = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, new
            {
                order.Id,
                order.OrderNumber,
                order.OrderDate,
                order.Status,
                order.TotalAmount,
                order.Notes,
                Items = order.OrderItems.Select(oi => new
                {
                    oi.Id,
                    oi.ProductId,
                    ProductName = oi.Product.Name,
                    oi.Quantity,
                    oi.UnitPrice,
                    oi.Subtotal
                })
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "Error al crear la orden", error = ex.Message });
        }
    }

    // PUT: api/orders/{id}/status
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto statusDto)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound(new { message = "Orden no encontrada" });

        var oldStatus = order.Status;
        var newStatus = statusDto.Status;

        // Validar transiciones de estado permitidas
        if (!IsValidStatusTransition(oldStatus, newStatus))
            return BadRequest(new { message = $"No se puede cambiar el estado de {oldStatus} a {newStatus}" });

        order.Status = newStatus;
        order.Notes = statusDto.Notes ?? order.Notes;

        // Si la orden se cancela, restaurar el stock
        if (newStatus == OrderStatus.Cancelled && oldStatus != OrderStatus.Cancelled)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in order.OrderItems)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.Stock += item.Quantity;
                        
                        var movement = new InventoryMovement
                        {
                            ProductId = item.ProductId,
                            Type = MovementType.Return,
                            Quantity = item.Quantity,
                            Reason = $"Cancelación de orden #{order.OrderNumber}",
                            MovementDate = DateTime.UtcNow,
                            OrderId = order.Id
                        };
                        _context.InventoryMovements.Add(movement);
                    }
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            order.Id,
            order.OrderNumber,
            order.Status,
            order.TotalAmount,
            message = $"Estado de orden actualizado de {oldStatus} a {newStatus}"
        });
    }

    private string GenerateOrderNumber()
    {
        var date = DateTime.UtcNow;
        var count = _context.Orders.Count() + 1;
        return $"ORD-{date:yyyyMMdd}-{count:D4}";
    }

    private bool IsValidStatusTransition(OrderStatus oldStatus, OrderStatus newStatus)
    {
        // Definir transiciones válidas
        var validTransitions = new Dictionary<OrderStatus, List<OrderStatus>>
        {
            { OrderStatus.Pending, new List<OrderStatus> { OrderStatus.Processing, OrderStatus.Cancelled } },
            { OrderStatus.Processing, new List<OrderStatus> { OrderStatus.Shipped, OrderStatus.Cancelled } },
            { OrderStatus.Shipped, new List<OrderStatus> { OrderStatus.Delivered, OrderStatus.Returned } },
            { OrderStatus.Delivered, new List<OrderStatus> { OrderStatus.Returned } },
            { OrderStatus.Cancelled, new List<OrderStatus>() },
            { OrderStatus.Returned, new List<OrderStatus>() }
        };

        return validTransitions.ContainsKey(oldStatus) && validTransitions[oldStatus].Contains(newStatus);
    }
}

public class CreateOrderDto
{
    public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    public string? Notes { get; set; }
    public string? OrderNumber { get; set; }
}

public class OrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class UpdateOrderStatusDto
{
    public OrderStatus Status { get; set; }
    public string? Notes { get; set; }
}
