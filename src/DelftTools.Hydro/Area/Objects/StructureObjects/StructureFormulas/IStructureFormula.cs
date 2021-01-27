using System;
using DelftTools.Utils.Data;

namespace DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas
{
    /// <summary>
    /// <see cref="IStructureFormula"/> defines the interface of structure formulas.
    /// </summary>
    public interface IStructureFormula : IUnique<long>, ICloneable
    {
        /// <summary>
        /// Gets the name of the <see cref="IStructureFormula"/>.
        /// </summary>
        string Name { get; }
    }
}