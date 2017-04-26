using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Forms.Properties
{
    [TestFixture]
    public class RealTimeControlModelPropertiesTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowProperties()
        {
            WindowsFormsTestHelper.ShowPropertyGridForObject(new RealTimeControlModelProperties { Data = new RealTimeControlModel() });
        }
    }
}