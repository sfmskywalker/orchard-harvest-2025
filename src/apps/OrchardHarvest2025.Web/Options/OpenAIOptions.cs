using Microsoft.Extensions.Options;

namespace OrchardHarvest2025.Web.Options;

public class OpenAIOptions : IAsyncOptions
{
    public string ApiKey { get; set; } = null!;
}
