using DelftTools.Hydro;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.Structures.Parsers
{
    /// <summary>
    /// Interface for structure parsers.
    /// </summary>
    public interface IStructureParser
    {
        /// <summary>
        /// Parses a structure.
        /// </summary>
        /// <returns>The parsed structure.</returns>
        IStructure1D ParseStructure();
    }
}