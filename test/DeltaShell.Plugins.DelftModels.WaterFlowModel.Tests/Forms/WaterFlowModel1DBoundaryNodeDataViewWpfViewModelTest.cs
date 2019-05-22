using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.Forms
{
    [TestFixture]
    public class WaterFlowModel1DBoundaryNodeDataViewWpfViewModelTest
    {
        private WaterFlowModel1DBoundaryNodeData boundaryNodeData;

        [SetUp]
        public void Setup()
        {
            boundaryNodeData = WaterFlowModel1DBoundaryAndLateralWpfViewHelper.GetStandardBoundaryNodeData();
        }

        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        [Test]
        public void TestChangeFlowDataTypeInViewModelIsReflectedInObjectModel()
        {
            using (var view = new WaterFlowModel1DBoundaryNodeDataViewWpf { Data = boundaryNodeData })
            {
                var viewModel = view.DataContext as WaterFlowModel1DBoundaryNodeDataViewWpfViewModel;
                Assert.NotNull(viewModel);

                viewModel.BoundaryNodeDataType = WaterFlowModel1DBoundaryNodeDataType.None;
                Assert.IsTrue(boundaryNodeData.DataType == WaterFlowModel1DBoundaryNodeDataType.None);

                viewModel.BoundaryNodeDataType = WaterFlowModel1DBoundaryNodeDataType.FlowConstant;
                Assert.IsTrue(boundaryNodeData.DataType == WaterFlowModel1DBoundaryNodeDataType.FlowConstant);
                
                viewModel.BoundaryNodeDataType = WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries;
                Assert.IsTrue(boundaryNodeData.DataType == WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries);
                
                viewModel.BoundaryNodeDataType = WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable;
                Assert.IsTrue(boundaryNodeData.DataType == WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable);
                
                viewModel.BoundaryNodeDataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant;
                Assert.IsTrue(boundaryNodeData.DataType == WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant);
                
                viewModel.BoundaryNodeDataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries;
                Assert.IsTrue(boundaryNodeData.DataType == WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void TestChangeSaltDataTypeInViewModelIsReflectedInObjectModel()
        {
            using (var view = new WaterFlowModel1DBoundaryNodeDataViewWpf { Data = boundaryNodeData })
            {
                var viewModel = view.DataContext as WaterFlowModel1DBoundaryNodeDataViewWpfViewModel;
                Assert.NotNull(viewModel);

                viewModel.SaltConditionType = SaltBoundaryConditionType.None;
                Assert.IsTrue(boundaryNodeData.SaltConditionType == SaltBoundaryConditionType.None);

                viewModel.SaltConditionType = SaltBoundaryConditionType.Constant;
                Assert.IsTrue(boundaryNodeData.SaltConditionType == SaltBoundaryConditionType.Constant);
                
                viewModel.SaltConditionType = SaltBoundaryConditionType.TimeDependent;
                Assert.IsTrue(boundaryNodeData.SaltConditionType == SaltBoundaryConditionType.TimeDependent);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void TestChangeTemperatureDataTypeInViewModelIsReflectedInObjectModel()
        {
            using (var view = new WaterFlowModel1DBoundaryNodeDataViewWpf { Data = boundaryNodeData })
            {
                var viewModel = view.DataContext as WaterFlowModel1DBoundaryNodeDataViewWpfViewModel;
                Assert.NotNull(viewModel);

                viewModel.TemperatureConditionType = TemperatureBoundaryConditionType.None;
                Assert.IsTrue(boundaryNodeData.TemperatureConditionType == TemperatureBoundaryConditionType.None);

                viewModel.TemperatureConditionType = TemperatureBoundaryConditionType.Constant;
                Assert.IsTrue(boundaryNodeData.TemperatureConditionType == TemperatureBoundaryConditionType.Constant);

                viewModel.TemperatureConditionType = TemperatureBoundaryConditionType.TimeDependent;
                Assert.IsTrue(boundaryNodeData.TemperatureConditionType == TemperatureBoundaryConditionType.TimeDependent);
            }
        }

        [Category(TestCategory.DataAccess)]
        [Test]
        public void TestUpdateConstantFlowDataInViewModelIsReflectedInObjectModel()
        {
            using (var view = new WaterFlowModel1DBoundaryNodeDataViewWpf { Data = boundaryNodeData })
            {
                var viewModel = view.DataContext as WaterFlowModel1DBoundaryNodeDataViewWpfViewModel;
                Assert.NotNull(viewModel);

                viewModel.WaterLevel = boundaryNodeData.WaterLevel + 0.555;
                Assert.AreEqual(viewModel.WaterLevel, boundaryNodeData.WaterLevel, 0.001);

                viewModel.Flow = boundaryNodeData.Flow + 0.555;
                Assert.AreEqual(viewModel.Flow, boundaryNodeData.Flow, 0.001);
            }
        }

        [Category(TestCategory.DataAccess)]
        [Test]
        public void TestUpdateConstantSaltDataInViewModelIsReflectedInObjectModel()
        {
            using (var view = new WaterFlowModel1DBoundaryNodeDataViewWpf { Data = boundaryNodeData })
            {
                var viewModel = view.DataContext as WaterFlowModel1DBoundaryNodeDataViewWpfViewModel;
                Assert.NotNull(viewModel);

                viewModel.SaltConcentrationConstant = boundaryNodeData.SaltConcentrationConstant + 0.555;
                Assert.AreEqual(viewModel.SaltConcentrationConstant, boundaryNodeData.SaltConcentrationConstant, 0.001);

                viewModel.ThatcherHarlemannCoefficient = boundaryNodeData.ThatcherHarlemannCoefficient + 0.555;
                Assert.AreEqual(viewModel.ThatcherHarlemannCoefficient, boundaryNodeData.ThatcherHarlemannCoefficient, 0.001);
            }
        }

        [Category(TestCategory.DataAccess)]
        [Test]
        public void TestUpdateConstantTemperatureDataInViewModelIsReflectedInObjectModel()
        {
            using (var view = new WaterFlowModel1DBoundaryNodeDataViewWpf { Data = boundaryNodeData })
            {
                var viewModel = view.DataContext as WaterFlowModel1DBoundaryNodeDataViewWpfViewModel;
                Assert.NotNull(viewModel);

                viewModel.TemperatureConstant = boundaryNodeData.TemperatureConstant + 0.555;
                Assert.AreEqual(viewModel.TemperatureConstant, boundaryNodeData.TemperatureConstant, 0.001);
            }
        }

    }
}