namespace WareFlow.Core.Interfaces;

/// <summary>
/// Genera vectores semánticos (embeddings) para la búsqueda en lenguaje natural.
/// La interfaz permite sustituir el proveedor (Gemini) por un stub en los tests.
/// </summary>
public interface IEmbeddingService
{
    /// <summary>Embedding de la consulta del usuario (prefijo de tarea "search result").</summary>
    Task<float[]> GenerateQueryEmbeddingAsync(string query);

    /// <summary>Embedding de un documento indexable (producto) para compararlo con consultas.</summary>
    Task<float[]> GenerateDocumentEmbeddingAsync(string title, string text);
}
