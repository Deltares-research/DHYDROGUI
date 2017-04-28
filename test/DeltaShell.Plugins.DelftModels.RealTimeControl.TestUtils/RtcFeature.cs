using System;
using DelftTools.Utils;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils
{
    public class RtcFeature : BranchFeature, IFeature, INameable
    {
        public object Clone()
        {
            throw new NotImplementedException();
        }

        public IGeometry Geometry { get; set; }
        public IFeatureAttributeCollection Attributes { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}