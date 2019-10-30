using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.FMSuite.Common.Gui.Forms;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.Forms
{
    [TestFixture]
    public class WaterFlowModel1DBoundaryNodeDataViewWpfViewModelTest
    {
        private Model1DBoundaryNodeData boundaryNodeData;

        [SetUp]
        public void Setup()
        {
            boundaryNodeData = WaterFlowModel1DBoundaryAndLateralWpfViewHelper.GetStandardBoundaryNodeData();
        }

        [Category(TestCategory.DataAccess)]
        [Test]
        public void TestChangeFlowDataTypeInViewModelIsReflectedInObjectModel()
        {
            using (var view = new Model1DBoundaryNodeDataViewWpf { Data = boundaryNodeData })
            {
                var viewModel = view.DataContext as Model1DBoundaryNodeDataViewWpfViewModel;
                Assert.NotNull(viewModel);

                viewModel.BoundaryNodeDataType = Model1DBoundaryNodeDataType.None;
                Assert.IsTrue(boundaryNodeData.DataType == Model1DBoundaryNodeDataType.None);

                viewModel.BoundaryNodeDataType = Model1DBoundaryNodeDataType.FlowConstant;
                Assert.IsTrue(boundaryNodeData.DataType == Model1DBoundaryNodeDataType.FlowConstant);
                
                viewModel.BoundaryNodeDataType = Model1DBoundaryNodeDataType.FlowTimeSeries;
                Assert.IsTrue(boundaryNodeData.DataType == Model1DBoundaryNodeDataType.FlowTimeSeries);
                
                viewModel.BoundaryNodeDataType = Model1DBoundaryNodeDataType.FlowWaterLevelTable;
                Assert.IsTrue(boundaryNodeData.DataType == Model1DBoundaryNodeDataType.FlowWaterLevelTable);
                
                viewModel.BoundaryNodeDataType = Model1DBoundaryNodeDataType.WaterLevelConstant;
                Assert.IsTrue(boundaryNodeData.DataType == Model1DBoundaryNodeDataType.WaterLevelConstant);
                
                viewModel.BoundaryNodeDataType = Model1DBoundaryNodeDataType.WaterLevelTimeSeries;
                Assert.IsTrue(boundaryNodeData.DataType == Model1DBoundaryNodeDataType.WaterLevelTimeSeries);
            }
        }

        [Category(TestCategory.DataAccess)]
        [Test]
        public void TestChangeSaltDataTypeInViewModelIsReflectedInObjectModel()
        {
            using (var view = new Model1DBoundaryNodeDataViewWpf { Data = boundaryNodeData })
            {
                var viewModel = view.DataContext as Model1DBoundaryNodeDataViewWpfViewModel;
                Assert.NotNull(viewModel);

                viewModel.SaltConditionType = SaltBoundaryConditionType.None;
                Assert.IsTrue(boundaryNodeData.SaltConditionType == SaltBoundaryConditionType.None);

                viewModel.SaltConditionType = SaltBoundaryConditionType.Constant;
                Assert.IsTrue(boundaryNodeData.SaltConditionType == SaltBoundaryConditionType.Constant);
                
                viewModel.SaltConditionType = SaltBoundaryConditionType.TimeDependent;
                Assert.IsTrue(boundaryNodeData.SaltConditionType == SaltBoundaryConditionType.TimeDependent);
            }
        }

        [Category(TestCategory.DataAccess)]
        [Test]
        public void TestChangeTemperatureDataTypeInViewModelIsReflectedInObjectModel()
        {
            using (var view = new Model1DBoundaryNodeDataViewWpf { Data = boundaryNodeData })
            {
                var viewModel = view.DataContext as Model1DBoundaryNodeDataViewWpfViewModel;
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
            using (var view = new Model1DBoundaryNodeDataViewWpf { Data = boundaryNodeData })
            {
                var viewModel = view.DataContext as Model1DBoundaryNodeDataViewWpfViewModel;
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
            using (var view = new Model1DBoundaryNodeDataViewWpf { Data = boundaryNodeData })
            {
                var viewModel = view.DataContext as Model1DBoundaryNodeDataViewWpfViewModel;
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
            using (var view = new Model1DBoundaryNodeDataViewWpf { Data = boundaryNodeData })
            {
                var viewModel = view.DataContext as Model1DBoundaryNodeDataViewWpfViewModel;
                Assert.NotNull(viewModel);

                viewModel.TemperatureConstant = boundaryNodeData.TemperatureConstant + 0.555;
                Assert.AreEqual(viewModel.TemperatureConstant, boundaryNodeData.TemperatureConstant, 0.001);
            }
        }

    }
}