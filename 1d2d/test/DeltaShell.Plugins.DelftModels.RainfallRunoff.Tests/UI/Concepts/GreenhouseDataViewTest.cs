using System.Threading;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI.Concepts
{
    [TestFixture, Apartment(ApartmentState.STA)]
    [Category(TestCategory.WindowsForms)]
    public class GreenhouseDataViewTest
    {
        [Test]
        public void ShowEmpty()
        {
            var greenhouseDataView = new GreenhouseDataView { Data = null };
            WindowsFormsTestHelper.ShowModal(greenhouseDataView);
        }

        [Test]
        public void ShowWithData()
        {
            var greenhouseData = new GreenhouseData(new Catchment())
                {
                    SurfaceLevel = 1.1,
                    MaximumRoofStorage = 3.3,
                    InitialRoofStorage = 2.2,
                    SubSoilStorageArea = 4.4,
                    SiloCapacity = 5.5,
                    PumpCapacity = 0.89
                };

            greenhouseData.AreaPerGreenhouse[GreenhouseEnums.AreaPerGreenhouseType.lessThan500] = 33.3;
            greenhouseData.AreaPerGreenhouse[GreenhouseEnums.AreaPerGreenhouseType.from1000to1500] = 22.2;
            greenhouseData.AreaPerGreenhouse[GreenhouseEnums.AreaPerGreenhouseType.from2500to3000] = 11.1;
            greenhouseData.AreaPerGreenhouse[GreenhouseEnums.AreaPerGreenhouseType.moreThan6000] = 5.5;

            var greenhouseDataView = new GreenhouseDataView { Data = greenhouseData };
            WindowsFormsTestHelper.ShowModal(greenhouseDataView);
        }
    }
}