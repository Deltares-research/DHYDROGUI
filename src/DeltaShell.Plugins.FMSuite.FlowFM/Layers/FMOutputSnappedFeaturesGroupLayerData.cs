using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.SharpMapGis;
using GeoAPI.Extensions.CoordinateSystems;
using log4net;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Layers
{
    public class FMOutputSnappedFeaturesGroupLayerData
    {
        private readonly string modelOutputSnappedFeaturesPath;
        private readonly string modelMduFilePath;
        public ICoordinateSystem coordinateSystem;
        private static readonly ILog Log = LogManager.GetLogger(typeof(FMOutputSnappedFeaturesGroupLayerData));

        public readonly static string OutputSnappedFeaturePostfix = "_snapped";

        private static readonly Dictionary<string, string> SnappedFeatureDict = new Dictionary<string, string>
        {
            {"Cross Sections", "_crs" },
            {"Weirs", "_weir"},
            {"Gates", "_gate"},
            {"Fixed weirs", "_fxw" },
            {"Thin dams", "_thd"},
            {"Observation stations", "_obs"},
            {"Embankments", "_emb"},
            {"Dry areas", "_dryarea"},
            {"Enclosures", "_enc"},
            {"Sources", "_src"},
        };

        public FMOutputSnappedFeaturesGroupLayerData(WaterFlowFMModel model)
        {
            this.modelOutputSnappedFeaturesPath = model.OutputSnappedFeaturesPath;
            this.modelMduFilePath = model.MduFilePath;
            this.coordinateSystem = model.CoordinateSystem;
        }

        public IList<ILayer> CreateLayers()
        {
            if (!Directory.Exists(modelOutputSnappedFeaturesPath))
            {
                Log.WarnFormat(Resources.FMOutputSnappedFeaturesGroupLayerData_GetValidLayersLocation_Output_snapped_feature_layers_location_not_found_at___0_, modelOutputSnappedFeaturesPath);
                return Enumerable.Empty<ILayer>().ToList();
            }

            string mduName = Path.GetFileNameWithoutExtension(modelMduFilePath);

            return SnappedFeatureDict.SelectMany(kvp => CreateLayerForSnappedFeatureShape(kvp.Key, kvp.Value, mduName, modelOutputSnappedFeaturesPath)).ToList();
        }

        private IEnumerable<ILayer> CreateLayerForSnappedFeatureShape(string layerName, string featurePostfix, string mduName, string layersLocation)
        {
            string layerFileName = String.Concat(mduName, OutputSnappedFeaturePostfix, featurePostfix, ".shp");
            string layerLocation = Path.Combine(layersLocation, layerFileName);
            if (!File.Exists(layerLocation))
            {
                Log.WarnFormat("Output snapped feature for {0} not found at: {1}", layerName, layerLocation);
                return Enumerable.Empty<ILayer>();
            }

            /* The file is meant to be one level below the mdu file, under the dflowfmoutput directory. */
            var layers = FileBasedLayerFactory.CreateLayersFromFile(layerLocation).ToList();
            if (layers.Count == 0)
            {
                Log.WarnFormat("Output snapped feature for {0} was not generated.", layerName);
            }

            layers.ForEach( l => l.Name = layerName);
            layers.ForEach(l => l.DataSource.CoordinateSystem = this.coordinateSystem);

            return layers;
        }
    }
}