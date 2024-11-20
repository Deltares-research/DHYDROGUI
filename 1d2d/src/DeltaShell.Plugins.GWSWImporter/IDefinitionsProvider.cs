using DelftTools.Utils.Collections.Generic;
using Deltares.Infrastructure.API.Logging;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    /// <summary>
    /// Interface for defining providers of collection of <see cref="GwswAttributeType"/>.
    /// </summary>
    public interface IDefinitionsProvider
    {
        /// <summary>
        /// Gets the definitions based on a gwsw directory.
        /// </summary>
        /// <param name="gwswDirectory">The GWSW directory.</param>
        /// <returns>A collection of <see cref="GwswAttributeType"/>.</returns>
        IEventedList<GwswAttributeType> GetDefinitions(string gwswDirectory);

        ILogHandler LogHandler { get; set; }
    }
}