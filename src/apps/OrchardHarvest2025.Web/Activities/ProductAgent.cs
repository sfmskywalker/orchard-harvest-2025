using Elsa.Workflows;
using Elsa.Workflows.Attributes;

namespace OrchardHarvest2025.Web.Activities;

[Activity("OrchardHarvest", "Orchard Harvest", "Produces product content", Kind = ActivityKind.Task)]
public class ProductAgent : CodeActivity<string>
{
    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var httpClientFactory = context.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient("OpenAI");
    }
}