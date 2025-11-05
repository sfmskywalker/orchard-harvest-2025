using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;

namespace OrchardHarvest2025.Web.Activities;

[Activity("OrchardHarvest", "Orchard Harvest", "Returns the sum of two numbers")]
public class Sum : CodeActivity<double>
{
    [Input(Description = "The first number to add")]
    public Input<double> A { get; set; } = null!;

    [Input(Description = "The second number to add")]
    public Input<double> B { get; set; } = null!;

    protected override void Execute(ActivityExecutionContext context)
    {
        var a = A.Get(context);
        var b = B.Get(context);
        var result = a + b;

        context.SetResult(result);
    }
}
