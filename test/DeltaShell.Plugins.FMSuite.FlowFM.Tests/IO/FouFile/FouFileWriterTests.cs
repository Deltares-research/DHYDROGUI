using System;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.FouFile;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.FouFile
{
    [TestFixture]
    public class FouFileWriterTests
    {
        [Test]
        public void Constructor_ModelDefinitionIsNull_ThrowsArgumentNullException()
        {
            FouFileWriter _;

            Assert.Throws<ArgumentNullException>(() => _ = new FouFileWriter(null));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        [Category(TestCategory.DataAccess)]
        public void CanWrite_WriteFouFilePropertyIsSet_ReturnsExpected(bool expected)
        {
            var modelDefinition = new WaterFlowFMModelDefinition();
            var fouFileWriter = new FouFileWriter(modelDefinition);

            SetPropertyValue(modelDefinition, GuiProperties.WriteFouFile, expected);

            Assert.AreEqual(expected, fouFileWriter.CanWrite());
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [Category(TestCategory.DataAccess)]
        public void WriteToDirectory_DirectoryIsNullOrEmpty_ThrowsArgumentException(string directory)
        {
            var modelDefinition = new WaterFlowFMModelDefinition();
            var fouFileWriter = new FouFileWriter(modelDefinition);

            Assert.Throws<ArgumentException>(() => fouFileWriter.WriteToDirectory(directory));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteToDirectory_WriteFouFilePropertyIsFalse_ThrowsInvalidOperationException()
        {
            var modelDefinition = new WaterFlowFMModelDefinition();
            var fouFileWriter = new FouFileWriter(modelDefinition);

            var directory = Guid.NewGuid().ToString();

            SetPropertyValue(modelDefinition, GuiProperties.WriteFouFile, false);

            Assert.Throws<InvalidOperationException>(() => fouFileWriter.WriteToDirectory(directory));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteToDirectory_ModelPropertyValuesAreSetToFalse_WritesFouFileWithHeaderOnly()
        {
            var modelDefinition = new WaterFlowFMModelDefinition();

            SetModelPropertyValues(modelDefinition, false);

            string fouFileData = WriteAndReadFouFile(modelDefinition);
            var expected = @"*var      tsrts     sstop     numcyc    knfac     v0plu     layno     elp";

            Assert.AreEqual(expected, fouFileData);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteToDirectory_ModelPropertyValuesAreSetToTrue_WritesFouFileWithHeaderAndVariables()
        {
            var modelDefinition = new WaterFlowFMModelDefinition();

            SetModelPropertyValues(modelDefinition, true);

            string fouFileData = WriteAndReadFouFile(modelDefinition);
            var expected = @"*var      tsrts     sstop     numcyc    knfac     v0plu     layno     elp       
wl        0         86400     0         1         0                   
wl        0         86400     0         1         0                   max
wl        0         86400     0         1         0                   min
uc        0         86400     0         1         0         1         
uc        0         86400     0         1         0         1         max
uc        0         86400     0         1         0         1         min
fb        0         86400     0         1         0                   
fb        0         86400     0         1         0                   max
fb        0         86400     0         1         0                   min
wdog      0         86400     0         1         0                   
wdog      0         86400     0         1         0                   max
wdog      0         86400     0         1         0                   min
vog       0         86400     0         1         0                   
vog       0         86400     0         1         0                   max
vog       0         86400     0         1         0                   min";

            Assert.AreEqual(expected, fouFileData);
        }

        private void SetModelPropertyValues(WaterFlowFMModelDefinition modelDefinition, bool value)
        {
            var fouFileDefinition = new FouFileDefinition();
            
            fouFileDefinition.ModelPropertyNames.ToList().ForEach(propertyName => SetPropertyValue(modelDefinition, propertyName, value));
        }

        private void SetPropertyValue(WaterFlowFMModelDefinition modelDefinition, string propertyName, object value)
        {
            WaterFlowFMProperty modelProperty = modelDefinition.GetModelProperty(propertyName);
            modelProperty.Value = value;
        }

        private string WriteAndReadFouFile(WaterFlowFMModelDefinition modelDefinition)
        {
            string targetDir = FileUtils.CreateTempDirectory();
            string fouFilePath = Path.Combine(targetDir, FouFileWriter.DefaultFileName);

            var fouFileWriter = new FouFileWriter(modelDefinition);
            
            SetPropertyValue(modelDefinition, GuiProperties.WriteFouFile, true);

            try
            {
                fouFileWriter.WriteToDirectory(targetDir);
                
                return File.ReadAllText(fouFilePath).Trim();
            }
            finally
            {
                FileUtils.DeleteIfExists(fouFilePath);
            }
        }
    }
}