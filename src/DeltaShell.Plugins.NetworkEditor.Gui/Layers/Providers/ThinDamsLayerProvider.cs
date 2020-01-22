using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common.Gui.Layers;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers.Providers
{
    public class ThinDamsLayerProvider : ILayerSubProvider
    {
        public bool CanCreateLayerFor(object sourceData, object parentData)
        {
            return sourceData is IEventedList<ThinDam2D> && parentData is HydroArea;
        }

        public ILayer CreateLayer(object sourceData, object parentData)
        {
            return parentData is HydroArea hydroArea
                       ? NetworkEditorLayerFactory.CreateThinDamsLayer(hydroArea)
                       : null;
        }

        public IEnumerable<object> GenerateChildLayerObjects(object data)
        {
            yield break;
        }
    }
}