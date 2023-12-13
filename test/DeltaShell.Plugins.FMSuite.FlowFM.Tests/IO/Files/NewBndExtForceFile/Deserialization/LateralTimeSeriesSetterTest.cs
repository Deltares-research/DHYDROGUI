using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.Laterals;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Deserialization;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.NewBndExtForceFile.Deserialization
{
    [TestFixture]
    public class LateralTimeSeriesSetterTest
    {
        private ILogHandler logHandler;

        [SetUp]
        public void SetUp()
        {
            logHandler = Substitute.For<ILogHandler>();
        }
        
        [Test]
        [TestCaseSource(nameof(Constructor_ArgNullCases))]
        public void Constructor_ArgNull_ThrowsArgumentNullException(ILogHandler logHandlerArgNullTest, IEnumerable<BcBlockData> bcBlockData)
        {
            // Call
            void Call() => _ = new LateralTimeSeriesSetter(logHandlerArgNullTest, bcBlockData);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void SetDischargeFunction_LateralIdNullOrWhiteSpace_ThrowsArgumentException(string lateralId)
        {
            IEnumerable<BcBlockData> bcBlockData = Enumerable.Empty<BcBlockData>();
            var lateralTimeSeriesSetter = new LateralTimeSeriesSetter(logHandler, bcBlockData);
            var lateralDischargeFunction = new LateralDischargeFunction();

            // Call
            void Call() => lateralTimeSeriesSetter.SetDischargeFunction(lateralId, lateralDischargeFunction);

            // Assert
            Assert.That(Call, Throws.ArgumentException);
        }

        [Test]
        public void SetDischargeFunction_DischargeFunctionNull_ThrowsArgumentNullException()
        {
            IEnumerable<BcBlockData> bcBlockData = Enumerable.Empty<BcBlockData>();
            var lateralTimeSeriesSetter = new LateralTimeSeriesSetter(logHandler, bcBlockData);
            const string lateralId = "some_id";

            // Call
            void Call() => lateralTimeSeriesSetter.SetDischargeFunction(lateralId, null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void SetDischargeFunction_MissingCorrespondingDataForId_ReportsError()
        {
            BcBlockData bcBlockData = BcBlockDataBuilder.Start().WithLateralId("some_id").WithFunctionType("timeseries").WithTimeQuantity().WithLateralDischargeQuantity().Finish();
            var lateralTimeSeriesSetter = new LateralTimeSeriesSetter(logHandler, new[] { bcBlockData });
            var lateralDischargeFunction = new LateralDischargeFunction();

            // Call
            lateralTimeSeriesSetter.SetDischargeFunction("some_other_id", lateralDischargeFunction);

            // Assert
            logHandler.Received(1).ReportError($"No BC data could be found for lateral with id 'some_other_id'.");
        }

        [Test]
        public void SetDischargeFunction_UnsupportedFunctionType_ReportsError()
        {
            BcBlockData bcBlockData = BcBlockDataBuilder.Start().WithLateralId("some_id").WithFunctionType("some_unsupported_function").WithTimeQuantity().WithLateralDischargeQuantity().Finish();
            var lateralTimeSeriesSetter = new LateralTimeSeriesSetter(logHandler, new[] { bcBlockData });
            var lateralDischargeFunction = new LateralDischargeFunction();

            // Call
            lateralTimeSeriesSetter.SetDischargeFunction("some_id", lateralDischargeFunction);

            // Assert
            logHandler.Received(1).ReportError($"Function type 'some_unsupported_function' is not supported for lateral with id 'some_id'. Line: 0");
        }

        [Test]
        public void SetDischargeFunction_MissingTimeQuantity_ReportsError()
        {
            BcBlockData bcBlockData = BcBlockDataBuilder.Start().WithLateralId("some_id").WithFunctionType("timeseries").WithLateralDischargeQuantity().Finish();
            var lateralTimeSeriesSetter = new LateralTimeSeriesSetter(logHandler, new[] { bcBlockData });
            var lateralDischargeFunction = new LateralDischargeFunction();

            // Call
            lateralTimeSeriesSetter.SetDischargeFunction("some_id", lateralDischargeFunction);

            // Assert
            logHandler.Received(1).ReportError($"Quantity 'time' could not be found for lateral with id 'some_id'. Line: 0");
        }

        [Test]
        public void SetDischargeFunction_MissingLateralDischargeQuantity_ReportsError()
        {
            BcBlockData bcBlockData = BcBlockDataBuilder.Start().WithLateralId("some_id").WithFunctionType("timeseries").WithTimeQuantity().Finish();
            var lateralTimeSeriesSetter = new LateralTimeSeriesSetter(logHandler, new[] { bcBlockData });
            var lateralDischargeFunction = new LateralDischargeFunction();

            // Call
            lateralTimeSeriesSetter.SetDischargeFunction("some_id", lateralDischargeFunction);

            // Assert
            logHandler.Received(1).ReportError($"Quantity 'lateral_discharge' could not be found for lateral with id 'some_id'. Line: 0");
        }

        [Test]
        public void SetDischargeFunction_WithInvalidDischargeValue_ReportsError()
        {
            BcBlockData bcBlockData = BcBlockDataBuilder.Start().WithLateralId("some_id").WithFunctionType("timeseries").WithTimeQuantity().Finish();
            var dischargeQuantityData = new BcQuantityData
            {
                QuantityName = "lateral_discharge",
                Unit = "m3/s",
                Values = new List<string>
                {
                    "1/.23",
                    "2.34",
                    "3.45"
                }
            };
            bcBlockData.Quantities.Add(dischargeQuantityData);
            var lateralTimeSeriesSetter = new LateralTimeSeriesSetter(logHandler, new[] { bcBlockData });
            var lateralDischargeFunction = new LateralDischargeFunction();

            // Call
            lateralTimeSeriesSetter.SetDischargeFunction("some_id", lateralDischargeFunction);

            // Assert
            logHandler.Received(1).ReportError($"Could not parse '1/.23' to a floating value. Line: 0");
        }

        [Test]
        public void SetDischargeFunction_SetsTheCorrectDataOnTheLateralDischargeFunction()
        {
            BcBlockData bcBlockData = BcBlockDataBuilder.Start().WithLateralId("some_id").WithFunctionType("timeseries").WithTimeQuantity().WithLateralDischargeQuantity().Finish();
            var lateralTimeSeriesSetter = new LateralTimeSeriesSetter(logHandler, new[] { bcBlockData });
            var lateralDischargeFunction = new LateralDischargeFunction();

            // Call
            lateralTimeSeriesSetter.SetDischargeFunction("some_id", lateralDischargeFunction);

            // Assert
            Assert.That(lateralDischargeFunction.Time.InterpolationType, Is.EqualTo(InterpolationType.Constant));

            var referenceDate = new DateTime(2023, 7, 31);
            Assert.That(lateralDischargeFunction[referenceDate.AddSeconds(60)], Is.EqualTo(1.23));
            Assert.That(lateralDischargeFunction[referenceDate.AddSeconds(120)], Is.EqualTo(2.34));
            Assert.That(lateralDischargeFunction[referenceDate.AddSeconds(180)], Is.EqualTo(3.45));
        }
        
        [Test]
        [TestCaseSource(nameof(TimeZones))]
        public void SetDischargeFunctionWithTimeZone_SetsTheCorrectTimeZoneOnTheLateralDischargeFunction(string timeQuantityTimeZone, TimeSpan expectedTimeZone)
        {
            // Arrange
            BcBlockData bcBlockData = BcBlockDataBuilder.Start().WithLateralId("some_id").WithFunctionType("timeseries").WithTimeQuantityWithTimeZone(timeQuantityTimeZone).WithLateralDischargeQuantity().Finish();
            var lateralTimeSeriesSetter = new LateralTimeSeriesSetter(logHandler, new[] { bcBlockData });
            var lateralDischargeFunction = new LateralDischargeFunction();

            // Call
            lateralTimeSeriesSetter.SetDischargeFunction("some_id", lateralDischargeFunction);

            // Assert
            Assert.That(lateralDischargeFunction.TimeZone, Is.EqualTo(expectedTimeZone));
        }
        
        [Test]
        public void SetDischargeFunctionWithNoTimeZone_SetsTheCorrectZeroTimeZoneOnTheLateralDischargeFunction()
        {
            // Arrange
            BcBlockData bcBlockData = BcBlockDataBuilder.Start().WithLateralId("some_id").WithFunctionType("timeseries").WithTimeQuantity().WithLateralDischargeQuantity().Finish();
            var lateralTimeSeriesSetter = new LateralTimeSeriesSetter(logHandler, new[] { bcBlockData });
            var lateralDischargeFunction = new LateralDischargeFunction();

            // Call
            lateralTimeSeriesSetter.SetDischargeFunction("some_id", lateralDischargeFunction);

            // Assert
            Assert.That(lateralDischargeFunction.TimeZone, Is.EqualTo(TimeSpan.Zero));
        }
        
        private static IEnumerable<TestCaseData> TimeZones()
        {
            var timeZone = new TimeSpan(1, 0, 0);
            yield return new TestCaseData($"+{timeZone:hh\\:mm}", timeZone).SetName("Time zone of +1 hour");
            
            timeZone = new TimeSpan(10, 0, 0);
            yield return new TestCaseData($"+{timeZone:hh\\:mm}", timeZone).SetName("Time zone of +10 hours");
            
            timeZone = new TimeSpan(-1, 0, 0);
            yield return new TestCaseData($"-{timeZone:hh\\:mm}", timeZone).SetName("Time zone of -1 hour");
            
            timeZone = new TimeSpan(-10, 0, 0);
            yield return new TestCaseData($"-{timeZone:hh\\:mm}", timeZone).SetName("Time zone of -10 hours");
        }

        private static IEnumerable<TestCaseData> Constructor_ArgNullCases()
        {
            var logHandler = Substitute.For<ILogHandler>();
            IEnumerable<BcBlockData> bcBlockData = Enumerable.Empty<BcBlockData>();

            yield return new TestCaseData(null, bcBlockData);
            yield return new TestCaseData(logHandler, null);
        }

        private class BcBlockDataBuilder
        {
            private readonly BcBlockData bcBlockData;
            private BcBlockDataBuilder() => bcBlockData = new BcBlockData { TimeInterpolationType = "block" };

            public static BcBlockDataBuilder Start() => new BcBlockDataBuilder();

            public BcBlockDataBuilder WithLateralId(string lateralId)
            {
                bcBlockData.SupportPoint = lateralId;
                return this;
            }

            public BcBlockDataBuilder WithFunctionType(string functionType)
            {
                bcBlockData.FunctionType = functionType;
                return this;
            }

            public BcBlockDataBuilder WithTimeQuantity()
            {
                var timeQuantityData = new BcQuantityData
                {
                    QuantityName = "time",
                    Unit = "seconds since 2023-07-31 00:00:00",
                    Values = new List<string>
                    {
                        "60",
                        "120",
                        "180"
                    }
                };
                bcBlockData.Quantities.Add(timeQuantityData);
                return this;
            }
            
            public BcBlockDataBuilder WithTimeQuantityWithTimeZone(string timeZone)
            {
                var timeQuantityData = new BcQuantityData
                {
                    QuantityName = "time",
                    Unit = $"seconds since 2023-07-31 00:00:00 {timeZone}",
                    Values = new List<string>
                    {
                        "60",
                        "120",
                        "180"
                    }
                };
                bcBlockData.Quantities.Add(timeQuantityData);
                return this;
            }

            public BcBlockDataBuilder WithLateralDischargeQuantity()
            {
                var dischargeQuantityData = new BcQuantityData
                {
                    QuantityName = "lateral_discharge",
                    Unit = "m3/s",
                    Values = new List<string>
                    {
                        "1.23",
                        "2.34",
                        "3.45"
                    }
                };
                bcBlockData.Quantities.Add(dischargeQuantityData);
                return this;
            }

            public BcBlockData Finish() => bcBlockData;
        }
    }
}