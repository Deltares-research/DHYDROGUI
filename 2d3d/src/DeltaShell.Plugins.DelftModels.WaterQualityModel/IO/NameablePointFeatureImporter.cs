using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Data.Providers;
using SharpMap.Extensions.CoordinateSystems;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    public abstract class NameablePointFeatureImporter<T> : IFileImporter where T : NameablePointFeature
    {
        public Func<double> GetDefaultZValue { get; set; }

        public ICoordinateSystem ModelCoordinateSystem { get; set; }
        public abstract string Name { get; }
        public string Category => "Hydro";

        public string Description => string.Empty;
        public abstract Bitmap Image { get; }

        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(IEventedList<T>);
            }
        }

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
            var list = target as IEventedList<T>;

            // stop importing if this was not the correct type
            if (list == null)
            {
                throw new NotSupportedException("Target type is not is supported.");
            }

            var file = new ShapeFile(path);

            if (file.GetFeatureCount() > 0)
            {
                ICoordinateTransformation transformation = null;
                if (ModelCoordinateSystem != null && file.CoordinateSystem != null)
                {
                    transformation =
                        new OgrCoordinateSystemFactory().CreateTransformation(
                            file.CoordinateSystem, ModelCoordinateSystem);
                }

                foreach (IFeature feature in file.Features)
                {
                    if (feature.Geometry is Point)
                    {
                        var transformedGeometry = (IGeometry) feature.Geometry.Clone();
                        if (transformation != null)
                        {
                            transformedGeometry = GeometryTransform.TransformGeometry(feature.Geometry,
                                                                                      transformation.MathTransform);
                        }

                        T newFeature = CreateFeature();
                        newFeature.Geometry = transformedGeometry;

                        ReadAttributes(newFeature, feature, list);

                        list.Add(newFeature);
                    }
                }
            }

            return target;
        }

        protected abstract string NewNameFormatString { get; }

        protected virtual void ReadAttributes(T newFeature, IFeature feature, IEnumerable<T> list)
        {
            const string nameAttributeName = "Name";
            if (feature.Attributes.ContainsKey(nameAttributeName))
            {
                var retrievedValue = feature.Attributes[nameAttributeName] as string;
                newFeature.Name = retrievedValue ?? NamingHelper.GetUniqueName(NewNameFormatString, list);
            }
            else
            {
                newFeature.Name = NamingHelper.GetUniqueName(NewNameFormatString, list);
            }

            const string zAttributeName = "Z";
            if (feature.Attributes.ContainsKey(zAttributeName))
            {
                object retrievedValue = feature.Attributes[zAttributeName];
                if (!(retrievedValue is DBNull))
                {
                    newFeature.Z = Convert.ToDouble(retrievedValue);
                }
                else
                {
                    newFeature.Z = TryGetDefaultZValue();
                }
            }
            else
            {
                newFeature.Z = TryGetDefaultZValue();
            }
        }

        protected abstract T CreateFeature();

        private double TryGetDefaultZValue()
        {
            return GetDefaultZValue?.Invoke() ?? double.NaN;
        }
    }
}