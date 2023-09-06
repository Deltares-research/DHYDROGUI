using System.Collections.Generic;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.Ini;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using log4net;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess
{
    /// <summary>
    /// The <see cref="MdwFile"/> class provides reader/writer functionality for D-Waves .mdw files.
    /// </summary>
    public partial class MdwFile : NGHSFileBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MdwFile));
        
        private readonly IniMerger mdwFileMerger = new IniMerger();

        /// <summary>
        /// These mdw sections can have multiplicity greater than 1 (or gui only),
        /// excluded them from the generic property treatment..
        /// </summary>
        public static IList<string> ExcludedSections { get; } = new List<string>
        {
            KnownWaveSections.TimePointSection,
            KnownWaveSections.DomainSection,
            KnownWaveSections.BoundarySection,
            KnownWaveSections.GuiOnlySection
        };

        /// <summary>
        /// Gets or sets the location of the MDW file.
        /// </summary>
        public string MdwFilePath { get; set; }
    }
}