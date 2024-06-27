using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Common.Gui.MapLayers.Providers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Extensions;
using log4net;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers.Providers.ProvidersOutput.SnappedFeatures
{
    /// <summary>
    /// <see cref="OutputSnappedFeatureGroupLayerSubProvider"/> is responsible for creating
    /// output snapped feature group layers out of <see cref="OutputSnappedFeatureGroupData"/>.
    /// </summary>
    /// <seealso cref="ILayerSubProvider"/>
    internal sealed class OutputSnappedFeatureGroupLayerSubProvider : ILayerSubProvider
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(OutputSnappedFeatureGroupLayerSubProvider));
        private readonly IFlowFMLayerInstanceCreator instanceCreator;

        /// <summary>
        /// Creates a new <see cref="OutputSnappedFeatureGroupLayerSubProvider"/> with the
        /// provided <paramref name="instanceCreator"/>.
        /// </summary>
        /// <param name="instanceCreator">The instance creator used to construct the layers.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="instanceCreator"/> is <c>null</c>.
        /// </exception>
        public OutputSnappedFeatureGroupLayerSubProvider(IFlowFMLayerInstanceCreator instanceCreator)
        {
            Ensure.NotNull(instanceCreator, nameof(instanceCreator));
            this.instanceCreator = instanceCreator;
        }

        public bool CanCreateLayerFor(object sourceData, object parentData) =>
            sourceData is OutputSnappedFeatureGroupData groupData &&
            groupData.Model.HasSnappedOutputFeatures();

        public ILayer CreateLayer(object sourceData, object parentData) =>
            CanCreateLayerFor(sourceData, parentData)
                ? instanceCreator.CreateOutputSnappedFeatureGroupLayer()
                : null;

        public IEnumerable<object> GenerateChildLayerObjects(object data)
        {
            if (!(data is OutputSnappedFeatureGroupData groupData &&
                  groupData.Model.HasSnappedOutputFeatures()))
            {
                yield break;
            }

            IWaterFlowFMModel model = groupData.Model;
            string mduName = Path.GetFileNameWithoutExtension(model.MduFilePath);

            foreach ((string layerName, string layerPath) in SnappedFeatures(model.OutputSnappedFeaturesPath, mduName))
            {
                if (File.Exists(layerPath))
                {
                    yield return new OutputSnappedFeatureData(model, layerName, layerPath);
                }
                else
                { 
                    log.WarnFormat("Output snapped feature for {0} not found at: {1}", layerName, layerPath);
                }
            }
        }

        private static IEnumerable<ValueTuple<string, string>> SnappedFeatures(string parentDirectory,
                                                                               string mduName) =>
            FeaturesDescription().Select(v => (v.Item1, ToPath(parentDirectory, mduName, v.Item2)));

        private static string ToPath(string parentDirectory, string mduName, string featurePostFix) =>
            Path.Combine(parentDirectory, $"{mduName}_snapped_{featurePostFix}.shp");

        private static IEnumerable<ValueTuple<string, string>> FeaturesDescription()
        {
            yield return ("Cross Sections", "crs");
            yield return ("Weirs", "weir");
            yield return ("Gates", "gate");
            yield return ("Fixed weirs", "fxw");
            yield return ("Thin dams", "thd");
            yield return ("Observation stations", "obs");
            yield return ("Embankments", "emb");
            yield return ("Dry areas", "dryarea");
            yield return ("Enclosures", "enc");
            yield return ("Sources", "src");
        }
    }
}