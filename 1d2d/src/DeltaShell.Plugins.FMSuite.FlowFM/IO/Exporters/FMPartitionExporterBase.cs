using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters
{
    /// <summary>
    /// Provides an abstract base class for a D-FlowFM partition file exporters.
    /// </summary>
    public abstract class FMPartitionExporterBase : IFileExporter
    {
        /// <inheritdoc/>
        public string Name => "Partition exporter";

        /// <inheritdoc/>
        public string Category => "General";

        /// <inheritdoc/>
        public string Description => Name;

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public Bitmap Icon => Resources.unstruc;

        /// <inheritdoc/>
        public abstract string FileFilter { get; }

        public int NumDomains { protected get; set; }

        public string PolygonFile { protected get; set; }

        public bool IsContiguous { protected get; set; }

        protected string TargetNetFilePath { get; set; }

        protected string SourceNetFilePath { get; set; }

        /// <inheritdoc/>
        public abstract bool Export(object item, string path);

        /// <inheritdoc/>
        public abstract IEnumerable<Type> SourceTypes();

        /// <inheritdoc/>
        public bool CanExportFor(object item) => true;

        protected void WriteNetPartition(IFlexibleMeshModelApi api)
        {
            if (PolygonFile != null)
            {
                api.WritePartitioning(SourceNetFilePath, TargetNetFilePath, PolygonFile);
            }
            else
            {
                api.WritePartitioning(SourceNetFilePath, TargetNetFilePath, NumDomains, IsContiguous);
            }
        }
    }
}