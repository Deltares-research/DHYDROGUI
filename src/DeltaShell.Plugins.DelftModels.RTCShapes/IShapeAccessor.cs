using System.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;

namespace DeltaShell.Plugins.DelftModels.RTCShapes
{
    /// <summary>
    /// Specifies an interface for retrieving RTC shape objects.
    /// </summary>
    public interface IShapeAccessor
    {
        /// <summary>
        /// Retrieves the shapes that belong to the specified control group.
        /// </summary>
        /// <param name="controlGroup">The control group for which to retreive the shapes.</param>
        /// <returns>A collection of shape objects.</returns>
        /// <exception cref="System.ArgumentNullException">When <paramref name="controlGroup"/> is <c>null</c>.</exception>
        IEnumerable<ShapeBase> GetShapes(ControlGroup controlGroup);
    }
}