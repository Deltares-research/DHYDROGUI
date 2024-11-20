using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO
{
    [TestFixture]
    public class DelwaqNetCdfHistoryFileReaderTest
    {
        [Test]
        public void Read_WithFilePathNotExisting_ThenThrowsArgumentException()
        {
            const string invalidPath = "no_exist";

            // Call
            DelwaqHisFileData[] Call() => DelwaqNetCdfHistoryFileReader.Read(invalidPath);

            // Assert
            DelwaqHisFileData[] data = {};
            IReadOnlyList<string> errorMessages = TestHelper.GetAllRenderedMessages(() => data = Call(), Level.Error).ToArray();

            Assert.That(data, Is.Empty);
            Assert.That(errorMessages, Has.Count.EqualTo(1));
            Assert.That(errorMessages.Single(), Is.EqualTo($"History file was not found at {invalidPath}."));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Read_FromValidNetCdfFile_ThenExpectedHisFileDataIsReturned()
        {
            // Given
            string filePath = TestHelper.GetTestFilePath(@"IO\deltashell_his.nc");

            // When
            DelwaqHisFileData[] hisFileData = DelwaqNetCdfHistoryFileReader.Read(filePath);

            // Then
            Assert.That(hisFileData, Has.Length.EqualTo(1),
                        "One HisFileData was expected to be created, because there was one observation point in the file.");
            DelwaqHisFileData data = hisFileData.Single();
            Assert.That(data.ObservationVariable, Is.EqualTo("Observation Point01"),
                        "Observation variable was different than expected.");
            Assert.That(data.TimeSteps, Is.EqualTo(GetExpectedDateTimes()),
                        "Date times were different than expected.");
            IEnumerable<List<double>> valuesPerTimeStep = data.TimeSteps.Select(t => data.GetValuesForTimeStep(t));
            Assert.That(valuesPerTimeStep.All(v => v.Count == 2), Is.True,
                        "2 values per time step were expected (one per output variable).");
            Assert.That(valuesPerTimeStep, Is.EqualTo(GetExpectedValues()),
                        "Time series values were different than expected.");
            Assert.That(data.OutputVariables.First(), Is.EqualTo("Salinity"),
                        "Output variable was different than expected.");
            Assert.That(data.OutputVariables.Last(), Is.EqualTo("EColi"),
                        "Output variable was different than expected.");
        }

        [TestCase("")]
        [TestCase(null)]
        public void Read_WithFilePathNullOrEmpty_ThenThrowsArgumentException(string filePathArgument)
        {
            // Call
            void Call() => DelwaqNetCdfHistoryFileReader.Read(filePathArgument);

            // Assert
            var exception = Assert.Throws<ArgumentException>(Call);
            Assert.That(exception.Message, Is.EqualTo("Argument 'filePath' cannot be null or empty."));
        }

        private static IEnumerable<DateTime> GetExpectedDateTimes()
        {
            var referenceDate = new DateTime(2001, 1, 1);
            for (var i = 0; i < 7; i++)
            {
                yield return referenceDate.AddHours(4 * i);
            }
        }

        private static IEnumerable<IEnumerable<double>> GetExpectedValues()
        {
            yield return new[]
            {
                5.0,
                5.0
            };
            yield return new[]
            {
                3.7563545703887939,
                3.3857002258300781
            };
            yield return new[]
            {
                2.8748416900634766,
                2.3378903865814209
            };
            yield return new[]
            {
                2.5587210655212402,
                1.8783777952194214
            };
            yield return new[]
            {
                2.1056840419769287,
                1.395979642868042
            };
            yield return new[]
            {
                1.6477864980697632,
                0.98714113235473633
            };
            yield return new[]
            {
                1.2499129772186279,
                0.67690962553024292
            };
        }
    }
}