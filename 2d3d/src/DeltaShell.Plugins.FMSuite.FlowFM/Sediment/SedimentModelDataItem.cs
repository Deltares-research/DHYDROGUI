using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Extensions.Coverages;
using SharpMap.Api.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Sediment
{
    public class SedimentModelDataItem
    {
        internal List<IGrouping<Type, UnstructuredGridCoverage>> Coverages { get; set; }
        internal Dictionary<object, string> DataItemNameLookup { get; set; }
        internal Dictionary<string, IList<ISpatialOperation>> SpatialOperation { get; set; }
        internal List<string> SpacialVariableNames { get; set; }
    }
}