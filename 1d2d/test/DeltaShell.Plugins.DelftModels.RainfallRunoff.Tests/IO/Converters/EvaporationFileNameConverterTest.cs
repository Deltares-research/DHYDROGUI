using System;
using System.Collections.Generic;
using System.ComponentModel;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Converters;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.DataAccessObjects;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.IO.Converters
{
    [TestFixture]
    public class EvaporationFileNameConverterTest
    {
        [Test]
        [TestCaseSource(nameof(ArgNullCases))]
        public void FromFileName_ArgNull_ThrowsArgumentNullException(string fileName, ILogHandler logHandler, string expParamName)
        {
            // Setup
            var converter = new EvaporationFileNameConverter();

            // Call
            void Call() => converter.FromFileName(fileName, logHandler);

            // Assert
            Assert.That(Call, Throws.Exception.TypeOf<ArgumentNullException>()
                                    .With.Property(nameof(ArgumentNullException.ParamName))
                                    .EqualTo(expParamName));
        }

        [Test]
        [TestCase("default.evp", IOEvaporationMeteoDataSource.UserDefined)]
        [TestCase("random.evp", IOEvaporationMeteoDataSource.UserDefined)]
        [TestCase("evapor.gem", IOEvaporationMeteoDataSource.LongTermAverage)]
        [TestCase("evapor.plv", IOEvaporationMeteoDataSource.GuidelineSewerSystems)]
        [TestCase("DEFAULT.evp", IOEvaporationMeteoDataSource.UserDefined)]
        [TestCase("RaNdoM.eVp", IOEvaporationMeteoDataSource.UserDefined)]
        [TestCase("evaPor.GEM", IOEvaporationMeteoDataSource.LongTermAverage)]
        [TestCase("evapor.PLV", IOEvaporationMeteoDataSource.GuidelineSewerSystems)]
        public void FromFileName_SupportedFileName_ReturnsCorrectMeteoDataSource(string fileName, IOEvaporationMeteoDataSource expResult)
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var converter = new EvaporationFileNameConverter();

            // Call
            IOEvaporationMeteoDataSource result = converter.FromFileName(fileName, logHandler);

            // Assert
            Assert.That(result, Is.EqualTo(expResult));
            Assert.That(logHandler.ReceivedCalls(), Is.Empty);
        }

        [Test]
        [TestCase("evapor.random")]
        [TestCase("default.gem")]
        [TestCase("default.plv")]
        public void FromFileName_UnsupportedFileName_LogsErrorAndReturnsUserDefined(string fileName)
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var converter = new EvaporationFileNameConverter();

            // Call
            IOEvaporationMeteoDataSource result = converter.FromFileName(fileName, logHandler);

            // Assert
            Assert.That(result, Is.EqualTo(IOEvaporationMeteoDataSource.UserDefined));
            logHandler.Received(1).ReportErrorFormat("{0} is not a supported evaporation file.", fileName);
        }

        [Test]
        public void ToFileName_IOEvaporationMeteoDataSourceUndefined_ThrowsInvalidEnumArgumentException()
        {
            // Setup
            var converter = new EvaporationFileNameConverter();

            // Call
            void Call() => converter.ToFileName((IOEvaporationMeteoDataSource)99);

            // Assert
            Assert.That(Call, Throws.Exception.TypeOf<InvalidEnumArgumentException>()
                                    .With.Message.EqualTo("ioEvaporationMeteoDataSource"));
        }

        [Test]
        [TestCase(IOEvaporationMeteoDataSource.UserDefined, "default.evp")]
        [TestCase(IOEvaporationMeteoDataSource.LongTermAverage, "EVAPOR.GEM")]
        [TestCase(IOEvaporationMeteoDataSource.GuidelineSewerSystems, "EVAPOR.PLV")]
        public void ToFileName_ReturnsCorrectFileName(IOEvaporationMeteoDataSource ioEvaporationMeteoDataSource, string expResult)
        {
            // Setup
            var converter = new EvaporationFileNameConverter();

            // Call
            string result = converter.ToFileName(ioEvaporationMeteoDataSource);

            // Assert
            Assert.That(result, Is.EqualTo(expResult));
        }

        private static IEnumerable<TestCaseData> ArgNullCases()
        {
            yield return new TestCaseData(null, Substitute.For<ILogHandler>(), "fileName").SetName("fileName null");
            yield return new TestCaseData("file.name", null, "logHandler").SetName("logHandler null");
        }
    }
}