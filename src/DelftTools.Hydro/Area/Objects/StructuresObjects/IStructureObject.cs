using DelftTools.Utils;

namespace DelftTools.Hydro.Area.Objects.StructuresObjects
{
    /// <summary>
    /// <see cref="IStructureObject"/> defines a 2D nameable, groupable
    /// feature, which serves as the base for all structure object
    /// </summary>
    /// <seealso cref="INameable" />
    /// <seealso cref="IGroupableFeature" />
    public interface IStructureObject : INameable, IGroupableFeature { }
}