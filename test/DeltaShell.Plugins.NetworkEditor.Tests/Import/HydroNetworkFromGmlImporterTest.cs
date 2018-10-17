using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Import;
using GeoAPI.Geometries;
using NetTopologySuite.IO.GML2;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Import
{
    [TestFixture]
    public class HydroNetworkFromGmlImporterTest
    {
        [Test]
        public void ImportNetworkFromGml()
        {
            var path = TestHelper.GetTestFilePath("hydroobject.gml");
            var importer = new HydroNetworkFromGmlImporter();
            var hydroNetwork = (HydroNetwork)importer.ImportItem(path);

            Assert.Greater(hydroNetwork.Channels.Count(), 0);
        }

        [Test]
        public void TestPointWithBlankAfterCoordinates()
        {
            //const string gml = "<gml:Point srsName=\"SDO:8265\" xmlns:gml=\"http://www.opengis.net/gml\"><gml:coordinates decimal=\".\" cs=\",\" ts=\" \">-89.5589359049658,44.535657997424 </gml:coordinates></gml:Point>";
            const string gml = "<nhi:FeatureCollection xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:gml=\"http://www.opengis.net/gml\" xmlns:nhi=\"http://www.nhi.nu/gml\"><gml:boundedBy><gml:Box srsName = \"EPSG:28992\"><gml:coordinates>123960.193999998,359289.137 177609.756000001,416086.979000001</gml:coordinates></gml:Box></gml:boundedBy></nhi:FeatureCollection>";
            var reader = new GMLReader();
            var geom = reader.Read(gml);

            Assert.IsNotNull(geom);
            Assert.IsInstanceOf<IPoint>(geom);
        }
    }
}