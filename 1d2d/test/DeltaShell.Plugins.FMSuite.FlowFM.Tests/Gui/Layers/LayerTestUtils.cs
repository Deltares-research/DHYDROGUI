using System.Collections.Generic;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers
{
    internal static class LayerTestUtils
    {
        public static ILayer FindLayerByNameRecursively(ILayer rootLayer, string layerName)
        {
            var layers = new Queue<ILayer>();
            layers.Enqueue(rootLayer);

            while (layers.Count > 0)
            {
                ILayer nextLayer = layers.Dequeue();

                if (nextLayer.Name == layerName)
                {
                    return nextLayer;
                }

                if (!(nextLayer is IGroupLayer groupLayer))
                {
                    continue;
                }

                foreach (ILayer subLayer in groupLayer.Layers)
                {
                    layers.Enqueue(subLayer);
                }
            }

            return null;
        }
    }
}
