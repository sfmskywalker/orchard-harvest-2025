using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using Elsa.Agents;
using Microsoft.SemanticKernel;

namespace OrchardHarvest2025.Web.Plugins;

public class JsonHumanFieldDetectorPluginProvider : PluginProvider
{
    public override IEnumerable<PluginDescriptor> GetPlugins()
    {
        yield return PluginDescriptor.From<JsonHumanFieldDetectorPlugin>();
    }
}

/// <summary>
/// Detects JSON pointer paths for fields likely containing human natural language text.
/// Heuristics exclude IDs, timestamps, GUID-like strings, slugs, hashes, short codes, and technical fields.
/// </summary>
public class JsonHumanFieldDetectorPlugin
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    private static readonly string[] HumanishNameHints =
    {
        "title", "displaytext", "subtitle", "description", "summary", "markdown", "body", "content", "text"
    };

    [KernelFunction("json_human_fields")]
    [Description("Returns a JSON array of JSON Pointer paths that likely contain human language text within the provided JSON object.")]
    public Task<string> JsonHumanFieldsAsync(
        [Description("Original JSON object string")] string originalJson,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var root = JsonNode.Parse(originalJson);
            if (root is null)
                return Task.FromResult("[]");

            var results = new List<string>();
            Traverse(root, string.Empty, results);
            var json = JsonSerializer.Serialize(results, SerializerOptions);
            return Task.FromResult(json);
        }
        catch (Exception ex)
        {
            // Return error sentinel entry.
            var json = JsonSerializer.Serialize(new[] { "ERROR:" + ex.Message }, SerializerOptions);
            return Task.FromResult(json);
        }
    }

    private static void Traverse(JsonNode node, string path, List<string> results)
    {
        switch (node)
        {
            case JsonObject obj:
                foreach (var kv in obj)
                {
                    var childPath = AppendPath(path, kv.Key);
                    if (kv.Value != null)
                        Traverse(kv.Value, childPath, results);
                }
                break;
            case JsonArray arr:
                for (var i = 0; i < arr.Count; i++)
                {
                    var childPath = string.IsNullOrEmpty(path) ? "/" + i : path + "/" + i;
                    var elem = arr[i];
                    if (elem != null)
                        Traverse(elem, childPath, results);
                }
                break;
            case JsonValue value:
                if (value.TryGetValue<string>(out var str))
                {
                    if (IsHumanLanguageCandidate(path, str))
                        results.Add(path);
                }
                break;
        }
    }

    private static bool IsHumanLanguageCandidate(string path, string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var trimmed = value.Trim();
        // Exclusions by pattern.
        if (IsHexId(trimmed) || IsGuidLike(trimmed) || IsIsoDateTime(trimmed) || IsNumeric(trimmed) || IsSlug(trimmed) || IsUpperAcronym(trimmed) || LooksLikePath(trimmed))
            return false;
        // Short tokens (e.g., single word < 4 chars) not human text.
        if (trimmed.Length < 4 && !trimmed.Contains(' ')) return false;
        // Name hints from property segments.
        var loweredPathSegments = path.Split('/', StringSplitOptions.RemoveEmptyEntries).Select(s => s.ToLowerInvariant());
        if (loweredPathSegments.Any(seg => HumanishNameHints.Contains(seg)))
            return true;
        // Content heuristics: contains space OR sentence punctuation OR markdown formatting.
        var hasSpace = trimmed.Contains(' ');
        var hasPunctuation = trimmed.IndexOfAny(new[] { '.', ',', '!', '?', ';', ':' }) >= 0;
        var hasMarkdown = trimmed.Contains("**") || trimmed.Contains("__") || trimmed.Contains("*") || trimmed.Contains("#") || trimmed.Contains("\n");
        var longEnough = trimmed.Length >= 20;
        return (hasSpace && hasPunctuation) || (hasSpace && longEnough) || hasMarkdown;
    }

    private static bool IsHexId(string s) => System.Text.RegularExpressions.Regex.IsMatch(s, "^[0-9a-f]{24}$");
    private static bool IsGuidLike(string s) => System.Text.RegularExpressions.Regex.IsMatch(s, "^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$");
    private static bool IsIsoDateTime(string s) => System.Text.RegularExpressions.Regex.IsMatch(s, "^\\d{4}-\\d{2}-\\d{2}T\\d{2}:\\d{2}:");
    private static bool IsNumeric(string s) => System.Text.RegularExpressions.Regex.IsMatch(s, "^\\d+$");
    private static bool IsSlug(string s) => System.Text.RegularExpressions.Regex.IsMatch(s, "^[a-z0-9]+(-[a-z0-9]+)+$");
    private static bool IsUpperAcronym(string s) => System.Text.RegularExpressions.Regex.IsMatch(s, "^[A-Z]{2,10}$");
    private static bool LooksLikePath(string s) => s.StartsWith('/') || s.Contains('\\') || System.Text.RegularExpressions.Regex.IsMatch(s, "^[a-zA-Z0-9_/.-]+\\.[a-zA-Z0-9]{1,5}$");

    private static string AppendPath(string basePath, string segment)
    {
        var escaped = segment.Replace("~", "~0").Replace("/", "~1");
        return string.IsNullOrEmpty(basePath) ? "/" + escaped : basePath + "/" + escaped;
    }
}

