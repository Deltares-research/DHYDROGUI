using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters
{
    public class CmpFileExporter: BoundaryDataExporterBase, IFileExporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CmpFileExporter));

        #region IFileExporter

        [ExcludeFromCodeCoverage]
        public string Name { get { return "Boundary data to .cmp file"; } }
        public string Description { get { return Name; } }

        [ExcludeFromCodeCoverage]
        public string Category { get { return "General"; } }

        public bool Export(object item, string path)
        {
            var boundaryCondition = item as IBoundaryCondition;
            if (boundaryCondition != null && ForcingTypes.Contains(boundaryCondition.DataType))
            {
                var writer = new CmpFile();
                var data = CreateHarmonicSeries(boundaryCondition, SelectedIndex);
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
        
        public string FileFilter
        {
            get { return "Harmonic series file|*.cmp"; }
        }

        [ExcludeFromCodeCoverage]
        public Bitmap Icon { get { return Properties.Resources.TextDocument; } }

        [ExcludeFromCodeCoverage]
        public bool CanExportFor(object item)
        {
            return true;
        }

        #endregion

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
            if (boundaryCondition == null) return Enumerable.Empty<HarmonicComponent>();

            var data = boundaryCondition.GetDataAtPoint(selectedIndex);

            if (data == null || !ForcingTypes.Contains(boundaryCondition.DataType))
            {
                return Enumerable.Empty<HarmonicComponent>();
            }

            return ExtForceFileHelper.ToHarmonicComponents(data);
        }
    }
}
