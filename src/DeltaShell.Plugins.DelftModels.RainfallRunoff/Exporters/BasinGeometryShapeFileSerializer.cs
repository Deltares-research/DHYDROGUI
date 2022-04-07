using System;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.Extensions.Feature;
using log4net;
using SharpMap.Data.Providers;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Exporters
{
    /// <inheritdoc cref="IBasinGeometrySerializer"/>
    internal class BasinGeometryShapeFileSerializer : IBasinGeometrySerializer
    {
        private static ILog log = LogManager.GetLogger(typeof(BasinGeometryShapeFileSerializer));

        /// <inheritdoc cref="IBasinGeometrySerializer.WriteCatchmentGeometry"/>
        public bool WriteCatchmentGeometry(IDrainageBasin basin, string path)
        {
            var features = basin.Catchments.Where(c => !c.IsGeometryDerivedFromAreaSize);
            try
            {
                Ensure.NotNullOrEmpty(path, nameof(path));
                new ShapeFileExporter().Export(features, path);
            }
            catch (Exception e)
            {
                if (!(e is IOException) && !(e is ArgumentException))
                {
                    throw;
                }

                log.Error($"Could not write catchment geometries of basin {basin.Name} to \"{path}\". {e.Message}");
                return false;
            }

            return true;
        }

        /// <inheritdoc cref="IBasinGeometrySerializer.ReadCatchmentGeometry"/>
        public bool ReadCatchmentGeometry(IDrainageBasin basin, string path)
        {
            Ensure.NotNullOrEmpty(path, nameof(path));

            using (ShapeFile shapeFile = GetShapeFile(basin.Name, path))
            {
                if (shapeFile == null)
                {
                    return false;
                }

                try
                {
                    var nameToFeatureLookup = shapeFile.Features.OfType<IFeature>().ToDictionary(f => f.Attributes["Name"]);
                    var catchmentByNameLookup = basin.Catchments.ToDictionary(c => c.Name);

                    foreach (var catchmentKvp in catchmentByNameLookup)
                    {
                        if (nameToFeatureLookup.TryGetValue(catchmentKvp.Key, out IFeature feature))
                        {
                            catchmentKvp.Value.Geometry = feature.Geometry;
                        }
                    }
                }
                catch (Exception e)
                {
                    if (!(e is IOException) && !(e is ArgumentException))
                    {
                        throw;
                    }

                    log.Error($"Either the basin catchments do not have unique id's or the features from shape file \"{path}\" do not have unique id's.");
                    return false;
                }

                return true;
            }
        }

        private static ShapeFile GetShapeFile(string basinName, string shapeFilePath)
        {
            try
            {
                if (!File.Exists(shapeFilePath))
                {
                    throw new FileNotFoundException("File does not exist.", shapeFilePath);
                }
                return new ShapeFile(shapeFilePath);
            }
            catch (Exception e)
            {
                if (!(e is IOException) && !(e is ArgumentException))
                {
                    throw;
                }

                log.Error($"Could not read catchment geometries for basin {basinName} from \"{shapeFilePath}\". {e.Message}");
                return null;
            }
        }
    }
}