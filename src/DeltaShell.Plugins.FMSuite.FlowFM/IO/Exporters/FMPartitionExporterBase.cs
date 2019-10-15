using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters
{
    public abstract class FMPartitionExporterBase : IFileExporter
    {
        public int NumDomains { protected get; set; }

        public string PolygonFile { protected get; set; }

        public bool IsContiguous { protected get; set; }

        public string FilePath { get; set; }

        protected string TargetNetFilePath { get; set; }

        protected string SourceNetFilePath { get; set; }

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

        #region IFileExporter

        public string Name
        {
            get { return "Partition exporter"; }
        }
        public string Description { get { return Name; } }
        public abstract bool Export(object item, string path);

        public string Category
        {
            get { return "General"; }
        }

        public abstract IEnumerable<Type> SourceTypes();

        public abstract string FileFilter { get; }

        [ExcludeFromCodeCoverage]
        public Bitmap Icon
        {
            get { return Properties.Resources.unstruc; }
        }

        public bool CanExportFor(object item)
        {
            return true;
        }

        #endregion
    }
}
