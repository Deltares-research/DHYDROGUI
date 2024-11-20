using System.Windows.Forms;
using DelftTools.TestUtils;
using DelftTools.Utils.PropertyBag.Dynamic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.PropertyClasses;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI
{
    [TestFixture]
    public class RainfallRunoffOutputPropertiesTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowRainfallRunoffOutputSetting()
        {
            WindowsFormsTestHelper.ShowModal(new PropertyGrid
                {
                    SelectedObject = new DynamicPropertyBag(new RainfallRunoffOutputSettingsProperties
                        {
                            Data = new RainfallRunoffModel()
                        })
                });
        }
    }
}
