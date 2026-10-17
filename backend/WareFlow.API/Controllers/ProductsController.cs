using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WareFlow.API.Services;
using WareFlow.Core.DTOs;
using WareFlow.Core.Interfaces;
using WareFlow.Core.Models;
using WareFlow.Infrastructure.Data;

namespace WareFlow.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
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

    // ---------------- Predicción de demanda (Fase IA.1) ----------------

    // GET: api/products/{id}/demand-forecast
    [HttpGet("{id}/demand-forecast")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDemandForecast(
        int id,
        [FromServices] DemandForecastService forecastService)
    {
        var forecast = await forecastService.ForecastAsync(id);
        if (forecast == null)
            return NotFound(new { message = "Producto no encontrado" });
        return Ok(forecast);
    }

    // GET: api/products/demand-forecast/top?limit=5
    // Se declara antes de {id}/demand-forecast: la ruta literal tiene prioridad
    // sobre la parametrizada en el routing de ASP.NET Core.
    [HttpGet("demand-forecast/top")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTopDemandForecasts(
        [FromServices] DemandForecastService forecastService,
        [FromQuery] int limit = 5)
    {
        if (limit < 1) limit = 5;

        var products = await _context.Products
            .Where(p => p.IsActive && p.Stock > 0)
            .Select(p => p.Id)
            .ToListAsync();

        var forecasts = new List<DemandForecast>();
        foreach (var productId in products)
        {
            var forecast = await forecastService.ForecastAsync(productId);
            if (forecast != null && forecast.DaysUntilMinimum.HasValue)
                forecasts.Add(forecast);
        }

        var top = forecasts
            .OrderBy(f => f.DaysUntilMinimum)
            .Take(limit)
            .ToList();

        return Ok(top);
    }

    // ---------------- Búsqueda semántica (Fase IA.3) ----------------

    // GET: api/products/semantic-search?q=laptops baratas con stock&limit=5
    // Compara el embedding de la consulta con el de cada producto (similitud
    // coseno en memoria: 66 productos no justifican una vector DB).
    [HttpGet("semantic-search", Order = -1)]
    [AllowAnonymous]
    public async Task<IActionResult> SemanticSearch(
        [FromQuery] string? q,
        [FromServices] IEmbeddingService embeddingService,
        [FromQuery] int limit = 5)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { message = "Query requerido" });

        if (limit < 1) limit = 1;
        if (limit > 20) limit = 20;

        // Embedding de la consulta
        float[] queryEmbedding;
        try
        {
            queryEmbedding = await embeddingService.GenerateQueryEmbeddingAsync(q.Trim());
        }
        catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException or TaskCanceledException)
        {
            // El proveedor es externo: su fallo no debe salir como 500 con stack trace.
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                message = "Búsqueda semántica no disponible",
                error = ex.Message
            });
        }

        // Solo productos activos ya indexados
        var candidates = await _context.Products
            .Where(p => p.IsActive && p.Embedding != null)
            .Select(p => new { p.Id, p.Name, p.SKU, p.Price, p.Stock, p.Category, p.ImageUrl, p.Embedding })
            .ToListAsync();

        static float[]? ParseEmbedding(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try
            {
                return JsonSerializer.Deserialize<float[]>(json);
            }
            catch (JsonException)
            {
                // Vector corrupto: el producto se omite en lugar de romper la búsqueda.
                return null;
            }
        }

        var results = candidates
            .Select(p => new { Product = p, Vector = ParseEmbedding(p.Embedding) })
            .Where(x => x.Vector is not null && x.Vector.Length == queryEmbedding.Length)
            .Select(x => new
            {
                x.Product.Id,
                x.Product.Name,
                x.Product.SKU,
                x.Product.Price,
                x.Product.Stock,
                x.Product.Category,
                x.Product.ImageUrl,
                Score = Math.Round(
                    GeminiEmbeddingService.CosineSimilarity(queryEmbedding, x.Vector!),
                    4)
            })
            .Where(r => r.Score > 0.5) // Umbral mínimo de relevancia
            .OrderByDescending(r => r.Score)
            .Take(limit)
            .ToList();

        return Ok(new
        {
            query = q,
            count = results.Count,
            results
        });
    }

    // POST: api/products/generate-embeddings
    // Indexa el catálogo activo con embeddings. Operación manual e idempotente:
    // se ejecuta tras cargar o editar productos.
    [HttpPost("generate-embeddings", Order = -1)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GenerateAllEmbeddings(
        [FromServices] IEmbeddingService embeddingService)
    {
        var products = await _context.Products
            .Where(p => p.IsActive)
            .ToListAsync();

        var processed = 0;
        var failed = 0;

        for (var i = 0; i < products.Count; i++)
        {
            var product = products[i];
            try
            {
                var embedding = await embeddingService.GenerateDocumentEmbeddingAsync(
                    product.Name,
                    $"{product.Category}. {product.Description}");
                product.Embedding = JsonSerializer.Serialize(embedding);
                processed++;
            }
            catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException or TaskCanceledException)
            {
                // Un producto sin embedding simplemente queda fuera de la búsqueda:
                // un fallo puntual no aborta el lote completo.
                failed++;
            }

            // Rate limit del tier gratuito: cada 10 peticiones se persiste el avance
            // y se espera 4 s, así un corte del proveedor no pierde lo ya indexado.
            if ((i + 1) % 10 == 0 && i + 1 < products.Count)
            {
                await _context.SaveChangesAsync();
                await Task.Delay(TimeSpan.FromSeconds(4));
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = $"{processed} productos procesados",
            total = products.Count,
            failed
        });
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
