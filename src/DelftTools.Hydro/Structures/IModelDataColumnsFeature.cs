using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;

namespace DelftTools.Hydro.Structures
{
    public interface IModelDataColumnsFeature

    {
        void UpdateDataColumns(ModelFeatureCoordinateData modelDataForFeatureWithDataColumns);
        IEventedList<IDataColumn> GenerateDataColumns(ModelFeatureCoordinateData modelDataForFeatureWithDataColumns);

        IGeometry Geometry { get; set; }
    }
}