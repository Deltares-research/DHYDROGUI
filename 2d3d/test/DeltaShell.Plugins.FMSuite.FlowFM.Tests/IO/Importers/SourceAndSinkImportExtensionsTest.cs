using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class SourceAndSinkImportExtensionsTest
    {
        [Test]
        public void TestCopyValuesFromFileToSourceAndSinkAttributes()
        {
            SourceAndSink sourceAndSink = GenerateSourceAndSink();
            // First import
            var numVariablesInFunction = 5;
            var numValues = 8;
            Function function = GenerateSimpleFunction(numVariablesInFunction, numValues); // simulate reading *.tim file

            sourceAndSink.CopyValuesFromFileToSourceAndSinkAttributes(function);

            List<KeyValuePair<string, object>> sourceAndSinkAttributes = sourceAndSink.Feature.Attributes.Where(a => a.Key.StartsWith(SourceAndSinkImportExtensions.TimFileColumnAttributePrefix)).ToList();
            Assert.AreEqual(numVariablesInFunction, sourceAndSinkAttributes.Count);
            Assert.True(sourceAndSinkAttributes.Select(a => a.Value).OfType<MultiDimensionalArray<double>>().All(v => v.Count == numValues));

            // Second import
            numVariablesInFunction = 7;
            numValues = 6;
            function = GenerateSimpleFunction(numVariablesInFunction, numValues); // simulate reading *.tim file

            sourceAndSink.CopyValuesFromFileToSourceAndSinkAttributes(function);

            sourceAndSinkAttributes = sourceAndSink.Feature.Attributes.Where(a => a.Key.StartsWith(SourceAndSinkImportExtensions.TimFileColumnAttributePrefix)).ToList();
            Assert.AreEqual(numVariablesInFunction, sourceAndSinkAttributes.Count);
            Assert.True(sourceAndSinkAttributes.Select(a => a.Value).OfType<MultiDimensionalArray<double>>().All(v => v.Count == numValues));
        }

        [Test]
        public void TestPopulateFunctionValuesFromAttributes_AllComponents()
        {
            const int numVariablesInFunction = 5;
            const int numValues = 8;

            SourceAndSink sourceAndSink = GenerateSourceAndSink(GenerateAttributes(numVariablesInFunction, numValues));
            AddExtraComponents(sourceAndSink.Data, 1);
            PopulateValues(sourceAndSink.Data, 8);

            sourceAndSink.PopulateFunctionValuesFromAttributes(null);

            List<DateTime> sourceAndSinkArgumentValues = sourceAndSink.Function.Arguments
                                                                      .OfType<IVariable<DateTime>>()
                                                                      .SelectMany(v => v.Values)
                                                                      .ToList();

            Assert.AreEqual(numValues, sourceAndSinkArgumentValues.Count);
            Assert.True(sourceAndSinkArgumentValues.HasUniqueValues());

            List<double> sourceAndSinkComponentValues = sourceAndSink.Function.Components
                                                                     .OfType<IVariable<double>>()
                                                                     .SelectMany(v => v.Values)
                                                                     .ToList();

            Assert.AreEqual(numValues * (numVariablesInFunction - 1), sourceAndSinkComponentValues.Count);
            Assert.True(sourceAndSinkComponentValues.HasUniqueValues());
        }

        [Test]
        public void TestPopulateFunctionValuesFromAttributes_SubsetOfComponents()
        {
            const int numVariablesInFunction = 5;
            const int numValues = 8;

            SourceAndSink sourceAndSink = GenerateSourceAndSink(GenerateAttributes(numVariablesInFunction, numValues));
            AddExtraComponents(sourceAndSink.Data, 1);

            var componentSettings = new Dictionary<string, bool>();
            var previousSetting = true;
            sourceAndSink.Function.Components.ForEach(c =>
            {
                componentSettings.Add(c.Name, previousSetting);
                previousSetting = !previousSetting;
            });

            sourceAndSink.PopulateFunctionValuesFromAttributes(componentSettings);

            List<DateTime> sourceAndSinkArgumentValues = sourceAndSink.Function.Arguments
                                                                      .OfType<IVariable<DateTime>>()
                                                                      .SelectMany(v => v.Values)
                                                                      .ToList();

            Assert.AreEqual(numValues, sourceAndSinkArgumentValues.Count);
            Assert.True(sourceAndSinkArgumentValues.HasUniqueValues());

            List<IVariable<double>> sourceAndSinkComponents = sourceAndSink.Function.Components
                                                                           .OfType<IVariable<double>>()
                                                                           .ToList();

            foreach (IVariable<double> component in sourceAndSinkComponents)
            {
                Assert.AreEqual(numValues, component.Values.Count);
                bool componentIsActive;
                if (!componentSettings.TryGetValue(component.Name, out componentIsActive))
                {
                    componentIsActive = true;
                }

                Assert.AreEqual(componentIsActive, component.Values.HasUniqueValues());
            }
        }

        [Test]
        public void TestPopulateFunctionValuesFromAttributes_LogsWarningWhenNumberOfColumnsFromFileIsMoreThanExpected()
        {
            const int numAttributes = 6;
            const int numValues = 8;

            SourceAndSink sourceAndSink = GenerateSourceAndSink(GenerateAttributes(numAttributes, numValues));

            TestHelper.AssertAtLeastOneLogMessagesContains(() => sourceAndSink.PopulateFunctionValuesFromAttributes(null),
                                                           string.Format(Resources.SourceAndSinkImportExtensions_GenerateFunctionFromAttributes_There_were_more_columns_in_the___tim_file_for__0__than_expected, sourceAndSink.Name));
        }

        [Test]
        public void TestPopulateFunctionValuesFromAttributes_LogsWarningWhenNumberOfColumnsFromFileIsLessThanExpected()
        {
            const int numAttributes = 5;
            const int numValues = 8;

            SourceAndSink sourceAndSink = GenerateSourceAndSink(GenerateAttributes(numAttributes, numValues));
            AddExtraComponents(sourceAndSink.Data, 2);

            TestHelper.AssertAtLeastOneLogMessagesContains(() => sourceAndSink.PopulateFunctionValuesFromAttributes(null),
                                                           string.Format(Resources.SourceAndSinkImportExtensions_GenerateFunctionFromAttributes_There_were_less_columns_in_the___tim_file_for__0__than_expected, sourceAndSink.Name));
        }

        [Test]
        public void TestPopulateFunctionValuesFromAttributes_RemovesAttributesFromFeature()
        {
            const int numAttributes = 5;
            const int numValues = 8;

            SourceAndSink sourceAndSink = GenerateSourceAndSink(GenerateAttributes(numAttributes, numValues));
            AddExtraComponents(sourceAndSink.Data, 2);

            Assert.True(sourceAndSink.Feature.Attributes.Any(a => a.Key.StartsWith(SourceAndSinkImportExtensions.TimFileColumnAttributePrefix)));
            sourceAndSink.PopulateFunctionValuesFromAttributes(null);
            Assert.False(sourceAndSink.Feature.Attributes.Any(a => a.Key.StartsWith(SourceAndSinkImportExtensions.TimFileColumnAttributePrefix)));
        }

        private void AddExtraComponents(IFunction function, int n)
        {
            for (var i = 1; i < n; i++)
            {
                function.Components.Add(new Variable<double> {Name = "Component" + i});
            }
        }

        private void PopulateValues(IFunction function, int n)
        {
            function.Arguments[0].Values = GenerateValues(n, DateTime.Now);
            foreach (IVariable variable in function.Components)
            {
                variable.Values = GenerateValues(n, 1);
            }
        }

        private SourceAndSink GenerateSourceAndSink(DictionaryFeatureAttributeCollection attributes = null)
        {
            var geometry = new LineString(new[]
            {
                new Coordinate(0.0, 0.0),
                new Coordinate(45.0, 55.0),
                new Coordinate(100.0, 100.0)
            });

            var sourceAndSink = new SourceAndSink()
            {
                Name = "SourceAndSink01",
                Feature = new Feature2D()
                {
                    Attributes = attributes ?? new DictionaryFeatureAttributeCollection(),
                    Geometry = geometry
                }
            };
            return sourceAndSink;
        }

        private Function GenerateSimpleFunction(int numVariables, int numValues)
        {
            var function = new Function();
            if (numVariables < 1)
            {
                return function;
            }

            DateTime t0 = DateTime.Now;
            function.Arguments = new EventedList<IVariable>()
            {
                new Variable<DateTime>()
                {
                    Name = "Time",
                    Values = GenerateValues(numValues, t0)
                }
            };

            for (var i = 1; i < numVariables; i++)
            {
                function.Components.Add(new Variable<double>()
                {
                    Name = "Component" + i,
                    Values = GenerateValues(numValues, i)
                });
            }

            return function;
        }

        private DictionaryFeatureAttributeCollection GenerateAttributes(int numAttributes, int numValues)
        {
            var attributes = new DictionaryFeatureAttributeCollection();
            if (numAttributes < 1)
            {
                return attributes;
            }

            DateTime t0 = DateTime.Now;
            attributes.Add(SourceAndSinkImportExtensions.TimFileColumnAttributePrefix + "0", GenerateValues(numValues, t0));

            for (var i = 1; i < numAttributes; i++)
            {
                attributes.Add(SourceAndSinkImportExtensions.TimFileColumnAttributePrefix + i, GenerateValues(numValues, i));
            }

            return attributes;
        }

        private MultiDimensionalArray<DateTime> GenerateValues(int numValues, DateTime baseValue)
        {
            var values = new MultiDimensionalArray<DateTime>();
            for (var i = 0; i < numValues; i++)
            {
                values.Add(baseValue.AddMinutes(i * 10));
            }

            return values;
        }

        private MultiDimensionalArray<double> GenerateValues(int numValues, int baseValue)
        {
            var values = new MultiDimensionalArray<double>();
            for (var i = 0; i < numValues; i++)
            {
                values.Add(baseValue + (i * 0.001));
            }

            return values;
        }
    }
}