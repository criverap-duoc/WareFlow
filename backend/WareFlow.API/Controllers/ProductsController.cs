using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using WareFlow.Core.DTOs;
using WareFlow.Core.Models;
using WareFlow.Infrastructure.Data;

namespace WareFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/products
    // Paginación server-side: devuelve { items, page, pageSize, totalItems, totalPages, hasNext, hasPrevious }.
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? category = null,
        [FromQuery] string? stockFilter = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = "asc",
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null)
    {
        // Validación de paginación
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        // Query base (soft delete)
        var query = _context.Products.Where(p => p.IsActive);

        // Filtro de búsqueda por nombre, SKU o categoría
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                p.SKU.ToLower().Contains(term) ||
                p.Category.ToLower().Contains(term));
        }

        // Filtro de categoría
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category == category);

        // Filtro de rango de precio (usado por el selector "Precio" del frontend)
        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);
        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

        // Filtro de stock
        if (!string.IsNullOrWhiteSpace(stockFilter))
        {
            query = stockFilter.ToLower() switch
            {
                "out" => query.Where(p => p.Stock == 0),
                "low" => query.Where(p => p.Stock > 0 && p.Stock <= p.MinimumStock),
                "alert" => query.Where(p => p.Stock == 0 || p.Stock <= p.MinimumStock),
                "ok" => query.Where(p => p.Stock > p.MinimumStock),
                _ => query
            };
        }

        // Ordenamiento
        query = (sortBy?.ToLower(), sortOrder?.ToLower()) switch
        {
            ("name", "desc") => query.OrderByDescending(p => p.Name),
            ("price", "desc") => query.OrderByDescending(p => p.Price),
            ("price", _) => query.OrderBy(p => p.Price),
            ("stock", "desc") => query.OrderByDescending(p => p.Stock),
            ("stock", _) => query.OrderBy(p => p.Stock),
            ("category", "desc") => query.OrderByDescending(p => p.Category),
            ("category", _) => query.OrderBy(p => p.Category),
            _ => query.OrderBy(p => p.Name)
        };

        // Contar el total antes de paginar
        var totalItems = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.SKU,
                p.Price,
                p.Stock,
                p.MinimumStock,
                p.Category,
                p.ImageUrl,
                p.CreatedAt,
                p.IsActive
            })
            .ToListAsync();

        return Ok(new PaginatedResponse<object>(items, page, pageSize, totalItems));
    }

    // GET: api/products/{id}
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        
        if (product == null)
            return NotFound(new { message = "Producto no encontrado" });
        
        return Ok(product);
    }

    // POST: api/products
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto productDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Verificar si SKU ya existe
        var existingProduct = await _context.Products
            .FirstOrDefaultAsync(p => p.SKU == productDto.SKU);
        
        if (existingProduct != null)
            return BadRequest(new { message = "Ya existe un producto con este SKU" });

        var product = new Product
        {
            Name = productDto.Name,
            Description = productDto.Description,
            SKU = productDto.SKU,
            Price = productDto.Price,
            Stock = productDto.Stock,
            MinimumStock = productDto.MinimumStock,
            Category = productDto.Category,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    // PUT: api/products/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Bodeguero")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto productDto)
    {
        var product = await _context.Products.FindAsync(id);
        
        if (product == null)
            return NotFound(new { message = "Producto no encontrado" });

        var isAdmin = User.IsInRole("Admin");

        // Fase B.1: solo Admin modifica los campos del catálogo (nombre, descripción,
        // precio, categoría). El Bodeguero únicamente ajusta stock y stock mínimo.
        // El payload puede ser parcial: un campo ausente (null) no cuenta como
        // intento de cambio ni se persiste.
        var changesCatalogFields =
            (productDto.Name is not null && productDto.Name != product.Name) ||
            (productDto.Description is not null && productDto.Description != product.Description) ||
            (productDto.Price.HasValue && productDto.Price.Value != product.Price) ||
            (productDto.Category is not null && productDto.Category != product.Category);

        if (!isAdmin && changesCatalogFields)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = "Solo Admin puede modificar campos del producto"
            });
        }

        // Actualizar únicamente los campos presentes en el payload
        if (productDto.Name is not null)
            product.Name = productDto.Name;
        if (productDto.Description is not null)
            product.Description = productDto.Description;
        if (productDto.Price.HasValue)
            product.Price = productDto.Price.Value;
        if (productDto.Category is not null)
            product.Category = productDto.Category;
        if (productDto.MinimumStock.HasValue)
            product.MinimumStock = productDto.MinimumStock.Value;
        if (productDto.Stock.HasValue)
            product.Stock = productDto.Stock.Value;

        product.UpdatedAt = DateTime.UtcNow;

        _context.Entry(product).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return Ok(product);
    }

    // DELETE: api/products/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        
        if (product == null)
            return NotFound(new { message = "Producto no encontrado" });

        // Soft delete
        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        
        _context.Entry(product).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Producto eliminado correctamente" });
    }
}

public class CreateProductDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(200, ErrorMessage = "El nombre no puede superar los 200 caracteres")]
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    [Required(ErrorMessage = "El SKU es obligatorio")]
    [StringLength(50, ErrorMessage = "El SKU no puede superar los 50 caracteres")]
    public string SKU { get; set; } = string.Empty;
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
    public decimal Price { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
    public int Stock { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo")]
    public int MinimumStock { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}

public class UpdateProductDto
{
    // Todos los campos son opcionales: el payload puede ser parcial y un campo
    // ausente (null) conserva el valor actual del producto. Así el Bodeguero
    // puede enviar únicamente stock y stock mínimo sin disparar el guard de rol.
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int? Stock { get; set; }
    public int? MinimumStock { get; set; }
    public string? Category { get; set; }
    public string? ImageUrl { get; set; }
}
