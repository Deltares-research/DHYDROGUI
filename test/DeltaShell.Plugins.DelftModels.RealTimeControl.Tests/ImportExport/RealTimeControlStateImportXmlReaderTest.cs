using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.Handlers;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.ImportExport
{
    [TestFixture]
    public class RealTimeControlStateImportXmlReaderTest
    {
        private RealTimeControlStateImportXmlReader stateImportReader;
        private ILogHandler logHandler;
        private readonly string directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "StateImportFiles"));

        [SetUp]
        public void SetUp()
        {
            logHandler = new LogHandler("");
            stateImportReader = new RealTimeControlStateImportXmlReader(logHandler);
        }

        [TearDown]
        public void TearDown()
        {
            logHandler = null;
            stateImportReader = null;
        }

        [Test]
        public void GivenANonExistingFile_WhenReading_ThenExpectedMessageIsGiven()
        {
            // Given
            const string filePath = "invalid";
            Assert.That(!File.Exists(filePath),
                $"File path '{filePath}' was expected to not exist.");

            // When
            stateImportReader.Read(filePath, new List<ConnectionPoint>());

            // Then
            Assert.IsTrue(logHandler.LogMessagesTable.AllMessages.Any(m => m.Contains(filePath)),
                "The collected log messages did not contain the expected message.");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnExistingFileAndNullIsGivenAsParameterConnectionPoints_WhenReading_ThenMethodDoesNotThrowAnException()
        {
            // Given
            var filePath = Path.Combine(directoryPath, "state_import.xml");
            Assert.That(File.Exists(filePath),
                $"File path '{filePath}' was expected to exist.");

            // When, Then
            Assert.DoesNotThrow(() => stateImportReader.Read(filePath, null));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnExistingFileWithValidData_WhenReading_ThenOutputValuesAreSet()
        {
            // Given
            var filePath = Path.Combine(directoryPath, "state_import.xml");
            Assert.That(File.Exists(filePath),
                $"File path '{filePath}' was expected to exist.");

            var connectionPoints = CreateListOfConnectionPoints();
            Assert.IsTrue(connectionPoints.All(p => p.Value == 0), 
                "Initial values of all the connection points were expected to be 0.");

            // When
            stateImportReader.Read(filePath, connectionPoints);

            // Then
            Assert.IsFalse(connectionPoints.All(p => p.Value == 0), 
                "Not all of the imported values for the connection points were expected to be 0.");
        }

        private static List<ConnectionPoint> CreateListOfConnectionPoints()
        {
            var connectionPoints = new List<ConnectionPoint>
            {
                new Output {Name = "[Output]a/b"},
                new Output {Name = "[Output]b/c"},
                new Input {Name = "[Input]c/d"},
                new Input {Name = "[Input]d/e"}
            };

            return connectionPoints;
        }
    }
}
