using System;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Sobek.Readers.Readers;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class SobekCompoundStructureReaderTest
    {
        [Test]
        public void ReadSobekReRecord()
        {
            string source = @"STCM id 'RT2_2237' st " + Environment.NewLine +
                            @"DLST  " + Environment.NewLine +
                            @"'RT2_01' 'RT2_02' 'RT2_2235' " + Environment.NewLine +
                            @"dlst " + Environment.NewLine +
                            @"stcm";
            var compoundStructure = SobekCompoundStructureReader.GetCompoundStructure(source);

            Assert.AreEqual(3, compoundStructure.Structures.Count);
            Assert.AreEqual("RT2_01", compoundStructure.Structures[0]);
            Assert.AreEqual("RT2_02", compoundStructure.Structures[1]);
            Assert.AreEqual("RT2_2235", compoundStructure.Structures[2]);
        }

        [Test]
        public void ReadCompoundStructures()
        {
            var compoundStructureFile = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly, @"compound.lit\2\struct.cmp");
            var compoundStructures = new SobekCompoundStructureReader().Read(compoundStructureFile);
            // the demo network has 1 of each of the 11 main structure types
            Assert.AreEqual(1, compoundStructures.Count());
            var compoundStructure = compoundStructures.FirstOrDefault();
            Assert.AreEqual(2, compoundStructure.Structures.Count);
        }

        [Test]
        public void ReadCompoundFromRe()
        {
            var compoundStructureFile = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly, @"ReModels\RIJN301.SBK\8\defstr.7");
            var compoundStructures = new SobekCompoundStructureReader().Read(compoundStructureFile);
            // the demo network has 1 of each of the 11 main structure types
            Assert.AreEqual(14, compoundStructures.Count());
        }
    }
}
