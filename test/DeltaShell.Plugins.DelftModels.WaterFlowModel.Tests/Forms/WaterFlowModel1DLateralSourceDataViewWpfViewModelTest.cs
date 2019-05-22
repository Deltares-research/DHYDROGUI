using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.Forms
{
    [TestFixture]
    public class WaterFlowModel1DLateralSourceDataViewWpfViewModelTest
    {
        private WaterFlowModel1DLateralSourceData lateralSourceData;
        
        [SetUp]
        public void Setup()
        {
            lateralSourceData = WaterFlowModel1DBoundaryAndLateralWpfViewHelper.GetStandardLateralSourceData();   
        }

        [Category(TestCategory.DataAccess)]
        [Test]
        public void TestChangeFlowDataTypeInViewModelIsReflectedInObjectModel()
        {
            using (var view = new WaterFlowModel1DLateralSourceDataViewWpf {Data = lateralSourceData})
            {
                var viewModel = view.DataContext as WaterFlowModel1DLateralSourceDataViewWpfViewModel;
                Assert.NotNull(viewModel);

                viewModel.LateralDischargeDataType = WaterFlowModel1DLateralDataType.FlowConstant;
                Assert.IsTrue(lateralSourceData.DataType == WaterFlowModel1DLateralDataType.FlowConstant);

                viewModel.LateralDischargeDataType = WaterFlowModel1DLateralDataType.FlowWaterLevelTable;
                Assert.IsTrue(lateralSourceData.DataType == WaterFlowModel1DLateralDataType.FlowWaterLevelTable);

                viewModel.LateralDischargeDataType = WaterFlowModel1DLateralDataType.FlowTimeSeries;
                Assert.IsTrue(lateralSourceData.DataType == WaterFlowModel1DLateralDataType.FlowTimeSeries);
            }
        }

        [Category(TestCategory.DataAccess)]
        [Test]
        public void TestChangeSaltDataTypeInViewModelIsReflectedInObjectModel()
        {
            using (var view = new WaterFlowModel1DLateralSourceDataViewWpf { Data = lateralSourceData })
            {
                var viewModel = view.DataContext as WaterFlowModel1DLateralSourceDataViewWpfViewModel;
                Assert.NotNull(viewModel);

                viewModel.SaltLateralDischargeType = SaltLateralDischargeType.Default;
                Assert.IsTrue(lateralSourceData.SaltLateralDischargeType == SaltLateralDischargeType.Default);

                viewModel.SaltLateralDischargeType = SaltLateralDischargeType.ConcentrationConstant;
                Assert.IsTrue(lateralSourceData.SaltLateralDischargeType == SaltLateralDischargeType.ConcentrationConstant);

                viewModel.SaltLateralDischargeType = SaltLateralDischargeType.ConcentrationTimeSeries;
                Assert.IsTrue(lateralSourceData.SaltLateralDischargeType == SaltLateralDischargeType.ConcentrationTimeSeries);

                viewModel.SaltLateralDischargeType = SaltLateralDischargeType.MassConstant;
                Assert.IsTrue(lateralSourceData.SaltLateralDischargeType == SaltLateralDischargeType.MassConstant);

                viewModel.SaltLateralDischargeType = SaltLateralDischargeType.MassTimeSeries;
                Assert.IsTrue(lateralSourceData.SaltLateralDischargeType == SaltLateralDischargeType.MassTimeSeries);
            }
        }

        [Category(TestCategory.DataAccess)]
        [Test]
        public void TestChangeTemperatureDataTypeInViewModelIsReflectedInObjectModel()
        {
            using (var view = new WaterFlowModel1DLateralSourceDataViewWpf { Data = lateralSourceData })
            {
                var viewModel = view.DataContext as WaterFlowModel1DLateralSourceDataViewWpfViewModel;
                Assert.NotNull(viewModel);

                viewModel.TemperatureDischargeType = TemperatureLateralDischargeType.None;
                Assert.IsTrue(lateralSourceData.TemperatureLateralDischargeType == TemperatureLateralDischargeType.None);

                viewModel.TemperatureDischargeType = TemperatureLateralDischargeType.Constant;
                Assert.IsTrue(lateralSourceData.TemperatureLateralDischargeType == TemperatureLateralDischargeType.Constant);

                viewModel.TemperatureDischargeType = TemperatureLateralDischargeType.TimeDependent;
                Assert.IsTrue(lateralSourceData.TemperatureLateralDischargeType == TemperatureLateralDischargeType.TimeDependent);
            }
        }

        [Category(TestCategory.DataAccess)]
        [Test]
        public void TestUpdateConstantFlowDataInViewModelIsReflectedInObjectModel()
        {
            using (var view = new WaterFlowModel1DLateralSourceDataViewWpf {Data = lateralSourceData})
            {
                var viewModel = view.DataContext as WaterFlowModel1DLateralSourceDataViewWpfViewModel;
                Assert.NotNull(viewModel);

                viewModel.Flow = lateralSourceData.Flow + 0.555;
                Assert.AreEqual(viewModel.Flow, lateralSourceData.Flow, 0.001);
            }
        }

        [Category(TestCategory.DataAccess)]
        [Test]
        public void TestUpdateConstantSaltDataInViewModelIsReflectedInObjectModel()
        {
            using (var view = new WaterFlowModel1DLateralSourceDataViewWpf { Data = lateralSourceData })
            {
                var viewModel = view.DataContext as WaterFlowModel1DLateralSourceDataViewWpfViewModel;
                Assert.NotNull(viewModel);

                viewModel.SaltConcentrationConstant = lateralSourceData.SaltConcentrationDischargeConstant + 0.555;
                Assert.AreEqual(viewModel.SaltConcentrationConstant, lateralSourceData.SaltConcentrationDischargeConstant, 0.001);

                viewModel.SaltMassConstant = lateralSourceData.SaltMassDischargeConstant + 0.555;
                Assert.AreEqual(viewModel.SaltMassConstant, lateralSourceData.SaltMassDischargeConstant, 0.001);
            }
        }

        [Category(TestCategory.DataAccess)]
        [Test]
        public void TestUpdateConstantTemperatureDataInViewModelIsReflectedInObjectModel()
        {
            using (var view = new WaterFlowModel1DLateralSourceDataViewWpf { Data = lateralSourceData })
            {
                var viewModel = view.DataContext as WaterFlowModel1DLateralSourceDataViewWpfViewModel;
                Assert.NotNull(viewModel);

                viewModel.TemperatureConstant = lateralSourceData.TemperatureConstant + 0.555;
                Assert.AreEqual(viewModel.TemperatureConstant, lateralSourceData.TemperatureConstant, 0.001);
            }
        }
    }
}