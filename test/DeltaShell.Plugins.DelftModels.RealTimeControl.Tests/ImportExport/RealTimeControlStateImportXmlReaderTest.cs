using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.ImportExport
{
    [TestFixture]
    public class RealTimeControlStateImportXmlReaderTest
    {
        [Test]
        public void GivenANonExistingFile_WhenReading_ThenExpectedMessageIsGiven()
        {
            // Given
            var filePath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "Invalid"));
            Assert.That(!File.Exists(filePath));

            var outputs = new List<Output>();

            TestHelper.AssertLogMessageIsGenerated(() =>
            {
                // When
                RealTimeControlStateImportXmlReader.Read(filePath, outputs);
            },
                string.Format(Resources.RealTimeControlStateImportXmlReader_Read_File___0___does_not_exist_, filePath));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnExistingFileAndNullIsGivenAsParameterForOutputs_WhenReading_ThenMethodIsReturnedAndNothingHappens()
        {
            // Given
            var fileName = "state_import.xml";
            var directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "StateImportFiles"));
            var filePath = Path.Combine(directoryPath, fileName);

            Assert.That(Directory.Exists(directoryPath));
            Assert.That(File.Exists(filePath));

            Assert.DoesNotThrow(() =>
            {
                RealTimeControlStateImportXmlReader.Read(filePath, null);
            });
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnExistingFileWithValidData_WhenReading_ThenCorrectOutputValuesAreSet()
        {
            // Given
            var fileName = "state_import.xml";
            var directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "StateImportFiles"));
            var filePath = Path.Combine(directoryPath, fileName);

            Assert.That(Directory.Exists(directoryPath));
            Assert.That(File.Exists(filePath));
          
            var output1 = new Output {Name = "[Output]a/b"};
            var output2 = new Output {Name = "[Output]b/c"};
            var output3 = new Output {Name = "[Output]c/d"};
            var output4 = new Output {Name = "[Output]d/e"};

            // When
            RealTimeControlStateImportXmlReader.Read(filePath, new [] { output1, output2, output3, output4 });

            // Then
            Assert.AreEqual(0, output1.Value);
            Assert.AreEqual(0.1, output2.Value);
            Assert.AreEqual(2.2, output3.Value);
            Assert.AreEqual(303.03, output4.Value);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnExistingFileWithExtraOutput_WhenReading_ThenExpectedErrorMessageIsGivenAndCorrectOutputValuesAreSetOnOtherOutputs()
        {
            // Given
            var fileName = "state_import.xml";
            var directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "StateImportFiles"));
            var filePath = Path.Combine(directoryPath, fileName);

            Assert.That(Directory.Exists(directoryPath));
            Assert.That(File.Exists(filePath));

            var output1 = new Output { Name = "[Output]a/b" };
            var output2 = new Output { Name = "[Output]b/c" };
            var output3 = new Output { Name = "[Output]c/d" };

            // When
            TestHelper.AssertLogMessageIsGenerated(() =>
            {
                RealTimeControlStateImportXmlReader.Read(filePath, new[] { output1, output2, output3 });
            }, 
                string.Format(Resources.RealTimeControlStateImportXmlReader_Read_Could_not_find_output_with_name___0___that_is_referenced_in_file___1____Please_check_file___2__, "[Output]d/e", filePath, RealTimeControlXMLFiles.XmlData ));
            
            // Then
            Assert.AreEqual(0, output1.Value);
            Assert.AreEqual(0.1, output2.Value);
            Assert.AreEqual(2.2, output3.Value);
        }
    }
}
