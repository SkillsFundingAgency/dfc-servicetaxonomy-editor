using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Shapes;

namespace DFC.ServiceTaxonomy.CustomEditor.Shapes
{
    public class SummaryAdminShapes : IShapeTableProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("Content_SummaryAdmin").OnDisplaying(context =>
            {
                dynamic shape = context.Shape;
                Shape actions = (Shape)shape.Actions;
                actions.Remove("ContentsButtonEdit_SummaryAdmin");
            });
        }
    }
}
