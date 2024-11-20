using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters
{
    public class QhFileExporter: BoundaryDataExporterBase, IFileExporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(QhFileExporter));

        #region IFileExporter

        public string Name { get { return "Boundary data to .qh file"; } }
        public string Description { get { return Name; } }
        public string Category { get { return "General"; } }

        public bool Export(object item, string path)
        {
            var boundaryCondition = item as IBoundaryCondition;
            if (boundaryCondition != null && boundaryCondition.DataType == BoundaryConditionDataType.Qh)
            {
                var writer = new QhFile();
                var data = SeriesToExport(boundaryCondition);
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

        public string FileFilter
        {
            get { return "Q-h series series file|*.qh"; }
        }

        [ExcludeFromCodeCoverage]
        public Bitmap Icon { get { return Properties.Resources.TextDocument; } }
        public bool CanExportFor(object item)
        {
            return true;
        }

        #endregion

        public override IEnumerable<BoundaryConditionDataType> ForcingTypes
        {
            get { yield return BoundaryConditionDataType.Qh; }
        }
    }
}
