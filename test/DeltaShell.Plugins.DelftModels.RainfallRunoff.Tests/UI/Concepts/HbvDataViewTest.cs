using System.Threading;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI.Concepts
{
    [TestFixture, Apartment(ApartmentState.STA)]
    [Category(TestCategory.WindowsForms)]
    class HbvDataViewTest
    {
        [Test]
        public void ShowEmpty()
        {
            var hbvDataView = new HbvDataView { Data = null };
            WindowsFormsTestHelper.ShowModal(hbvDataView);
        }

        [Test]
        public void ShowWithData()
        {
            var hbvData = new HbvData(new Catchment())
                {
                    MeteoStationName = "station",
                    TemperatureStationName = "station3"
                };

            var hbvDataView = new HbvDataView
                {
                    Data = hbvData,
                    MeteoStations = new EventedList<string>(new[] {"station", "station2"}),
                    TemperatureStations = new EventedList<string>(new[] {"station3", "station4"}),
                    UseMeteoStations = true
                };
            WindowsFormsTestHelper.ShowModal(hbvDataView);
        }
    }
}
