using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using NetTopologySuite.Extensions.Features;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Editors
{
    [TestFixture]
    public class FlowBoundaryConditionsListViewTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowTableForTwoFlowBoundaryConditions()
        {
            var feature = new Feature2D {Name = "aap"};

            var bc1 = new FlowBoundaryCondition(FlowBoundaryQuantityType.Salinity,
                                                BoundaryConditionDataType.AstroComponents) {Feature = feature};

            var bc2 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                BoundaryConditionDataType.TimeSeries) {Feature = feature};

            var bc3 = new FlowBoundaryCondition(FlowBoundaryQuantityType.VelocityVector,
                                                BoundaryConditionDataType.Harmonics) {Feature = feature};

            var bcSet = new BoundaryConditionSet
            {
                Feature = feature,
                BoundaryConditions = new EventedList<IBoundaryCondition>(new[]
                {
                    bc1,
                    bc2,
                    bc3
                })
            };

            var view = new BoundaryConditionListView
            {
                Data = new EventedList<BoundaryConditionSet>(new[]
                {
                    bcSet
                })
            };

            WindowsFormsTestHelper.ShowModal(view);
        }
    }
}