using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Feature;

namespace DelftTools.Hydro.Structures
{
    public interface IModelFeatureCoordinateData
    {
        IModelDataColumnsFeature Feature { get; set; }
        IEventedList<IDataColumn> DataColumns { get; }

        object Selector { get; set; }
    }
}
