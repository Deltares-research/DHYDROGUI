using System.Collections.Generic;
using DelftTools.Hydro;
using DeltaShell.NGHS.Common.Gui.Layers;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers
{
    /// <summary>
    /// <see cref="HydroAreaLayerProvider"/> implements the
    /// <see cref="ILayerSubProvider"/> for data of type <see cref="HydroArea"/>.
    /// </summary>
    /// <seealso cref="ILayerSubProvider" />
    public class HydroAreaLayerProvider : ILayerSubProvider
    {
        public bool CanCreateLayerFor(object sourceData, object parentData)
        {
            return sourceData is HydroArea;
        }

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
            yield return hydroArea.Weirs;
            yield return hydroArea.LandBoundaries;
            yield return hydroArea.DryPoints;
            yield return hydroArea.DryAreas;
            yield return hydroArea.Embankments;
            yield return hydroArea.Enclosures;
            yield return hydroArea.BridgePillars;
        }
    }
}