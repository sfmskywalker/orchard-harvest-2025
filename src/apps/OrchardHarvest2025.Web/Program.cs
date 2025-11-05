using Elsa.Mediator.Options;
using OrchardCore.Elsa.Extensions;
using OrchardCore.Elsa.Middleware;
using OrchardHarvest2025.Web;
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
    .AddOrchardCms(orchard => orchard.RegisterStartup<Startup>());

builder.Services.Configure<MediatorOptions>(options => options.JobWorkerCount = 1);
builder.Services.ConfigureWebAssemblyStaticFiles();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.RewriteElsaStudioWebAssemblyAssets();
app.UseStaticFiles();
app.UseOrchardCore();

app.Run();