using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Web;
using Elsa.Agents;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using OrchardHarvest2025.Web.Options;

namespace OrchardHarvest2025.Web.Plugins;

public class WebSearchPluginProvider : PluginProvider
{
    public override IEnumerable<PluginDescriptor> GetPlugins()
    {
        yield return PluginDescriptor.From<WebSearchPlugin>();
    }
}

/// <summary>
/// A Semantic Kernel plugin that provides web search capabilities to agents using Google Custom Search API.
/// </summary>
public class WebSearchPlugin(IHttpClientFactory httpClientFactory, IOptionsMonitor<GoogleSearchOptions> optionsMonitor)
{
    [KernelFunction("search_web")]
    [Description("Searches the web for information about a given query and returns relevant results.")]
    public async Task<string> SearchWebAsync([Description("The search query to look up on the web")] string query, CancellationToken cancellationToken = default)
    {
        var options = optionsMonitor.CurrentValue;
        var httpClient = httpClientFactory.CreateClient();

        // Use Google Custom Search JSON API
        var encodedQuery = HttpUtility.UrlEncode(query);
        var searchUrl = $"https://www.googleapis.com/customsearch/v1?key={options.ApiKey}&cx={options.SearchEngineId}&q={encodedQuery}&num=5";

        try
        {
            var response = await httpClient.GetAsync(searchUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            var searchResponse = await response.Content.ReadFromJsonAsync<GoogleSearchResponse>(cancellationToken: cancellationToken);

            if (searchResponse?.Items == null || searchResponse.Items.Count == 0)
            {
                return $"No results found for '{query}'.";
            }

            // Format results
            var formattedResults = $"Search results for '{query}':\n\n";
            foreach (var item in searchResponse.Items)
            {
                formattedResults += $"- {item.Title}\n";
                if (!string.IsNullOrEmpty(item.Snippet))
                {
                    formattedResults += $"  {item.Snippet}\n";
                }
                formattedResults += $"  {item.Link}\n\n";
            }

            return formattedResults;
        }
        catch (Exception ex)
        {
            return $"Error performing search: {ex.Message}";
        }
    }
}

public class GoogleSearchResponse
{
    [JsonPropertyName("items")]
    public List<GoogleSearchItem>? Items { get; set; }
}

public class GoogleSearchItem
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("link")]
    public string Link { get; set; } = "";

    [JsonPropertyName("snippet")]
    public string Snippet { get; set; } = "";
}