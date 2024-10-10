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
    public class FouFileWriterTest
    {
        private WaterFlowFMModelDefinition modelDefinition;
        private FouFileWriter fouFileWriter;
        
        [SetUp]
        public void SetUp()
        {
            modelDefinition = new WaterFlowFMModelDefinition();
            fouFileWriter = new FouFileWriter(modelDefinition);
        }

        [Test]
        public void Constructor_ModelDefinitionIsNull_ThrowsArgumentNullException()
        {
            FouFileWriter _;

            Assert.That(() => _ = new FouFileWriter(null), Throws.ArgumentNullException);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        [Category(TestCategory.DataAccess)]
        public void CanWrite_WriteFouFilePropertyIsSet_ReturnsExpected(bool expected)
        {
            SetPropertyValue(GuiProperties.WriteFouFile, expected);

            Assert.That(fouFileWriter.CanWrite(), Is.EqualTo(expected));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [Category(TestCategory.DataAccess)]
        public void WriteToDirectory_DirectoryIsNullOrEmpty_ThrowsArgumentException(string directory)
        {
            Assert.That(() => fouFileWriter.WriteToDirectory(directory), Throws.ArgumentException);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteToDirectory_WriteFouFilePropertyIsFalse_ThrowsInvalidOperationException()
        {
            var directory = Guid.NewGuid().ToString();

            SetPropertyValue(GuiProperties.WriteFouFile, false);

            Assert.That(() => fouFileWriter.WriteToDirectory(directory), Throws.InvalidOperationException);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteToDirectory_ModelPropertyValuesAreSetToFalse_WritesFouFileWithHeaderOnly()
        {
            SetWriteFouVariableProperties(false);

            string fouFileData = WriteAndReadFouFile();

            Assert.That(fouFileData, Is.EqualTo(FouFileConstants.FileHeader));
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteToDirectory_WithDefaultModelPropertyValues_WritesExpectedFouFileContents()
        {
            string fouFileData = WriteAndReadFouFile();
            
            string expected = GetExpectedFouFileContents(@"     
uc        0         86400     0         1         0         1         max
wl        0         86400     0         1         0                   max");

            Assert.That(fouFileData, Is.EqualTo(expected));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteToDirectory_ModelPropertyValuesAreSetToTrue_WritesExpectedFouFileContents()
        {
            SetWriteFouVariableProperties(true);

            string fouFileData = WriteAndReadFouFile();
            
            string expected = GetExpectedFouFileContents(@"
bs        0         86400     0         1         0                   
bs        0         86400     0         1         0                   max
bs        0         86400     0         1         0                   min
eh        0         86400     0         1         0                   
eh        0         86400     0         1         0                   max
eh        0         86400     0         1         0                   min
fb        0         86400     0         1         0                   
fb        0         86400     0         1         0                   max
fb        0         86400     0         1         0                   min
q1        0         86400     0         1         0         1         
q1        0         86400     0         1         0         1         max
q1        0         86400     0         1         0         1         min
sul       0         86400     0         1         0                   
sul       0         86400     0         1         0                   max
sul       0         86400     0         1         0                   min
uc        0         86400     0         1         0         1         
uc        0         86400     0         1         0         1         max
uc        0         86400     0         1         0         1         min
ux        0         86400     0         1         0         1         
ux        0         86400     0         1         0         1         max
ux        0         86400     0         1         0         1         min
uxa       0         86400     0         1         0                   
uxa       0         86400     0         1         0                   max
uxa       0         86400     0         1         0                   min
uy        0         86400     0         1         0         1         
uy        0         86400     0         1         0         1         max
uy        0         86400     0         1         0         1         min
uya       0         86400     0         1         0                   
uya       0         86400     0         1         0                   max
uya       0         86400     0         1         0                   min
vog       0         86400     0         1         0                   
vog       0         86400     0         1         0                   max
vog       0         86400     0         1         0                   min
wd        0         86400     0         1         0                   
wd        0         86400     0         1         0                   max
wd        0         86400     0         1         0                   min
wdog      0         86400     0         1         0                   
wdog      0         86400     0         1         0                   max
wdog      0         86400     0         1         0                   min
wl        0         86400     0         1         0                   
wl        0         86400     0         1         0                   max
wl        0         86400     0         1         0                   min");

            Assert.That(fouFileData, Is.EqualTo(expected));
        }

        private void SetWriteFouVariableProperties(bool value)
        {
            var fouFileDefinition = new FouFileDefinition();
            
            fouFileDefinition.ModelPropertyNames.ToList().ForEach(
                propertyName => SetPropertyValue(propertyName, value));
        }

        private void SetPropertyValue(string propertyName, object value)
        {
            WaterFlowFMProperty modelProperty = modelDefinition.GetModelProperty(propertyName);
            Assert.That(modelProperty, Is.Not.Null, $"Model property '{propertyName}' does not exist in model definition.");
            
            modelProperty.Value = value;
        }

        private string WriteAndReadFouFile()
        {
            string targetDir = FileUtils.CreateTempDirectory();
            string fouFilePath = Path.Combine(targetDir, FouFileConstants.DefaultFileName);
           
            SetPropertyValue(GuiProperties.WriteFouFile, true);

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

        private static string GetExpectedFouFileContents(string contents)
        {
            return FouFileConstants.FileHeader + Environment.NewLine + contents.Trim();
        }
    }
}