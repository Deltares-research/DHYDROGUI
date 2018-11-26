using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.Helpers;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Boundary
{
    public static class BoundaryTestHelper
    {
        public static IList<IDelftBcQuantityData> GetBcQuantityDataTable(DateTime startTime,
                                                                         IEnumerable<DateTime> timeValues,
                                                                         IList<double> values,
                                                                         string quantityStr,
                                                                         string unitStr)
        {
            var table = new List<IDelftBcQuantityData>();

            var timeQuantity = new DelftIniProperty(BoundaryRegion.Quantity.Key, BoundaryRegion.QuantityStrings.Time, BoundaryRegion.Quantity.Description);
            var timeUnitString = $"{BoundaryRegion.UnitStrings.TimeMinutes} {startTime.ToString(BoundaryRegion.UnitStrings.TimeFormat)}";
            var timeUnit = new DelftIniProperty(BoundaryRegion.Unit.Key, timeUnitString, BoundaryRegion.Unit.Description);

            var formattedDateTimes = timeValues.Select(t => (t - startTime).TotalMinutes).ToList();
            table.Add(new DelftBcQuantityData(timeQuantity, timeUnit, formattedDateTimes));

            var quantity = new DelftIniProperty(BoundaryRegion.Quantity.Key, quantityStr, BoundaryRegion.Quantity.Description);
            var unit = new DelftIniProperty(BoundaryRegion.Unit.Key, unitStr, BoundaryRegion.Unit.Description);

            var data = values;
            if (!data.Any()) data = Enumerable.Repeat(0.0, formattedDateTimes.Count).ToList();

            table.Add(new DelftBcQuantityData(quantity, unit, data));
            return table;
        }

        public static IList<IDelftBcQuantityData> GetBcQuantityConstantValue(double value,
                                                                             string quantityStr, 
                                                                             string unitStr)
        {
            var quantity = new DelftIniProperty(BoundaryRegion.Quantity.Key, quantityStr, BoundaryRegion.Quantity.Description);
            var unit = new DelftIniProperty(BoundaryRegion.Unit.Key, unitStr, BoundaryRegion.Unit.Description);
            return new List<IDelftBcQuantityData>() { new DelftBcQuantityData(quantity, unit, new List<double>() { value }) };
        }

        public static IList<IDelftBcQuantityData> GetBcQuantityDataTable(IFunction data)
        {
            var levelQuantity = new DelftIniProperty(BoundaryRegion.Quantity.Key, BoundaryRegion.QuantityStrings.WaterLevel, BoundaryRegion.Quantity.Description);
            var levelUnit = new DelftIniProperty(BoundaryRegion.Unit.Key, BoundaryRegion.UnitStrings.WaterLevel, BoundaryRegion.Unit.Description);
            var levelData = ((MultiDimensionalArray<double>)(data.Arguments[0].Values)).ToList();

            var dischargeQuantity = new DelftIniProperty(BoundaryRegion.Quantity.Key, BoundaryRegion.QuantityStrings.WaterDischarge, BoundaryRegion.Quantity.Description);
            var dischargeUnit = new DelftIniProperty(BoundaryRegion.Unit.Key, BoundaryRegion.UnitStrings.WaterDischarge, BoundaryRegion.Unit.Description);
            var dischargeData = ((MultiDimensionalArray<double>)(data.Components[0].Values)).ToList();
            if (levelData.Count == 0 && dischargeData.Count == 0)
            {
                levelData.Add(0.0);
                dischargeData.Add(0.0);
            }

            return new List<IDelftBcQuantityData>()
            {
                new DelftBcQuantityData(levelQuantity, levelUnit, levelData), new DelftBcQuantityData(dischargeQuantity, dischargeUnit, dischargeData)
            };
        }

        public static void Shuffle<T>(IList<T> someList)
        {
            var rand = new Random();

            T temp;
            var n = someList.Count;
            for (var i = 0; i < n - 2; i++)
            {
                var j = rand.Next(i, n);
                temp = someList[j];
                someList[j] = someList[i];
                someList[i] = temp;
            }
        }

        public enum HasComponent
        {
            None,
            Constant,
            Table,
            TimeDependent
        }

        public static IDelftBcCategory GetCommonCategory(string header,
            string name,
            string functionString,
            InterpolationType interpolationType,
            string isPeriodic)
        {
            IDefinitionGeneratorBoundary componentDefinitionGenerator =
                new DefinitionGeneratorBoundary(header);

            // Set common elements
            var boundaryDefinition = componentDefinitionGenerator.CreateRegion(
                name,
                functionString,
                interpolationType == InterpolationType.Constant
                    ? BoundaryRegion.TimeInterpolationStrings.BlockFrom
                    : BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate,
                isPeriodic);
            return boundaryDefinition;
        }

        public static IFunction GetNewTimeFunction(string quantity, string unitName, string unitVal)
        {
            var function = new Function();

            function.Arguments.Add(new Variable<DateTime>("Time")
            {
                InterpolationType = InterpolationType.Linear,
                ExtrapolationType = ExtrapolationType.Periodic
            });

            function.Components.Add(new Variable<double>(quantity, new Unit(unitName, unitVal)));
            return function;
        }

        public static string GetTimeSeriesIsPeriodicProperty(IFunction timeSeries)
        {
            string periodic = null;
            if (timeSeries?.Arguments != null && timeSeries.Arguments.Count > 0)
            {
                periodic = timeSeries.Arguments[0].ExtrapolationType == ExtrapolationType.Periodic ? "true" : null;
            }
            return periodic;
        }

        public static void AssertThatTimeDependentFunctionIsEqualTo(IFunction actual, IFunction expected)
        {
            Assert.That(actual, Is.Not.Null);
            Assert.That(expected, Is.Not.Null);

            var nValues = expected.Arguments[0].Values.Count;
            Assert.That(expected.Arguments[0].Values.Count,
                Is.EqualTo(nValues));
            Assert.That(actual.Components[0].Values.Count,
                Is.EqualTo(nValues));

            for (var i = 0; i < nValues; i++)
            {
                Assert.That(actual.Arguments[0].Values[i],
                    Is.EqualTo(expected.Arguments[0].Values[i]));
                Assert.That(actual.Components[0].Values[i],
                    Is.EqualTo(actual.Components[0].Values[i]));
            }
        }

    }
}
