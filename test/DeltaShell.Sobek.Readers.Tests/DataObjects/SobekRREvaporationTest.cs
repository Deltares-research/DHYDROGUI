using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.DataObjects
{
    [TestFixture]
    public class SobekRREvaporationTest
    {
        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var evaporation = new SobekRREvaporation();

            // Assert
            Assert.That(evaporation.IsLongTimeAverage, Is.False);
            Assert.That(evaporation.NumberOfLocations, Is.Zero);
            Assert.That(evaporation.Data, Is.Not.Null);
            Assert.That(evaporation.Data, Is.Empty);
        }

        [Test]
        public void Add_ValuesNull_ThrowsArgumentNullException()
        {
            // Setup
            var evaporation = new SobekRREvaporation();

            // Call
            void Call() => evaporation.Add(2021, 06, 15, null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("values"));
        }

        [Test]
        public void Add_LongTimeAverageFalse_AddsCorrectlyToData()
        {
            // Setup
            var evaporation = new SobekRREvaporation { IsLongTimeAverage = false };
            var values = new[]
            {
                0.5,
                0.3
            };

            // Call
            evaporation.Add(2021, 6, 15, values);

            // Assert
            Assert.That(evaporation.Data, Has.Count.EqualTo(1));
            Assert.That(evaporation.Data[new DateTime(2021, 6, 15)], Is.EqualTo(values));
        }

        [Test]
        public void Add_LongTimeAverageTrue_AddsCorrectlyToData()
        {
            // Setup
            var evaporation = new SobekRREvaporation { IsLongTimeAverage = true };
            var values = new[]
            {
                0.5,
                0.3
            };

            // Call
            evaporation.Add(0, 6, 15, values);

            // Assert
            Assert.That(evaporation.Data, Has.Count.EqualTo(1));
            Assert.That(evaporation.Data[new DateTime(4, 6, 15)], Is.EqualTo(values));
        }

        [Test]
        public void Add_InvalidDate_LogErrorAndDoesNotAddToData()
        {
            // Setup
            var evaporation = new SobekRREvaporation();

            // Call
            void Call() => evaporation.Add(2021, 13, 32, Array.Empty<double>());

            // Assert
            string error = TestHelper.GetAllRenderedMessages(Call, Level.Error).Single();
            Assert.That(error, Is.EqualTo("2021/13/32 is not a valid date."));
            Assert.That(evaporation.Data, Is.Empty);
        }

        [Test]
        public void Add_ZeroValues_LogErrorAndDoesNotAddToData()
        {
            // Setup
            var evaporation = new SobekRREvaporation();

            // Call
            void Call() => evaporation.Add(2021, 6, 15, Array.Empty<double>());

            // Assert
            string error = TestHelper.GetAllRenderedMessages(Call, Level.Error).Single();
            Assert.That(error, Is.EqualTo("2021/06/15 should have at least 1 evaporation value."));
            Assert.That(evaporation.Data, Is.Empty);
        }

        [Test]
        [TestCaseSource(nameof(Add_IncorrectNumberOfValuesCases))]
        public void Add_IncorrectNumberOfValues_LogErrorAndDoesNotAddToData(double[] values, int expNumberOfLocations, string expValueStr)
        {
            // Setup
            var evaporation = new SobekRREvaporation();
            evaporation.Add(2021, 6, 15, values);

            // Precondition
            Assert.That(evaporation.NumberOfLocations, Is.EqualTo(expNumberOfLocations));

            // Call
            void Call() =>
                evaporation.Add(2021, 6, 16, new[]
                {
                    0.1,
                    0.2,
                    0.3,
                    0.4
                });

            // Assert
            string error = TestHelper.GetAllRenderedMessages(Call, Level.Error).Single();
            Assert.That(error, Is.EqualTo($"2021/06/16 should have {expNumberOfLocations} evaporation {expValueStr}."));
            Assert.That(evaporation.Data, Has.Count.EqualTo(1));
        }

        [Test]
        public void ToLongTimeAverage_LongTimeAverageFalse_ThrowsInvalidOperationException()
        {
            // Setup
            var evaporation = new SobekRREvaporation { IsLongTimeAverage = false };

            // Call
            void Call() => evaporation.ToLongTimeAverage(DateTime.Today, DateTime.Today.AddDays(1));

            // Assert
            var e = Assert.Throws<InvalidOperationException>(Call);
            Assert.That(e.Message, Is.EqualTo("The evaporation should be long time average."));
        }

        [Test]
        public void ToLongTimeAverage_StopTimePrecedesStartTime_ThrowsArgumentOutOfRangeException()
        {
            // Setup
            var evaporation = new SobekRREvaporation { IsLongTimeAverage = true };

            // Call
            void Call() => evaporation.ToLongTimeAverage(DateTime.Today.AddDays(1), DateTime.Today);

            // Assert
            var e = Assert.Throws<ArgumentOutOfRangeException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("stopTime"));
            Assert.That(e.Message, Does.StartWith("The stop date should not precede the start date."));
        }

        [Test]
        public void ToLongTimeAverage_ReturnsCorrectData()
        {
            // Setup
            var evaporation = new SobekRREvaporation { IsLongTimeAverage = true };
            AddLongTimeAverageData(evaporation);

            var startTime = new DateTime(2020, 12, 27);
            var stopTime = new DateTime(2021, 1, 5);

            // Call
            evaporation.ToLongTimeAverage(startTime, stopTime);

            // Assert
            Assert.That(evaporation.Data, Has.Count.EqualTo(10));
            Assert.That(evaporation.Data[startTime].Single(), Is.EqualTo(362));
            Assert.That(evaporation.Data[startTime.AddDays(1)].Single(), Is.EqualTo(363));
            Assert.That(evaporation.Data[startTime.AddDays(2)].Single(), Is.EqualTo(364));
            Assert.That(evaporation.Data[startTime.AddDays(3)].Single(), Is.EqualTo(365));
            Assert.That(evaporation.Data[startTime.AddDays(4)].Single(), Is.EqualTo(366));
            Assert.That(evaporation.Data[startTime.AddDays(5)].Single(), Is.EqualTo(1));
            Assert.That(evaporation.Data[startTime.AddDays(6)].Single(), Is.EqualTo(2));
            Assert.That(evaporation.Data[startTime.AddDays(7)].Single(), Is.EqualTo(3));
            Assert.That(evaporation.Data[startTime.AddDays(8)].Single(), Is.EqualTo(4));
            Assert.That(evaporation.Data[startTime.AddDays(9)].Single(), Is.EqualTo(5));
        }

        [Test]
        public void Dates_GetsTheEvaporationDates()
        {
            // Setup
            var evaporation = new SobekRREvaporation();
            evaporation.Add(2021, 6, 15, new double[1]);
            evaporation.Add(2021, 6, 16, new double[1]);
            evaporation.Add(2021, 6, 17, new double[1]);

            // Call
            IEnumerable<DateTime> dates = evaporation.Dates;

            // Assert
            var expectedDates = new[]
            {
                new DateTime(2021, 6, 15),
                new DateTime(2021, 6, 16),
                new DateTime(2021, 6, 17),
            };
            Assert.That(dates, Is.EqualTo(expectedDates));
        }

        [Test]
        public void GetValuesByStationIndex_IndexNegative_ThrowsArgumentOutOfRangeException()
        {
            // Setup
            var evaporation = new SobekRREvaporation();
            evaporation.Add(2021, 6, 15, new double[2]);
            evaporation.Add(2021, 6, 16, new double[2]);
            evaporation.Add(2021, 6, 17, new double[2]);

            // Call
            void Call() => evaporation.GetValuesByStationIndex(-1);

            // Assert
            var e = Assert.Throws<ArgumentOutOfRangeException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("index"));
        }

        [Test]
        [TestCase(2)]
        [TestCase(3)]
        public void GetValuesByStationIndex_IndexEqualToOrHigherThanNumberOfLocations_ThrowsArgumentOutOfRangeException(int index)
        {
            // Setup
            var evaporation = new SobekRREvaporation();
            evaporation.Add(2021, 6, 15, new double[2]);
            evaporation.Add(2021, 6, 16, new double[2]);
            evaporation.Add(2021, 6, 17, new double[2]);

            // Call
            void Call() => evaporation.GetValuesByStationIndex(index);

            // Assert
            var e = Assert.Throws<ArgumentOutOfRangeException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("index"));
            Assert.That(e.Message, Does.StartWith($"index ({index}) can not be equal to or higher than NumberOfLocations (2)."));
        }

        [Test]
        [TestCaseSource(nameof(GetValuesByStationIndexCases))]
        public void GetValuesByStationIndex_GetsTheCorrectValues(int index, double[] expValues)
        {
            // Setup
            var evaporation = new SobekRREvaporation();
            evaporation.Add(2021, 6, 15, new double[3]
            {
                1,
                2,
                3
            });
            evaporation.Add(2021, 6, 16, new double[3]
            {
                4,
                5,
                6
            });
            evaporation.Add(2021, 6, 17, new double[3]
            {
                7,
                8,
                9
            });

            // Call
            IEnumerable<double> values = evaporation.GetValuesByStationIndex(index);

            // Assert
            Assert.That(values, Is.EqualTo(expValues));
        }

        private static IEnumerable<TestCaseData> Add_IncorrectNumberOfValuesCases()
        {
            yield return new TestCaseData(new[]
            {
                0.5
            }, 1, "value");
            yield return new TestCaseData(new[]
            {
                0.5,
                0.3
            }, 2, "values");
            yield return new TestCaseData(new[]
            {
                0.5,
                0.3,
                0.2
            }, 3, "values");
        }

        private static IEnumerable<TestCaseData> GetValuesByStationIndexCases()
        {
            yield return new TestCaseData(0, new double[]
            {
                1,
                4,
                7
            });
            yield return new TestCaseData(1, new double[]
            {
                2,
                5,
                8
            });
            yield return new TestCaseData(2, new double[]
            {
                3,
                6,
                9
            });
        }

        private static void AddLongTimeAverageData(SobekRREvaporation evaporation)
        {
            var startDate = new DateTime(4, 1, 1);
            var stopDate = new DateTime(4, 12, 31);

            for (DateTime date = startDate; date <= stopDate; date = date.AddDays(1))
            {
                evaporation.Add(date.Year, date.Month, date.Day, new double[]
                {
                    date.DayOfYear
                });
            }
        }
    }
}