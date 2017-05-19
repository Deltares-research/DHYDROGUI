using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class MorphologyFileTest
    {
        [Test]
        public void LoadAndSaveMorFlowFMWithCustomProperties()
        {
            var mduFilePath = TestHelper.GetTestFilePath(@"sedmor\FlowFMCustomProperties\FlowFMCustomPropertiesSedMor.mdu");
            var flowFM = new WaterFlowFMModel(mduFilePath);
            Assert.NotNull(flowFM);
            TestMorphologyContainsAllUnknownProperties(flowFM.ModelDefinition);

            /* Check if properties have been written again. */
            var mduFile = new MduFile();
            const string saveToDir = "LoadAndSaveMorFlowFM";
            Directory.CreateDirectory(saveToDir);
            var mduFileSaveToPath = Path.Combine(saveToDir, "FlowFMWithCustomProperties.mdu");
            mduFile.Write(mduFileSaveToPath, flowFM.ModelDefinition, flowFM.Area);

            /* Check if properties have been written again. */
            var newFlowFM = new WaterFlowFMModel(mduFileSaveToPath);
            Assert.NotNull(newFlowFM);
            var newModelDefinition = flowFM.ModelDefinition;
            TestMorphologyContainsAllUnknownProperties(newModelDefinition);
        }

        private static void TestMorphologyContainsAllUnknownProperties(WaterFlowFMModelDefinition modelDefinition)
        {
            Assert.NotNull(modelDefinition);

            /*Test check if model contains custom (unknown) properties */
            Assert.True(modelDefinition.Properties.Any(
                p => p.PropertyDefinition.FileCategoryName.Equals(MorphologyFile.MorphologyUnknownProperty) &&
                     p.PropertyDefinition.FilePropertyName.Equals("MyCustomStringProp") &&
                     p.Value.Equals("\"123\"")));
            Assert.True(modelDefinition.Properties.Any(
                p => p.PropertyDefinition.FileCategoryName.Equals(MorphologyFile.MorphologyUnknownProperty) &&
                     p.PropertyDefinition.FilePropertyName.Equals("MyCustomBoolProp") &&
                     p.Value.Equals("1")));
            Assert.True(modelDefinition.Properties.Any(
                p => p.PropertyDefinition.FileCategoryName.Equals(MorphologyFile.MorphologyUnknownProperty) &&
                     p.PropertyDefinition.FilePropertyName.Equals("MyCustomDoubleProp") &&
                     p.Value.Equals("1.23")));
            Assert.True(modelDefinition.Properties.Any(
                p => p.PropertyDefinition.FileCategoryName.Equals(MorphologyFile.MorphologyUnknownProperty) &&
                     p.PropertyDefinition.FilePropertyName.Equals("MyCustomIntProp") &&
                     p.Value.Equals("123")));
        }

        [Test]
        public void SaveMorFile()
        {
            var morFile = Path.GetTempFileName();
            try
            {
                var modelDefinition = new WaterFlowFMModelDefinition();
                var def = WaterFlowFMProperty.CreatePropertyDefinitionForUnknownProperty(KnownProperties.morphology,
                    "myprop", string.Empty);
                var prop = new WaterFlowFMProperty(def, "801");
                modelDefinition.AddProperty(prop);

                MorphologyFile.Save(morFile, modelDefinition);
                var morWritten = File.ReadAllText(morFile);
                Assert.That(morWritten, Is.StringContaining(MorphologyFile.GeneralHeader));
                Assert.That(morWritten, Is.StringContaining(MorphologyFile.Header));
                Assert.That(morWritten, Is.StringContaining("myprop"));
                Assert.That(morWritten, Is.StringContaining("801"));
            }
            finally
            {
                FileUtils.DeleteIfExists(morFile);
            }
        }
    }
}