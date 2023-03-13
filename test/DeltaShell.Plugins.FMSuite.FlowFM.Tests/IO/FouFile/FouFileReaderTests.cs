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
        public void Constructor_ModelDefinitionIsNull_ThrowsArgumentNullException()
        {
            FouFileReader _;

            Assert.Throws<ArgumentNullException>(() => _ = new FouFileReader(null));
        }
        
        [Test]
        [TestCase(null)]
        [TestCase("")]
        [Category(TestCategory.DataAccess)]
        public void CanReadFromDirectory_DirectoryIsNullOrEmpty_ThrowsArgumentException(string directory)
        {
            var modelDefinition = new WaterFlowFMModelDefinition();
            var fouFileReader = new FouFileReader(modelDefinition);

            Assert.Throws<ArgumentException>(() => fouFileReader.CanReadFromDirectory(directory));
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void CanReadFromDirectory_NoFouFileExists_ReturnsFalse()
        {
            var modelDefinition = new WaterFlowFMModelDefinition();
            var fouFileReader = new FouFileReader(modelDefinition);

            var directory = Guid.NewGuid().ToString();

            bool canRead = fouFileReader.CanReadFromDirectory(directory);
            
            Assert.IsFalse(canRead);
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void CanReadFromDirectory_FouFileExists_ReturnsTrue()
        {
            var modelDefinition = new WaterFlowFMModelDefinition();
            var fouFileReader = new FouFileReader(modelDefinition);

            var fouFileName = "Maxima.fou";

            string directory = FileUtils.CreateTempDirectory();
            string fouFilePath = Path.Combine(directory, fouFileName);

            File.WriteAllText(fouFilePath, string.Empty);

            SetPropertyValue(modelDefinition, KnownProperties.FouFile, fouFileName);

            try
            {
                bool canRead = fouFileReader.CanReadFromDirectory(directory);
            
                Assert.IsTrue(canRead);
            }
            finally
            {
                FileUtils.DeleteIfExists(directory);
            }
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [Category(TestCategory.DataAccess)]
        public void ReadFromDirectory_DirectoryIsNullOrEmpty_ThrowsArgumentException(string directory)
        {
            var modelDefinition = new WaterFlowFMModelDefinition();
            var fouFileReader = new FouFileReader(modelDefinition);

            Assert.Throws<ArgumentException>(() => fouFileReader.ReadFromDirectory(directory));
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFromDirectory_FouFileDoesNotExists_ThrowsArgumentException()
        {
            var modelDefinition = new WaterFlowFMModelDefinition();
            var fouFileReader = new FouFileReader(modelDefinition);

            var directory = Guid.NewGuid().ToString();

            Assert.Throws<ArgumentException>(() => fouFileReader.ReadFromDirectory(directory));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFromDirectory_FouFileExists_WriteFouFilePropertyIsTrue()
        {
            var modelDefinition = new WaterFlowFMModelDefinition();

            SetPropertyValue(modelDefinition, GuiProperties.WriteFouFile, false);
            WriteAndReadFouFile(modelDefinition, string.Empty);

            var writeFouFile = GetPropertyValue<bool>(modelDefinition, GuiProperties.WriteFouFile);

            Assert.IsTrue(writeFouFile);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFromDirectory_FouFileIsEmpty_ModelPropertyValuesAreSetToFalse()
        {
            var modelDefinition = new WaterFlowFMModelDefinition();

            WriteAndReadFouFile(modelDefinition, string.Empty);

            Assert.IsTrue(AreAllModelPropertyValuesEqualTo(modelDefinition, false));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFromDirectory_FouFileContainsHeaderOnly_ModelPropertyValuesAreSetToFalse()
        {
            var modelDefinition = new WaterFlowFMModelDefinition();

            var fouFileData = @"*var      tsrts     sstop     numcyc    knfac     v0plu     layno     elp";

            WriteAndReadFouFile(modelDefinition, fouFileData);

            Assert.IsTrue(AreAllModelPropertyValuesEqualTo(modelDefinition, false));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFromDirectory_FouFileContainsUnknownVariables_ModelPropertyValuesAreSetToFalse()
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

            Assert.IsTrue(AreAllModelPropertyValuesEqualTo(modelDefinition, false));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFromDirectory_FouFileContainsVariablesWithInvalidTokens_ModelPropertyValuesAreSetToFalse()
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

            Assert.IsTrue(AreAllModelPropertyValuesEqualTo(modelDefinition, false));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFromDirectory_FouFileIsValid_ModelPropertyValuesAreSetToTrue()
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

            Assert.IsTrue(AreAllModelPropertyValuesEqualTo(modelDefinition, true));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFouFile_ContainsFouFilePropertiesUpperCase_ModelPropertyValuesAreSetToTrue()
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

            Assert.IsTrue(AreAllModelPropertyValuesEqualTo(modelDefinition, true));
        }

        private bool AreAllModelPropertyValuesEqualTo(WaterFlowFMModelDefinition modelDefinition, bool expected)
        {
            var fouFileDefinition = new FouFileDefinition();
            
            return fouFileDefinition.ModelPropertyNames.Select(propertyName => GetPropertyValue<bool>(modelDefinition, propertyName))
                                    .All(propertyValue => propertyValue == expected);
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
            var fouFileName = "Maxima.fou";

            string directory = FileUtils.CreateTempDirectory();
            string fouFilePath = Path.Combine(directory, fouFileName);
            
            var fouFileReader = new FouFileReader(modelDefinition);

            SetPropertyValue(modelDefinition, KnownProperties.FouFile, fouFileName);

            try
            {
                File.WriteAllText(fouFilePath, fouFileData);
                fouFileReader.ReadFromDirectory(directory);
            }
            finally
            {
                FileUtils.DeleteIfExists(directory);
            }
        }
    }
}