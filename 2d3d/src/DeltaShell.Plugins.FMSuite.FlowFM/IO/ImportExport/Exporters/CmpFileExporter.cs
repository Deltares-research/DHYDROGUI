using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters
{
    public class CmpFileExporter : BoundaryDataExporterBase, IFileExporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CmpFileExporter));

        public override IEnumerable<BoundaryConditionDataType> ForcingTypes
        {
            get
            {
                yield return BoundaryConditionDataType.AstroComponents;
                yield return BoundaryConditionDataType.AstroCorrection;
                yield return BoundaryConditionDataType.Harmonics;
                yield return BoundaryConditionDataType.HarmonicCorrection;
            }
        }

        private IEnumerable<HarmonicComponent> CreateHarmonicSeries(IBoundaryCondition boundaryCondition,
                                                                    int selectedIndex)
        {
            if (boundaryCondition == null)
            {
                return Enumerable.Empty<HarmonicComponent>();
            }

            IFunction data = boundaryCondition.GetDataAtPoint(selectedIndex);

            if (data == null || !ForcingTypes.Contains(boundaryCondition.DataType))
            {
                return Enumerable.Empty<HarmonicComponent>();
            }

            return ExtForceFileHelper.ToHarmonicComponents(data);
        }

        #region IFileExporter

        [ExcludeFromCodeCoverage]
        public string Name => "Boundary data to .cmp file";

        [ExcludeFromCodeCoverage]
        public string Category => "General";

        public string Description => string.Empty;

        public bool Export(object item, string path)
        {
            var boundaryCondition = item as IBoundaryCondition;
            if (boundaryCondition != null && ForcingTypes.Contains(boundaryCondition.DataType))
            {
                var writer = new CmpFile();
                IEnumerable<HarmonicComponent> data = CreateHarmonicSeries(boundaryCondition, SelectedIndex);
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

            return false;
        }

        public IEnumerable<Type> SourceTypes()
        {
            yield break;
        }

        public string FileFilter => "Harmonic series file|*.cmp";

        [ExcludeFromCodeCoverage]
        public Bitmap Icon => Resources.TextDocument;

        [ExcludeFromCodeCoverage]
        public bool CanExportFor(object item)
        {
            return true;
        }

        #endregion
    }
}