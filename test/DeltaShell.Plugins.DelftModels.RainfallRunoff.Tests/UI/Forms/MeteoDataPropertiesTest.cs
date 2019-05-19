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
        public void ShowEmpty()
        {
            WindowsFormsTestHelper.ShowModal(new PropertyGrid
                {
                    SelectedObject = new DynamicPropertyBag(new MeteoDataProperties
                        {
                            Data = new MeteoData(MeteoDataAggregationType.Cumulative) { Name = "HihihiHahaha" }
                        })
                });
        }
    }
}
