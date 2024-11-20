using System.Collections.Generic;
using System.Windows.Forms;
using DelftTools.TestUtils;
using DelftTools.Utils.PropertyBag.Dynamic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI.Forms
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    public class MeteoDataPropertiesTest
    {
        [Test]
        [TestCaseSource(nameof(MeteoDataCases))]
        public void ShowEmpty(MeteoData meteoData)
        {
            WindowsFormsTestHelper.ShowModal(new PropertyGrid
            {
                SelectedObject = new DynamicPropertyBag(new MeteoDataProperties
                {
                    Data = meteoData
                })
            });
        }
        
        private static IEnumerable<MeteoData> MeteoDataCases()
        {
            yield return new EvaporationMeteoData();
            yield return new PrecipitationMeteoData();
            yield return new TemperatureMeteoData();
        }
    }
}
