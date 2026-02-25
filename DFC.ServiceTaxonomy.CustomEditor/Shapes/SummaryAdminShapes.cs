using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Shapes;

namespace DFC.ServiceTaxonomy.CustomEditor.Shapes
{
    public class SummaryAdminShapes : IShapeTableProvider
    {
        public async ValueTask DiscoverAsync(ShapeTableBuilder builder)
        {
            await Task.Yield();
            builder.Describe("Content_SummaryAdmin").OnDisplaying(context =>
            {
                dynamic shape = context.Shape;
                Shape actions = (Shape)shape.Actions;
                actions.Remove("ContentsButtonEdit_SummaryAdmin");
            });
        }
    }
}
