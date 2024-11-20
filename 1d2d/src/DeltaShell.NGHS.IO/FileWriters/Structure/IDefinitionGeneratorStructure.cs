using DelftTools.Hydro;
using Deltares.Infrastructure.IO.Ini;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    /// <summary>
    /// <see cref="IDefinitionGeneratorStructure"/> defines the method to generate
    /// a <see cref="IniSection"/> from a <see cref="IHydroObject"/>.
    /// </summary>
    public interface IDefinitionGeneratorStructure
    {
        /// <summary>
        /// Create a <see cref="IniSection"/> from the given <paramref name="hydroObject"/>.
        /// </summary>
        /// <param name="hydroObject">The object to generate from.</param>
        /// <returns>
        /// A <see cref="IniSection"/> corresponding with the <paramref name="hydroObject"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="hydroObject"/> is <c>null</c>.
        /// </exception>
        IniSection CreateStructureRegion(IHydroObject hydroObject);
    }
}