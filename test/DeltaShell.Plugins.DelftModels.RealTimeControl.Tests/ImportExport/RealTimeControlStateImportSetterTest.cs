using DeltaShell.NGHS.IO.Handlers;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Xsd;
using NUnit.Framework;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.ImportExport
{
    [TestFixture]
    public class RealTimeControlStateImportSetterTest
    {
        private RealTimeControlStateImportSetter stateImportSetter;
        private ILogHandler logHandler;

        const string InputName = "[Input]a/b";
        const string OutputName = "[Output]c/d";

        [SetUp]
        public void SetUp()
        {
            logHandler = new LogHandler("");
            stateImportSetter = new RealTimeControlStateImportSetter(logHandler);
        }

        [TearDown]
        public void TearDown()
        {
            logHandler = null;
            stateImportSetter = null;
        }

        [Test]
        public void GivenAListOfConnectionPointsAndAListOfMatchingTreeVectorLeafXmlObject_WhenSetStateImportOnConnectionPointsIsCalled_ThenCorrectValuesAreSet()
        {
            // Given
            var connectionPoints = CreateListOfConnectionPoints(InputName, OutputName);

            const double inputValue = 0.1;
            const double outputValue = 303.03;
            var treeVectorLeaves = CreateListOfTreeVectorLeaves(InputName, OutputName, inputValue, outputValue);

            // When
            stateImportSetter.SetStateImportOnConnectionPoints(connectionPoints, treeVectorLeaves);

            // Then
            Assert.AreEqual(inputValue, connectionPoints.OfType<Input>().First().Value,
                $"Import state for input was expected to be {inputValue}.");
            Assert.AreEqual(outputValue, connectionPoints.OfType<Output>().First().Value,
                $"Import state for output was expected to be {outputValue}.");
        }

        [Test]
        public void GivenAListOfConnectionPointsThatDoesNotContainConnectionPointReferencedByTreeVectorLeafXmlObject_WhenSetStateImportOnConnectionPointsIsCalled_ThenCorrectValuesAreSet()
        {
            // Given
            var treeVectorLeaves = CreateListOfTreeVectorLeaves(InputName, OutputName);

            var expectedMessage = string.Format(
                Resources.RealTimeControlStateImportXmlReader_Read_Could_not_find_output_with_name___0___that_is_referenced_in_file___1____Please_check_file___2__,
                InputName, RealTimeControlXMLFiles.XmlImportState, RealTimeControlXMLFiles.XmlData);

            // When
            stateImportSetter.SetStateImportOnConnectionPoints(new List<ConnectionPoint>(), treeVectorLeaves);

            // Then
            Assert.IsTrue(logHandler.LogMessagesTable.AllMessages.Contains(expectedMessage),
                "The collected log messages did not contain the expected message.");
        }

        private static List<ConnectionPoint> CreateListOfConnectionPoints(string inputName, string outputName)
        {
            var connectionPoints = new List<ConnectionPoint>
            {
                new Input {Name = inputName},
                new Output {Name = outputName}
            };

            return connectionPoints;
        }

        private static IList<TreeVectorLeafXML> CreateListOfTreeVectorLeaves(
            string inputName, string outputName,
            double inputValue = 1, double outputValue = 1)
        {
            var treeVectorLeaves = new List<TreeVectorLeafXML>
            {
                new TreeVectorLeafXML
                {
                    id = inputName,
                    vector = inputValue.ToString(CultureInfo.InvariantCulture)
                },
                new TreeVectorLeafXML
                {
                    id = outputName,
                    vector = outputValue.ToString(CultureInfo.InvariantCulture)
                }
            };

            return treeVectorLeaves;
        }
    }
}
