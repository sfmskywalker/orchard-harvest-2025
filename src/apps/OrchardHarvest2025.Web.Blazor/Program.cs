using Elsa.Studio.Extensions;
using Elsa.Studio.Models;
using Elsa.Studio.Options;
using Elsa.Studio.Workflows.Extensions;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OrchardCore.Elsa.Designer.Extensions;
using OrchardHarvest2025.Web.Blazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
var services = builder.Services;
var configuration = builder.Configuration;

builder.AddElsaDesigner();
services.AddActivityDisplaySettingsProvider<ActivityIconProvider>();

var config = new BackendApiConfig()
{
    ConfigureBackendOptions = (Action<BackendOptions>) (options => configuration.GetSection("Backend").Bind(options))
};
services.AddAgentsModule(config);

var app = builder.Build();

await app.RunStartupTasksAsync();
await app.RunAsync();
