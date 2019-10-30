using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.FMSuite.Common.Gui.Forms;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.Forms
{
    [TestFixture]
    public class WaterFlowModel1DLateralSourceDataViewWpfViewModelTest
    {
        private Model1DLateralSourceData lateralSourceData;
        
        [SetUp]
        public void Setup()
        {
            lateralSourceData = WaterFlowModel1DBoundaryAndLateralWpfViewHelper.GetStandardLateralSourceData();   
        }

        [Category(TestCategory.DataAccess)]
        [Test]
        public void TestChangeFlowDataTypeInViewModelIsReflectedInObjectModel()
        {
            using (var view = new Model1DLateralSourceDataViewWpf {Data = lateralSourceData})
            {
                var viewModel = view.DataContext as Model1DLateralSourceDataViewWpfViewModel;
                Assert.NotNull(viewModel);

                viewModel.LateralDischargeDataType = Model1DLateralDataType.FlowConstant;
                Assert.IsTrue(lateralSourceData.DataType == Model1DLateralDataType.FlowConstant);

                viewModel.LateralDischargeDataType = Model1DLateralDataType.FlowWaterLevelTable;
                Assert.IsTrue(lateralSourceData.DataType == Model1DLateralDataType.FlowWaterLevelTable);

                viewModel.LateralDischargeDataType = Model1DLateralDataType.FlowTimeSeries;
                Assert.IsTrue(lateralSourceData.DataType == Model1DLateralDataType.FlowTimeSeries);
            }
        }

        [Category(TestCategory.DataAccess)]
        [Test]
        public void TestChangeSaltDataTypeInViewModelIsReflectedInObjectModel()
        {
            using (var view = new Model1DLateralSourceDataViewWpf { Data = lateralSourceData })
            {
                var viewModel = view.DataContext as Model1DLateralSourceDataViewWpfViewModel;
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
            using (var view = new Model1DLateralSourceDataViewWpf { Data = lateralSourceData })
            {
                var viewModel = view.DataContext as Model1DLateralSourceDataViewWpfViewModel;
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
            using (var view = new Model1DLateralSourceDataViewWpf {Data = lateralSourceData})
            {
                var viewModel = view.DataContext as Model1DLateralSourceDataViewWpfViewModel;
                Assert.NotNull(viewModel);

                viewModel.Flow = lateralSourceData.Flow + 0.555;
                Assert.AreEqual(viewModel.Flow, lateralSourceData.Flow, 0.001);
            }
        }

        [Category(TestCategory.DataAccess)]
        [Test]
        public void TestUpdateConstantSaltDataInViewModelIsReflectedInObjectModel()
        {
            using (var view = new Model1DLateralSourceDataViewWpf { Data = lateralSourceData })
            {
                var viewModel = view.DataContext as Model1DLateralSourceDataViewWpfViewModel;
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
            using (var view = new Model1DLateralSourceDataViewWpf { Data = lateralSourceData })
            {
                var viewModel = view.DataContext as Model1DLateralSourceDataViewWpfViewModel;
                Assert.NotNull(viewModel);

                viewModel.TemperatureConstant = lateralSourceData.TemperatureConstant + 0.555;
                Assert.AreEqual(viewModel.TemperatureConstant, lateralSourceData.TemperatureConstant, 0.001);
            }
        }
    }
}