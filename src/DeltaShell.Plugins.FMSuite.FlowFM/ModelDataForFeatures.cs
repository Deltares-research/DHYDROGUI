using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class ModelDataForFeatures
    {
        private IList<ModelFeatureCoordinateData> modelFeatureCoordinateDatas = new List<ModelFeatureCoordinateData>();
        public void Attach(ModelFeatureCoordinateData modelFeatureCoordinateData)
        {
            modelFeatureCoordinateDatas.Add(modelFeatureCoordinateData);
        }

        public void Detach(IModelDataColumnsFeature feature)
        {
            modelFeatureCoordinateDatas.RemoveAllWhere(mfcd =>
            {
                if (mfcd.Feature != null && mfcd.Feature.Equals(feature))
                {
                    mfcd.Dispose();
                    return true;
                }

                return false;
            });
        }

        public void UpdateDataColums(IModelDataColumnsFeature modelDataColumnsFeature, object selector)
        {
            foreach (ModelFeatureCoordinateData modelFeatureCoordinateData in modelFeatureCoordinateDatas)
            {
                modelFeatureCoordinateData.Selector = selector;
            }
        }
    }
}