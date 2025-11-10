using Elsa.Agents;
using Elsa.Extensions;
using Elsa.Workflows.CommitStates.Strategies;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardHarvest2025.Web.Plugins;
using StartupBase = OrchardCore.Modules.StartupBase;

namespace OrchardHarvest2025.Web;

[RequireFeatures("OrchardCore.Elsa")]
public class Startup(IServiceProvider serviceProvider) : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        var shellConfiguration = serviceProvider.GetRequiredService<IShellConfiguration>();
        services.Configure<AgentsOptions>(options => { shellConfiguration.Bind("Elsa:Agents", options); });
        services.AddPluginProvider<WebSearchPluginProvider>();
        services.AddPluginProvider<JsonDiffPluginProvider>();
        services.AddPluginProvider<JsonHumanFieldDetectorPluginProvider>();
        services.ConfigureElsa(elsa =>
        {
            elsa.AddActivitiesFrom<Program>();
            elsa.UseAgentActivities();
            elsa.UseWorkflows(workflows =>
            {
                workflows.UseCommitStrategies(strategies =>
                {
                    strategies.AddStandardStrategies();
                    strategies.Add("Every 10 seconds", new PeriodicWorkflowStrategy(TimeSpan.FromSeconds(10)));
                });
            });
            elsa.UseJavaScript(options =>
            {
                options.AllowClrAccess = true;
                options.ConfigureEngine(engine =>
                {
                    engine.Execute("function greet(name) { return `Hello ${name}!`; }");
                    engine.Execute("function sayHelloWorld() { return greet('World'); }");
                });
            });
        });
    }
}