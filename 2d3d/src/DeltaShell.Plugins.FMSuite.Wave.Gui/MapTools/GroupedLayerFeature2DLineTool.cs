using System.Drawing;
using System.Linq;
using SharpMap.Layers;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.MapTools
{
    /// <summary>
    /// <see cref="GroupedLayerFeature2DLineTool"/> extends <see cref="Feature2DLineTool"/>
    /// to work with a <see cref="GroupLayer"/> containing a specified child vector layer.
    /// This makes it possible to define map tools that need to act on a child layer of a
    /// <see cref="GroupLayer"/> in the same way as you would for a regular
    /// <see cref="VectorLayer"/>.
    /// </summary>
    /// <seealso cref="Feature2DLineTool"/>
    public class GroupedLayerFeature2DLineTool : Feature2DLineTool
    {
        /// <summary>
        /// Creates a new <see cref="GroupedLayerFeature2DLineTool"/>.
        /// </summary>
        /// <param name="targetGroupLayerName">Name of the target group layer.</param>
        /// <param name="targetChildVectorLayerName">Name of the target child vector layer.</param>
        /// <param name="name">The name.</param>
        /// <param name="icon">The icon.</param>
        public GroupedLayerFeature2DLineTool(string targetGroupLayerName,
                                             string targetChildVectorLayerName,
                                             string name,
                                             Bitmap icon) :
            base(targetGroupLayerName, name, icon)
        {
            TargetChildVectorLayerName = targetChildVectorLayerName;
        }

        /// <summary>
        /// Gets the name of the target child vector layer.
        /// </summary>
        /// <value>
        /// The name of the target child vector layer.
        /// </value>
        protected virtual string TargetChildVectorLayerName { get; }

        protected override VectorLayer VectorLayer
        {
            get
            {
                GroupLayer groupedLayer = Layers.OfType<GroupLayer>().FirstOrDefault();
                return groupedLayer?.Layers.OfType<VectorLayer>().FirstOrDefault(x => x.Name == TargetChildVectorLayerName);
            }
        }
    }
}