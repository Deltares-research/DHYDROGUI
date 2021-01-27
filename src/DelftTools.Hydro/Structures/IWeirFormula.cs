using System;
using DelftTools.Utils.Data;

namespace DelftTools.Hydro.Structures
{
    /// <summary>
    /// <see cref="IWeirFormula"/> defines the interface of structure formulas.
    /// </summary>
    public interface IWeirFormula : IUnique<long>, ICloneable
    {
        /// <summary>
        /// Gets the name of the <see cref="IWeirFormula"/>.
        /// </summary>
        string Name { get; }
    }
}