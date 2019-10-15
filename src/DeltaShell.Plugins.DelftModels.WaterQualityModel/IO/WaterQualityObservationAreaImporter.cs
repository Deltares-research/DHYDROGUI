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

using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;

using NetTopologySuite.Geometries;

using NetTopologySuite.Extensions.Features;

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
        public string Name
        {
            get { return "Observation area from GIS importer"; }
        }
        public string Description { get { return Name; } }
        public string Category
        {
            get { return "Hydro"; }
        }

        public Bitmap Image
        {
            get { return null; }
        }

        public IEnumerable<Type> SupportedItemTypes
        {
            get { return new[]{typeof(WaterQualityObservationAreaCoverage)}; }
        }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public bool CanImportOnRootLevel
        {
            get { return false; }
        }

        public string FileFilter
        {
            get { return "Shape file (*.shp)|*.shp"; }
        }

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public bool OpenViewAfterImport
        {
            get { return true; }
        }

        /// <summary>
        /// Coordinate system of the model of the import target.
        /// </summary>
        public ICoordinateSystem ModelCoordinateSystem { get; set; }

        /// <summary>
        /// Method to retrieve the <see cref="IDataItem"/> storing the import target.
        /// </summary>
        public Func<WaterQualityObservationAreaCoverage, IDataItem> GetDataItemForTarget { get; set; }

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
                var dataItem = GetDataItemForTarget(observationAreas);
                var spatialOperationValueConverter = SpatialOperationValueConverterFactory.GetOrCreateSpatialOperationValueConverter(dataItem, dataItem.Name);

                var transformation = GetCoordinateTransformationToModelCoordinateSystem(shapeFile);

                foreach (var feature in shapeFile.Features.OfType<IFeature>().Where(f => f.Geometry is Polygon))
                {
                    var clonedFeature = CloneFeatureInModelCoordinateSystem(feature, transformation);
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
                transformation = new OgrCoordinateSystemFactory().CreateTransformation(shapeFile.CoordinateSystem, ModelCoordinateSystem);
            }
            return transformation;
        }

        private static IFeature CloneFeatureInModelCoordinateSystem(IFeature feature, ICoordinateTransformation transformation)
        {
            var clonedFeature = (IFeature)feature.Clone();
            IGeometry transformedGeometry = (IGeometry)feature.Geometry.Clone();
            if (transformation != null)
            {
                transformedGeometry = GeometryTransform.TransformGeometry(transformedGeometry,
                    transformation.MathTransform);
            }
            clonedFeature.Geometry = transformedGeometry;
            return clonedFeature;
        }

        private void AddSetLabelOperationForPolygon(SpatialOperationSetValueConverter spatialOperationValueConverter, IFeature clonedFeature)
        {
            var setLabelOperation = new SetLabelOperation
            {
                Name = GetUniqueOperationName(spatialOperationValueConverter),
                Label = GetNameAttributeValue(clonedFeature, spatialOperationValueConverter.SpatialOperationSet.Operations),
                OperationType = PointwiseOperationType.Overwrite
            };
            setLabelOperation.SetInputData(SpatialOperation.MaskInputName,
                new FeatureCollection(new EventedList<IFeature>(new[] { clonedFeature }), typeof(Feature))
                {
                    CoordinateSystem = ModelCoordinateSystem
                });

            spatialOperationValueConverter.SpatialOperationSet.AddOperation(setLabelOperation);
        }

        private static string GetUniqueOperationName(SpatialOperationSetValueConverter spatialOperationValueConverter)
        {
            var allOperations = spatialOperationValueConverter.SpatialOperationSet.GetOperationsRecursive();
            var operationName = NamingHelper.GetUniqueName("Set Label {0}", allOperations);
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
            return NamingHelper.GenerateUniqueNameFromList("Observation Area {0}", false, operations.OfType<SetLabelOperation>().Select(o => o.Label));
        }
    }
}