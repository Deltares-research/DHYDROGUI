using System;
using System.Collections.Generic;
using System.Globalization;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter.RRBoundaryConditionsHelpers;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.PartialSobekImport.RRBoundaryConditionsHelpers
{
    [TestFixture]
    public class RRBoundaryConditionsConstantParserTest
    {
        [Test]
        public void Constructor_ArgumentNull_ThrowsArgumentNullException()
        {
            // Arrange & Act
            void Call() => new RRBoundaryConditionsConstantParser(null);

            // Assert
            Assert.Throws<ArgumentNullException>(Call);
        }

        [Test]
        public void Parse_ArgumentNull_ThrowsArgumentNullException()
        {
            // Arrange
            var parser = new RRBoundaryConditionsConstantParser(Substitute.For<ILogHandler>());

            // Act
            void Call() => parser.Parse(null);

            // Assert
            Assert.Throws<ArgumentNullException>(Call);
        }

        [Test]
        public void Parse_ArgumentNotConstant_ThrowsArgumentException()
        {
            // Arrange
            var parser = new RRBoundaryConditionsConstantParser(Substitute.For<ILogHandler>());
            const string expectedArgumentExceptionMessage = "The provided 'BcBlockData' is not constant.";
            var bcBlockData = new BcBlockData { FunctionType = "NotConstant" };

            // Act
            void Call() => parser.Parse(bcBlockData);

            // Assert
            var e = Assert.Throws<ArgumentException>(Call);
            Assert.That(e.Message, Is.EqualTo(expectedArgumentExceptionMessage));
        }

        [Test]
        public void GivenValidBcDataBlock_WhenParsing_ThenExpectCorrectlyParsed()
        {
            // Arrange
            var parser = new RRBoundaryConditionsConstantParser(Substitute.For<ILogHandler>());
            var quantityData = new BcQuantityData();
            const double value = 10;
            quantityData.Values.Add(value.ToString(CultureInfo.InvariantCulture));
            var bcBlockData = new BcBlockData()
            {
                SupportPoint = "NameOfSupportPoint",
                FunctionType = "constant",
                Quantities = new List<BcQuantityData>() { quantityData }
            };

            // Act
            RainfallRunoffBoundaryData rainfallRunoffBoundaryData = parser.Parse(bcBlockData);

            // Assert
            Assert.That(rainfallRunoffBoundaryData.IsConstant, Is.True);
            Assert.That(rainfallRunoffBoundaryData.Value, Is.EqualTo(value));
        }

        [Test]
        public void GivenNoDataInBcDataBlock_WhenParsing_ThenExpectWarningAndValueOf0()
        {
            // Arrange
            const string nameOfSupportPoint = "NameOfSupportPoint";
            var logHandler = Substitute.For<ILogHandler>();
            var parser = new RRBoundaryConditionsConstantParser(logHandler);

            var bcBlockData = new BcBlockData()
            {
                SupportPoint = nameOfSupportPoint,
                FunctionType = "constant",
            };

            // Act
            RainfallRunoffBoundaryData rainfallRunoffBoundaryData = parser.Parse(bcBlockData);

            // Assert
            var expectedMessage = $"No boundary data available for boundary \"{bcBlockData.SupportPoint}\"";
            logHandler.Received(1).ReportWarning(expectedMessage);

            Assert.That(rainfallRunoffBoundaryData.IsConstant, Is.True);
            Assert.That(rainfallRunoffBoundaryData.Value, Is.EqualTo(0));
        }

        [Test]
        public void GivenInvalidDataInBcDataBlock_WhenParsing_ThenExpectWarningAndValueOf0()
        {
            // Arrange
            const string nameOfSupportPoint = "NameOfSupportPoint";
            const string invalidData = "InvalidData";
            var logHandler = Substitute.For<ILogHandler>();
            var parser = new RRBoundaryConditionsConstantParser(logHandler);
            var quantityData = new BcQuantityData();
            quantityData.Values.Add(invalidData);
            var bcBlockData = new BcBlockData()
            {
                SupportPoint = nameOfSupportPoint,
                FunctionType = "constant",
                Quantities = new List<BcQuantityData>() { quantityData }
            };

            // Act
            RainfallRunoffBoundaryData rainfallRunoffBoundaryData = parser.Parse(bcBlockData);

            // Assert
            const string expectedValueType = "Double";
            var expectedMessage = $"No valid data available for boundary \"{bcBlockData.SupportPoint}\", data \"{invalidData}\" is not of format \"{expectedValueType}\"";
            logHandler.Received(1).ReportError(expectedMessage);

            Assert.That(rainfallRunoffBoundaryData.IsConstant, Is.True);
            Assert.That(rainfallRunoffBoundaryData.Value, Is.EqualTo(0));
        }
    }
}