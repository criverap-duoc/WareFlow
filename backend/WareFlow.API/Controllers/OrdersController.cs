using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WareFlow.Core.Models;
using WareFlow.Core.Enums;
using WareFlow.Core.DTOs;
using WareFlow.Infrastructure.Data;
using System.Security.Claims;

namespace WareFlow.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public OrdersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/orders
    // Paginación server-side: devuelve { items, page, pageSize, totalItems, totalPages, hasNext, hasPrevious }.
    [HttpGet]
    public async Task<IActionResult> GetOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        // Validación de paginación
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // Fase B.1: Admin y Bodeguero ven todas las órdenes; Vendedor solo las suyas.
        var canSeeAllOrders = User.IsInRole("Admin") || User.IsInRole("Bodeguero");

        var query = _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.User)
            .AsQueryable();

        if (!canSeeAllOrders)
        {
            query = query.Where(o => o.UserId == userId);
        }

        query = query.OrderByDescending(o => o.OrderDate);

        // Contar el total antes de paginar
        var totalItems = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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

        return Ok(new PaginatedResponse<object>(items, page, pageSize, totalItems));
    }

    // GET: api/orders/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // Fase B.1: Admin y Bodeguero ven cualquier orden; Vendedor solo la suya.
        var canSeeAllOrders = User.IsInRole("Admin") || User.IsInRole("Bodeguero");

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
        if (!canSeeAllOrders && order.User.Id != userId)
            return Forbid();

        return Ok(order);
    }

    // El IVA vigente en Chile es 19% y los precios del catálogo vienen con IVA
    // incluido, por lo que basta el factor para derivar el neto del total.
    private const decimal IvaFactor = 1.19m;
    private const int IvaTasa = 19;

    // GET: api/orders/{id}/dte
    // Simulación de un DTE (Documento Tributario Electrónico; tipo 33 = factura
    // afecta a IVA) con los campos mínimos del SII. No se firma ni se envía al
    // SII: es la representación tributaria de la orden para el portafolio.
    [HttpGet("{id}/dte")]
    public async Task<IActionResult> GetDte(int id)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound(new { message = "Orden no encontrada" });

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // Mismo criterio de visibilidad que GetOrder: Admin y Bodeguero ven todas.
        var canSeeAllOrders = User.IsInRole("Admin") || User.IsInRole("Bodeguero");
        if (!canSeeAllOrders && order.UserId != userId)
            return Forbid();

        var dte = new
        {
            tipoDte = 33,
            folio = order.OrderNumber,
            fechaEmision = TimeZoneInfo.ConvertTimeFromUtc(order.OrderDate, ChileTimeZone)
                .ToString("yyyy-MM-dd"),
            emisor = new
            {
                rut = "76.543.210-K",
                razonSocial = "WareFlow SpA",
                giro = "Venta de articulos de tecnologia",
                direccion = "Av. Providencia 1234, Santiago"
            },
            receptor = new
            {
                rut = "12.345.678-9",
                razonSocial = $"{order.User.FirstName} {order.User.LastName}",
                direccion = "Sin direccion registrada"
            },
            detalles = order.OrderItems.Select(oi => new
            {
                nombre = oi.Product.Name,
                cantidad = oi.Quantity,
                precioUnitario = oi.UnitPrice,
                montoItem = oi.Subtotal
            }),
            montoNeto = order.NetAmount,
            iva = order.TaxAmount,
            montoTotal = order.TotalAmount,
            tasaImpuesto = IvaTasa
        };

        return Ok(dte);
    }

    // POST: api/orders
    [HttpPost]
    [Authorize(Roles = "Admin,Vendedor")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto orderDto)
    {
        // Dos SaveChangesAsync: el primero persiste la orden (items + stock) y el
        // segundo los movimientos, que necesitan el Id recién generado. Sin
        // transacción explícita: cada SaveChangesAsync ya es atómico (EF Core lo
        // envuelve en una transacción implícita) y el proveedor InMemory que usan
        // los tests de integración no soporta transacciones.
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
            }

            // Los precios del catálogo incluyen IVA: el total de los items ya es el
            // monto con IVA, así que el neto se deriva dividiendo por el factor.
            var totalWithIva = totalAmount;
            var netAmount = Math.Round(totalWithIva / IvaFactor, 2);
            var taxAmount = totalWithIva - netAmount;

            // Crear la orden
            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                TotalAmount = totalWithIva,
                NetAmount = netAmount,
                TaxAmount = taxAmount,
                Notes = orderDto.Notes ?? string.Empty,
                OrderItems = orderItems
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Los movimientos se registran después del primer SaveChanges: en ese
            // momento la orden ya tiene Id, así que OrderId queda poblado y se
            // mantiene la trazabilidad orden ↔ movimiento.
            foreach (var orderItem in order.OrderItems)
            {
                _context.InventoryMovements.Add(new InventoryMovement
                {
                    ProductId = orderItem.ProductId,
                    Type = MovementType.Sale,
                    Quantity = orderItem.Quantity,
                    Reason = $"Venta - Orden #{order.OrderNumber}",
                    MovementDate = DateTime.UtcNow,
                    UserId = userId,
                    OrderId = order.Id
                });
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, new
            {
                order.Id,
                order.OrderNumber,
                order.OrderDate,
                order.Status,
                order.TotalAmount,
                order.NetAmount,
                order.TaxAmount,
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

        // Fase B.1: el Vendedor solo puede cambiar el estado de sus propias órdenes.
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var canSeeAllOrders = User.IsInRole("Admin") || User.IsInRole("Bodeguero");
        if (!canSeeAllOrders && order.UserId != userId)
            return Forbid();

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
            foreach (var item in order.OrderItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.Stock += item.Quantity;

                    _context.InventoryMovements.Add(new InventoryMovement
                    {
                        ProductId = item.ProductId,
                        Type = MovementType.Return,
                        Quantity = item.Quantity,
                        Reason = $"Cancelación de orden #{order.OrderNumber}",
                        MovementDate = DateTime.UtcNow,
                        OrderId = order.Id
                    });
                }
            }
        }

        // Un único SaveChangesAsync persiste estado, stock y movimientos de forma atómica.
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

    // Windows usa el identificador "Pacific SA Standard Time"; Linux y macOS
    // (ICU) usan "America/Santiago". Sin este fallback, la creación de órdenes
    // falla en Linux: contenedor Docker y runners de CI.
    private static readonly TimeZoneInfo ChileTimeZone = ResolveChileTimeZone();

    private static TimeZoneInfo ResolveChileTimeZone()
    {
        foreach (var id in new[] { "Pacific SA Standard Time", "America/Santiago" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
            {
                // Identificador no disponible en esta plataforma: se prueba el siguiente.
            }
        }

        return TimeZoneInfo.Utc;
    }

    private string GenerateOrderNumber()
    {
        var date = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ChileTimeZone);
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
