using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Data;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;

namespace DeltaShell.NGHS.IO.DataObjects.Friction
{
    /// <summary>
    /// Friction definition for a <see cref="IPipe"/>.
    /// </summary>
    public class PipeFrictionDefinition : Unique<long>, IFeature
    {
        public PipeFrictionDefinition(IPipe pipe)
        {
            Pipe = pipe;
        }

        public IPipe Pipe { get; private set; }

        #region Implementation of ICloneable

        public object Clone()
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region Implementation of IFeature

        public IGeometry Geometry
        {
            get => Pipe.Geometry;
            set => Pipe.Geometry = value;
        }

        public IFeatureAttributeCollection Attributes
        {
            get => Pipe.Attributes;
            set => Pipe.Attributes = value;
        }

        #endregion

        public override string ToString()
        {
            return "1D Roughness";
        }
    }
}