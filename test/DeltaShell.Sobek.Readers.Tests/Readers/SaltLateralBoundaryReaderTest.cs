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
    public class SaltLateralBoundaryReaderTest
    {
        [Test]
        public void ReadSaltLateralConcentrationConstant()
        {
            const string saltBoundaryText = @"STBR id '5576457' ty 1 le 0.11 lo lt 0 9.9999e+009 9.9999e+009 di 'P_22' co ct 0 0.2 9.9999e+009 stbr";

            var saltBoundary = new SaltLateralBoundaryReader().GetSobekSaltBoundary(saltBoundaryText);

            Assert.AreEqual("5576457", saltBoundary.Id);
            Assert.AreEqual("P_22", saltBoundary.LateralId);
            // ty 1 = 
            Assert.AreEqual(SaltBoundaryType.Concentration, saltBoundary.SaltBoundaryType);
            Assert.AreEqual(SaltStorageType.Constant, saltBoundary.SaltStorageType);
            Assert.AreEqual(0.11, saltBoundary.Length, 1.0e-6);
            Assert.AreEqual(0.2, saltBoundary.ConcentrationConst, 1.0e-6);
        }

        [Test]
        public void ReadSaltLateralConcentrationTimeSeries()
        {
            var saltBoundaryText =
                @"STBR id '17' ty 1 le 0 lo lt 0 9.9999e+009 9.9999e+009 di '15' co ct 1 9.9999e+009 9.9999e+009 'Salt concentration (kg/m3)' PDIN 0 0 '' pdin CLTT 'Time' 'Concentration' cltt CLID '(null)' '(null)' clid TBLE " +
                Environment.NewLine +
                @"'1994/01/01;00:01:00' 23 < " + Environment.NewLine +
                @"'1994/01/02;00:01:00' 34 < " + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @" stbr";

            var saltBoundary = new SaltLateralBoundaryReader().GetSobekSaltBoundary(saltBoundaryText);

            Assert.AreEqual("17", saltBoundary.Id);
            Assert.AreEqual("15", saltBoundary.LateralId);
            // ty 1 = 
            Assert.AreEqual(SaltBoundaryType.Concentration, saltBoundary.SaltBoundaryType);
            Assert.AreEqual(SaltStorageType.FunctionOfTime, saltBoundary.SaltStorageType);
            Assert.AreEqual(0.0, saltBoundary.Length, 1.0e-6);
            Assert.AreEqual(2, saltBoundary.ConcentrationTable.Rows.Count);
            Assert.AreEqual(new DateTime(1994, 1, 1, 0, 1, 0), saltBoundary.ConcentrationTable.Rows[0][0]);
            Assert.AreEqual(23.0, saltBoundary.ConcentrationTable.Rows[0][1]);
            Assert.AreEqual(new DateTime(1994, 1, 2, 0, 1, 0), saltBoundary.ConcentrationTable.Rows[1][0]);
            Assert.AreEqual(34.0, saltBoundary.ConcentrationTable.Rows[1][1]);
        }

        [Test]
        public void ReadSaltLateralDryLoadTimeSeries()
        {
            var saltBoundaryText =
                @"STBR id '21' ty 0 le 56 lo lt 1 9.9999e+009 9.9999e+009 'Dry load (kg/s)' PDIN 0 0 '' pdin CLTT 'Time' 'Load' cltt CLID '(null)' '(null)' clid TBLE " +
                Environment.NewLine +
                @"'1994/02/01;00:01:00' 101 < " + Environment.NewLine +
                @"'1994/03/01;00:01:00' 102 < " + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @"di '-1' co ct 0 9.9999e+009 9.9999e+009 stbr";

            var saltBoundary = new SaltLateralBoundaryReader().GetSobekSaltBoundary(saltBoundaryText);

            // ty 1 = 
            Assert.AreEqual(SaltBoundaryType.DrySubstance, saltBoundary.SaltBoundaryType);
            Assert.AreEqual(SaltStorageType.FunctionOfTime, saltBoundary.SaltStorageType);
            Assert.AreEqual(56.0, saltBoundary.Length, 1.0e-6);
            Assert.AreEqual(2, saltBoundary.DryLoadTable.Rows.Count);
            Assert.AreEqual(new DateTime(1994, 2, 1, 0, 1, 0), saltBoundary.DryLoadTable.Rows[0][0]);
            Assert.AreEqual(101.0, saltBoundary.DryLoadTable.Rows[0][1]);
            Assert.AreEqual(new DateTime(1994, 3, 1, 0, 1, 0), saltBoundary.DryLoadTable.Rows[1][0]);
            Assert.AreEqual(102.0, saltBoundary.DryLoadTable.Rows[1][1]);
        }


        [Test]
        [Category(TestCategory.Integration)]
        [Category("Quarantine")]
        public void ReadSaltBoundaryFile()
        {
            var path = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowFMModelImporterTest).Assembly, @"ReModels\NatSobek.sbk\6\DEFCND.7");
            var saltBoundaries = new SaltLateralBoundaryReader().Read(path);
            Assert.AreEqual(8, saltBoundaries.Count());
            var saltBoundary = saltBoundaries.Where(sb => sb.Id == "5576465").FirstOrDefault();
            Assert.IsNotNull(saltBoundary);
            Assert.AreEqual(0.2, saltBoundary.ConcentrationConst, 1.0e-6);
        }
    }
}
