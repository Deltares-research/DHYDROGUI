using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using DelftTools.Functions;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters
{
    public class QhFileExporter : BoundaryDataExporterBase, IFileExporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(QhFileExporter));

        public override IEnumerable<BoundaryConditionDataType> ForcingTypes
        {
            get
            {
                yield return BoundaryConditionDataType.Qh;
            }
        }

        #region IFileExporter

        public string Name => "Boundary data to .qh file";

        public string Category => "General";

        public string Description => string.Empty;

        public bool Export(object item, string path)
        {
            var boundaryCondition = item as IBoundaryCondition;
            if (boundaryCondition != null && boundaryCondition.DataType == BoundaryConditionDataType.Qh)
            {
                var writer = new QhFile();
                IFunction data = SeriesToExport(boundaryCondition);
                if (data != null)
                {
                    try
                    {
                        writer.Write(path, data);
                        return true;
                    }
                    catch (Exception e)
                    {
                        Log.ErrorFormat("Failed to export data to {0}: {1}", path, e.Message);
                        return false;
                    }
                }
            }

            return false;
        }

        public IEnumerable<Type> SourceTypes()
        {
            yield break;
        }

        public string FileFilter => "Q-h series series file|*.qh";

        [ExcludeFromCodeCoverage]
        public Bitmap Icon => Resources.TextDocument;

        public bool CanExportFor(object item)
        {
            return true;
        }

        #endregion
    }
}