using System.Collections.Generic;
using DelftTools.Hydro;
using DeltaShell.NGHS.Common.Gui.Layers;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers
{
    /// <summary>
    /// Provides logic for creating <see cref="ILayer"/> objects for <see cref="HydroArea"/> objects.
    /// </summary>
    internal sealed class HydroAreaLayerProvider : ILayerSubProvider
    {
        /// <inheritdoc/>
        public bool CanCreateLayerFor(object sourceData, object parentData)
        {
            return sourceData is HydroArea;
        }

        /// <inheritdoc/>
        public ILayer CreateLayer(object sourceData, object parentData)
        {
            return sourceData is HydroArea hydroArea
                       ? new HydroAreaLayer
                       {
                           HydroArea = hydroArea,
                           NameIsReadOnly = true
                       }
                       : null;
        }

        /// <inheritdoc/>
        public IEnumerable<object> GenerateChildLayerObjects(object data)
        {
            if (!(data is HydroArea hydroArea))
            {
                yield break;
            }

            yield return hydroArea.ThinDams;
            yield return hydroArea.FixedWeirs;
            yield return hydroArea.ObservationPoints;
            yield return hydroArea.ObservationCrossSections;
            yield return hydroArea.Pumps;
            yield return hydroArea.Structures;
            yield return hydroArea.LandBoundaries;
            yield return hydroArea.DryPoints;
            yield return hydroArea.DryAreas;
            yield return hydroArea.Enclosures;
            yield return hydroArea.BridgePillars;
        }
    }
}