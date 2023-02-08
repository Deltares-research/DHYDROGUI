using System;
using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
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
        public void UseFouFile_ModelDefinitionIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => FouFileWriter.UseFouFile(null));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        [Category(TestCategory.DataAccess)]
        public void UseFouFile_WriteFouFilePropertyValueIsTestCaseValue_ReturnsExpected(bool expected)
        {
            var modelDefinition = new WaterFlowFMModelDefinition();

            SetPropertyValue(modelDefinition, FouFileProperties.WriteFouFile, expected);

            Assert.AreEqual(expected, FouFileWriter.UseFouFile(modelDefinition));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [Category(TestCategory.DataAccess)]
        public void Process_TargetDirIsNullOrEmpty_ThrowsArgumentException(string targetDir)
        {
            var modelDefinition = new WaterFlowFMModelDefinition();

            Assert.Throws<ArgumentException>(() => FouFileWriter.Process(targetDir, modelDefinition));
        }

        [Test]
        public void Process_ModelDefinitionIsNull_ThrowsArgumentNullException()
        {
            var targetDir = Guid.NewGuid().ToString();

            Assert.Throws<ArgumentNullException>(() => FouFileWriter.Process(targetDir, null));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Process_FouFilePropertyValuesAreSetToFalse_WritesFouFileWithHeaderOnly()
        {
            var modelDefinition = new WaterFlowFMModelDefinition();

            SetFouFilePropertyValues(modelDefinition, false);

            string fouFileData = WriteAndReadFouFile(modelDefinition);
            var expected = @"*var      tsrts     sstop     numcyc    knfac     v0plu     layno     elp";

            Assert.AreEqual(expected, fouFileData);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Process_FouFilePropertyValuesAreSetToTrue_WritesFouFileWithHeaderAndProperties()
        {
            var modelDefinition = new WaterFlowFMModelDefinition();

            SetFouFilePropertyValues(modelDefinition, true);

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

        private void SetFouFilePropertyValues(WaterFlowFMModelDefinition modelDefinition, bool value)
        {
            FouFileProperties.PropertyNames.ForEach(propertyName => SetPropertyValue(modelDefinition, propertyName, value));
        }

        private void SetPropertyValue(WaterFlowFMModelDefinition modelDefinition, string propertyName, object value)
        {
            WaterFlowFMProperty modelProperty = modelDefinition.GetModelProperty(propertyName);
            modelProperty.Value = value;
        }

        private string WriteAndReadFouFile(WaterFlowFMModelDefinition modelDefinition)
        {
            string targetDir = FileUtils.CreateTempDirectory();
            string fouFilePath = Path.Combine(targetDir, FouFileProperties.FouFileName);

            try
            {
                FouFileWriter.Process(targetDir, modelDefinition);
                return File.ReadAllText(fouFilePath).Trim();
            }
            finally
            {
                FileUtils.DeleteIfExists(fouFilePath);
            }
        }
    }
}