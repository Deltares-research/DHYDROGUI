using System;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain
{
    [Entity(FireOnCollectionChange = false)]
    public class RtcTestFeature : Unique<long>, IFeature, INameable
    {
        public double Value { get; set; }

        public IGeometry Geometry { get; set; }

        public IFeatureAttributeCollection Attributes { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }
    }
}