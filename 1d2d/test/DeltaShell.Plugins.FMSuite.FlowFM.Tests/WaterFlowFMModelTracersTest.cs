using System;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap.Data.Providers;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [Category(TestCategory.Integration)]
    [Category(TestCategory.Slow)]
    public class WaterFlowFMModelTracersTest
    {
        private static WaterFlowFMModel CreateSimpleBoxModel()
        {
            var mduPath = TestHelper.GetTestFilePath(@"simpleBox\simplebox.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            return new WaterFlowFMModel(mduPath);
        }

        [Test]
        public void AddRemoveTracerDefinitionShouldAddRemoveData()
        {
            // create
            var model = CreateSimpleBoxModel();
            model.TracerDefinitions.Add("substance_1");
            model.TracerDefinitions.Add("substance_2");

            Assert.AreEqual(2, model.InitialTracers.Count);

            //add tracer bc
            model.BoundaryConditionSets[0].BoundaryConditions.Add(
                new FlowBoundaryCondition(FlowBoundaryQuantityType.Tracer, BoundaryConditionDataType.TimeSeries)
                {
                    TracerName = "substance_2"
                });

            model.TracerDefinitions.RemoveAt(1);

            Assert.AreEqual(1, model.InitialTracers.Count);
            Assert.IsEmpty(
                model.BoundaryConditions.OfType<FlowBoundaryCondition>()
                    .Where(bc => bc.FlowQuantity == FlowBoundaryQuantityType.Tracer)
                    .ToList());

            model.TracerDefinitions.Clear();

            Assert.IsEmpty(model.InitialTracers.ToList());
            Assert.IsEmpty(
                model.BoundaryConditions.OfType<FlowBoundaryCondition>()
                    .Where(bc => bc.FlowQuantity == FlowBoundaryQuantityType.Tracer)
                    .ToList());
        }

        [Test]
        public void AddTracerBoundaryConditionAndReload()
        {
            // create
            var model = CreateSimpleBoxModel();
            model.TracerDefinitions.Add("substance_1");

            // add tracer bc
            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.Tracer,
                BoundaryConditionDataType.TimeSeries)
            {
                TracerName = "substance_1",
                Feature = model.Boundaries.FirstOrDefault()
            };
            flowBoundaryCondition.AddPoint(0);
            var timeSeries = flowBoundaryCondition.GetDataAtPoint(0);
            timeSeries[model.StartTime] = 120.0;
            timeSeries[model.StopTime] = 140.0;
            model.BoundaryConditionSets[0].BoundaryConditions.Add(
                flowBoundaryCondition);

            //reload
            model.ExportTo("tracertest/simplebox.mdu", false);
            var newModel = new WaterFlowFMModel("tracertest/simplebox.mdu");
            Assert.AreEqual(new[] {"substance_1"}, newModel.TracerDefinitions);
            var boundaryCondition = newModel.BoundaryConditions.OfType<FlowBoundaryCondition>()
                .FirstOrDefault(bc => bc.FlowQuantity == FlowBoundaryQuantityType.Tracer);
            Assert.IsNotNull(boundaryCondition);
            Assert.IsNotNull(boundaryCondition.GetDataAtPoint(0));
            Assert.AreEqual(new[] {model.StartTime, model.StopTime},
                boundaryCondition.GetDataAtPoint(0).Arguments[0].GetValues<DateTime>());
            Assert.AreEqual(new[] {120.0, 140.0}, boundaryCondition.GetDataAtPoint(0).GetValues<double>());
        }

        [Test]
        public void AddInitialTracerOperationsAndReload()
        {
            // create
            var model = CreateSimpleBoxModel();
            model.TracerDefinitions.Add("substance_1");

            // add init tracer
            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.Tracer,
                BoundaryConditionDataType.TimeSeries)
            {
                TracerName = "substance_1",
                Feature = model.Boundaries.FirstOrDefault()
            };
            flowBoundaryCondition.AddPoint(0);
            var timeSeries = flowBoundaryCondition.GetDataAtPoint(0);
            timeSeries[model.StartTime] = 120.0;
            timeSeries[model.StopTime] = 140.0;
            model.BoundaryConditionSets[0].BoundaryConditions.Add(
                flowBoundaryCondition);
            var dataItem = model.DataItems.First(di => ReferenceEquals(di.Value, model.InitialTracers[0]));
            var valueConverter =
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
                            new Coordinate(0.0, 0.0), new Coordinate(0.0, 1.0), new Coordinate(1.0, 1.0),
                            new Coordinate(1.0, 0.0), new Coordinate(0.0, 0.0)
                        }))
            };
            var featureCollection = new FeatureCollection {FeatureType = typeof (Feature2DPolygon)};
            featureCollection.Features.Add(polygon);
            overwriteValueOperation.SetInputData(SpatialOperation.MaskInputName, featureCollection);
            valueConverter.SpatialOperationSet.AddOperation(overwriteValueOperation);
            
            // reload
            model.ExportTo("tracertest/simplebox.mdu", false);
            var newModel = new WaterFlowFMModel("tracertest/simplebox.mdu");
            Assert.AreEqual(new[] { "substance_1" }, newModel.TracerDefinitions);
            Assert.AreEqual(1, newModel.InitialTracers.Count);
            var newDataItem = newModel.DataItems.FirstOrDefault(di => di.Value == newModel.InitialTracers[0]);
            Assert.IsNotNull(newDataItem);
            Assert.IsNotNull(newDataItem.ValueConverter as SpatialOperationSetValueConverter);
            var setValueOperation =
                ((SpatialOperationSetValueConverter) newDataItem.ValueConverter).SpatialOperationSet.Operations
                    .FirstOrDefault() as SetValueOperation;
            Assert.IsNotNull(setValueOperation);
            Assert.IsNotNull(setValueOperation.Mask.Provider.GetFeature(0));
            Assert.AreEqual(polygon.Geometry.Coordinates.ToArray(), setValueOperation.Mask.Provider.GetFeature(0).Geometry.Coordinates.ToArray());
        }

        [Test]
        public void AddInitialTracerOperationsAndBoundaryAndReload()
        {
            // create
            var model = CreateSimpleBoxModel();
            model.TracerDefinitions.Add("substance_1");

            // add tracer bc
            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.Tracer,
                BoundaryConditionDataType.TimeSeries)
            {
                TracerName = "substance_1",
                Feature = model.Boundaries.FirstOrDefault()
            };
            flowBoundaryCondition.AddPoint(0);
            var timeSeries = flowBoundaryCondition.GetDataAtPoint(0);
            timeSeries[model.StartTime] = 120.0;
            timeSeries[model.StopTime] = 140.0;
            model.BoundaryConditionSets[0].BoundaryConditions.Add(
                flowBoundaryCondition);

            // add init tracer
            var dataItem = model.DataItems.First(di => ReferenceEquals(di.Value, model.InitialTracers[0]));
            var valueConverter =
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
                            new Coordinate(0.0, 0.0), new Coordinate(0.0, 1.0), new Coordinate(1.0, 1.0),
                            new Coordinate(1.0, 0.0), new Coordinate(0.0, 0.0)
                        }))
            };
            var featureCollection = new FeatureCollection { FeatureType = typeof(Feature2DPolygon) };
            featureCollection.Features.Add(polygon);
            overwriteValueOperation.SetInputData(SpatialOperation.MaskInputName, featureCollection);
            valueConverter.SpatialOperationSet.AddOperation(overwriteValueOperation);

            //reload
            model.ExportTo("tracertest/simplebox.mdu", false);
            var newModel = new WaterFlowFMModel("tracertest/simplebox.mdu");
            Assert.AreEqual(new[] { "substance_1" }, newModel.TracerDefinitions);
            var boundaryCondition = newModel.BoundaryConditions.OfType<FlowBoundaryCondition>()
                .FirstOrDefault(bc => bc.FlowQuantity == FlowBoundaryQuantityType.Tracer);
            Assert.IsNotNull(boundaryCondition);
            Assert.IsNotNull(boundaryCondition.GetDataAtPoint(0));
            Assert.AreEqual(new[] { model.StartTime, model.StopTime },
                boundaryCondition.GetDataAtPoint(0).Arguments[0].GetValues<DateTime>());
            Assert.AreEqual(new[] { 120.0, 140.0 }, boundaryCondition.GetDataAtPoint(0).GetValues<double>());
            Assert.AreEqual(1, newModel.InitialTracers.Count);
            var newDataItem = newModel.DataItems.FirstOrDefault(di => di.Value == newModel.InitialTracers[0]);
            Assert.IsNotNull(newDataItem);
            Assert.IsNotNull(newDataItem.ValueConverter as SpatialOperationSetValueConverter);
            var setValueOperation =
                ((SpatialOperationSetValueConverter)newDataItem.ValueConverter).SpatialOperationSet.Operations
                    .FirstOrDefault() as SetValueOperation;
            Assert.IsNotNull(setValueOperation);
            Assert.IsNotNull(setValueOperation.Mask.Provider.GetFeature(0));
            Assert.AreEqual(polygon.Geometry.Coordinates.ToArray(), setValueOperation.Mask.Provider.GetFeature(0).Geometry.Coordinates.ToArray());
        }

        [Test]
        public void AddInitialTracerOperationAndBoundaryTracerAndReload()
        {
            // create
            var model = CreateSimpleBoxModel();
            model.TracerDefinitions.AddRange(new[] {"substance_1", "substance_2"});

            // add tracer bc
            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.Tracer,
                BoundaryConditionDataType.TimeSeries)
            {
                TracerName = "substance_1",
                Feature = model.Boundaries.FirstOrDefault()
            };
            flowBoundaryCondition.AddPoint(0);
            var timeSeries = flowBoundaryCondition.GetDataAtPoint(0);
            timeSeries[model.StartTime] = 120.0;
            timeSeries[model.StopTime] = 140.0;
            model.BoundaryConditionSets[0].BoundaryConditions.Add(
                flowBoundaryCondition);

            // add init tracer
            var dataItem = model.DataItems.First(di => ReferenceEquals(di.Value, model.InitialTracers[1]));
            var valueConverter =
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
                            new Coordinate(0.0, 0.0), new Coordinate(0.0, 1.0), new Coordinate(1.0, 1.0),
                            new Coordinate(1.0, 0.0), new Coordinate(0.0, 0.0)
                        }))
            };
            var featureCollection = new FeatureCollection {FeatureType = typeof (Feature2DPolygon)};
            featureCollection.Features.Add(polygon);
            overwriteValueOperation.SetInputData(SpatialOperation.MaskInputName, featureCollection);
            valueConverter.SpatialOperationSet.AddOperation(overwriteValueOperation);

            //reload
            model.ExportTo("tracertest/simplebox.mdu", false);
            var newModel = new WaterFlowFMModel("tracertest/simplebox.mdu");
            Assert.AreEqual(new[] {"substance_1", "substance_2"}, newModel.TracerDefinitions);
            var boundaryCondition = newModel.BoundaryConditions.OfType<FlowBoundaryCondition>()
                .FirstOrDefault(bc => bc.FlowQuantity == FlowBoundaryQuantityType.Tracer);
            Assert.IsNotNull(boundaryCondition);
            Assert.IsNotNull(boundaryCondition.GetDataAtPoint(0));
            Assert.AreEqual(new[] {model.StartTime, model.StopTime},
                boundaryCondition.GetDataAtPoint(0).Arguments[0].GetValues<DateTime>());
            Assert.AreEqual(new[] {120.0, 140.0}, boundaryCondition.GetDataAtPoint(0).GetValues<double>());
            Assert.AreEqual(2, newModel.InitialTracers.Count);
            var newDataItem = newModel.DataItems.FirstOrDefault(di => di.Value == newModel.InitialTracers[1]);
            Assert.IsNotNull(newDataItem);
            Assert.IsNotNull(newDataItem.ValueConverter as SpatialOperationSetValueConverter);
            var setValueOperation =
                ((SpatialOperationSetValueConverter) newDataItem.ValueConverter).SpatialOperationSet.Operations
                    .FirstOrDefault() as SetValueOperation;
            Assert.IsNotNull(setValueOperation);
            Assert.IsNotNull(setValueOperation.Mask.Provider.GetFeature(0));
            Assert.AreEqual(polygon.Geometry.Coordinates.ToArray(),
                setValueOperation.Mask.Provider.GetFeature(0).Geometry.Coordinates.ToArray());
        }
    }
}
