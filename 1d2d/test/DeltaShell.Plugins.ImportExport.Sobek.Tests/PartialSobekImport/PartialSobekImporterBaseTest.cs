using System;
using System.IO;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using DeltaShell.Sobek.Readers;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.PartialSobekImport
{
    [TestFixture]
    public class PartialSobekImporterBaseTest
    {
        [Test]
        public void GetFilePath()
        {
            var tempDir = Path.GetTempPath();
            var path = Path.Combine(tempDir,"Network.TP");
            var fileName = "Hahaha.haha";

            var partialSobekImporterBaseTestClass = new PartialSobekImporterBaseTestClass();

            partialSobekImporterBaseTestClass.PathSobek = path;

            var getFilePath = partialSobekImporterBaseTestClass.GetFilePathTest(fileName);

            Assert.AreEqual(Path.Combine(tempDir,fileName), getFilePath);
        }

        [Test]
        public void SobekType212()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\network1\network.tp";

            var partialSobekImporterBaseTestClass = new PartialSobekImporterBaseTestClass();

            partialSobekImporterBaseTestClass.PathSobek = pathToSobekNetwork;

            Assert.AreEqual(SobekType.Sobek212,partialSobekImporterBaseTestClass.SobekType);
            Assert.AreEqual(SobekType.Sobek212, partialSobekImporterBaseTestClass.SobekFileNames.SobekType);
        }

        [Test]
        public void SobekTypeRE()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\ReModels\J_10BANK.sbk\4\DEFTOP.1";

            var partialSobekImporterBaseTestClass = new PartialSobekImporterBaseTestClass();

            partialSobekImporterBaseTestClass.PathSobek = pathToSobekNetwork;

            Assert.AreEqual(SobekType.SobekRE, partialSobekImporterBaseTestClass.SobekType);
            Assert.AreEqual(SobekType.SobekRE, partialSobekImporterBaseTestClass.SobekFileNames.SobekType);
        }

        [Test]
        public void IsActiveTest()
        {
            var activeSobekPartialImporter = new PartialSobekImporterBaseTestClass{ IsActive = true };
            var nonActiveSobekPartialImporter = new PartialSobekImporterBaseTestClass{ IsActive = false };

            activeSobekPartialImporter.Import();
            nonActiveSobekPartialImporter.Import();

            Assert.AreEqual(0, nonActiveSobekPartialImporter.PartialImportCount);
            Assert.AreEqual(1, activeSobekPartialImporter.PartialImportCount);

        }

        [Test]
        public void SetTargetSourceShouldBubbleDownTheChain()
        {
            var firstSobekPartialImporter = new PartialSobekImporterBaseTestClass();
            var secondSobekPartialImporter = new PartialSobekImporterBaseTestClass { PartialSobekImporter = firstSobekPartialImporter };
            var targetObject = "haha";

            secondSobekPartialImporter.TargetObject = targetObject;

            Assert.AreSame(targetObject, firstSobekPartialImporter.TargetObject);

        }
    }


    public class PartialSobekImporterBaseTestClass: PartialSobekImporterBase
    {
        private int partialImportCount;
 
        protected override void PartialImport()
        {
            partialImportCount++;
        }

        public int PartialImportCount
        {
            get
            {
                return partialImportCount;
            }
        }

        public string GetFilePathTest(string fileName)
        {
            return GetFilePath(fileName);
        }

        public SobekType SobekType
        {
            get
            {
                return base.SobekType;
            }
        }

        public override string DisplayName
        {
            get { throw new NotImplementedException(); }
        }

        public override SobekImporterCategories Category { get; }

        public SobekFileNames SobekFileNames
        {
            get
            {
                return base.SobekFileNames;
            }
        }           

    }
}
