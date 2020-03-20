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
    public class SobekSaltLocalDispersionReaderTest
    {
        [Test]
        public void ReadConstant()
        {
            const string source = @"DSPN id '77' nm '(null)' ci '78' ty 0 f1 50 f2 9.9999e+009 f3 0 f4 0.006 dl lt dspn";

            // Option type formulation is set in GLDS record; DSPN has values id type is function of place
            var localDispersion = SobekSaltLocalDispersionReader.GetLocalDispersion(source);
            Assert.AreEqual("77", localDispersion.Id);
            Assert.AreEqual("(null)", localDispersion.Name);
            Assert.AreEqual("78", localDispersion.BranchId);
            Assert.AreEqual(50.0, localDispersion.F1, 1.0e-6);
            Assert.AreEqual(9.9999e+009, localDispersion.F2, 1.0e-6);
            Assert.AreEqual(0.0, localDispersion.F3, 1.0e-6);
            Assert.AreEqual(0.006, localDispersion.F4, 1.0e-6);
        }

        [Test]
        public void ReadFunctionOfPace()
        {
            string source =
                @"DSPN id '97' nm '(null)' ci '98' ty 2 f1 0 f2 9.9999e+009 f3 1 f4 0 dl lt 'Dispersion' PDIN 0 0 '' pdin CLTT 'Location' 'F1' 'F2' 'F3' 'F4' cltt CLID '(null)' '(null)' '(null)' '(null)' '(null)' clid TBLE " +
                Environment.NewLine +
                @"0 100 9.9999e+009 0 0 < " + Environment.NewLine +
                @"6400 100 9.9999e+009 0 0 < " + Environment.NewLine +
                @"6401 1 9.9999e+009 0.11 0.12 < " + Environment.NewLine +
                @"6412 1 9.9999e+009 0 0 < " + Environment.NewLine +
                @"7611 101 9.9999e+009 1 0 < " + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @" dspn";
            var localDispersion = SobekSaltLocalDispersionReader.GetLocalDispersion(source);
            Assert.AreEqual("97", localDispersion.Id);
            Assert.AreEqual("(null)", localDispersion.Name);
            Assert.AreEqual("98", localDispersion.BranchId);
            Assert.AreEqual(0.0, localDispersion.F1, 1.0e-6);
            Assert.AreEqual(9.9999e+009, localDispersion.F2, 1.0e-6);
            Assert.AreEqual(1.0, localDispersion.F3, 1.0e-6);
            Assert.AreEqual(0.0, localDispersion.F4, 1.0e-6);
            Assert.AreEqual(DispersionType.FunctionOfPlace, localDispersion.DispersionType);
            Assert.AreEqual(5, localDispersion.Data.Rows.Count);
            Assert.AreEqual(0.0, (double)localDispersion.Data.Rows[0][0], 1.0e-6);
            Assert.AreEqual(100, (double)localDispersion.Data.Rows[0][1], 1.0e-6);

            Assert.AreEqual(6401.0, (double)localDispersion.Data.Rows[2][0], 1.0e-6);
            Assert.AreEqual(1.0, (double)localDispersion.Data.Rows[2][1], 1.0e-6);
            Assert.AreEqual(9.9999e+009, (double)localDispersion.Data.Rows[2][2], 1.0e-6);
            Assert.AreEqual(0.11, (double)localDispersion.Data.Rows[2][3], 1.0e-6);
            Assert.AreEqual(0.12, (double)localDispersion.Data.Rows[2][4], 1.0e-6);

            Assert.AreEqual(7611.0, (double)localDispersion.Data.Rows[4][0], 1.0e-6);
            Assert.AreEqual(101, (double)localDispersion.Data.Rows[4][1], 1.0e-6);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ReadFile()
        {
            var path = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowFMModelImporterTest).Assembly, @"ReModels\NatSobek.sbk\6\DEFDIS.2");
            var localDispersions = new SobekSaltLocalDispersionReader().Read(path);
            Assert.AreEqual(134, localDispersions.Count());
            var localDispersion = localDispersions.Where(sb => sb.Id == "5591118").FirstOrDefault();
            Assert.IsNotNull(localDispersion);
            Assert.AreEqual(DispersionType.Constant, localDispersion.DispersionType);
            Assert.AreEqual(50.0, localDispersion.F1, 1.0e-6);
        }

    }
}