using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _context.Products
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
        
        return Ok(products);
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
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto productDto)
    {
        var product = await _context.Products.FindAsync(id);
        
        if (product == null)
            return NotFound(new { message = "Producto no encontrado" });

        // Actualizar campos
        product.Name = productDto.Name;
        product.Description = productDto.Description;
        product.Price = productDto.Price;
        product.Category = productDto.Category;
        product.MinimumStock = productDto.MinimumStock;
        product.UpdatedAt = DateTime.UtcNow;

        _context.Entry(product).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return Ok(product);
    }

    // DELETE: api/products/{id}
    [HttpDelete("{id}")]
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
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int MinimumStock { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}

public class UpdateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int MinimumStock { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}
