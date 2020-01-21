using System;
using DelftTools.Hydro;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers
{
    /// <summary>
    /// A class factory that provides methods for constructing
    /// <see cref="ILayer"/> objects for the NetworkEditor plugin.
    /// </summary>
    public class NetworkEditorLayerFactory
    {
        /// <summary>
        /// Creates a new hydro area layer.
        /// </summary>
        /// <param name="hydroArea"> The hydro area. </param>
        /// <returns> A hydro area layer containing the given hydro area. </returns>
        /// <exception cref="ArgumentNullException">
        /// Throw when <paramref name="hydroArea"/> is <c>null</c>.
        /// </exception>
        public ILayer CreateAreaLayer(HydroArea hydroArea)
        {
            Ensure.NotNull(hydroArea, nameof(hydroArea));
            return new AreaLayer
            {
                HydroArea = hydroArea,
                NameIsReadOnly = true
            };
        }
    }
}