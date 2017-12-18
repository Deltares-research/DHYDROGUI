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

        [TestCase(3, true, true, 10)]
        [TestCase(3, true, false, 10)]
        [TestCase(3, false, true, 10)]
        [TestCase(3, false, false, 10)]

        [TestCase(2, true, true, 10)]
        [TestCase(2, true, false, 10)]
        [TestCase(2, false, true, 10)]
        [TestCase(2, false, false, 10)]

        [TestCase(1, true, true, 10)]
        [TestCase(1, true, false, 10)]
        [TestCase(1, false, true, 10)]
        [TestCase(1, false, false, 10)]
        
        public void TestAdaptComponentValuesFromFileToSourceAndSinkFunction_CorrectlyProcessesSalinityAndTemperatureComponents(
            int numComponentsWithValues, bool salinityEnabled, bool temperatureEnabled, int numValues)
        {
            var sourceAndSink = new SourceAndSink();
            var originalFunction = sourceAndSink.Function;

            // simulate reading from file
            AddValuesToSourceAndSinkFunction(originalFunction, numValues, numComponentsWithValues);

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
            AddValuesToSourceAndSinkFunction(originalFunction, numValues, 1);

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

        private static void AddValuesToSourceAndSinkFunction(IFunction function, int numValues, int numComponentsWithValues)
        {
            var timeArgument = function.Arguments.FirstOrDefault(
                a => a.Name.Equals(SourceAndSink.TimeVariableName, StringComparison.InvariantCultureIgnoreCase))
                as Variable<DateTime>;

            Assert.NotNull(timeArgument);
            SetValuesOfSourceAndSinkVariable(timeArgument, numValues);

            for (var i = 0; i < numComponentsWithValues; i++)
            {
                Assert.IsTrue(function.Components.Count > i);
                var component = function.Components[i] as Variable<double>;

                Assert.NotNull(component);
                SetValuesOfSourceAndSinkVariable(component, numValues);
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
            var originalTemperatureValuesComponentIndex = salinityEnabled ? 2 : 1;
            for (var i = 0; i < numValues; i++)
            {
                Assert.AreEqual(originalFunction.Arguments[0].Values[i], adaptedFunction.Arguments[0].Values[i]);
                Assert.AreEqual(originalFunction.Components[0].Values[i], adaptedFunction.Components[0].Values[i]);

                var expectedSalinityValue = salinityEnabled
                    ? originalFunction.Components[1].Values[i]
                    : adaptedFunction.Components[1].DefaultValue;

                Assert.AreEqual(expectedSalinityValue, adaptedFunction.Components[1].Values[i]);

                var expectedTemperatureValue = temperatureEnabled
                    ? originalFunction.Components[originalTemperatureValuesComponentIndex].Values[i]
                    : adaptedFunction.Components[2].DefaultValue;

                Assert.AreEqual(expectedTemperatureValue, adaptedFunction.Components[2].Values[i]);
            }
        }
    }
}
