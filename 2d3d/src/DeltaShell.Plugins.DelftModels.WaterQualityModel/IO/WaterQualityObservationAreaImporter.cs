using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using SharpMap.Api.SpatialOperations;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Data.Providers;
using SharpMap.Extensions.CoordinateSystems;
using SharpMap.SpatialOperations;
using PointwiseOperationType = DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas.PointwiseOperationType;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    public class WaterQualityObservationAreaImporter : IFileImporter
    {
        /// <summary>
        /// Coordinate system of the model of the import target.
        /// </summary>
        public ICoordinateSystem ModelCoordinateSystem { get; set; }

        /// <summary>
        /// Method to retrieve the <see cref="IDataItem"/> storing the import target.
        /// </summary>
        public Func<WaterQualityObservationAreaCoverage, IDataItem> GetDataItemForTarget { get; set; }

        public string Name => "Observation area from GIS importer";

        public string Category => "Hydro";

        public string Description => string.Empty;

        public Bitmap Image => null;

        public IEnumerable<Type> SupportedItemTypes => new[]
        {
            typeof(WaterQualityObservationAreaCoverage)
        };

        public bool CanImportOnRootLevel => false;

        public string FileFilter => "Shape file (*.shp)|*.shp";

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public bool OpenViewAfterImport => true;

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public object ImportItem(string path, object target = null)
        {
            var observationAreas = target as WaterQualityObservationAreaCoverage;
            if (observationAreas == null)
            {
                throw new NotSupportedException("Target should be Water Quality Observation Area Spatial Data.");
            }

            var shapeFile = new ShapeFile(path);
            if (shapeFile.GetFeatureCount() > 0)
            {
                IDataItem dataItem = GetDataItemForTarget(observationAreas);
                SpatialOperationSetValueConverter spatialOperationValueConverter =
                    SpatialOperationValueConverterFactory.GetOrCreateSpatialOperationValueConverter(
                        dataItem, dataItem.Name);

                ICoordinateTransformation transformation =
                    GetCoordinateTransformationToModelCoordinateSystem(shapeFile);

                foreach (IFeature feature in shapeFile.Features.OfType<IFeature>().Where(f => f.Geometry is Polygon))
                {
                    IFeature clonedFeature = CloneFeatureInModelCoordinateSystem(feature, transformation);
                    AddSetLabelOperationForPolygon(spatialOperationValueConverter, clonedFeature);
                }

                spatialOperationValueConverter.SpatialOperationSet.Execute();

                return dataItem.Value;
            }

            return observationAreas;
        }

        private ICoordinateTransformation GetCoordinateTransformationToModelCoordinateSystem(ShapeFile shapeFile)
        {
            ICoordinateTransformation transformation = null;
            if (ModelCoordinateSystem != null && shapeFile.CoordinateSystem != null)
            {
                transformation =
                    new OgrCoordinateSystemFactory().CreateTransformation(
                        shapeFile.CoordinateSystem, ModelCoordinateSystem);
            }

            return transformation;
        }

        private static IFeature CloneFeatureInModelCoordinateSystem(IFeature feature,
                                                                    ICoordinateTransformation transformation)
        {
            var clonedFeature = (IFeature) feature.Clone();
            var transformedGeometry = (IGeometry) feature.Geometry.Clone();
            if (transformation != null)
            {
                transformedGeometry = GeometryTransform.TransformGeometry(transformedGeometry,
                                                                          transformation.MathTransform);
            }

            clonedFeature.Geometry = transformedGeometry;
            return clonedFeature;
        }

        private void AddSetLabelOperationForPolygon(SpatialOperationSetValueConverter spatialOperationValueConverter,
                                                    IFeature clonedFeature)
        {
            var setLabelOperation = new SetLabelOperation
            {
                Name = GetUniqueOperationName(spatialOperationValueConverter),
                Label = GetNameAttributeValue(clonedFeature,
                                              spatialOperationValueConverter.SpatialOperationSet.Operations),
                OperationType = PointwiseOperationType.Overwrite
            };
            setLabelOperation.SetInputData(SpatialOperation.MaskInputName,
                                           new FeatureCollection(new EventedList<IFeature>(new[]
                                           {
                                               clonedFeature
                                           }), typeof(Feature)) {CoordinateSystem = ModelCoordinateSystem});

            spatialOperationValueConverter.SpatialOperationSet.AddOperation(setLabelOperation);
        }

        private static string GetUniqueOperationName(SpatialOperationSetValueConverter spatialOperationValueConverter)
        {
            IEnumerable<ISpatialOperation> allOperations =
                spatialOperationValueConverter.SpatialOperationSet.GetOperationsRecursive();
            string operationName = NamingHelper.GetUniqueName("Set Label {0}", allOperations);
            return operationName;
        }

        private string GetNameAttributeValue(IFeature feature, IEventedList<ISpatialOperation> operations)
        {
            const string nameAttributeName = "Name";
            if (feature.Attributes.ContainsKey(nameAttributeName))
            {
                var retrievedValue = feature.Attributes[nameAttributeName] as string;
                return retrievedValue;
            }

            return NamingHelper.GenerateUniqueNameFromList("Observation Area {0}", false,
                                                           operations.OfType<SetLabelOperation>().Select(o => o.Label));
        }
    }
}