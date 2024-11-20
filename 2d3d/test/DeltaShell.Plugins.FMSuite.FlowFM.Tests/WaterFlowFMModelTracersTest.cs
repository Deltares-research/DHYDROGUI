using System;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap.Api.SpatialOperations;
using SharpMap.Data.Providers;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [Category(TestCategory.Integration)]
    [Category(TestCategory.Slow)]
    public class WaterFlowFMModelTracersTest
    {
        [Test]
        public void AddRemoveTracerDefinitionShouldAddRemoveData()
        {
            // create
            WaterFlowFMModel model = CreateSimpleBoxModel();
            model.TracerDefinitions.Add("substance_1");
            model.TracerDefinitions.Add("substance_2");

            Assert.AreEqual(2, model.SpatialData.InitialTracers.Count());

            //add tracer bc
            model.BoundaryConditionSets[0].BoundaryConditions.Add(
                new FlowBoundaryCondition(FlowBoundaryQuantityType.Tracer, BoundaryConditionDataType.TimeSeries) {TracerName = "substance_2"});

            model.TracerDefinitions.RemoveAt(1);

            Assert.AreEqual(1, model.SpatialData.InitialTracers.Count());
            Assert.IsEmpty(
                model.BoundaryConditions.OfType<FlowBoundaryCondition>()
                     .Where(bc => bc.FlowQuantity == FlowBoundaryQuantityType.Tracer)
                     .ToList());

            model.TracerDefinitions.Clear();

            Assert.IsEmpty(model.SpatialData.InitialTracers.ToList());
            Assert.IsEmpty(
                model.BoundaryConditions.OfType<FlowBoundaryCondition>()
                     .Where(bc => bc.FlowQuantity == FlowBoundaryQuantityType.Tracer)
                     .ToList());
        }

        [Test]
        public void AddTracerBoundaryConditionAndReload()
        {
            // create
            WaterFlowFMModel model = CreateSimpleBoxModel();
            model.TracerDefinitions.Add("substance_1");

            // add tracer bc
            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.Tracer,
                                                                  BoundaryConditionDataType.TimeSeries)
            {
                TracerName = "substance_1",
                Feature = model.Boundaries.FirstOrDefault()
            };
            flowBoundaryCondition.AddPoint(0);
            IFunction timeSeries = flowBoundaryCondition.GetDataAtPoint(0);
            timeSeries[model.StartTime] = 120.0;
            timeSeries[model.StopTime] = 140.0;
            model.BoundaryConditionSets[0].BoundaryConditions.Add(
                flowBoundaryCondition);

            //reload
            model.ExportTo("tracertest/simplebox.mdu", false);

            var newModel = new WaterFlowFMModel();
            newModel.ImportFromMdu("tracertest/simplebox.mdu");

            Assert.AreEqual(new[]
            {
                "substance_1"
            }, newModel.TracerDefinitions);
            FlowBoundaryCondition boundaryCondition = newModel.BoundaryConditions.OfType<FlowBoundaryCondition>()
                                                              .FirstOrDefault(bc => bc.FlowQuantity == FlowBoundaryQuantityType.Tracer);
            Assert.IsNotNull(boundaryCondition);
            Assert.IsNotNull(boundaryCondition.GetDataAtPoint(0));
            Assert.AreEqual(new[]
                            {
                                model.StartTime,
                                model.StopTime
                            },
                            boundaryCondition.GetDataAtPoint(0).Arguments[0].GetValues<DateTime>());
            Assert.AreEqual(new[]
            {
                120.0,
                140.0
            }, boundaryCondition.GetDataAtPoint(0).GetValues<double>());
        }

        [Test]
        public void AddInitialTracerOperationsAndReload()
        {
            // create
            WaterFlowFMModel model = CreateSimpleBoxModel();
            model.TracerDefinitions.Add("substance_1");

            // add init tracer
            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.Tracer,
                                                                  BoundaryConditionDataType.TimeSeries)
            {
                TracerName = "substance_1",
                Feature = model.Boundaries.FirstOrDefault()
            };
            flowBoundaryCondition.AddPoint(0);
            IFunction timeSeries = flowBoundaryCondition.GetDataAtPoint(0);
            timeSeries[model.StartTime] = 120.0;
            timeSeries[model.StopTime] = 140.0;
            model.BoundaryConditionSets[0].BoundaryConditions.Add(
                flowBoundaryCondition);
            IDataItem dataItem = model.AllDataItems.First(di => ReferenceEquals(di.Value, model.SpatialData.InitialTracers.ElementAt(0)));
            SpatialOperationSetValueConverter valueConverter =
                SpatialOperationValueConverterFactory.GetOrCreateSpatialOperationValueConverter(dataItem);
            var overwriteValueOperation = new SetValueOperation
            {
                Value = 250,
                OperationType = PointwiseOperationType.Overwrite,
                Name = "Overwrite"
            };
            var polygon = new Feature2DPolygon
            {
                Name = "poly1",
                Geometry =
                    new Polygon(
                        new LinearRing(new[]
                        {
                            new Coordinate(0.0, 0.0),
                            new Coordinate(0.0, 1.0),
                            new Coordinate(1.0, 1.0),
                            new Coordinate(1.0, 0.0),
                            new Coordinate(0.0, 0.0)
                        }))
            };
            var featureCollection = new FeatureCollection {FeatureType = typeof(Feature2DPolygon)};
            featureCollection.Features.Add(polygon);
            overwriteValueOperation.SetInputData(SpatialOperation.MaskInputName, featureCollection);
            valueConverter.SpatialOperationSet.AddOperation(overwriteValueOperation);

            // reload
            model.ExportTo("tracertest/simplebox.mdu", false);

            var newModel = new WaterFlowFMModel();
            newModel.ImportFromMdu("tracertest/simplebox.mdu");

            Assert.AreEqual(new[]
            {
                "substance_1"
            }, newModel.TracerDefinitions);
            Assert.AreEqual(1, newModel.SpatialData.InitialTracers.Count());
            IDataItem newDataItem = newModel.AllDataItems.FirstOrDefault(di => di.Value == newModel.SpatialData.InitialTracers.ElementAt(0));
            Assert.IsNotNull(newDataItem);
            Assert.IsNotNull(newDataItem.ValueConverter as SpatialOperationSetValueConverter);

            AssertCorrectSpatialOperations(newDataItem, polygon, "initialtracersubstance_1");
        }

        [Test]
        public void AddInitialTracerOperationsAndBoundaryAndReload()
        {
            // create
            WaterFlowFMModel model = CreateSimpleBoxModel();
            model.TracerDefinitions.Add("substance_1");

            // add tracer bc
            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.Tracer,
                                                                  BoundaryConditionDataType.TimeSeries)
            {
                TracerName = "substance_1",
                Feature = model.Boundaries.FirstOrDefault()
            };
            flowBoundaryCondition.AddPoint(0);
            IFunction timeSeries = flowBoundaryCondition.GetDataAtPoint(0);
            timeSeries[model.StartTime] = 120.0;
            timeSeries[model.StopTime] = 140.0;
            model.BoundaryConditionSets[0].BoundaryConditions.Add(
                flowBoundaryCondition);

            // add init tracer
            IDataItem dataItem = model.AllDataItems.First(di => ReferenceEquals(di.Value, model.SpatialData.InitialTracers.ElementAt(0)));
            SpatialOperationSetValueConverter valueConverter =
                SpatialOperationValueConverterFactory.GetOrCreateSpatialOperationValueConverter(dataItem);
            var overwriteValueOperation = new SetValueOperation
            {
                Value = 250,
                OperationType = PointwiseOperationType.Overwrite,
                Name = "Overwrite"
            };
            var polygon = new Feature2DPolygon
            {
                Name = "poly1",
                Geometry =
                    new Polygon(
                        new LinearRing(new[]
                        {
                            new Coordinate(0.0, 0.0),
                            new Coordinate(0.0, 1.0),
                            new Coordinate(1.0, 1.0),
                            new Coordinate(1.0, 0.0),
                            new Coordinate(0.0, 0.0)
                        }))
            };
            var featureCollection = new FeatureCollection {FeatureType = typeof(Feature2DPolygon)};
            featureCollection.Features.Add(polygon);
            overwriteValueOperation.SetInputData(SpatialOperation.MaskInputName, featureCollection);
            valueConverter.SpatialOperationSet.AddOperation(overwriteValueOperation);

            //reload
            model.ExportTo("tracertest/simplebox.mdu", false);

            var newModel = new WaterFlowFMModel();
            newModel.ImportFromMdu("tracertest/simplebox.mdu");

            Assert.AreEqual(new[]
            {
                "substance_1"
            }, newModel.TracerDefinitions);
            FlowBoundaryCondition boundaryCondition = newModel.BoundaryConditions.OfType<FlowBoundaryCondition>()
                                                              .FirstOrDefault(bc => bc.FlowQuantity == FlowBoundaryQuantityType.Tracer);
            Assert.IsNotNull(boundaryCondition);
            Assert.IsNotNull(boundaryCondition.GetDataAtPoint(0));
            Assert.AreEqual(new[]
                            {
                                model.StartTime,
                                model.StopTime
                            },
                            boundaryCondition.GetDataAtPoint(0).Arguments[0].GetValues<DateTime>());
            Assert.AreEqual(new[]
            {
                120.0,
                140.0
            }, boundaryCondition.GetDataAtPoint(0).GetValues<double>());
            Assert.AreEqual(1, newModel.SpatialData.InitialTracers.Count());
            IDataItem newDataItem = newModel.AllDataItems.FirstOrDefault(di => di.Value == newModel.SpatialData.InitialTracers.ElementAt(0));
            Assert.IsNotNull(newDataItem);

            AssertCorrectSpatialOperations(newDataItem, polygon, "initialtracersubstance_1");
        }

        [Test]
        public void AddInitialTracerOperationAndBoundaryTracerAndReload()
        {
            // create
            WaterFlowFMModel model = CreateSimpleBoxModel();
            model.TracerDefinitions.AddRange(new[]
            {
                "substance_1",
                "substance_2"
            });

            // add tracer bc
            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.Tracer,
                                                                  BoundaryConditionDataType.TimeSeries)
            {
                TracerName = "substance_1",
                Feature = model.Boundaries.FirstOrDefault()
            };
            flowBoundaryCondition.AddPoint(0);
            IFunction timeSeries = flowBoundaryCondition.GetDataAtPoint(0);
            timeSeries[model.StartTime] = 120.0;
            timeSeries[model.StopTime] = 140.0;
            model.BoundaryConditionSets[0].BoundaryConditions.Add(
                flowBoundaryCondition);

            // add init tracer
            IDataItem dataItem = model.AllDataItems.First(di => ReferenceEquals(di.Value, model.SpatialData.InitialTracers.ElementAt(1)));
            SpatialOperationSetValueConverter valueConverter =
                SpatialOperationValueConverterFactory.GetOrCreateSpatialOperationValueConverter(dataItem);
            var overwriteValueOperation = new SetValueOperation
            {
                Value = 250,
                OperationType = PointwiseOperationType.Overwrite,
                Name = "Overwrite"
            };
            var polygon = new Feature2DPolygon
            {
                Name = "poly1",
                Geometry =
                    new Polygon(
                        new LinearRing(new[]
                        {
                            new Coordinate(0.0, 0.0),
                            new Coordinate(0.0, 1.0),
                            new Coordinate(1.0, 1.0),
                            new Coordinate(1.0, 0.0),
                            new Coordinate(0.0, 0.0)
                        }))
            };
            var featureCollection = new FeatureCollection {FeatureType = typeof(Feature2DPolygon)};
            featureCollection.Features.Add(polygon);
            overwriteValueOperation.SetInputData(SpatialOperation.MaskInputName, featureCollection);
            valueConverter.SpatialOperationSet.AddOperation(overwriteValueOperation);

            //reload
            model.ExportTo("tracertest/simplebox.mdu", false);

            var newModel = new WaterFlowFMModel();
            newModel.ImportFromMdu("tracertest/simplebox.mdu");

            Assert.AreEqual(new[]
            {
                "substance_1",
                "substance_2"
            }, newModel.TracerDefinitions);
            FlowBoundaryCondition boundaryCondition = newModel.BoundaryConditions.OfType<FlowBoundaryCondition>()
                                                              .FirstOrDefault(bc => bc.FlowQuantity == FlowBoundaryQuantityType.Tracer);
            Assert.IsNotNull(boundaryCondition);
            Assert.IsNotNull(boundaryCondition.GetDataAtPoint(0));
            Assert.AreEqual(new[]
                            {
                                model.StartTime,
                                model.StopTime
                            },
                            boundaryCondition.GetDataAtPoint(0).Arguments[0].GetValues<DateTime>());
            Assert.AreEqual(new[]
            {
                120.0,
                140.0
            }, boundaryCondition.GetDataAtPoint(0).GetValues<double>());
            Assert.AreEqual(2, newModel.SpatialData.InitialTracers.Count());
            IDataItem newDataItem = newModel.AllDataItems.FirstOrDefault(di => di.Value == newModel.SpatialData.InitialTracers.ElementAt(1));
            Assert.IsNotNull(newDataItem);

            AssertCorrectSpatialOperations(newDataItem, polygon, "initialtracersubstance_2");
        }

        private static void AssertCorrectSpatialOperations(IDataItem newDataItem, Feature2DPolygon polygon, string quantityName)
        {
            IEventedList<ISpatialOperation> operation = ((SpatialOperationSetValueConverter) newDataItem.ValueConverter).SpatialOperationSet.Operations;
            var importOperation = ((SpatialOperationSet) operation[0]).Operations.Single() as ImportSamplesOperation;
            Assert.That(importOperation, Is.Not.Null);
            Assert.That(importOperation.Name, Is.EqualTo(quantityName));

            var interpolateOperation = operation[1] as InterpolateOperation;
            Assert.That(interpolateOperation, Is.Not.Null);

            var setValueOperation = operation[2] as SetValueOperation;
            Assert.IsNotNull(setValueOperation);
            Assert.IsNotNull(setValueOperation.Mask.Provider.GetFeature(0));

            CollectionAssert.AreEqual(polygon.Geometry.Coordinates, setValueOperation.Mask.Provider.GetFeature(0).Geometry.Coordinates);
        }

        private static WaterFlowFMModel CreateSimpleBoxModel()
        {
            string mduPath = TestHelper.GetTestFilePath(@"simpleBox\simplebox.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var waterFlowFmModel = new WaterFlowFMModel();
            waterFlowFmModel.ImportFromMdu(mduPath);

            return waterFlowFmModel;
        }
    }
}