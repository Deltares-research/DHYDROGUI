using System;
using DelftTools.Hydro;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain
{
    [Entity]
    public class RunoffBoundaryData : Unique<long>, ICloneable
    {
        //nhib
        protected RunoffBoundaryData(){ }

        [Aggregation]
        public RunoffBoundary Boundary { get; set; }

        public RainfallRunoffBoundaryData Series { get; set; }

        public RunoffBoundaryData(RunoffBoundary runoffBoundary)
        {
            Boundary = runoffBoundary;
            Series = new RainfallRunoffBoundaryData();
        }

        public object Clone()
        {
            return new RunoffBoundaryData
                {
                    Boundary = Boundary,
                    Series = Series != null ? (RainfallRunoffBoundaryData) Series.Clone() : null
                };
        }
    }
}