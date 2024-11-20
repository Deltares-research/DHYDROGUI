using System;
using DelftTools.Utils.Data;

namespace DelftTools.Hydro.Structures
{
    /// <summary>
    /// Formule of weirs for Sobek
    /// </summary>
    public interface IWeirFormula : IUnique<long>, ICloneable
    {
        /// <summary>
        /// Name of the weirformula
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Secondary property: Freeform/Rectangle
        /// </summary>
        bool IsRectangle { get; }
        
        /// <summary>
        /// Secondary property: Has flowdirection
        /// </summary>
        bool HasFlowDirection { get; }
    }
}