using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public abstract class BoundaryDataImporterBase
    {
        public abstract IEnumerable<BoundaryConditionDataType> ForcingTypes { get; }

        public IList<int> DataPointIndices { get; set; }

        public DateTime? ModelReferenceDate { get; set; }

        protected IEnumerable<IFunction> SeriesToFill(IBoundaryCondition boundaryCondition)
        {
            var maxIndex = boundaryCondition.Feature.Geometry.Coordinates.Count();

            foreach (var dataPointIndex in DataPointIndices.Where(i => i < maxIndex))
            {
                if (!boundaryCondition.DataPointIndices.Contains(dataPointIndex))
                {
                    boundaryCondition.AddPoint(dataPointIndex);
                }

                yield return boundaryCondition.GetDataAtPoint(dataPointIndex);
            }
        }

        public abstract void Import(string fileName, FlowBoundaryCondition boundaryCondition);
        public abstract bool CanImportOnBoundaryCondition(FlowBoundaryCondition boundaryCondition);
        public abstract string FileFilter { get; }
    }
}
