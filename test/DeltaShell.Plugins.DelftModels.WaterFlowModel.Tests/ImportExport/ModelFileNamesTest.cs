using System;
using System.IO;
using System.Linq;
using System.Reflection;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport
{
    [TestFixture]
    public class ModelFileNamesTest
    {
        private static class CustomModelFilenames
        {
            public static string SavePath { private get; set; }
            public static string NetworkFilename {get { return Path.Combine(SavePath, "My_NetworkDefinition.ini");}}
            public static string CrossSectionLocationFilename {get { return Path.Combine(SavePath, "My_CrossSectionLocations.ini");}} 
            public static string CrossSectionDefinitionFilename {get { return Path.Combine(SavePath, "My_CrossSectionDefinitions.ini");}}
            public static string StructureFilename {get { return Path.Combine(SavePath, "My_Structures.ini");}}
            public static string ObservationPointFilename {get { return Path.Combine(SavePath, "My_ObservationPoints.ini");}}
                          
            public static string roughnessFile1 {get { return Path.Combine(SavePath, "My_roughness-1.ini");}}
            public static string roughnessFile2 {get { return Path.Combine(SavePath, "My_roughness-2.ini");}}
                          
            public static string BoundaryLocationFilename {get { return Path.Combine(SavePath, "My_BoundaryLocations.ini");}} 
            public static string LateralDischargeFilename {get { return Path.Combine(SavePath, "My_LateralDischargeLocations.ini");}}
            public static string BoundaryConditionsFilename {get { return Path.Combine(SavePath, "My_BoundaryConditions.bc");}}
                          
            public static string SobekSimFilename {get { return Path.Combine(SavePath, "My_SobekSim.ini");}}
            public static string RetentionFilename {get { return Path.Combine(SavePath, "My_Retention.ini");}}
            public static string LogFileName {get { return Path.Combine(SavePath, "My_sobek.log");}}
        }

        [Test]
		public void GivenCustomSaved_md1d_iniFileWhenReadingFileNamesThenFilenamesSet()
		{
            var expectedFile = TestHelper.GetTestFilePath(@"FileWriters/ModelNames_expected.txt");
            var testFile = TestHelper.CreateLocalCopySingleFile(expectedFile);
            var testFileDir = Path.GetDirectoryName(testFile);
            var destFileName = Path.Combine(testFileDir,"ModelNames_expected.md1d");

            FileUtils.CopyFile(testFile, destFileName, true);
            var fileName = new ModelFileNames(destFileName);

            CustomModelFilenames.SavePath = testFileDir;

            Assert.That(fileName.Network, Is.EqualTo(CustomModelFilenames.NetworkFilename));
			Assert.That(fileName.CrossSectionLocations, Is.EqualTo(CustomModelFilenames.CrossSectionLocationFilename));
			Assert.That(fileName.CrossSectionDefinitions, Is.EqualTo(CustomModelFilenames.CrossSectionDefinitionFilename));
			Assert.That(fileName.Structures, Is.EqualTo(CustomModelFilenames.StructureFilename));
			Assert.That(fileName.ObservationPoints, Is.EqualTo(CustomModelFilenames.ObservationPointFilename));
			
            Assert.That(fileName.RoughnessFiles.Count, Is.EqualTo(2));
            Assert.That(fileName.RoughnessFiles.ElementAt(0), Is.EqualTo(CustomModelFilenames.roughnessFile1));
            Assert.That(fileName.RoughnessFiles.ElementAt(1), Is.EqualTo(CustomModelFilenames.roughnessFile2));
            
            Assert.That(fileName.BoundaryLocations, Is.EqualTo(CustomModelFilenames.BoundaryLocationFilename));
            Assert.That(fileName.LateralDischarge, Is.EqualTo(CustomModelFilenames.LateralDischargeFilename));
            Assert.That(fileName.BoundaryConditions, Is.EqualTo(CustomModelFilenames.BoundaryConditionsFilename));

            Assert.That(fileName.SobekSim, Is.EqualTo(CustomModelFilenames.SobekSimFilename));
            Assert.That(fileName.Retention, Is.EqualTo(CustomModelFilenames.RetentionFilename));
            Assert.That(fileName.LogFile, Is.EqualTo(CustomModelFilenames.LogFileName));
        }
        
        [Test]
		public void GiveStandardSaved_md1d_iniFileWhenReadingFileNamesThenFilenamesSet()
		{
            var destFileName = Path.Combine(Environment.CurrentDirectory,"ModelNames_standard.md1d");
            FileUtils.DeleteIfExists(destFileName);
            var fileName = new ModelFileNames(destFileName);

            Assert.That(fileName.Network, Is.EqualTo(GetValueFromModelNamesOf("NetworkFilename")));
			Assert.That(fileName.CrossSectionLocations, Is.EqualTo(GetValueFromModelNamesOf("CrossSectionLocationFilename")));
			Assert.That(fileName.CrossSectionDefinitions, Is.EqualTo(GetValueFromModelNamesOf("CrossSectionDefinitionFilename")));
			Assert.That(fileName.Structures, Is.EqualTo(GetValueFromModelNamesOf("StructureFilename")));
			Assert.That(fileName.ObservationPoints, Is.EqualTo(GetValueFromModelNamesOf("ObservationPointFilename")));
           
            Assert.That(fileName.BoundaryLocations, Is.EqualTo(GetValueFromModelNamesOf("BoundaryLocationFilename")));
            Assert.That(fileName.LateralDischarge, Is.EqualTo(GetValueFromModelNamesOf("LateralDischargeFilename")));
            Assert.That(fileName.BoundaryConditions, Is.EqualTo(GetValueFromModelNamesOf("BoundaryConditionsFilename")));

            Assert.That(fileName.SobekSim, Is.EqualTo(GetValueFromModelNamesOf("SobekSimFilename")));
            Assert.That(fileName.Retention, Is.EqualTo(GetValueFromModelNamesOf("RetentionFilename")));
            Assert.That(fileName.LogFile, Is.EqualTo(GetValueFromModelNamesOf("LogFileName")));
		}

        private object GetValueFromModelNamesOf(string name)
        {
            var fieldInfo =
                typeof (ModelFileNames).GetFields(
                    BindingFlags.NonPublic | BindingFlags.Static)
                    .FirstOrDefault(fi => fi.Name == name);
            return fieldInfo == null ? null : Path.Combine(Environment.CurrentDirectory, fieldInfo.GetValue(null).ToString());
        }
    }
}