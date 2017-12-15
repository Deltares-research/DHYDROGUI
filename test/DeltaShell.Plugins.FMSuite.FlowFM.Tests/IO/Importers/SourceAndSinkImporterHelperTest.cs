using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class SourceAndSinkImporterHelperTest
    {
        [Test]
        public void TestAdaptComponentValuesFromFileToSourceAndSinkFunction_HandlesInvalidFunction()
        {
            var sourceAndSink = new SourceAndSink();
            var function = sourceAndSink.Function;
            function.Components.RemoveAt(0); // simulate missing component

            Assert.False(SourceAndSinkImporterHelper.AdaptComponentValuesFromFileToSourceAndSinkFunction(function, null));
        }

        [TestCase(true, true, true, true, 10)]
        [TestCase(true, true, true, false, 10)]
        [TestCase(true, true, false, true, 10)]
        [TestCase(true, true, false, false, 10)]

        [TestCase(true, false, true, true, 10)]
        [TestCase(true, false, true, false, 10)]
        [TestCase(true, false, false, true, 10)]
        [TestCase(true, false, false, false, 10)]

        [TestCase(false, true, true, true, 10)]
        [TestCase(false, true, true, false, 10)]
        [TestCase(false, true, false, true, 10)]
        [TestCase(false, true, false, false, 10)]

        [TestCase(false, false, true, true, 10)]
        [TestCase(false, false, true, false, 10)]
        [TestCase(false, false, false, true, 10)]
        [TestCase(false, false, false, false, 10)]

        public void TestAdaptComponentValuesFromFileToSourceAndSinkFunction_CorrectlyProcessesSalinityAndTemperatureComponents(
            bool generateSalinityValues, bool generateTemperatureValues, bool salinityEnabled, bool temperatureEnabled, int numValues)
        {
            var sourceAndSink = new SourceAndSink();
            var originalFunction = sourceAndSink.Function;

            AddValuesToSourceAndSinkFunction(originalFunction, numValues, generateSalinityValues, generateTemperatureValues);

            var adaptedFunction = (IFunction)originalFunction.Clone(true);
            var componentSettings = GetComponentSettings(salinityEnabled, temperatureEnabled);
            Assert.True(SourceAndSinkImporterHelper.AdaptComponentValuesFromFileToSourceAndSinkFunction(adaptedFunction, componentSettings));

            VerifyComponentValues(salinityEnabled, temperatureEnabled, numValues, originalFunction, adaptedFunction);
        }

        [Test]
        public void TestAdaptComponentValuesFromFileToSourceAndSinkFunction_AddsDefaultValuesForMissingValues()
        {
            var sourceAndSink = new SourceAndSink();
            var originalFunction = sourceAndSink.Function;

            var numValues = 10;
            AddValuesToSourceAndSinkFunction(originalFunction, numValues, false, false);

            // simulate missing values
            originalFunction.Components[0].Values.Clear();
            originalFunction.Components[1].Values.Clear();
            originalFunction.Components[2].Values.Clear();

            var adaptedFunction = (IFunction)originalFunction.Clone(true);
            var componentSettings = GetComponentSettings(false, false);
            Assert.True(SourceAndSinkImporterHelper.AdaptComponentValuesFromFileToSourceAndSinkFunction(adaptedFunction, componentSettings));

            Assert.AreEqual(numValues, adaptedFunction.Components[0].Values.Count);
            Assert.AreEqual(numValues, adaptedFunction.Components[1].Values.Count);
            Assert.AreEqual(numValues, adaptedFunction.Components[2].Values.Count);
        }

        private static void AddValuesToSourceAndSinkFunction(IFunction function, int numValues, bool generateSalinityValues, bool generateTemperatureValues)
        {
            var timeArgument = function.Arguments.FirstOrDefault(
                a => a.Name.Equals(SourceAndSink.TimeVariableName, StringComparison.InvariantCultureIgnoreCase))
                as Variable<DateTime>;

            Assert.NotNull(timeArgument);
            SetValuesOfSourceAndSinkVariable(timeArgument, numValues);
            
            var dischargeComponent = function.Components.FirstOrDefault(
                a => a.Name.Equals(SourceAndSink.DischargeVariableName, StringComparison.InvariantCultureIgnoreCase))
                as Variable<double>;

            Assert.NotNull(dischargeComponent);
            SetValuesOfSourceAndSinkVariable(dischargeComponent, numValues);

            if (generateSalinityValues) // else DefaultValues will remain
            {
                var salinityComponent = function.Components.FirstOrDefault(
                    a => a.Name.Equals(SourceAndSink.SalinityVariableName, StringComparison.InvariantCultureIgnoreCase))
                    as Variable<double>;

                Assert.NotNull(salinityComponent);
                SetValuesOfSourceAndSinkVariable(salinityComponent, numValues);
            }

            if (generateTemperatureValues) // else DefaultValues will remain
            {
                var temperatureComponent = function.Components.FirstOrDefault(
                    a => a.Name.Equals(SourceAndSink.TemperatureVariableName, StringComparison.InvariantCultureIgnoreCase))
                    as Variable<double>;

                Assert.NotNull(temperatureComponent);
                SetValuesOfSourceAndSinkVariable(temperatureComponent, numValues);
            }
        }

        private static void SetValuesOfSourceAndSinkVariable(IVariable<DateTime> variable, int numValues)
        {
            var t0 = new DateTime(1984, 11, 11);
            var values = Enumerable.Range(0, numValues).Select(i => t0.AddMinutes(i)).ToList();
            variable.SetValues(values);
        }

        private static void SetValuesOfSourceAndSinkVariable(IVariable<double> variable, int numValues)
        {
            const double factor = 0.1; // should always be a double
            var values = Enumerable.Range(0, numValues).Select(i => i * factor).ToList();
            variable.SetValues(values);
        }

        private static IDictionary<string, bool> GetComponentSettings(bool salinityEnabled, bool temperatureEnabled)
        {
            return new Dictionary<string, bool>()
            {
                { SourceAndSink.SalinityVariableName, salinityEnabled },
                { SourceAndSink.TemperatureVariableName, temperatureEnabled },
            };
        }

        private static void VerifyComponentValues(bool salinityEnabled, bool temperatureEnabled, int numValues,
            IFunction originalFunction, IFunction adaptedFunction)
        {
            for (var i = 0; i < numValues; i++)
            {
                Assert.AreEqual(originalFunction.Arguments[0].Values[i], adaptedFunction.Arguments[0].Values[i]);
                Assert.AreEqual(originalFunction.Components[0].Values[i], adaptedFunction.Components[0].Values[i]);

                var expectedSalinityValue = salinityEnabled
                    ? originalFunction.Components[1].Values[i]
                    : adaptedFunction.Components[1].DefaultValue;

                Assert.AreEqual(expectedSalinityValue, adaptedFunction.Components[1].Values[i]);

                var expectedTemperatureValue = temperatureEnabled
                    ? originalFunction.Components[2].Values[i]
                    : adaptedFunction.Components[2].DefaultValue;

                Assert.AreEqual(expectedTemperatureValue, adaptedFunction.Components[2].Values[i]);
            }
        }
    }
}
