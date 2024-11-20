using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekRrReaders
{

    [TestFixture]
    public class SobekRROpenWaterReaderTest
    {
        //OWRR id 'rr-ownode' ar 10000 ms 'meteostation' aaf 0.9 owrr

        //id 	= id of node
	    //ar 	= area of node (m2)
	    //ms 	= meteostation 
	    //aaf = area adjustment factor

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadOpenWaterForDRRLineFromGeert()
        {
            string line =
                @"OWRR id 'rr-ownode' ar 10000 ms 'meteostation' aaf 0.9 owrr";

            var rrOpenWater = new SobekRROpenWaterReader().Parse(line).First();
            Assert.AreEqual("rr-ownode", rrOpenWater.Id);
            Assert.AreEqual(10000.0, rrOpenWater.Area);
            Assert.AreEqual("meteostation", rrOpenWater.MeteoStationId);
            Assert.AreEqual(0.9, rrOpenWater.AreaAjustmentFactor);
            Assert.AreEqual(RationalMethodType.None, rrOpenWater.MethodType);

        }
    }
}

