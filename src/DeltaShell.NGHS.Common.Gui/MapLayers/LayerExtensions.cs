using DelftTools.Utils.Guards;
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
    }
}