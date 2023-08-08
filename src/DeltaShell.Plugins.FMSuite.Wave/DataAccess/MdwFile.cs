using System.Collections.Generic;
using DeltaShell.NGHS.IO;
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

        /// <summary>
        /// These mdw categories can have multiplicity greater than 1 (or gui only),
        /// excluded them from the generic property treatment..
        /// </summary>
        public static IList<string> ExcludedCategories { get; } = new List<string>
        {
            KnownWaveCategories.TimePointCategory,
            KnownWaveCategories.DomainCategory,
            KnownWaveCategories.BoundaryCategory,
            KnownWaveCategories.GuiOnlyCategory
        };

        /// <summary>
        /// Gets or sets the location of the MDW file.
        /// </summary>
        public string MdwFilePath { get; set; }
    }
}