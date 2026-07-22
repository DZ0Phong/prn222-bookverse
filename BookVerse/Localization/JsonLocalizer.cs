using System.Collections.Concurrent;
using System.Text.Json;

namespace BookVerse.Localization;

public sealed class JsonLocalizer : IJsonLocalizer
{
    private readonly IWebHostEnvironment _environment;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ConcurrentDictionary<string, ResourceDocument> _cache = new(StringComparer.OrdinalIgnoreCase);

    public JsonLocalizer(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
    {
        _environment = environment;
        _httpContextAccessor = httpContextAccessor;
    }

    public string this[string key] => Get(key);

    public string Get(string key, params object[] arguments)
    {
        var culture = GetCurrentCulture();
        var resources = Load(culture);
        var value = resources.Strings.TryGetValue(key, out var translated)
            ? translated
            : Load("vi").Strings.GetValueOrDefault(key, key);
        return arguments.Length == 0 ? value : string.Format(value, arguments);
    }

    public IReadOnlyDictionary<string, string> GetClientTextMap(string culture) => Load(Normalize(culture)).TextMap;

    private string GetCurrentCulture()
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        var value = request?.Cookies["bookverse.culture"] ?? request?.Query["culture"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(value) && request?.Path.StartsWithSegments("/Account") == true
            && (request.Path.Value?.Contains("/Login", StringComparison.OrdinalIgnoreCase) == true
                || request.Path.Value?.Contains("/Register", StringComparison.OrdinalIgnoreCase) == true))
            return "en";
        return Normalize(value);
    }

    private static string Normalize(string? culture) =>
        string.Equals(culture, "en", StringComparison.OrdinalIgnoreCase) ? "en" : "vi";

    private ResourceDocument Load(string culture) => _cache.GetOrAdd(culture, key =>
    {
        var path = Path.Combine(_environment.ContentRootPath, "Resources", $"{key}.json");
        if (!File.Exists(path)) return new ResourceDocument();
        using var stream = File.OpenRead(path);
        return JsonSerializer.Deserialize<ResourceDocument>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new ResourceDocument();
    });

    private sealed class ResourceDocument
    {
        public Dictionary<string, string> Strings { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> TextMap { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
