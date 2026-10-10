namespace WareFlow.Core.DTOs;

/// <summary>
/// Envoltorio genérico para respuestas paginadas del API.
/// Se usa en los listados (productos, órdenes) para no devolver colecciones
/// completas: el cliente recibe la página solicitada más los totales.
/// </summary>
public class PaginatedResponse<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public bool HasNext => Page < TotalPages;
    public bool HasPrevious => Page > 1;

    public PaginatedResponse(IEnumerable<T> items, int page, int pageSize, int totalItems)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalItems = totalItems;
        TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
    }
}
