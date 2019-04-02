using System;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class SaltNodeBoundaryReaderTest
    {
        [Test]
        public void ReadSaltNodeConcentrationConstant()
        {
            const string saltBoundaryText = @"STBO id '14' ty 1 co co 0 9.9999e+009 9.9999e+009 tl 9.9999e+009 tu 0 stbo";

            var saltBoundary = SaltNodeBoundaryReader.GetSobekSaltBoundary(saltBoundaryText);

            Assert.AreEqual("14", saltBoundary.Id);
            // ty 1 = 
            Assert.AreEqual(SaltBoundaryNodeType.ZeroFlux, saltBoundary.SaltBoundaryNodeType);
            Assert.AreEqual(SaltStorageType.Constant, saltBoundary.SaltStorageType);
            Assert.IsNull(saltBoundary.ConcentrationTable);
        }

        [Test]
        public void ReadSaltNodeConcentrationTimeSeries()
        {
            var saltBoundaryText =
                @"STBO id '13' ty 0 co co 1 9.9999e+009 9.9999e+009 'Salt concentration (kg/m3)' PDIN 0 0 '' pdin CLTT 'Time' 'Concentration' cltt CLID '(null)' '(null)' clid TBLE " +
                Environment.NewLine +
                @"'1994/01/01;00:01:00' 12 < " + Environment.NewLine +
                @"'1994/01/01;00:02:00' 13 < " + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @" tl 9.9999e+009 tu 0 stbo";// +Environment.NewLine +
            //@"STBO id '14' ty 1 co co 0 9.9999e+009 9.9999e+009 tl 9.9999e+009 tu 0 stbo";

            var saltBoundary = SaltNodeBoundaryReader.GetSobekSaltBoundary(saltBoundaryText);

            Assert.AreEqual("13", saltBoundary.Id);
            // ty 1 = 
            Assert.AreEqual(SaltBoundaryNodeType.Concentration, saltBoundary.SaltBoundaryNodeType);
            Assert.AreEqual(SaltStorageType.FunctionOfTime, saltBoundary.SaltStorageType);
            Assert.AreEqual(2, saltBoundary.ConcentrationTable.Rows.Count);
            Assert.AreEqual(new DateTime(1994, 1, 1, 0, 1, 0), saltBoundary.ConcentrationTable.Rows[0][0]);
            Assert.AreEqual(12.0, saltBoundary.ConcentrationTable.Rows[0][1]);
            Assert.AreEqual(new DateTime(1994, 1, 1, 0, 2, 0), saltBoundary.ConcentrationTable.Rows[1][0]);
            Assert.AreEqual(13.0, saltBoundary.ConcentrationTable.Rows[1][1]);
        }

        [Test]
        public void ReadSaltNodeTimeLag()
        {
            var saltBoundaryText = @"STBO id '51' ty 0 co co 0 33.0 9.9999e+009 tl 180 tu 1 stbo";

            var saltBoundary = SaltNodeBoundaryReader.GetSobekSaltBoundary(saltBoundaryText);

            Assert.AreEqual("51", saltBoundary.Id);
            Assert.AreEqual(SaltBoundaryNodeType.Concentration, saltBoundary.SaltBoundaryNodeType);
            Assert.AreEqual(SaltStorageType.Constant, saltBoundary.SaltStorageType);
            Assert.AreEqual(33.0, saltBoundary.ConcentrationConst);
            Assert.AreEqual(180.0, saltBoundary.TimeLag);

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ReadSaltBoundaryFile()
        {
            var path = TestHelper.GetTestDataPath(typeof(SobekWaterFlowModel1DImporterTest).Assembly, @"ReModels\NatSobek.sbk\6\DEFCND.6");
            var saltBoundaries = new SaltNodeBoundaryReader().Read(path);
            Assert.AreEqual(44, saltBoundaries.Count());
            // STBO id 'P_P_5603879' ty 1 co co 0 9.9999e+009 9.9999e+009 tl 9.9999e+009 tu 0 stbo
            var saltBoundary = saltBoundaries.Where(sb => sb.Id == "P_P_5603879").FirstOrDefault();
            Assert.IsNotNull(saltBoundary);
            Assert.AreEqual(SaltBoundaryNodeType.ZeroFlux, saltBoundary.SaltBoundaryNodeType);
        }
    }
}