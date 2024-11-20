using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Sediment;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.FeatureData
{
    [Category(TestCategory.Integration)]
    [Category(TestCategory.Slow)]
    public class BoundaryConditionIntegrationTest
    {
        [Test]
        public void AfterAddBoundaryAllBoundariesAreSavedToBcFile()
        {
            WaterFlowFMModel model = CreateWaterFlowFMModel();

            Assert.AreEqual(2, model.Boundaries.Count);

            Feature2D boundary = model.Boundaries.Last();
            var boundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                              BoundaryConditionDataType.TimeSeries) {Feature = boundary};

            boundaryCondition.AddPoint(0);
            boundaryCondition.PointData[0][model.StartTime] = 0.5;
            boundaryCondition.PointData[0][model.StopTime] = 0.6;

            boundaryCondition.AddPoint(1);
            boundaryCondition.PointData[1][model.StartTime] = 0.55;
            boundaryCondition.PointData[1][model.StopTime] = 0.65;

            BoundaryConditionSet boundaryConditionSet = model.BoundaryConditionSets.FirstOrDefault(bs => Equals(bs.Feature, boundary));

            Assert.IsNotNull(boundaryConditionSet);

            boundaryConditionSet.BoundaryConditions.Add(boundaryCondition);

            model.ExportTo("boundaries/test.mdu");

            Assert.IsFalse(File.Exists("boundaries/right.pli"));
            Assert.IsFalse(File.Exists("boundaries/right_0001.cmp"));
            Assert.IsTrue(File.Exists("boundaries/left.pli"));
            Assert.IsTrue(File.Exists("boundaries/L1.pli"));
            Assert.IsTrue(File.Exists("boundaries/WaterLevel.bc"));

            Directory.Delete("boundaries", true);
        }

        [Test]
        public void SaveLoadWithOldAndNewBoundaryConditionFormats()
        {
            WaterFlowFMModel model = CreateWaterFlowFMModel();

            Feature2D boundary = model.Boundaries.Last();
            var boundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                              BoundaryConditionDataType.TimeSeries) {Feature = boundary};

            boundaryCondition.AddPoint(0);
            boundaryCondition.PointData[0][model.StartTime] = 0.5;
            boundaryCondition.PointData[0][model.StopTime] = 0.6;

            boundaryCondition.AddPoint(1);
            boundaryCondition.PointData[1][model.StartTime] = 0.55;
            boundaryCondition.PointData[1][model.StopTime] = 0.65;

            BoundaryConditionSet boundaryConditionSet = model.BoundaryConditionSets.FirstOrDefault(bs => Equals(bs.Feature, boundary));

            Assert.IsNotNull(boundaryConditionSet);

            boundaryConditionSet.BoundaryConditions.Add(boundaryCondition);

            model.ExportTo("boundaries/test.mdu");

            var importedModel = new WaterFlowFMModel();
            importedModel.ImportFromMdu("boundaries/test.mdu");

            Assert.AreEqual(2, importedModel.Boundaries.Count);
            Assert.AreEqual(2, importedModel.BoundaryConditions.Count());
            Assert.AreEqual(BoundaryConditionDataType.Harmonics, importedModel.BoundaryConditions.First().DataType);
            Assert.AreEqual(FlowBoundaryQuantityType.WaterLevel,
                            importedModel.BoundaryConditions.OfType<FlowBoundaryCondition>().First().FlowQuantity);
            Assert.AreEqual(1, importedModel.BoundaryConditions.First().DataPointIndices.Count);
            Assert.AreEqual(BoundaryConditionDataType.TimeSeries, importedModel.BoundaryConditions.Last().DataType);
            Assert.AreEqual(2, importedModel.BoundaryConditions.Last().DataPointIndices.Count);
            Assert.AreEqual(FlowBoundaryQuantityType.WaterLevel,
                            importedModel.BoundaryConditions.OfType<FlowBoundaryCondition>().Last().FlowQuantity);

            Directory.Delete("boundaries", true);
        }

        [Test]
        public void SaveLoadWithOldAndNewNeumannBoundaryCondition()
        {
            WaterFlowFMModel model = CreateWaterFlowFMModel();

            Feature2D boundary = model.Boundaries.Last();
            var boundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.Neumann,
                                                              BoundaryConditionDataType.TimeSeries) {Feature = boundary};

            boundaryCondition.AddPoint(0);
            boundaryCondition.PointData[0][model.StartTime] = 0.5;
            boundaryCondition.PointData[0][model.StopTime] = 0.6;

            boundaryCondition.AddPoint(1);
            boundaryCondition.PointData[1][model.StartTime] = 0.55;
            boundaryCondition.PointData[1][model.StopTime] = 0.65;

            BoundaryConditionSet boundaryConditionSet = model.BoundaryConditionSets.FirstOrDefault(bs => Equals(bs.Feature, boundary));

            Assert.IsNotNull(boundaryConditionSet);

            boundaryConditionSet.BoundaryConditions.Add(boundaryCondition);

            model.ExportTo("boundaries/test.mdu");

            var importedModel = new WaterFlowFMModel();
            importedModel.ImportFromMdu("boundaries/test.mdu");

            Assert.AreEqual(2, importedModel.Boundaries.Count);
            Assert.AreEqual(BoundaryConditionDataType.Harmonics, importedModel.BoundaryConditions.First().DataType);
            Assert.AreEqual(FlowBoundaryQuantityType.WaterLevel,
                            importedModel.BoundaryConditions.OfType<FlowBoundaryCondition>().First().FlowQuantity);
            Assert.AreEqual(1, importedModel.BoundaryConditions.First().DataPointIndices.Count);
            Assert.AreEqual(BoundaryConditionDataType.TimeSeries, importedModel.BoundaryConditions.Last().DataType);
            Assert.AreEqual(2, importedModel.BoundaryConditions.Last().DataPointIndices.Count);
            Assert.AreEqual(FlowBoundaryQuantityType.Neumann,
                            importedModel.BoundaryConditions.OfType<FlowBoundaryCondition>().Last().FlowQuantity);

            Directory.Delete("boundaries", true);
        }

        [Test]
        public void SaveLoadWithOldAndNewWaterLevelAndVelocity()
        {
            WaterFlowFMModel model = CreateWaterFlowFMModel();

            Feature2D boundary = model.Boundaries.Last();

            var waterLevelBc = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                         BoundaryConditionDataType.AstroComponents) {Feature = boundary};

            waterLevelBc.AddPoint(0);
            waterLevelBc.PointData[0]["A0"] = new[]
            {
                0.5,
                0
            };
            waterLevelBc.PointData[0]["M2"] = new[]
            {
                0.8,
                0
            };

            var velocityLevelBc = new FlowBoundaryCondition(FlowBoundaryQuantityType.NormalVelocity,
                                                            BoundaryConditionDataType.TimeSeries) {Feature = boundary};

            velocityLevelBc.AddPoint(0);
            velocityLevelBc.PointData[0][model.StartTime] = 1.5;
            velocityLevelBc.PointData[0][model.StopTime] = 1.6;

            velocityLevelBc.AddPoint(1);
            velocityLevelBc.PointData[1][model.StartTime] = 1.55;
            velocityLevelBc.PointData[1][model.StopTime] = 1.65;

            BoundaryConditionSet boundaryConditionSet = model.BoundaryConditionSets.FirstOrDefault(bs => Equals(bs.Feature, boundary));

            Assert.IsNotNull(boundaryConditionSet);

            boundaryConditionSet.BoundaryConditions.Add(waterLevelBc);
            boundaryConditionSet.BoundaryConditions.Add(velocityLevelBc);

            model.ExportTo("boundaries/test.mdu");

            var importedModel = new WaterFlowFMModel();
            importedModel.ImportFromMdu("boundaries/test.mdu");

            Assert.AreEqual(2, importedModel.Boundaries.Count);

            List<IBoundaryCondition> boundaryConditions = importedModel.BoundaryConditions.ToList();
            Assert.AreEqual(3, importedModel.BoundaryConditions.Count());

            Assert.AreEqual(BoundaryConditionDataType.AstroComponents, boundaryConditions[1].DataType);
            Assert.AreEqual(FlowBoundaryQuantityType.WaterLevel,
                            ((FlowBoundaryCondition) boundaryConditions[1]).FlowQuantity);
            Assert.AreEqual(1, boundaryConditions[1].DataPointIndices.Count);

            Assert.AreEqual(BoundaryConditionDataType.TimeSeries, boundaryConditions[2].DataType);
            Assert.AreEqual(FlowBoundaryQuantityType.NormalVelocity,
                            ((FlowBoundaryCondition) boundaryConditions[2]).FlowQuantity);
            Assert.AreEqual(2, boundaryConditions[2].DataPointIndices.Count);

            Directory.Delete("boundaries", true);
        }

        [Test]
        public void RunWithOldAndNewBoundaryConditionFormats()
        {
            WaterFlowFMModel model = CreateWaterFlowFMModel();

            Feature2D boundary = model.Boundaries.Last();
            var boundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                              BoundaryConditionDataType.TimeSeries) {Feature = boundary};

            boundaryCondition.AddPoint(0);
            boundaryCondition.PointData[0][model.StartTime] = 1.5;
            boundaryCondition.PointData[0][model.StopTime] = 0.2;

            boundaryCondition.AddPoint(1);
            boundaryCondition.PointData[1][model.StartTime] = 1.55;
            boundaryCondition.PointData[1][model.StopTime] = 0.25;

            BoundaryConditionSet boundaryConditionSet = model.BoundaryConditionSets.FirstOrDefault(bs => Equals(bs.Feature, boundary));

            Assert.IsNotNull(boundaryConditionSet);

            boundaryConditionSet.BoundaryConditions.Add(boundaryCondition);

            ActivityRunner.RunActivity(model);

            Assert.AreNotEqual(ActivityStatus.Failed, model.Status);
        }

        [Test]
        public void AddSedimentBoundary()
        {
            WaterFlowFMModel model = CreateWaterFlowFMModel();
            var sedFrac = new SedimentFraction() {Name = "Frac1"};
            model.SedimentFractions.Add(sedFrac);
            model.ModelDefinition.GetModelProperty(KnownProperties.BedlevType).SetValueFromString("1");
            model.ModelDefinition.GetModelProperty(GuiProperties.UseMorSed).SetValueFromString("1");
            model.ModelDefinition.GetModelProperty(KnownProperties.Conveyance2d).SetValueFromString("0");

            Feature2D boundary = model.Boundaries.Last();
            var boundaryCondition = new FlowBoundaryCondition(
                FlowBoundaryQuantityType.SedimentConcentration,
                BoundaryConditionDataType.TimeSeries)
            {
                Feature = boundary,
                SedimentFractionName = sedFrac.Name,
                SedimentFractionNames = model.SedimentFractions.Select(sf => sf.Name).ToList()
            };

            boundaryCondition.AddPoint(0);
            boundaryCondition.PointData[0][model.StartTime] = 1.5;
            boundaryCondition.PointData[0][model.StopTime] = 0.2;

            boundaryCondition.AddPoint(1);
            boundaryCondition.PointData[1][model.StartTime] = 1.55;
            boundaryCondition.PointData[1][model.StopTime] = 0.25;

            BoundaryConditionSet boundaryConditionSet = model.BoundaryConditionSets.FirstOrDefault(bs => Equals(bs.Feature, boundary));

            Assert.IsNotNull(boundaryConditionSet);

            boundaryConditionSet.BoundaryConditions.Add(boundaryCondition);

            var flowBoundaryCondition = new FlowBoundaryCondition(
                FlowBoundaryQuantityType.WaterLevel,
                BoundaryConditionDataType.TimeSeries)
            {
                Feature = boundary,
                SedimentFractionName = sedFrac.Name,
                SedimentFractionNames = model.SedimentFractions.Select(sf => sf.Name).ToList()
            };

            var startTime = (DateTime) model.ModelDefinition.GetModelProperty(KnownProperties.StartDateTime).Value;
            var stopTime = (DateTime) model.ModelDefinition.GetModelProperty(KnownProperties.StopDateTime).Value;

            flowBoundaryCondition.AddPoint(0);
            IFunction data = flowBoundaryCondition.GetDataAtPoint(0);
            FillTimeSeries(data, i => 0.75 * Math.Sin(0.6 * Math.PI * i), startTime, stopTime, 2);

            boundaryConditionSet.BoundaryConditions.Add(flowBoundaryCondition);

            ValidationReport report = model.Validate();
            Assert.AreEqual(0, report.ErrorCount, string.Format("Model validation failed : {0}", string.Join(Environment.NewLine, report.AllErrors.Where(e => e.Severity == ValidationSeverity.Error).Select(e => e.Message))));

            ActivityRunner.RunActivity(model);
            Assert.AreNotEqual(ActivityStatus.Failed, model.Status);

            Assert.AreEqual(1, model.SedimentFractions.Count);

            model.SedimentFractions.RemoveAt(0);

            Assert.AreEqual(0, model.SedimentFractions.Count);
        }

        [Test]
        public void TestBedLoadTransportNotContainingMudFraction()
        {
            /* Expected mock behavior */
            var mocks = new MockRepository();
            var feature = mocks.DynamicMock<Feature2D>();
            var featureName = "MyFeature";
            var featureGeometry = mocks.StrictMock<IGeometry>();
            featureGeometry.Expect(g => g.Coordinates).Return(new[]
            {
                new Coordinate()
            }).Repeat.Once();

            feature.Expect(f => f.Name).Return(featureName).Repeat.Times(2);
            feature.Expect(f => f.Geometry).Return(featureGeometry).Repeat.Times(2);

            mocks.ReplayAll();

            /* Create a FlowBoundaryConditionFactory to test the CreateBoundaryCondition method */
            var sandName = "SandFraction";
            var mudName = "MudFraction";
            var bedloadName = "BedLoadFraction";
            var fmModel = new WaterFlowFMModel
            {
                SedimentFractions = new EventedList<ISedimentFraction>
                {
                    new SedimentFraction
                    {
                        Name = sandName,
                        CurrentSedimentType = new SedimentType
                        {
                            Name = "Sand",
                            Key = "sand"
                        }
                    },
                    new SedimentFraction
                    {
                        Name = mudName,
                        CurrentSedimentType = new SedimentType
                        {
                            Name = "Mud",
                            Key = "mud"
                        }
                    },
                    new SedimentFraction
                    {
                        Name = bedloadName,
                        CurrentSedimentType = new SedimentType
                        {
                            Name = "Bed-load",
                            Key = "bedload"
                        }
                    }
                }
            };
            var boundaryConditionFactory = new FlowBoundaryConditionFactory {Model = fmModel};

            /* Create a FlowBoundaryCondition and test that the SedimentFraction with CurrentSedimentType.Key = "mud" has been filtered out */
            var variable = "MorphologyBedLoadTransport";
            var dataType = BoundaryConditionDataType.TimeSeries;
            var quantityType = "Morphology";
            IBoundaryCondition boundaryCondition = boundaryConditionFactory.CreateBoundaryCondition(feature, variable, dataType, quantityType);
            var flowBoundaryCondition = boundaryCondition as FlowBoundaryCondition;

            Assert.IsNotNull(flowBoundaryCondition);
            Assert.AreEqual(2, flowBoundaryCondition.SedimentFractionNames.Count);
            Assert.AreEqual(BoundaryConditionDataType.TimeSeries, flowBoundaryCondition.DataType);
            Assert.IsFalse(flowBoundaryCondition.SedimentFractionNames.Any(sf => sf == mudName));
            Assert.AreEqual(1, flowBoundaryCondition.SedimentFractionNames.Count(sf => sf == sandName));
            Assert.AreEqual(1, flowBoundaryCondition.SedimentFractionNames.Count(sf => sf == bedloadName));

            mocks.VerifyAll();
        }

        private static WaterFlowFMModel CreateWaterFlowFMModel()
        {
            string mduPath = TestHelper.GetTestFilePath(@"simpleBox\simplebox.mdu");
            string localCopy = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(localCopy);

            model.StopTime = model.StartTime.AddMinutes(2);

            double left = model.Grid.Vertices.Min(v => v.X);
            List<Coordinate> leftVertices = model.Grid.Vertices.Where(v => v.X <= left).ToList();
            double leftUpperY = leftVertices.Max(v => v.Y);
            double leftLowerY = leftVertices.Min(v => v.Y);

            var geometry =
                new LineString(new[]
                {
                    new Coordinate(left - 1, leftLowerY),
                    new Coordinate(left - 1, leftUpperY)
                });

            var boundary = new Feature2D
            {
                Name = "left",
                Geometry = geometry
            };

            model.Boundaries.Add(boundary);
            return model;
        }

        private static void FillTimeSeries(IFunction function, Func<int, double> mapping, DateTime start, DateTime stop, int steps)
        {
            TimeSpan deltaT = stop - start;
            IEnumerable<DateTime> times = Enumerable.Range(0, steps).Select(i => start + new TimeSpan(i * deltaT.Ticks));
            IEnumerable<double> values = Enumerable.Range(0, steps).Select(mapping);
            FunctionHelper.SetValuesRaw(function.Arguments[0], times);
            FunctionHelper.SetValuesRaw(function.Components[0], values);
        }
    }
}