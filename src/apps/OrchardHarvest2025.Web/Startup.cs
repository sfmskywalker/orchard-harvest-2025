using Elsa.Agents;
using Elsa.Extensions;
using OrchardCore.Environment.Shell.Configuration;
using OrchardHarvest2025.Web.Options;
using StartupBase = OrchardCore.Modules.StartupBase;

namespace OrchardHarvest2025.Web;

public class Startup(IServiceProvider serviceProvider) : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        var shellConfiguration = serviceProvider.GetRequiredService<IShellConfiguration>();
        services.Configure<OpenAIOptions>(options =>
        {
            shellConfiguration.Bind("Elsa:OpenAI", options);
        });
        services.Configure<AgentsOptions>(options =>
        {
            shellConfiguration.Bind("Elsa:Agents", options);
        });
        services.ConfigureElsa(elsa =>
        {
            elsa.AddActivitiesFrom<Program>();
            elsa.UseAgentActivities();
        });
    }
}