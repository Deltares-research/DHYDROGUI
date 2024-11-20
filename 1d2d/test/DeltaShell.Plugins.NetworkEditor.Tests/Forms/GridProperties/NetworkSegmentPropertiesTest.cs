using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.GridProperties
{
    [TestFixture]
    public class NetworkSegmentPropertiesTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowProperties()
        {
            WindowsFormsTestHelper.ShowPropertyGridForObject(new NetworkSegmentProperties { Data = new NetworkSegment() });
        }
    }
}