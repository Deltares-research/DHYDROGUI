using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.GridProperties
{
    [TestFixture]
    public class NetworkCoveragePropertiesTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowProperties()
        {
            WindowsFormsTestHelper.ShowPropertyGridForObject(new NetworkCoverageProperties { Data = new NetworkCoverage() });
        }
    }
}