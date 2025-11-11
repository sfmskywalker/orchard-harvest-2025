using Elsa.Mediator.Options;
using OrchardCore.Elsa.Extensions;
using OrchardCore.Elsa.Middleware;
using OrchardHarvest2025.Web;
using OrchardHarvest2025.Web.Options;
using Quartz;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, logger) =>
{
    logger
        .ReadFrom.Configuration(context.Configuration);
});

builder.Services
    .AddQuartz()
    .AddQuartzHostedService()
    .ConfigureWebAssemblyStaticFiles()
    .AddOrchardCms(orchard => orchard.RegisterStartup<Startup>());

builder.Services.Configure<MediatorOptions>(options => options.JobWorkerCount = 1);
builder.Services.Configure<OpenAIOptions>(options => builder.Configuration.Bind("Elsa:OpenAI", options));
builder.Services.Configure<GoogleSearchOptions>(options => builder.Configuration.Bind("Elsa:GoogleSearch", options));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.RewriteElsaStudioWebAssemblyAssets();
app.UseStaticFiles();
app.UseOrchardCore();

app.Run();