using System.Threading;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Unpaved;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI.Concepts
{
    [TestFixture, Apartment(ApartmentState.STA)]
    [Category(TestCategory.WindowsForms)]
    public class UnpavedDataViewTest
    {
        [Test]
        public void ShowEmpty()
        {
            var unpavedDataView = new UnpavedDataView {Data = null};
            WindowsFormsTestHelper.ShowModal(unpavedDataView);
        }

        [Test]
        public void ShowWithData()
        {
            var unpavedData = new UnpavedData(new Catchment());

            unpavedData.AreaPerCrop[UnpavedEnums.CropType.Grain] = 1.0;
            unpavedData.AreaPerCrop[UnpavedEnums.CropType.GreenhouseArea] = 3.0;
            unpavedData.AreaPerCrop[UnpavedEnums.CropType.Orchard] = 11.0;
            unpavedData.AreaPerCrop[UnpavedEnums.CropType.BulbousPlants] = 34.34;
            unpavedData.AreaPerCrop[UnpavedEnums.CropType.NonArableLand] = 0.5;

            var unpavedDataView = new UnpavedDataView { Data = unpavedData };
            WindowsFormsTestHelper.ShowModal(unpavedDataView);
        }

        [Test]
        public void ShowWithDataWithCatchmentLinkedToNode()
        {
            var catchment = new Catchment();
            catchment.Links.Add(new HydroLink(catchment, new HydroNode()));
            var unpavedData = new UnpavedData(catchment);

            unpavedData.AreaPerCrop[UnpavedEnums.CropType.Grain] = 1.0;
            unpavedData.AreaPerCrop[UnpavedEnums.CropType.GreenhouseArea] = 3.0;
            unpavedData.AreaPerCrop[UnpavedEnums.CropType.Orchard] = 11.0;
            unpavedData.AreaPerCrop[UnpavedEnums.CropType.BulbousPlants] = 34.34;
            unpavedData.AreaPerCrop[UnpavedEnums.CropType.NonArableLand] = 0.5;

            var unpavedDataView = new UnpavedDataView { Data = unpavedData };
            WindowsFormsTestHelper.ShowModal(unpavedDataView);
        }

        [Test]
        public void ShowWithDataWithCatchmentLinkedToRunoffBoundary()
        {
            var catchment = new Catchment();
            catchment.Links.Add(new HydroLink(catchment, new RunoffBoundary()));
            var unpavedData = new UnpavedData(catchment);

            unpavedData.AreaPerCrop[UnpavedEnums.CropType.Grain] = 1.0;
            unpavedData.AreaPerCrop[UnpavedEnums.CropType.GreenhouseArea] = 3.0;
            unpavedData.AreaPerCrop[UnpavedEnums.CropType.Orchard] = 11.0;
            unpavedData.AreaPerCrop[UnpavedEnums.CropType.BulbousPlants] = 34.34;
            unpavedData.AreaPerCrop[UnpavedEnums.CropType.NonArableLand] = 0.5;

            var unpavedDataView = new UnpavedDataView { Data = unpavedData };
            WindowsFormsTestHelper.ShowModal(unpavedDataView);
        }

        [Test]
        public void ShowWithDataAndMeteoStations()
        {
            var unpavedData = new UnpavedData(new Catchment());

            unpavedData.AreaPerCrop[UnpavedEnums.CropType.Grain] = 1.0;
            unpavedData.AreaPerCrop[UnpavedEnums.CropType.GreenhouseArea] = 3.0;
            unpavedData.AreaPerCrop[UnpavedEnums.CropType.Orchard] = 11.0;
            unpavedData.AreaPerCrop[UnpavedEnums.CropType.BulbousPlants] = 34.34;
            unpavedData.AreaPerCrop[UnpavedEnums.CropType.NonArableLand] = 0.5;
            
            var unpavedDataView = new UnpavedDataView { Data = unpavedData };

            unpavedDataView.UseMeteoStations = true;
            unpavedDataView.MeteoStations = new EventedList<string>(new[] {"station 1", "station 2"});

            WindowsFormsTestHelper.ShowModal(unpavedDataView);
        }

        [Test]
        public void ShowWithDataAndSwitchToOtherDrainageFormula()
        {
            var unpavedData = new UnpavedData(new Catchment());

            unpavedData.AreaPerCrop[UnpavedEnums.CropType.Grain] = 1.0;
            unpavedData.AreaPerCrop[UnpavedEnums.CropType.GreenhouseArea] = 3.0;
            unpavedData.AreaPerCrop[UnpavedEnums.CropType.Orchard] = 11.0;
            unpavedData.AreaPerCrop[UnpavedEnums.CropType.BulbousPlants] = 34.34;
            unpavedData.AreaPerCrop[UnpavedEnums.CropType.NonArableLand] = 0.5;

            var unpavedDataView = new UnpavedDataView { Data = unpavedData };
            WindowsFormsTestHelper.ShowModal(unpavedDataView,
                                             f =>
                                             unpavedData.SwitchDrainageFormula<KrayenhoffVanDeLeurDrainageFormula>());
        }

        [TestCase(false)]
        [TestCase(true)]
        public void SetInitialWorkFlowStateTest(bool isRunningParallel)
        {
            var unpavedData = new UnpavedData(new Catchment());
            var unpavedDataView = new TestClassUnpavedDataView { Data = unpavedData };
            
            unpavedDataView.SetInitialWorkFlowState(isRunningParallel);

            var viewModel = unpavedDataView.GetViewModel();
            Assert.AreEqual(isRunningParallel,viewModel.ModelRunningParallelWithFlow);
        }
        
        [TestCase(false)]
        [TestCase(true)]
        public void WorkflowChangedTest(bool isRunningParallel)
        {
            var unpavedData = new UnpavedData(new Catchment());
            var unpavedDataView = new TestClassUnpavedDataView { Data = unpavedData };
            
            unpavedDataView.WorkflowChanged(new object(),isRunningParallel);

            var viewModel = unpavedDataView.GetViewModel();
            Assert.AreEqual(isRunningParallel,viewModel.ModelRunningParallelWithFlow);
        }

        public class TestClassUnpavedDataView : UnpavedDataView
        {
            public UnpavedDataViewModel GetViewModel()
            {
                return base.ViewModel;
            }
        }
    }
}
