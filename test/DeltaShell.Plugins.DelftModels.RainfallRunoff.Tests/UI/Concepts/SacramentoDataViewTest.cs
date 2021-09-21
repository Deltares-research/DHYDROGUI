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
    public class SacramentoDataViewTest
    {
        [Test]
        public void ShowEmpty()
        {
            var sacramentoView = new SacramentoDataView { Data = null };
            WindowsFormsTestHelper.ShowModal(sacramentoView);
        }

        [Test]
        public void ShowWithData()
        {
            var sacramentoData = new SacramentoData(new Catchment()) { MeteoStationName = "station" };

            var sacramentoDataView = new SacramentoDataView
                {
                    Data = sacramentoData,
                    MeteoStations = new EventedList<string>(new[] { "station", "station2" }),
                    UseMeteoStations = true
                };
            WindowsFormsTestHelper.ShowModal(sacramentoDataView);
        }
    }
}