using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests
{
    [TestFixture]
    public class SobekNetworkImporterPerformanceTest
    {
        [Test]
        [Category(TestCategory.Performance)]
        [Category(TestCategory.Slow)]
        public void ReadPoNetwork()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\POup_GV.lit\7\network.tp";
            var importer = new SobekNetworkImporter();

            HydroNetwork network = null;

            TestHelper.AssertIsFasterThan(28000, () => network = (HydroNetwork)importer.ImportItem(pathToSobekNetwork));

            var channelsWithoutCrossSections = network.Channels.Where(c => !c.CrossSections.Any());

            //linkagenode -> branches with same order number: interpolation/extrapolation cross-sections over node
            foreach (var channelWithoutCrossSection in channelsWithoutCrossSections)
            {
                Assert.Greater(
                    network.Channels.Count(c => c.OrderNumber == channelWithoutCrossSection.OrderNumber),
                    1,
                    string.Format("Channel {0} should have the same order number as another branch for extra/interpolation of cross-section", channelWithoutCrossSection.Name)
                );
            }
        }
    }
}