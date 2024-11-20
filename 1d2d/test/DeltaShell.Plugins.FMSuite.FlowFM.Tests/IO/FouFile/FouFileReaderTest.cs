using System;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.FouFile;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.FouFile
{
    [TestFixture]
    public class FouFileReaderTest
    {
        private WaterFlowFMModelDefinition modelDefinition;
        private FouFileReader fouFileReader;

        [SetUp]
        public void SetUp()
        {
            modelDefinition = new WaterFlowFMModelDefinition();
            fouFileReader = new FouFileReader(modelDefinition);
        }

        [Test]
        public void Constructor_ModelDefinitionIsNull_ThrowsArgumentNullException()
        {
            FouFileReader _;

            Assert.That(() => _ = new FouFileReader(null), Throws.ArgumentNullException);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [Category(TestCategory.DataAccess)]
        public void CanReadFromDirectory_DirectoryIsNullOrEmpty_ThrowsArgumentException(string directory)
        {
            Assert.That(() => fouFileReader.CanReadFromDirectory(directory), Throws.ArgumentException);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void CanReadFromDirectory_NoFouFileExists_ReturnsFalse()
        {
            var directory = Guid.NewGuid().ToString();

            bool canRead = fouFileReader.CanReadFromDirectory(directory);

            Assert.That(canRead, Is.False);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void CanReadFromDirectory_FouFileExists_ReturnsTrue()
        {
            const string fouFileName = "Maxima.fou";

            string directory = FileUtils.CreateTempDirectory();
            string fouFilePath = Path.Combine(directory, fouFileName);

            File.WriteAllText(fouFilePath, string.Empty);

            SetPropertyValue(KnownProperties.FouFile, fouFileName);

            try
            {
                bool canRead = fouFileReader.CanReadFromDirectory(directory);

                Assert.That(canRead, Is.True);
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
            Assert.That(() => fouFileReader.ReadFromDirectory(directory), Throws.ArgumentException);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFromDirectory_FouFileDoesNotExists_ThrowsArgumentException()
        {
            var directory = Guid.NewGuid().ToString();

            Assert.That(() => fouFileReader.ReadFromDirectory(directory), Throws.ArgumentException);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFromDirectory_FouFileExists_WriteFouFilePropertyIsTrue()
        {
            SetPropertyValue(GuiProperties.WriteFouFile, false);
            WriteAndReadFouFile(string.Empty);

            var writeFouFile = GetPropertyValue<bool>(GuiProperties.WriteFouFile);

            Assert.That(writeFouFile, Is.True);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFromDirectory_FouFileIsEmpty_WriteFouVariablePropertiesSetToFalse()
        {
            WriteAndReadFouFile(string.Empty);

            AssertWriteFouVariablePropertiesAreEqualTo(false);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFromDirectory_FouFileWithHeaderOnly_WriteFouVariablePropertiesSetToFalse()
        {
            const string fouFileData = @"*var      tsrts     sstop     numcyc    knfac     v0plu     layno     elp";

            WriteAndReadFouFile(fouFileData);

            AssertWriteFouVariablePropertiesAreEqualTo(false);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFromDirectory_FouFileWithMultiLineHeaderOnly_WriteFouVariablePropertiesSetToFalse()
        {
            const string fouFileData = @"* Fourier file with multi-line header
* Comment line
*var      tsrts     sstop     numcyc    knfac     v0plu     layno     elp";

            WriteAndReadFouFile(fouFileData);

            AssertWriteFouVariablePropertiesAreEqualTo(false);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFromDirectory_FouFileWithUnknownVariables_WriteFouVariablePropertiesSetToFalse()
        {
            const string fouFileData = @"
*var      tsrts     sstop     numcyc    knfac     v0plu     layno     elp       
wl1       0         86400     0         1         0                   
uc2       0         86400     0         1         0         1         min";

            WriteAndReadFouFile(fouFileData);

            AssertWriteFouVariablePropertiesAreEqualTo(false);
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFromDirectory_FouFileWithUnknownQuantity_LogsErrorMessage()
        {
            const string fouFileData = @"
*var      tsrts     sstop     numcyc    knfac     v0plu     layno     elp       
uc2       0         86400     0         1         0         1         min";

            const string expectedMessage = "No D-Flow FM property defined for fou file quantity 'uc2' with analysis type 'min'.";
            
            TestHelper.AssertLogMessageIsGenerated(() => WriteAndReadFouFile(fouFileData), expectedMessage, Level.Error);
        }
        
        [Test]
        [TestCase("cs")]
        [TestCase("ct")]
        [TestCase("ws")]
        [TestCase("c1")]
        [TestCase("c2")]
        [TestCase("c11")]
        [TestCase("c11323")]
        [Category(TestCategory.DataAccess)]
        public void ReadFromDirectory_FouFileWithUnsupportedQuantities_LogsWarningMessage(string quantity)
        {
            var fouFileData = $@"*var      tsrts     sstop     numcyc    knfac     v0plu     layno     elp       
{quantity}       0         86400     0         1         0         1         min";

            var expectedMessage = $"The selected fou file quantity '{quantity}' is not yet available or validated for 1D.";
            
            TestHelper.AssertLogMessageIsGenerated(() => WriteAndReadFouFile(fouFileData), expectedMessage, Level.Warn);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFromDirectory_FouFileWithInvalidTokens_WriteFouVariablePropertiesSetToFalse()
        {
            const string fouFileData = @"
*var      tsrts     sstop     numcyc    knfac     v0plu     layno     elp       
fb        abc         xyz     x0        x1        fff              
wdog      abc         xyz     x0        x1        fff                 
";

            WriteAndReadFouFile(fouFileData);

            AssertWriteFouVariablePropertiesAreEqualTo(false);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFromDirectory_ValidFouFileWithMultiLineHeader_WriteFouVariablePropertiesSetToTrue()
        {
            const string fouFileData = @"*test fou file
*var      tsrts     sstop     numcyc    knfac     v0plu     layno     elp       
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
wl        0         86400     0         1         0                   min";

            WriteAndReadFouFile(fouFileData);

            AssertWriteFouVariablePropertiesAreEqualTo(true);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFouFile_FouFileWithUpperCase_WriteFouVariablePropertiesSetToTrue()
        {
            const string fouFileData = @"
*VAR      TSRTS     SSTOP     NUMCYC    KNFAC     V0PLU     LAYNO     ELP       
WDOG      0         86400     0         1         0                   
WDOG      0         86400     0         1         0                   MAX
WDOG      0         86400     0         1         0                   MIN";

            WriteAndReadFouFile(fouFileData);

            AssertWriteFouVariablePropertyIsEqualTo("WriteWdogAverage", true);
            AssertWriteFouVariablePropertyIsEqualTo("WriteWdogMaximum", true);
            AssertWriteFouVariablePropertyIsEqualTo("WriteWdogMinimum", true);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadFouFile_FouFileValuesInDoubleFormat_WriteFouVariablePropertiesSetToTrue()
        {
            const string fouFileData = @"
*var      tsrts     sstop     numcyc    knfac     v0plu     layno     elp       
uc        0         86400     0.0       1.0       0.0       1.0       
uc        0         86400     0.0       1.0       0.0       1.0       max
uc        0         86400     0.0       1.0       0.0       1.0       min";

            WriteAndReadFouFile(fouFileData);

            AssertWriteFouVariablePropertyIsEqualTo("WriteUcAverage", true);
            AssertWriteFouVariablePropertyIsEqualTo("WriteUcMaximum", true);
            AssertWriteFouVariablePropertyIsEqualTo("WriteUcMinimum", true);
        }

        private void AssertWriteFouVariablePropertiesAreEqualTo(bool expected)
        {
            var fouFileDefinition = new FouFileDefinition();

            bool allValuesEqual = fouFileDefinition.ModelPropertyNames
                                                   .Select(GetPropertyValue<bool>)
                                                   .All(propertyValue => propertyValue == expected);

            Assert.That(allValuesEqual, Is.True, $"Expected write Fou variable properties set to {expected}.");
        }

        private void AssertWriteFouVariablePropertyIsEqualTo(string propertyName, bool expected)
        {
            bool valueEqual = GetPropertyValue<bool>(propertyName) == expected;

            Assert.That(valueEqual, Is.True, $"Expected property '{propertyName}' set to {expected}.");
        }

        private T GetPropertyValue<T>(string propertyName)
        {
            WaterFlowFMProperty modelProperty = GetModelProperty(propertyName);
            return (T)modelProperty.Value;
        }

        private void SetPropertyValue(string propertyName, object value)
        {
            WaterFlowFMProperty modelProperty = GetModelProperty(propertyName);
            modelProperty.Value = value;
        }

        private WaterFlowFMProperty GetModelProperty(string propertyName)
        {
            WaterFlowFMProperty modelProperty = modelDefinition.GetModelProperty(propertyName);
            Assert.That(modelProperty, Is.Not.Null, $"Model property '{propertyName}' does not exist in model definition.");

            return modelProperty;
        }

        private void WriteAndReadFouFile(string fouFileData)
        {
            using (var temp = new TemporaryDirectory())
            {
                temp.CreateFile(FouFileConstants.DefaultFileName, fouFileData);
                SetPropertyValue(KnownProperties.FouFile, FouFileConstants.DefaultFileName);

                fouFileReader.ReadFromDirectory(temp.Path);
            }
        }
    }
}