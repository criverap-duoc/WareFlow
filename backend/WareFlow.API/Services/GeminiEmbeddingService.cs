using System.Net.Http.Json;
using System.Text.Json;
using WareFlow.Core.Interfaces;

namespace WareFlow.API.Services;

/// <summary>
/// Cliente del endpoint embedContent de Gemini (modelo gemini-embedding-2).
/// gemini-embedding-2 no acepta taskType: la tarea se indica con un prefijo en
/// el texto ("task: search result | query: ..." para consultas y
/// "title: ... | text: ..." para documentos).
/// </summary>
public class GeminiEmbeddingService : IEmbeddingService
{
    private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta";

    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly int _dimensions;

    public GeminiEmbeddingService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;

        // La key puede faltar en un entorno sin configurar: se valida al usar el
        // servicio, no al construirlo, para que la app arranque igual y la API
        // responda 503 en vez de tumbar el host al resolver el endpoint.
        _apiKey = configuration["Gemini:ApiKey"] ?? string.Empty;
        _model = configuration["Gemini:EmbeddingModel"] ?? "gemini-embedding-2";
        _dimensions = int.TryParse(configuration["Gemini:EmbeddingDimensions"], out var dimensions)
            ? dimensions
            : 768;
    }

    public Task<float[]> GenerateQueryEmbeddingAsync(string query) =>
        GenerateEmbeddingAsync($"task: search result | query: {query}");

    public Task<float[]> GenerateDocumentEmbeddingAsync(string title, string text) =>
        GenerateEmbeddingAsync($"title: {title} | text: {text}");

    private async Task<float[]> GenerateEmbeddingAsync(string prompt)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new InvalidOperationException("Gemini API key not configured");

        var url = $"{BaseUrl}/models/{_model}:embedContent";

        var payload = new
        {
            model = $"models/{_model}",
            content = new
            {
                parts = new[] { new { text = prompt } }
            },
            outputDimensionality = _dimensions
        };

        // La key viaja en el header (no en la query string) para que no quede
        // registrada en logs de proxies ni en el historial de peticiones.
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Add("x-goog-api-key", _apiKey);

        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        return doc.RootElement
            .GetProperty("embedding")
            .GetProperty("values")
            .EnumerateArray()
            .Select(v => v.GetSingle())
            .ToArray();
    }

    /// <summary>Similitud coseno entre dos vectores. Devuelve 0 si no son comparables.</summary>
    public static double CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length) return 0;

        double dot = 0, magA = 0, magB = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dot += (double)a[i] * b[i];
            magA += (double)a[i] * a[i];
            magB += (double)b[i] * b[i];
        }

        if (magA == 0 || magB == 0) return 0;

        return dot / (Math.Sqrt(magA) * Math.Sqrt(magB));
    }
}
