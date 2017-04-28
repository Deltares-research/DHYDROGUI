using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.SedMor.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.SedMor.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.SedMor.IO
{
    [TestFixture]
    public class SedFileTest
    {
        [Test]
        public void ReadSedFile()
        {
            var path = TestHelper.GetTestFilePath("sedmor\\files\\WaimakSediment.sed");

            var sedFile = new SedFile();
            var sedDefinition = sedFile.Load(path);

            Assert.AreEqual(1600.0, sedDefinition.ReferenceDensity);
            Assert.AreEqual("Delft3D FLOW-GUI, Version: 3.41.06.10981",
                            sedDefinition.Properties[SedProperties.FileCreatedBy].Value);
            Assert.AreEqual(8, sedDefinition.Sediments.Count);

            var firstSediment = sedDefinition.Sediments[0];
            Assert.AreEqual("Sediment_CoarseSand", firstSediment.Name);
            Assert.AreEqual("bedload", firstSediment.Properties[SedProperties.SedTyp].GetValueAsString());
            Assert.AreEqual(2650.0, firstSediment.Density);
            Assert.AreEqual(7, firstSediment.Properties.Count);
        }

        [Test]
        public void ReadWriteReadSedFile()
        {
            var path = TestHelper.GetTestFilePath("sedmor\\files\\WaimakSediment.sed");

            var sedFile = new SedFile();
            var sedDefinition = sedFile.Load(path);

            Assert.AreEqual(1600.0, sedDefinition.ReferenceDensity);
            Assert.AreEqual(8, sedDefinition.Sediments.Count);
            var firstSediment = sedDefinition.Sediments[0];
            Assert.AreEqual(7, firstSediment.Properties.Count);

            var sed2Path = "new.sed";
            sedFile.Save(sed2Path, sedDefinition);

            var sedFile2 = new SedFile();
            var def2 = sedFile2.Load(sed2Path);
            
            Assert.AreEqual(1600.0, def2.ReferenceDensity);
            Assert.AreEqual(8, def2.Sediments.Count);
            Assert.AreEqual(7, def2.Sediments[0].Properties.Count);
            Assert.AreEqual(2650.0, def2.Sediments[0].Density);
        }

        [Test]
        public void ReadOtherSedFile()
        {
            var path = TestHelper.GetTestFilePath("sedmor\\files\\SCbijkero.sed");

            var sedFile = new SedFile();
            var sedDefinition = sedFile.Load(path);

            Assert.AreEqual(1600.0, sedDefinition.ReferenceDensity);
            Assert.AreEqual("Sasha Izru", sedDefinition.Properties[SedProperties.FileCreatedBy].Value);
            Assert.AreEqual(1, sedDefinition.Sediments.Count);

            var firstSediment = sedDefinition.Sediments[0];
            Assert.AreEqual("Sediment arena", firstSediment.Name);
            Assert.AreEqual("sand", firstSediment.Properties[SedProperties.SedTyp].GetValueAsString());
            Assert.AreEqual(2626.6141, firstSediment.Density);
            Assert.AreEqual("bijker.frm", firstSediment.Properties[SedProperties.TraFrm].Value);
            Assert.AreEqual(8, firstSediment.Properties.Count);
        }

        [Test]
        public void ReadWriteReadOtherSedFile()
        {
            var path = TestHelper.GetTestFilePath("sedmor\\files\\SCbijkero.sed");

            var sedFile = new SedFile();
            var sedDefinition = sedFile.Load(path);

            Assert.AreEqual(1600.0, sedDefinition.ReferenceDensity);
            Assert.AreEqual("Sasha Izru", sedDefinition.Properties[SedProperties.FileCreatedBy].Value);
            Assert.AreEqual(1, sedDefinition.Sediments.Count);

            var firstSediment = sedDefinition.Sediments[0];
            Assert.AreEqual("Sediment arena", firstSediment.Name);
            Assert.AreEqual(8, firstSediment.Properties.Count);

            var path2 = "new2.sed";
            sedFile.Save(path2, sedDefinition);

            var sedFile2 = new SedFile();
            var sedDefinition2 = sedFile2.Load(path2);

            Assert.AreEqual(1600.0, sedDefinition2.ReferenceDensity);
            Assert.AreEqual("Sediment arena", sedDefinition2.Sediments[0].Name);
            Assert.AreEqual(8, sedDefinition2.Sediments[0].Properties.Count);
        }
    }
}