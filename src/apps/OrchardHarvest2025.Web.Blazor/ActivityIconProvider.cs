using Elsa.Studio.Workflows.UI.Contracts;
using Elsa.Studio.Workflows.UI.Models;
using JetBrains.Annotations;

namespace OrchardHarvest2025.Web.Blazor;

[UsedImplicitly]
public class ActivityIconProvider : IActivityDisplaySettingsProvider
{
    public IDictionary<string, ActivityDisplaySettings> GetSettings()
    {
        var anthracite = "#2E2E2E";        
        
        return new Dictionary<string, ActivityDisplaySettings>
        {
            ["OrchardHarvest.ProductAgent"] = new(anthracite, CustomIcons.Robot)
        };
    }
}