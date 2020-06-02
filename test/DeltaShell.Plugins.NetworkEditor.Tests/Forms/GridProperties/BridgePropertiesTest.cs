using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.GridProperties
{
    [TestFixture]
    public class BridgePropertiesTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowProperties()
        {
            WindowsFormsTestHelper.ShowPropertyGridForObject(new BridgeProperties {Data = new Bridge()});
        }
    }
}