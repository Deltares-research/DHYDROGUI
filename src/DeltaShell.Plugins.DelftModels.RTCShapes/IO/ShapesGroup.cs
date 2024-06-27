using System.Collections.Generic;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;

namespace DeltaShell.Plugins.DelftModels.RTCShapes.IO
{
    /// <summary>
    /// Represents a group of shapes.
    /// </summary>
    public class ShapesGroup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShapesGroup"/> class.
        /// </summary>
        /// <param name="groupId">The unique identifier for the group.</param>
        /// <param name="shapes">The list of shapes in the group.</param>
        /// <exception cref="System.ArgumentException">When <paramref name="groupId"/> is <c>null</c> or empty.</exception>
        /// <exception cref="System.ArgumentNullException">When <paramref name="shapes"/> is <c>null</c>.</exception>
        public ShapesGroup(string groupId, IReadOnlyList<ShapeBase> shapes)
        {
            Ensure.NotNullOrEmpty(groupId, nameof(groupId));
            Ensure.NotNull(shapes, nameof(shapes));

            GroupId = groupId;
            Shapes = shapes;
        }

        /// <summary>
        /// Gets the unique identifier for the group.
        /// </summary>
        public string GroupId { get; }

        /// <summary>
        /// Gets the list of shapes in the group.
        /// </summary>
        public IReadOnlyList<ShapeBase> Shapes { get; }
    }
}