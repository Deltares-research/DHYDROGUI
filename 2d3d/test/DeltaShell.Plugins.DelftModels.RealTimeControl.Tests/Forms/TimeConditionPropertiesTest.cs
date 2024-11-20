using System.Windows.Forms;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms.Properties;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Forms
{
    [TestFixture]
    public class TimeConditionPropertiesTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void Show()
        {
            WindowsFormsTestHelper.ShowModal(new PropertyGrid {SelectedObject = new TimeConditionProperties {Data = new TimeCondition()}});
        }
    }
}