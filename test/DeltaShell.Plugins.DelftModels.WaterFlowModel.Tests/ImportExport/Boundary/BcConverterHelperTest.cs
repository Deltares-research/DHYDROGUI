using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Boundary;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Boundary
{
    [TestFixture]
    public class BcConverterHelperTest
    {
        /// <summary>
        /// GIVEN a IDelftBcQuantityData column with valid double values
        /// WHEN BcConverterHelper ParseDoubleValuesFromTableColumn is called
        /// THEN an enumerable containing the same valid double values in the same order is returned
        /// </summary>
        [Test]
        public void GivenAIDelftBcQuantityDataColumnWithValidDoubleValues_WhenBcConverterHelperParseDoubleValuesFromTableColumnIsCalled_ThenAnEnumerableContainingTheSameValidDoubleValuesInTheSameOrderIsReturned()
        {
            // Given
            var timeQuantity = new DelftIniProperty(BoundaryRegion.Quantity.Key, BoundaryRegion.QuantityStrings.Time, BoundaryRegion.Quantity.Description);
            var timeUnitString = String.Format("{0} {1}", BoundaryRegion.UnitStrings.TimeMinutes, DateTime.Today.ToString(BoundaryRegion.UnitStrings.TimeFormat));
            var timeUnit = new DelftIniProperty(BoundaryRegion.Unit.Key, timeUnitString, BoundaryRegion.Unit.Description);

            var values = new List<double>() {0.0, 5.0, 10.0, 15.0, 20.0};
            var column = new DelftBcQuantityData(timeQuantity, timeUnit, values);

            // When
            var outputDoubles = BcConverterHelper.ParseDoubleValuesFromTableColumn(column).ToList();

            // Then
            Assert.That(outputDoubles.Count, Is.EqualTo(values.Count));
            for (var i = 0; i < values.Count; i++)
            {
                Assert.That(outputDoubles[i], Is.EqualTo(values[i]));
            }
        }

        /// <summary>
        /// GIVEN a valid unit string
        ///   AND a IDelftBCQuantityData describing a set of valid DateTimeValues with this unit string
        /// WHEN BcConverterHelper ParseDateTimeValuesFromTableColumn is called
        /// THEN an enumerable containing the same date time values is returned
        /// </summary>
        [TestCase(BoundaryRegion.UnitStrings.TimeSeconds)]
        [TestCase(BoundaryRegion.UnitStrings.TimeMinutes)]
        [TestCase(BoundaryRegion.UnitStrings.TimeHours)]
        public void GivenAValidUnitStringAndAIDelftBCQuantityDataDescribingASetOfValidDateTimeValuesWithThisUnitString_WhenBcConverterHelperParseDateTimeValuesFromTableColumnIsCalled_ThenAnEnumerableContainingTheSameDateTimeValuesIsReturned(string unit)
        {
            // Given
            // - Build set of time data
            var referenceDate = DateTime.Today;
            var timeValues = new List<DateTime>();

            const int nDateTimes = 20;
            const double stepFactor = 5.0;

            for (var i = 0; i < nDateTimes; i++)
            {
                timeValues.Add(CalcDateTimeFrom(referenceDate, unit, i * stepFactor));
            }

            // - Build actual column from time data
            var timeQuantity = new DelftIniProperty(BoundaryRegion.Quantity.Key, BoundaryRegion.QuantityStrings.Time, BoundaryRegion.Quantity.Description);
            var timeUnitString = String.Format("{0} {1}", unit, referenceDate.ToString(BoundaryRegion.UnitStrings.TimeFormat));
            var timeUnit = new DelftIniProperty(BoundaryRegion.Unit.Key, timeUnitString, BoundaryRegion.Unit.Description);

            var timeValuesDoubles = CalcDoublesFrom(referenceDate, timeValues, unit);

            var column =  new DelftBcQuantityData(timeQuantity, timeUnit, timeValuesDoubles);

            // When
            var outputValues = BcConverterHelper.ParseDateTimesValuesFromTableColumn(column).ToList();

            // Then
            Assert.That(outputValues, Is.Not.Empty);
            Assert.That(outputValues.Count, Is.EqualTo(timeValues.Count));

            for (var i = 0; i < outputValues.Count; i++)
            {
                Assert.That(outputValues[i], Is.EqualTo(timeValues[i]));
            }
        }

        [ExcludeFromCodeCoverage]
        private static DateTime CalcDateTimeFrom(DateTime refDate, string unit, double timeStep)
        {
            if (unit.Equals(BoundaryRegion.UnitStrings.TimeSeconds))
                return refDate.AddSeconds(timeStep);
            if (unit.Equals(BoundaryRegion.UnitStrings.TimeMinutes))
                return refDate.AddMinutes(timeStep);
            if (unit.Equals(BoundaryRegion.UnitStrings.TimeHours))
                return refDate.AddHours(timeStep);
            return refDate;
        }

        [ExcludeFromCodeCoverage]
        private static IEnumerable<double> CalcDoublesFrom(DateTime refDate, IEnumerable<DateTime> timeValues, string unit)
        {
            if (unit.Equals(BoundaryRegion.UnitStrings.TimeSeconds))
                return timeValues.Select(t => (t - refDate).TotalSeconds).ToList();
            if (unit.Equals(BoundaryRegion.UnitStrings.TimeMinutes))
                return timeValues.Select(t => (t - refDate).TotalMinutes).ToList();
            if (unit.Equals(BoundaryRegion.UnitStrings.TimeHours))
                return timeValues.Select(t => (t - refDate).TotalHours).ToList();
            return null;
        }
    }
}
