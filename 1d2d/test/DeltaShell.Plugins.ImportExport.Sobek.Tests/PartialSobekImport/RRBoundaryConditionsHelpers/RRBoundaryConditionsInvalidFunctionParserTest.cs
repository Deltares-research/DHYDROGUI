using System;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter.RRBoundaryConditionsHelpers;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.PartialSobekImport.RRBoundaryConditionsHelpers
{
    [TestFixture]
    public class RRBoundaryConditionsInvalidFunctionParserTest
    {
        [Test]
        public void Parse_ArgumentNull_ThrowsArgumentNullException()
        {
            // Arrange
            var parser = new RRBoundaryConditionsInvalidFunctionParser();

            // Act
            void Call() => parser.Parse(null);

            // Assert
            Assert.Throws<ArgumentNullException>(Call);
        }

        [Test]
        public void GivenBcBlockData_WhenParsing_ThenReturnNewRainfallRunoffBoundaryData()
        {
            // Arrange
            var parser = new RRBoundaryConditionsInvalidFunctionParser();

            // Act
            RainfallRunoffBoundaryData data = parser.Parse(new BcBlockData());

            //Assert
            var expectedData = new RainfallRunoffBoundaryData();
            Assert.That(data.Data.Arguments.Count, Is.EqualTo(expectedData.Data.Arguments.Count));
            Assert.That(data.Data.Components.Count, Is.EqualTo(expectedData.Data.Components.Count));
            Assert.That(data.Value, Is.EqualTo(expectedData.Value));
            Assert.That(data.IsConstant, Is.EqualTo(expectedData.IsConstant));
        }
    }
}