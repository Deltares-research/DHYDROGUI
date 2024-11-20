using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Sobek.Readers.Readers;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class SobekProfileDatFileReaderTest
    {
        [Test]
        public void ReadProfileDat()
        {
            var str = "CRSN id 'KA45' di '294' rl 0 rs 109.03 crsn\n" +
                      "CRSN id 'KA44' di '293' rl 0 rs 106.89 crsn\n" +
                      "CRSN id 'KA43' di '292' rl 0 rs 104.96 crsn\n" +
                      "CRSN id 'KA42' di '291' rl 0 rs 101.89 crsn\n" +
                      "CRSN id 'KA41' di '290' rl 0 rs 94.4 crsn";

            var reader = new SobekProfileDatFileReader();
            var mappings = reader.Parse(str).ToList();
            Assert.AreEqual(5, mappings.Count);
            Assert.AreEqual("KA43", mappings[2].LocationId);
            Assert.AreEqual("292", mappings[2].DefinitionId);
            Assert.AreEqual(0.0, mappings[2].RefLevel1);
            Assert.AreEqual(104.96, mappings[2].SurfaceLevelRight);
        }

        [Test]
        public void ReadProfileDatForFHM()
        {
            var path = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly, @"FHM2011F.lit\1\PROFILE.DAT");
            var reader = new SobekProfileDatFileReader();
            var mappings = reader.Read(path).ToList();
            Assert.AreEqual(2552, mappings.Count);

            //3: CRSN id 'KA43' di '292' rl 0 rs 104.96 crsn
            //2477: CRSN id 'CP39' di 'Sun_39.*_1' rl 0.75 rs 11.42 crsn
            
            Assert.AreEqual("KA43", mappings[2].LocationId);
            Assert.AreEqual("292", mappings[2].DefinitionId);
            Assert.AreEqual(0.0, mappings[2].RefLevel1);
            Assert.AreEqual(104.96, mappings[2].SurfaceLevelRight);

            Assert.AreEqual("CP39", mappings[2476].LocationId);
            Assert.AreEqual("Sun_39.*_1", mappings[2476].DefinitionId);
            Assert.AreEqual(0.75, mappings[2476].RefLevel1);
            Assert.AreEqual(11.42, mappings[2476].SurfaceLevelRight);
        }

        [Test]
        public void ReadProfileDatForPo()
        {
            var path = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly, @"POup_GV.lit\7\PROFILE.DAT");
            var reader = new SobekProfileDatFileReader();
            var mappings = reader.Read(path).ToList();
            Assert.AreEqual(4195, mappings.Count);

            //433: CRSN id 'P_Dora di La Thuile_0' di 'P_Dora di La Thuile_0' rl 0 rs 996 us 9.9999e+009 ds 9.9999e+009                      crsn

            var mapping = mappings[433];
            Assert.AreEqual("P_Dora di La Thuile_0", mapping.LocationId);
            Assert.AreEqual("P_Dora di La Thuile_0", mapping.DefinitionId); //difficult because 'di' appears in the id
            Assert.AreEqual(0, mapping.RefLevel1);
            Assert.AreEqual(996, mapping.SurfaceLevelRight);
        }

        [Test]
        public void TestReModel()
        {
            var path = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly, @"ReModels\J_10BANK.sbk\4\DEFCRS.3");
            var reader = new SobekProfileDatFileReader();
            var mappings = reader.Read(path).ToList();
            Assert.AreEqual(810, mappings.Count);

            //3: CRSN id '1003' di '1003' rl 0 us 9.9999e+009 ds 9.9999e+009 crsn
            //803: CRSN id 'RWZ020-RWZ021_29' di 'RWZ020-RWZ021_29' rl 0 us 9.9999e+009 ds 9.9999e+009 crsn

            Assert.AreEqual("1003", mappings[2].LocationId);
            Assert.AreEqual("1003", mappings[2].DefinitionId);
            Assert.AreEqual(0.0, mappings[2].RefLevel1);
            Assert.AreEqual(9999900000, mappings[2].UpstreamSlope);
            Assert.AreEqual(9999900000, mappings[2].DownstreamSlope);

            Assert.AreEqual("RWZ020-RWZ021_29", mappings[802].LocationId);
            Assert.AreEqual("RWZ020-RWZ021_29", mappings[802].DefinitionId);
            Assert.AreEqual(0.0, mappings[802].RefLevel1);
            Assert.AreEqual(9999900000, mappings[802].UpstreamSlope);
            Assert.AreEqual(9999900000, mappings[802].DownstreamSlope);
        }
    }
}