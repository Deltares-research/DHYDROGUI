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
    public class FouFileReaderTests
    {
        [Test]
        [TestCase(null)]
        [TestCase("")]
        [Category(TestCategory.DataAccess)]
        public void ReadFouFile_TargetDirIsNullOrEmpty_ThrowsArgumentException(string targetDir)
        {
            var modelDefinition = new WaterFlowFMModelDefinition();

            Assert.Throws<ArgumentException>(() => FouFileReader.ReadFouFile(targetDir, modelDefinition));
        }

        [Test]
        public void ReadFouFile_ModelDefinitionIsNull_ThrowsArgumentNullException()
        {
            var targetDir = Guid.NewGuid().ToString();

            Assert.Throws<ArgumentNullException>(() => FouFileReader.ReadFouFile(targetDir, null));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFouFile_FouFileExists_WriteFouFilePropertyIsTrue()
        {
            var modelDefinition = new WaterFlowFMModelDefinition();

            SetPropertyValue(modelDefinition, FouFileProperties.WriteFouFile, false);
            WriteAndReadFouFile(modelDefinition, string.Empty);

            var writeFouFile = GetPropertyValue<bool>(modelDefinition, FouFileProperties.WriteFouFile);

            Assert.IsTrue(writeFouFile);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFouFile_EmptyFouFile_FouFilePropertiesValuesAreSetToFalse()
        {
            var modelDefinition = new WaterFlowFMModelDefinition();

            WriteAndReadFouFile(modelDefinition, string.Empty);

            Assert.IsFalse(CanWriteProperties(modelDefinition));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFouFile_ContainsHeaderOnly_FouFilePropertiesValuesAreSetToFalse()
        {
            var modelDefinition = new WaterFlowFMModelDefinition();

            var fouFileData = @"*var      tsrts     sstop     numcyc    knfac     v0plu     layno     elp";

            WriteAndReadFouFile(modelDefinition, fouFileData);

            Assert.IsFalse(CanWriteProperties(modelDefinition));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFouFile_ContainsUnknownFouFileProperties_FouFilePropertiesValuesAreSetToFalse()
        {
            var modelDefinition = new WaterFlowFMModelDefinition();

            var fouFileData = @"*var      tsrts     sstop     numcyc    knfac     v0plu     layno     elp       
wl1       0         86400     0         1         0                   
wl1       0         86400     0         1         0                   max
wl1       0         86400     0         1         0                   min
uc2       0         86400     0         1         0         1         
uc2       0         86400     0         1         0         1         max
uc2       0         86400     0         1         0         1         min
fb3       0         86400     0         1         0                   
fb3       0         86400     0         1         0                   max
fb3       0         86400     0         1         0                   min
wdog4     0         86400     0         1         0                   
wdog4     0         86400     0         1         0                   max
wdog4     0         86400     0         1         0                   min
vog5      0         86400     0         1         0                   
vog5      0         86400     0         1         0                   max
vog5      0         86400     0         1         0                   min";

            WriteAndReadFouFile(modelDefinition, fouFileData);

            Assert.IsFalse(CanWriteProperties(modelDefinition));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFouFile_ContainsFouFilePropertiesWithInvalidTokens_FouFilePropertiesValuesAreSetToFalse()
        {
            var modelDefinition = new WaterFlowFMModelDefinition();

            var fouFileData = @"*var      tsrts     sstop     numcyc    knfac     v0plu     layno     elp       
wl        abc         xyz     x0         x1         fff                   
wl        abc         xyz     x0         x1         fff                   max
wl        abc         xyz     x0         x1         fff                   min
uc        abc         xyz     x0         x1         fff         1         
uc        abc         xyz     x0         x1         fff         1         max
uc        abc         xyz     x0         x1         fff         1         min
fb        abc         xyz     x0         x1         fff                
fb        abc         xyz     x0         x1         fff                   max
fb        abc         xyz     x0         x1         fff                   min
wdog      abc         xyz     x0         x1         fff                   
wdog      abc         xyz     x0         x1         fff                   max
wdog      abc         xyz     x0         x1         fff                   min
vog       abc         xyz     x0         x1         fff                   
vog       abc         xyz     x0         x1         fff                   max
vog       abc         xyz     x0         x1         fff                   min";

            WriteAndReadFouFile(modelDefinition, fouFileData);

            Assert.IsFalse(CanWriteProperties(modelDefinition));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFouFile_ContainsFouFileProperties_FouFilePropertiesValuesAreSetToTrue()
        {
            var modelDefinition = new WaterFlowFMModelDefinition();

            var fouFileData = @"*var      tsrts     sstop     numcyc    knfac     v0plu     layno     elp       
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

            WriteAndReadFouFile(modelDefinition, fouFileData);

            Assert.IsTrue(CanWriteProperties(modelDefinition));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFouFile_ContainsFouFilePropertiesUpperCase_FouFilePropertiesValuesAreSetToTrue()
        {
            var modelDefinition = new WaterFlowFMModelDefinition();

            var fouFileData = @"*VAR      TSRTS     SSTOP     NUMCYC    KNFAC     V0PLU     LAYNO     ELP       
WL        0         86400     0         1         0                   
WL        0         86400     0         1         0                   MAX
WL        0         86400     0         1         0                   MIN
UC        0         86400     0         1         0         1         
UC        0         86400     0         1         0         1         MAX
UC        0         86400     0         1         0         1         MIN
FB        0         86400     0         1         0                   
FB        0         86400     0         1         0                   MAX
FB        0         86400     0         1         0                   MIN
WDOG      0         86400     0         1         0                   
WDOG      0         86400     0         1         0                   MAX
WDOG      0         86400     0         1         0                   MIN
VOG       0         86400     0         1         0                   
VOG       0         86400     0         1         0                   MAX
VOG       0         86400     0         1         0                   MIN";

            WriteAndReadFouFile(modelDefinition, fouFileData);

            Assert.IsTrue(CanWriteProperties(modelDefinition));
        }

        private bool CanWriteProperties(WaterFlowFMModelDefinition modelDefinition)
        {
            return FouFileProperties.PropertyNames.Select(propertyName => GetPropertyValue<bool>(modelDefinition, propertyName))
                                    .All(propertyValue => propertyValue);
        }

        private T GetPropertyValue<T>(WaterFlowFMModelDefinition modelDefinition, string propertyName)
        {
            WaterFlowFMProperty modelProperty = modelDefinition.GetModelProperty(propertyName);
            return (T)modelProperty.Value;
        }

        private void SetPropertyValue(WaterFlowFMModelDefinition modelDefinition, string propertyName, object value)
        {
            WaterFlowFMProperty modelProperty = modelDefinition.GetModelProperty(propertyName);
            modelProperty.Value = value;
        }

        private void WriteAndReadFouFile(WaterFlowFMModelDefinition modelDefinition, string fouFileData)
        {
            string targetDir = FileUtils.CreateTempDirectory();
            string fouFilePath = Path.Combine(targetDir, FouFileProperties.FouFileName);

            SetPropertyValue(modelDefinition, FouFileProperties.MduFouFileProperty, FouFileProperties.FouFileName);

            try
            {
                File.WriteAllText(fouFilePath, fouFileData);
                FouFileReader.ReadFouFile(targetDir, modelDefinition);
            }
            finally
            {
                FileUtils.DeleteIfExists(fouFilePath);
            }
        }
    }
}