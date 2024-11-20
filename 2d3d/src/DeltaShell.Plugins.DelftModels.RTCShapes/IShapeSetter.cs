using System.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;

namespace DeltaShell.Plugins.DelftModels.RTCShapes
{
    /// <summary>
    /// Specifies an interface for setting RTC shape objects.
    /// </summary>
    public interface IShapeSetter
    {
        /// <summary>
        /// Sets the shapes for the specified control group.
        /// </summary>
        /// <param name="controlGroup">The control group for which to set the shapes.</param>
        /// <param name="shapes">The collection of shape objects to set.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="controlGroup"/> or <paramref name="shapes"/> is <c>null</c>.
        /// </exception>
        void SetShapes(ControlGroup controlGroup, IEnumerable<ShapeBase> shapes);
    }
}