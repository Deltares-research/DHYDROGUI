using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.SedMor.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.SedMor.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.SedMor.IO
{
    [TestFixture]
    public class MorFileTest
    {
        [Test]
        public void ReadMorFile()
        {
            var path = TestHelper.GetTestFilePath("sedmor\\files\\MorfoFlowO.mor");

            var morFile = new MorFile();
            var morDefinition = morFile.Load(path);

            Assert.AreEqual(0.5, morDefinition.Bed);
            Assert.AreEqual("Tue Aug 07 2012, 17:20:13",
                            morDefinition.Properties[MorProperties.FileCreationDate].Value);
            Assert.AreEqual(1, morDefinition.Boundaries.Count);

            var firstBoundary = morDefinition.Boundaries[0];
            Assert.AreEqual("Eastward", firstBoundary.Name);
            Assert.AreEqual("4", firstBoundary.Properties[MorProperties.IBedCond].GetValueAsString());
            Assert.AreEqual(3, firstBoundary.Properties.Count);
        }

        [Test]
        public void ReadWriteReadMorFile()
        {
            var path = TestHelper.GetTestFilePath("sedmor\\files\\MorfoFlowO.mor");

            var morFile = new MorFile();
            var morDefinition = morFile.Load(path);

            Assert.AreEqual(0.5, morDefinition.Bed);
            Assert.AreEqual(1, morDefinition.Boundaries.Count);
            Assert.AreEqual(3, morDefinition.Boundaries[0].Properties.Count);

            var mor2Path = "new.mor";
            morFile.Save(mor2Path, morDefinition);

            var morFile2 = new MorFile();
            var def2 = morFile2.Load(mor2Path);

            Assert.AreEqual(0.5, def2.Bed);
            Assert.AreEqual(1, def2.Boundaries.Count);
            Assert.AreEqual(3, def2.Boundaries[0].Properties.Count);
            Assert.AreEqual("Eastward", def2.Boundaries[0].Name);
        }

        [Test]
        public void ReadOtherMorFile()
        {
            var path = TestHelper.GetTestFilePath("sedmor\\files\\STFormulaTest.mor");

            var morFile = new MorFile();
            var morDefinition = morFile.Load(path);

            Assert.AreEqual(1.0, morDefinition.Bed);
            Assert.AreEqual("Wed Feb 20 2013, 17:08:20", morDefinition.Properties[MorProperties.FileCreationDate].Value);
            Assert.AreEqual(true, morDefinition.Properties[MorProperties.ShearVelocity].Value);
            Assert.AreEqual(0, morDefinition.Boundaries.Count);
        }

        [Test]
        public void ReadWriteReadOtherMorFile()
        {
            var path = TestHelper.GetTestFilePath("sedmor\\files\\STFormulaTest.mor");

            var morFile = new MorFile();
            var morDefinition = morFile.Load(path);

            var path2 = "new2.mor";
            morFile.Save(path2, morDefinition);

            var morFile2 = new MorFile();
            var morDefinition2 = morFile2.Load(path2);

            Assert.AreEqual("10 50 90",
                            morDefinition2.Properties.Values.First(
                                p => p.PropertyDefinition.FilePropertyName == "Percentiles").Value);
        }
    }
}