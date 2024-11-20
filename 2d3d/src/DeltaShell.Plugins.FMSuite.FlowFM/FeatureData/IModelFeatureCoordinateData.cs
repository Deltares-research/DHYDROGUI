using System;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData
{
    public interface IModelFeatureCoordinateData : IDisposable
    {
        IFeature Feature { get; set; }

        IEventedList<IDataColumn> DataColumns { get; }
    }
}