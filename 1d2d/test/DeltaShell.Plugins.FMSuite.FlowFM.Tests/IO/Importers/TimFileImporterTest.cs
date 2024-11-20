using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class TimFileImporterTest
    {
        [Test]
        public void GivenTimFileImporterWhenBoundaryConditionHasCorrectDataTypeThenValidate()
        {
            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.TimeSeries);
            var result = new TimFileImporter().CanImportOnBoundaryCondition(flowBoundaryCondition);

            Assert.IsTrue(result);
        }

        [Test]
        public void GivenTimFileImporterWhenBoundaryConditionHasInCorrectDataTypeThenValidate()
        {
            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.AstroComponents);
            var result = new TimFileImporter().CanImportOnBoundaryCondition(flowBoundaryCondition);

            Assert.IsFalse(result);
        }

        [Test]
        public void TestImportItem_SourceAndSinks_LogsErrorMessageForNullModel()
        {
            // setup
            var sourceAndSink = new SourceAndSink();

            // do the import & check results
            var importer = new TimFileImporter()
            {
                GetModelForSourceAndSink = input => null
            };

            var expectedLogMessage = string.Format(Resources.Tim_file_import_failed__could_not_retrieve_model_for_SourceAndSink___0_, sourceAndSink.Name);
            TestHelper.AssertAtLeastOneLogMessagesContains(() => importer.ImportItem(string.Empty, sourceAndSink), expectedLogMessage);
        }

        [Test]
        public void TestImportItem_SourceAndSinks_LogsErrorMessageForNullFunction()
        {
            // setup
            var sourceAndSink = new SourceAndSink() { Data = null };
            var fmModel = new WaterFlowFMModel();
            fmModel.SourcesAndSinks.Add(sourceAndSink);

            // do the import & check results
            var importer = new TimFileImporter()
            {
                GetModelForSourceAndSink = input => fmModel
            };

            var expectedLogMessage = string.Format(Resources.Tim_file_import_failed__could_not_retrieve_function_for_SourceAndSink___0_, sourceAndSink.Name);
            TestHelper.AssertAtLeastOneLogMessagesContains(() => importer.ImportItem(string.Empty, sourceAndSink), expectedLogMessage);
        }

        [TestCase(true, HeatFluxModelType.TransportOnly, true, true, true)]
        [TestCase(false, HeatFluxModelType.TransportOnly, true, true, true)]
        [TestCase(true, HeatFluxModelType.None, true, true, true)]
        [TestCase(true, HeatFluxModelType.TransportOnly, false, true, true)]
        [TestCase(true, HeatFluxModelType.TransportOnly, true, false, true)]
        [TestCase(true, HeatFluxModelType.TransportOnly, true, true, false)]
        [TestCase(false, HeatFluxModelType.None, true, true, true)]
        [TestCase(false, HeatFluxModelType.TransportOnly, false, true, true)]
        [TestCase(false, HeatFluxModelType.TransportOnly, true, false, true)]
        [TestCase(false, HeatFluxModelType.TransportOnly, true, true, false)]
        [TestCase(true, HeatFluxModelType.None, false, true, true)]
        [TestCase(true, HeatFluxModelType.None, true, false, true)]
        [TestCase(true, HeatFluxModelType.None, true, true, false)]
        [TestCase(true, HeatFluxModelType.TransportOnly, false, false, true)]
        [TestCase(true, HeatFluxModelType.TransportOnly, false, true, false)]
        [TestCase(true, HeatFluxModelType.TransportOnly, true, false, false)]
        [TestCase(false, HeatFluxModelType.None, false, true, true)]
        [TestCase(false, HeatFluxModelType.None, true, false, true)]
        [TestCase(false, HeatFluxModelType.None, true, true, false)]
        [TestCase(false, HeatFluxModelType.TransportOnly, false, false, true)]
        [TestCase(false, HeatFluxModelType.TransportOnly, false, true, false)]
        [TestCase(false, HeatFluxModelType.TransportOnly, true, false, false)]
        [TestCase(true, HeatFluxModelType.None, false, false, true)]
        [TestCase(true, HeatFluxModelType.None, false, true, false)]
        [TestCase(true, HeatFluxModelType.None, true, false, false)]
        [TestCase(true, HeatFluxModelType.TransportOnly, false, false, false)]
        [TestCase(false, HeatFluxModelType.None, false, false, true)]
        [TestCase(false, HeatFluxModelType.None, false, true, false)]
        [TestCase(false, HeatFluxModelType.None, true, false, false)]
        [TestCase(false, HeatFluxModelType.TransportOnly, false, false, false)]
        [TestCase(true, HeatFluxModelType.None, false, false, false)]
        [TestCase(false, HeatFluxModelType.None, false, false, false)]
        public void TestImportItem_SourceAndSinks(bool useSalinity, HeatFluxModelType temperature, bool useSedimentMorphology, bool useSecondaryFlow, bool useTracers)
        {
            var testFilePath = TestHelper.GetTestFilePath(@"timFiles\10Columns10Values.tim");
            var useTemperature = (temperature != HeatFluxModelType.None);
            // setup
            var fmModel = SetupFMModelWithSourceAndSink(useSalinity, temperature, useSedimentMorphology, useSecondaryFlow, useTracers);
            var sourceAndSink = fmModel.SourcesAndSinks.FirstOrDefault();
            Assert.NotNull(sourceAndSink);
            
            // do the import
            var importer = new TimFileImporter()
            {
                GetModelForSourceAndSink = input => fmModel
            };

            importer.ImportItem(testFilePath, sourceAndSink);

            // check results
            ValidateImportedSourceAndSinkFunction(sourceAndSink.Function, useSalinity, useTemperature, useSecondaryFlow);
        }

        private WaterFlowFMModel SetupFMModelWithSourceAndSink(bool useSalinity, HeatFluxModelType temperature, bool useSedimentMorphology, bool useSecondaryFlow, bool useTracers)
        {
            var expectedNumberOfComponents = 4; // Discharge, Salinity, Tmeperature, SecondaryFlow
            var fmModel = new WaterFlowFMModel();

            var geometry = new LineString(new[]
            {
                new Coordinate(0.0, 0.0),
                new Coordinate(45.0, 55.0),
                new Coordinate(100.0, 100.0)
            });
            
            var sourceAndSink = new SourceAndSink() { Feature = new Feature2D { Geometry = geometry } };
            fmModel.SourcesAndSinks.Add(sourceAndSink);

            var modelDefinition = fmModel.ModelDefinition;

            var salinityProperty = modelDefinition.GetModelProperty(KnownProperties.UseSalinity);
            salinityProperty.Value = useSalinity;

            var tempertureProperty = modelDefinition.GetModelProperty(KnownProperties.Temperature);
            tempertureProperty.SetValueFromString(((int)temperature).ToString());


            var sedimentMorphologyProperty = modelDefinition.GetModelProperty(GuiProperties.UseMorSed);
            sedimentMorphologyProperty.Value = useSedimentMorphology;
            
            if (useSedimentMorphology)
            {
                fmModel.SedimentFractions.AddRange(new List<SedimentFraction>()
                {
                    new SedimentFraction() { Name = "SedFrac1" },
                    new SedimentFraction() { Name = "SedFrac2" },
                });
                expectedNumberOfComponents += 2;
            }

            var secondaryFlowProperty = modelDefinition.GetModelProperty(KnownProperties.SecondaryFlow);
            secondaryFlowProperty.Value = useSecondaryFlow;

            if (useTracers)
            {
                fmModel.TracerDefinitions.Add("Tracer1");
                fmModel.TracerDefinitions.Add("Tracer2");

                var boundaryConditionSet = new BoundaryConditionSet();
                fmModel.BoundaryConditionSets.Add(boundaryConditionSet);
                boundaryConditionSet.BoundaryConditions.AddRange(new List<IBoundaryCondition>()
                {
                    new FlowBoundaryCondition(FlowBoundaryQuantityType.Tracer, BoundaryConditionDataType.Constant) { TracerName = "Tracer1" },
                    new FlowBoundaryCondition(FlowBoundaryQuantityType.Tracer, BoundaryConditionDataType.Constant) { TracerName = "Tracer2" }
                });

                expectedNumberOfComponents += 2;
            }
            
            Assert.AreEqual(expectedNumberOfComponents, sourceAndSink.Function.Components.Count);

            return fmModel;
        }

        private void ValidateImportedSourceAndSinkFunction(IFunction function, bool useSalinity, bool useTemperature, bool useSecondaryFlow)
        {
            var dischargeVariable = function.Components.FirstOrDefault(c => c.Name == SourceAndSink.DischargeVariableName);
            Assert.NotNull(dischargeVariable);

            var dischargeValues = ((MultiDimensionalArray<double>)dischargeVariable.Values).ToList();
            Assert.True(dischargeValues.All(v => v >= double.Epsilon),
                "Possible incorrect values for Discharge");

            var salinityVariable = function.Components.FirstOrDefault(c => c.Name == SourceAndSink.SalinityVariableName);
            Assert.NotNull(salinityVariable);

            var salinityValues = ((MultiDimensionalArray<double>)salinityVariable.Values).ToList();
            Assert.True(useSalinity 
                ? salinityValues.All(v => v >= double.Epsilon) 
                : salinityValues.All(v => v < double.Epsilon),
                "Possible incorrect values for Salinity");

            var temperatureVariable = function.Components.FirstOrDefault(c => c.Name == SourceAndSink.TemperatureVariableName);
            Assert.NotNull(temperatureVariable);

            var temperatureValues = ((MultiDimensionalArray<double>)temperatureVariable.Values).ToList();
            Assert.True(useTemperature
                ? temperatureValues.All(v => v >= double.Epsilon) 
                : temperatureValues.All(v => v < double.Epsilon),
                "Possible incorrect values for Temperature");

            var secondaryFlowVariable = function.Components.FirstOrDefault(c => c.Name == SourceAndSink.SecondaryFlowVariableName);
            Assert.NotNull(secondaryFlowVariable);

            var secondaryFlowValues = ((MultiDimensionalArray<double>)secondaryFlowVariable.Values).ToList();
            Assert.True(useSecondaryFlow
                ? secondaryFlowValues.All(v => v >= double.Epsilon)
                : secondaryFlowValues.All(v => v < double.Epsilon),
                "Possible incorrect values for SecondaryFlow");

            // remove empty components
            if (!useSalinity) function.Components.Remove(salinityVariable);
            if (!useTemperature) function.Components.Remove(temperatureVariable);
            if (!useSecondaryFlow) function.Components.Remove(secondaryFlowVariable);

            // verify values in remaining components (in the test file, the values always increase through the components)
            var allValues = function.Components.SelectMany(c => (MultiDimensionalArray<double>)c.Values).ToList();
            var orderedByAsc = allValues.OrderBy(d => d);
            Assert.True(allValues.SequenceEqual(orderedByAsc),
                "Values may have been read into the wrong components");
        }

    }
}
