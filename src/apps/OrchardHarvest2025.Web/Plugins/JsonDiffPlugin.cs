using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using Elsa.Agents;
using Microsoft.SemanticKernel;

namespace OrchardHarvest2025.Web.Plugins;

public class JsonDiffPluginProvider : PluginProvider
{
    public override IEnumerable<PluginDescriptor> GetPlugins()
    {
        yield return PluginDescriptor.From<JsonDiffPlugin>();
    }
}

/// <summary>
/// Provides a function to compute a JSON Patch-like diff between two JSON objects.
/// Supports granular object and array diffs. Arrays of primitives are diffed by index; arrays containing
/// objects or nested arrays are recursed element-by-element to produce nested diffs where possible.
/// </summary>
public class JsonDiffPlugin
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    private sealed class DiffEntry
    {
        public string Op { get; set; } = string.Empty; // replace | add | remove | error
        public string Path { get; set; } = string.Empty; // JSON Pointer path
        public JsonNode? From { get; set; }
        public JsonNode? To { get; set; }
        public string? Reason { get; set; } // For error entries.
    }

    [KernelFunction("json_diff")]
    [Description("Produces a JSON Patch-like diff array between the original and updated JSON objects. Each entry has op, path, and from/to (or reason) fields as applicable.")]
    public Task<string> JsonDiffAsync(
        [Description("Original JSON object string")] string originalJson,
        [Description("Updated (corrected) JSON object string")] string updatedJson,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var originalNode = JsonNode.Parse(originalJson);
            var updatedNode = JsonNode.Parse(updatedJson);
            if (originalNode is null || updatedNode is null)
                return Task.FromResult("[]");

            var entries = new List<DiffEntry>();
            BuildDiff(originalNode, updatedNode, string.Empty, entries);

            var json = JsonSerializer.Serialize(entries, SerializerOptions);
            return Task.FromResult(json);
        }
        catch (Exception ex)
        {
            var errorEntry = new DiffEntry
            {
                Op = "error",
                Path = string.Empty,
                Reason = ex.Message
            };
            return Task.FromResult(JsonSerializer.Serialize(new[] { errorEntry }, SerializerOptions));
        }
    }

    private static void BuildDiff(JsonNode original, JsonNode updated, string path, List<DiffEntry> entries)
    {
        // Object vs object
        if (original is JsonObject oObj && updated is JsonObject uObj)
        {
            foreach (var kv in oObj)
            {
                var propName = kv.Key;
                var oChild = kv.Value;
                var uChild = uObj.ContainsKey(propName) ? uObj[propName] : null;
                var childPath = AppendPath(path, propName);

                if (uChild == null)
                {
                    entries.Add(new DiffEntry
                    {
                        Op = "remove",
                        Path = childPath,
                        From = CloneNode(oChild)
                    });
                    continue;
                }

                if (IsLeaf(oChild) && IsLeaf(uChild))
                {
                    if (!JsonNodeDeepEquals(oChild, uChild))
                    {
                        entries.Add(new DiffEntry
                        {
                            Op = "replace",
                            Path = childPath,
                            From = CloneNode(oChild),
                            To = CloneNode(uChild)
                        });
                    }
                }
                else
                {
                    BuildDiff(oChild!, uChild!, childPath, entries);
                }
            }

            // Added properties
            foreach (var kv in uObj)
            {
                if (!oObj.ContainsKey(kv.Key))
                {
                    var childPath = AppendPath(path, kv.Key);
                    entries.Add(new DiffEntry
                    {
                        Op = "add",
                        Path = childPath,
                        To = CloneNode(kv.Value)
                    });
                }
            }
            return;
        }

        // Arrays: granular compare
        if (original is JsonArray oArr && updated is JsonArray uArr)
        {
            var min = Math.Min(oArr.Count, uArr.Count);
            for (var i = 0; i < min; i++)
            {
                var oElem = oArr[i];
                var uElem = uArr[i];
                var elemPath = path + "/" + i; // JSON Pointer index

                if (oElem == null && uElem == null)
                    continue;
                if (oElem == null)
                {
                    entries.Add(new DiffEntry { Op = "add", Path = elemPath, To = CloneNode(uElem) });
                    continue;
                }
                if (uElem == null)
                {
                    entries.Add(new DiffEntry { Op = "remove", Path = elemPath, From = CloneNode(oElem) });
                    continue;
                }

                if (IsLeaf(oElem) && IsLeaf(uElem))
                {
                    if (!JsonNodeDeepEquals(oElem, uElem))
                    {
                        entries.Add(new DiffEntry
                        {
                            Op = "replace",
                            Path = elemPath,
                            From = CloneNode(oElem),
                            To = CloneNode(uElem)
                        });
                    }
                }
                else
                {
                    BuildDiff(oElem!, uElem!, elemPath, entries);
                }
            }

            // Removed tail elements
            for (var i = uArr.Count; i < oArr.Count; i++)
            {
                var elemPath = path + "/" + i;
                entries.Add(new DiffEntry
                {
                    Op = "remove",
                    Path = elemPath,
                    From = CloneNode(oArr[i])
                });
            }

            // Added tail elements
            for (var i = oArr.Count; i < uArr.Count; i++)
            {
                var elemPath = path + "/" + i;
                entries.Add(new DiffEntry
                {
                    Op = "add",
                    Path = elemPath,
                    To = CloneNode(uArr[i])
                });
            }
            return;
        }

        // Primitive or type mismatch
        if (!JsonNodeDeepEquals(original, updated))
        {
            entries.Add(new DiffEntry
            {
                Op = "replace",
                Path = path,
                From = CloneNode(original),
                To = CloneNode(updated)
            });
        }
    }

    private static bool IsLeaf(JsonNode? node) => node is null || node is JsonValue;

    private static bool JsonNodeDeepEquals(JsonNode? a, JsonNode? b)
    {
        if (a is null && b is null) return true;
        if (a is null || b is null) return false;
        return SerializeValue(a) == SerializeValue(b);
    }

    private static JsonNode? CloneNode(JsonNode? node) => node == null ? null : JsonNode.Parse(SerializeValue(node));

    private static string SerializeValue(JsonNode? node) => node == null ? "null" : JsonSerializer.Serialize(node, SerializerOptions);

    private static string AppendPath(string basePath, string segment)
    {
        var escaped = segment.Replace("~", "~0").Replace("/", "~1");
        return string.IsNullOrEmpty(basePath) ? "/" + escaped : basePath + "/" + escaped;
    }
}
