using System.Collections.Generic;
using System.Linq;
using SharpMap.Api.Layers;

namespace DeltaShell.NGHS.Common.Gui
{
    public static class LayerExtensions
    {
        public static void SetRenderOrderByObjectOrder(this IGroupLayer groupLayer, IList<object> objectsInRenderOrder, IDictionary<ILayer, object> objectsLookup)
        {
            var objectsFound = groupLayer.Layers.ToDictionary(l => objectsLookup[l], l => l);
            var renderOrderToAssign = 1;

            foreach (object objectToSearch in objectsInRenderOrder)
            {
                if (objectsFound.TryGetValue(objectToSearch, out var foundLayer))
                {
                    if (foundLayer is IGroupLayer foundGroupLayer)
                    {
                        ResetRenderOrder(foundGroupLayer.Layers, renderOrderToAssign);
                        renderOrderToAssign += foundGroupLayer.Layers.GetLayersRecursive(false, true).Count();
                        continue;
                    }

                    foundLayer.RenderOrder = renderOrderToAssign++;
                }
            }
        }

        private static void ResetRenderOrder(IEnumerable<ILayer> layers, int offset)
        {
            var allMapLayers = layers
                .GetLayersRecursive(false, true)
                .OrderBy(l => l.RenderOrder)
                .ToList();

            var count = offset;

            foreach (var layer in allMapLayers)
            {
                layer.RenderOrder = count++;
            }
        }
    }
}
