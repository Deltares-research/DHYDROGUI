using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using NUnit.Framework;
using SharpTestsEx;

namespace DeltaShell.Plugins.NetworkEditor.Tests
{
    [TestFixture]
    public class NetworkEditorApplicationPluginTest
    {
        [Test]
        [Category(TestCategory.Integration)]
        public void AddChildRegionsWithUniqueNames()
        {
            // Create an application plugin instance
            var applicationPlugin = new NetworkEditorApplicationPlugin();

            // Obtain the data item info for creating hydro networks
            var dataItemInfo = applicationPlugin.GetDataItemInfos().First(dii => dii.ValueType == typeof (HydroNetwork));

            // Create two hydro networks based on the data item info
            var region = new HydroRegion();
            var hydroNetwork1 = (HydroNetwork) dataItemInfo.CreateData(region);
            var hydroNetwork2 = (HydroNetwork) dataItemInfo.CreateData(region);

            // The name of the second network should differ from the name of the first network
            hydroNetwork1.Name.Should().Not.Be.EqualTo(hydroNetwork2.Name);
        }
    }
}