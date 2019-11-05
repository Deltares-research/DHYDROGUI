using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.NGHS.IO.DataObjects.Model1D;
using GeoAPI.Extensions.Feature;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests
{
    [TestFixture]
    public class WaterFlowModel1DBranchFeatureValueConverterTest
    {
        private static readonly MockRepository mocks = new MockRepository();

        [Test]
        public void Clone()
        {
            var model = mocks.Stub<IModel>();
            var feature = mocks.Stub<IFeature>();

            var converter = new Model1DBranchFeatureValueConverter(model, feature, "parameter", QuantityType.CrestLevel, ElementSet.Observations, DataItemRole.Output, "m");

            var converterClone = (Model1DBranchFeatureValueConverter)converter.DeepClone();

            converterClone.Model.Should().Be.SameInstanceAs(model);
            converterClone.Location.Should().Be.SameInstanceAs(feature);
            converterClone.ParameterName.Should().Be.EqualTo("parameter");
            converterClone.QuantityType.Should().Be.EqualTo(QuantityType.CrestLevel);
            converterClone.ElementSet.Should().Be.EqualTo(ElementSet.Observations);
            converterClone.Role.Should().Be.EqualTo(DataItemRole.Output);
            converterClone.Unit.Should().Be.EqualTo("m");
        }
    }
}
