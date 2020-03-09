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
    public class SobekSaltGlobalDispersionReaderTest
    {
        [Test]
        public void ReadThatcherHarlemannFromNatSobek()
        {
            var source =
                @"GLDS op 2 ty 2 f1 9.9999e+009 f2 9.9999e+009 f3 9.9999e+009 f4 9.9999e+009 DSPN id '5576437' nm '(null)' ci '-1' ty 0 f1 200 f2 9.9999e+009 f3 0.01 f4 0.02 dl lt dspn" +
                Environment.NewLine +
                @"glds";

            var globalDispersion = new SobekSaltGlobalDispersionReader().Parse(source).First();
            Assert.AreEqual(DispersionOptionType.ThatcherHarlemann, globalDispersion.DispersionOptionType);
            Assert.AreEqual(9.9999e+009, globalDispersion.F1, 1.0e-6);
            Assert.AreEqual(9.9999e+009, globalDispersion.F2, 1.0e-6);
            Assert.AreEqual(9.9999e+009, globalDispersion.F3, 1.0e-6);
            Assert.AreEqual(9.9999e+009, globalDispersion.F4, 1.0e-6);
            Assert.AreEqual(DispersionType.FunctionOfPlace, globalDispersion.DispersionType);
            Assert.AreEqual(DispersionType.Constant, globalDispersion.SobekSaltLocalDispersion.DispersionType);
            var localDispersion = globalDispersion.SobekSaltLocalDispersion;
            Assert.AreEqual(200.0, localDispersion.F1, 1.0e-6);
            Assert.AreEqual(9.9999e+009, localDispersion.F2, 1.0e-6);
            Assert.AreEqual(0.01, localDispersion.F3, 1.0e-6);
            Assert.AreEqual(0.02, localDispersion.F4, 1.0e-6);
        }

        [Test]
        public void ReadGlobalDispersionFromZoutig()
        {
            const string source = @"GLDS op 0 ty 2 f1 9.9999e+009 f2 9.9999e+009 f3 9.9999e+009 f4 9.9999e+009 glds";
            var globalDispersion = new SobekSaltGlobalDispersionReader().Parse(source).First();
            Assert.IsNull(globalDispersion.SobekSaltLocalDispersion);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category("Quarantine")]
        public void ReadFile()
        {
            // see also test ReadThatcherHarlemann; some values modified for test
            var path = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowFMModelImporterTest).Assembly, @"ReModels\NatSobek.sbk\6\DEFDIS.1");
            var globalDispersion = new SobekSaltGlobalDispersionReader().Read(path).First();
            Assert.AreEqual(DispersionOptionType.ThatcherHarlemann, globalDispersion.DispersionOptionType);
            var localDispersion = globalDispersion.SobekSaltLocalDispersion;
            Assert.AreEqual(0.0, localDispersion.F4, 1.0e-6);
        }
    }
}

