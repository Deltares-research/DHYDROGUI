using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.SharpMapGis;
using GeoAPI.Extensions.CoordinateSystems;
using log4net;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers
{
    public class FMOutputSnappedFeaturesGroupLayerData
    {
        public static readonly string OutputSnappedFeaturePostfix = "_snapped";
        private static readonly ILog Log = LogManager.GetLogger(typeof(FMOutputSnappedFeaturesGroupLayerData));

        private static readonly Dictionary<string, string> SnappedFeatureDict = new Dictionary<string, string>
        {
            {"Cross Sections", "_crs"},
            {"Weirs", "_weir"},
            {"Gates", "_gate"},
            {"Fixed weirs", "_fxw"},
            {"Thin dams", "_thd"},
            {"Observation stations", "_obs"},
            {"Dry areas", "_dryarea"},
            {"Enclosures", "_enc"},
            {"Sources", "_src"}
        };

        private readonly string modelOutputSnappedFeaturesPath;
        private readonly string modelMduFilePath;

        public FMOutputSnappedFeaturesGroupLayerData(WaterFlowFMModel model)
        {
            modelOutputSnappedFeaturesPath = model.OutputSnappedFeaturesPath;
            modelMduFilePath = model.MduFilePath;
            CoordinateSystem = model.CoordinateSystem;
        }

        public ICoordinateSystem CoordinateSystem { get; set; }

        public IList<ILayer> CreateLayers()
        {
            if (!Directory.Exists(modelOutputSnappedFeaturesPath))
            {
                Log.WarnFormat(
                    Resources
                        .FMOutputSnappedFeaturesGroupLayerData_GetValidLayersLocation_Output_snapped_feature_layers_location_not_found_at___0_,
                    modelOutputSnappedFeaturesPath);
                return Enumerable.Empty<ILayer>().ToList();
            }

            string mduName = Path.GetFileNameWithoutExtension(modelMduFilePath);

            return SnappedFeatureDict
                   .SelectMany(
                       kvp => CreateLayerForSnappedFeatureShape(kvp.Key, kvp.Value, mduName,
                                                                modelOutputSnappedFeaturesPath)).ToList();
        }

        private IEnumerable<ILayer> CreateLayerForSnappedFeatureShape(string layerName, string featurePostfix,
                                                                      string mduName, string layersLocation)
        {
            string layerFileName = string.Concat(mduName, OutputSnappedFeaturePostfix, featurePostfix, ".shp");
            string layerLocation = Path.Combine(layersLocation, layerFileName);
            if (!File.Exists(layerLocation))
            {
                Log.WarnFormat("Output snapped feature for {0} not found at: {1}", layerName, layerLocation);
                return Enumerable.Empty<ILayer>();
            }

            /* The file is meant to be one level below the mdu file, under the dflowfmoutput directory. */
            List<ILayer> layers = FileBasedLayerFactory.CreateLayersFromFile(layerLocation).ToList();
            if (layers.Count == 0)
            {
                Log.WarnFormat("Output snapped feature for {0} was not generated.", layerName);
            }

            layers.ForEach(l => l.Name = layerName);
            layers.ForEach(l => l.DataSource.CoordinateSystem = CoordinateSystem);

            return layers;
        }
    }
}