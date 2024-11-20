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
    public class PavedDataViewTest
    {
        [Test]
        public void ShowEmpty()
        {
            var pavedDataView = new PavedDataView() { Data = null };
            WindowsFormsTestHelper.ShowModal(pavedDataView);
        }

        [Test]
        public void ShowWithRandomData()
        {
            var pavedData = new PavedData(new Catchment());

            ReflectionTestHelper.FillRandomValuesForValueTypeProperties(pavedData, new string[]
            {
                nameof(pavedData.DryWeatherFlowOptions),
                nameof(pavedData.DryWeatherFlowSewerPumpDischarge),
                nameof(pavedData.MixedAndOrRainfallSewerPumpDischarge),
                nameof(pavedData.SewerType),
                nameof(pavedData.SpillingDefinition),
            });

            var pavedDataView = new PavedDataView { Data = pavedData };
            WindowsFormsTestHelper.ShowModal(pavedDataView);
        }

        [Test]
        public void ShowWithData()
        {
            var pavedData = new PavedData(new Catchment());
            pavedData.SewerType = PavedEnums.SewerType.ImprovedSeparateSystem;
            pavedData.DryWeatherFlowOptions = PavedEnums.DryWeatherFlowOptions.VariableDWF;
            
            var pavedDataView = new PavedDataView { Data = pavedData };
            WindowsFormsTestHelper.ShowModal(pavedDataView);
        }
    }
}