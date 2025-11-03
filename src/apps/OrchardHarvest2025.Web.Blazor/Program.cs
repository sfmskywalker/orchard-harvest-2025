using Elsa.Studio.Workflows.Extensions;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OrchardCore.Elsa.Designer.Extensions;
using OrchardHarvest2025.Web.Blazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
var services = builder.Services;

builder.AddElsaDesigner();
services.AddActivityDisplaySettingsProvider<ActivityIconProvider>();

var app = builder.Build();

await app.RunStartupTasksAsync();
await app.RunAsync();
