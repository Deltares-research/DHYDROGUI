using System;
using System.Collections.Generic;
using DelftTools.Functions;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters
{
    public abstract class BoundaryDataExporterBase
    {
        public abstract IEnumerable<BoundaryConditionDataType> ForcingTypes { get; }

        public int SelectedIndex { get; set; }

        public DateTime ModelReferenceDate { get; set; }

        protected IFunction SeriesToExport(IBoundaryCondition boundaryCondition)
        {
            return boundaryCondition.GetDataAtPoint(SelectedIndex);
        }
    }
}
