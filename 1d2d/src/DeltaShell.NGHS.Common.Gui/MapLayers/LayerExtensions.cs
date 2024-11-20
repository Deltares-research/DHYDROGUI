using System.Collections.Generic;
using System.Linq;
using Deltares.Infrastructure.API.Guards;
using SharpMap.Api.Layers;
using SharpMap.Layers;

namespace DeltaShell.NGHS.Common.Gui.MapLayers
{
    /// <summary>
    /// Contains extensions for <see cref="Layer"/>.
    /// </summary>
    public static class LayerExtensions
    {
        /// <summary>
        /// Sets the name of the layer, regardless of whether the name is read-only.
        /// </summary>
        /// <param name="layer"> The layer of which to set the name. </param>
        /// <param name="name"> The name name of the layer. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="layer"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// Restores the <seealso cref="Layer.NameIsReadOnly"/> property of the <paramref name="layer"/>.
        /// </remarks>
        public static void SetName(this Layer layer, string name)
        {
            Ensure.NotNull(layer, nameof(layer));

            bool nameIsReadOnly = layer.NameIsReadOnly;
            layer.NameIsReadOnly = false;

            layer.Name = name;

            layer.NameIsReadOnly = nameIsReadOnly;
        }

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