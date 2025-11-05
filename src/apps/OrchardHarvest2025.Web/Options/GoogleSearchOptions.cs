using Microsoft.Extensions.Options;

namespace OrchardHarvest2025.Web.Options;

public class GoogleSearchOptions : IAsyncOptions
{
    public string ApiKey { get; set; } = null!;
    public string SearchEngineId { get; set; } = null!;
}
