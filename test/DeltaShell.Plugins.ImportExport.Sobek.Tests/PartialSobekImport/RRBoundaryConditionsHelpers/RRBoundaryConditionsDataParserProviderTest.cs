using System;
using System.Collections.Generic;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter.RRBoundaryConditionsHelpers;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.PartialSobekImport.RRBoundaryConditionsHelpers
{
    [TestFixture]
    public class RRBoundaryConditionsDataParserProviderTest
    {
        [Test]
        [TestCaseSource(nameof(ConstructorArgumentNullCases))]
        public void Constructor_ArgumentNull_ThrowsArgumentNullException(ILogHandler logHandler, IBcSectionParser parser)
        {
            // Arrange & Act
            void Call() => _ = new RRBoundaryConditionsDataParserProvider(logHandler, parser);

            // Assert
            Assert.Throws<ArgumentNullException>(Call);
        }

        [Test]
        public void GetParser_ArgumentNull_ThrowsArgumentNullException()
        {
            // Arrange & Act
            var parser = new RRBoundaryConditionsDataParserProvider(Substitute.For<ILogHandler>(), Substitute.For<IBcSectionParser>());
            void Call() => parser.GetParser(null);

            // Assert
            Assert.Throws<ArgumentNullException>(Call);
        }

        [Test]
        [TestCaseSource(nameof(ParserCases))]
        public void GivenBcDataBlockOfType_WhenGetParser_ThenReturnExpectedParserType(BcBlockData givenData, Type expectedParserType)
        {
            // Arrange
            var parser = new RRBoundaryConditionsDataParserProvider(Substitute.For<ILogHandler>(), Substitute.For<IBcSectionParser>());

            // Act
            IRRBoundaryConditionsDataParser retrievedParser = parser.GetParser(givenData);

            // Assert
            Assert.That(retrievedParser.GetType(), Is.EqualTo(expectedParserType));
        }

        [Test]
        public void GivenBcDataBlockWithInvalidFunctionType_WhenGetParser_ThenReturnInvalidFunctionParserAndLogging()
        {
            // Arrange
            const string boundaryConditionName = "SupportPoint";
            var logHandler = Substitute.For<ILogHandler>();
            var parser = new RRBoundaryConditionsDataParserProvider(logHandler, Substitute.For<IBcSectionParser>());
            var givenData = new BcBlockData
            {
                FunctionType = "unknown",
                SupportPoint = boundaryConditionName
            };

            // Act
            IRRBoundaryConditionsDataParser retrievedParser = parser.GetParser(givenData);

            //Assert
            var expectedMessage = $"Invalid function type for boundary condition \"{boundaryConditionName}\"";
            logHandler.Received(1).ReportError(expectedMessage);

            Assert.That(retrievedParser.GetType(), Is.EqualTo(typeof(RRBoundaryConditionsInvalidFunctionParser)));
        }

        private static IEnumerable<TestCaseData> ParserCases()
        {
            yield return new TestCaseData(new BcBlockData { FunctionType = "constant" },
                                          typeof(RRBoundaryConditionsConstantParser));
            yield return new TestCaseData(new BcBlockData { FunctionType = "timeseries" },
                                          typeof(RRBoundaryConditionsTimeSeriesParser));
        }

        private static IEnumerable<TestCaseData> ConstructorArgumentNullCases()
        {
            yield return new TestCaseData(null, Substitute.For<IBcSectionParser>());
            yield return new TestCaseData(Substitute.For<ILogHandler>(), null);
        }
    }
}