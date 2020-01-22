using System;
using DelftTools.Hydro;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Providers;
using SharpMap.Api.Layers;
using SharpMap.Editors.Interactors;
using SharpMap.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers
{
    /// <summary>
    /// A class factory that provides methods for constructing
    /// <see cref="ILayer"/> objects for the NetworkEditor plugin.
    /// </summary>
    public static class NetworkEditorLayerFactory
    {
        /// <summary>
        /// Creates a new hydro area layer.
        /// </summary>
        /// <param name="hydroArea"> The hydro area. </param>
        /// <returns> A hydro area layer containing the given hydro area. </returns>
        /// <exception cref="ArgumentNullException">
        /// Throw when <paramref name="hydroArea"/> is <c>null</c>.
        /// </exception>
        public static ILayer CreateAreaLayer(HydroArea hydroArea)
        {
            Ensure.NotNull(hydroArea, nameof(hydroArea));
            return new AreaLayer
            {
                HydroArea = hydroArea,
                NameIsReadOnly = true
            };
        }

        public static ILayer CreateThinDamsLayer(HydroArea hydroArea)
        {
            Ensure.NotNull(hydroArea, nameof(hydroArea));
            return new VectorLayer(HydroArea.ThinDamsPluralName)
            {
                FeatureEditor = new Feature2DEditor(hydroArea),
                Style = AreaLayerStyles.ThinDamStyle,
                DataSource = new HydroAreaFeature2DCollection(hydroArea).Init(hydroArea.ThinDams, "ThinDam", "NetworkEditorModelName", hydroArea.CoordinateSystem),
                NameIsReadOnly = true
            };
        }
    }
}