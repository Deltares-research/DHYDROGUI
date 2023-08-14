using System.Drawing;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using SharpMap.Api.Layers;
using SharpMap.Data.Providers;
using SharpMap.Editors.Interactors;
using SharpMap.Layers;
using SharpMap.Styles;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers
{
    /// <summary>
    /// Map layer provided for lateral features.
    /// </summary>
    public static class LateralMapLayerProvider
    {
        /// <summary>
        /// The lateral map layer name.
        /// </summary>
        public const string LayerName = "Laterals";

        private static readonly VectorStyle layerStyle = new VectorStyle
        {
            Line = new Pen(Color.Red, 3f),
            GeometryType = typeof(IPoint),
            Symbol = Resources.LateralPoint
        };

        /// <summary>
        /// Create a layer for the provided lateral features.
        /// </summary>
        /// <param name="lateralFeatures"> The lateral features. </param>
        /// <param name="model"> The model with the lateral features. </param>
        /// <returns>
        /// A new <see cref="VectorLayer"/> instance.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="lateralFeatures"/> or <paramref name="model"/> is <c>null</c>.
        /// </exception>
        public static ILayer Create(IEventedList<Feature2D> lateralFeatures, WaterFlowFMModel model)
        {
            Ensure.NotNull(lateralFeatures, nameof(lateralFeatures));
            Ensure.NotNull(model, nameof(model));

            return new VectorLayer(LayerName)
            {
                DataSource = new Feature2DCollection().Init(lateralFeatures, "Lateral", nameof(WaterFlowFMModel),
                                                            model.CoordinateSystem),
                FeatureEditor = new Feature2DEditor(model),
                Style = layerStyle,
                NameIsReadOnly = true,
                ShowInLegend = false
            };
        }
    }
}