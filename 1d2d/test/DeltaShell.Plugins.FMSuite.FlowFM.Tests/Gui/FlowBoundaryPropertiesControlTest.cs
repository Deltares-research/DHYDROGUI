using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    public class FlowBoundaryPropertiesControlTest
    {
        [Test]
        public void ShowWaterLevelDataViewTimeSeries()
        {
            var bc = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.TimeSeries);

            var view = new FlowBoundaryConditionPropertiesControl
                {
                    Controller = new FlowBoundaryConditionEditorController(),
                    BoundaryCondition = bc
                };

            WindowsFormsTestHelper.ShowModal(view);
        }

        [Test]
        public void ShowCurrentsDataViewTimeSeries()
        {
            var bc = new FlowBoundaryCondition(FlowBoundaryQuantityType.Velocity, BoundaryConditionDataType.TimeSeries);

            var view = new FlowBoundaryConditionPropertiesControl
                {
                    
                    Controller = new FlowBoundaryConditionEditorController(),
                    BoundaryCondition = bc
                };

            WindowsFormsTestHelper.ShowModal(view);
        }

        [Test]
        public void ShowRiemannDataViewTimeSeries()
        {
            var bc = new FlowBoundaryCondition(FlowBoundaryQuantityType.Riemann, BoundaryConditionDataType.TimeSeries);

            var view = new FlowBoundaryConditionPropertiesControl
                {
                    Controller = new FlowBoundaryConditionEditorController(),
                    BoundaryCondition = bc
                };

            WindowsFormsTestHelper.ShowModal(view);
        }

        [Test]
        public void ShowDischargeDataViewTimeSeries()
        {
            var bc = new FlowBoundaryCondition(FlowBoundaryQuantityType.Discharge, BoundaryConditionDataType.TimeSeries);

            var view = new FlowBoundaryConditionPropertiesControl
                {
                    
                    Controller = new FlowBoundaryConditionEditorController(),
                    BoundaryCondition = bc
                };

            WindowsFormsTestHelper.ShowModal(view);
        }

        [Test]
        public void ShowConstituentDataView()
        {
            var bc = new FlowBoundaryCondition(FlowBoundaryQuantityType.Salinity, BoundaryConditionDataType.TimeSeries);

            var view = new FlowBoundaryConditionPropertiesControl
                {
                    Controller = new FlowBoundaryConditionEditorController(),
                    BoundaryCondition = bc
                };

            WindowsFormsTestHelper.ShowModal(view);
        }
    }
}
