using System.Text.Json.Serialization;
using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Microsoft.Extensions.Options;
using OrchardHarvest2025.Web.Options;

namespace OrchardHarvest2025.Web.Activities;

[Activity("OrchardHarvest", "Orchard Harvest", "Produces product content", Kind = ActivityKind.Task)]
public class ProductAgent : CodeActivity<string>
{
    [Input(Description = "The product name")]
    public Input<string> ProductName { get; set; } = null!;

    [Input(Description = "The product description")]
    public Input<string> Description { get; set; } = null!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var httpClientFactory = context.GetRequiredService<IHttpClientFactory>();
        var optionsMonitor = context.GetRequiredService<IOptionsMonitor<OpenAIOptions>>();
        var options = optionsMonitor.CurrentValue;

        var httpClient = httpClientFactory.CreateClient("OpenAI");
        httpClient.DefaultRequestHeaders.Authorization = new("Bearer", options.ApiKey);

        var productName = ProductName.Get(context);
        var description = Description.Get(context);

        var requestBody = new
        {
            model = "gpt-4o",
            input = new[]
            {
                new
                {
                    role = "system",
                    content = "You are a professional copywriter who crafts engaging and persuasive product descriptions. You will receive structured product information in a key/value format that includes fields such as name, category, description and price. Your task is to write a concise and compelling product description of about two to three paragraphs that emphasizes the product's unique selling points and benefits, uses natural persuasive language suited for online product listings, and matches the indicated tone. If the tone is not specified, default to a confident and inviting tone."
                },
                new
                {
                    role = "user",
                    content = $"Name={productName}, Description={description}"
                }
            }
        };

        var response = await httpClient.PostAsJsonAsync("https://api.openai.com/v1/responses", requestBody, context.CancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OpenAIResponse>(cancellationToken: context.CancellationToken);
        var text = result?.Output?.FirstOrDefault()?.Content?.FirstOrDefault()?.Text ?? string.Empty;

        context.Set(Result, text);
    }
}

public class OpenAIResponse
{
    [JsonPropertyName("output")]
    public List<OpenAIMessage>? Output { get; set; }
}

public class OpenAIMessage
{
    [JsonPropertyName("content")]
    public List<OpenAIContent>? Content { get; set; }
}

public class OpenAIContent
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}