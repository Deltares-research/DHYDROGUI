using System;
using DelftTools.Hydro;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;

namespace DeltaShell.NGHS.IO.DataObjects.Friction
{
    /// <summary>
    /// Friction definition for a <see cref="IChannel"/>.
    /// </summary>
    [Entity]
    public class ChannelFrictionDefinition : Unique<long>, IFeature
    {
        private ChannelFrictionSpecificationType specificationType;

        public ChannelFrictionDefinition(IChannel channel)
        {
            Channel = channel;

            SpecificationType = ChannelFrictionSpecificationType.ModelSettings;
        }

        public IChannel Channel { get; private set; }

        public ChannelFrictionSpecificationType SpecificationType
        {
            get => specificationType;
            set
            {
                specificationType = value;

                ConstantChannelFrictionDefinition = null;
                SpatialChannelFrictionDefinition = null;

                switch (specificationType)
                {
                    case ChannelFrictionSpecificationType.ConstantChannelFrictionDefinition:
                        ConstantChannelFrictionDefinition = new ConstantChannelFrictionDefinition();
                        break;
                    case ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition:
                        SpatialChannelFrictionDefinition = new SpatialChannelFrictionDefinition();
                        break;
                    case ChannelFrictionSpecificationType.ModelSettings:
                    case ChannelFrictionSpecificationType.RoughnessSections:
                    case ChannelFrictionSpecificationType.CrossSectionFrictionDefinitions:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public ConstantChannelFrictionDefinition ConstantChannelFrictionDefinition { get; private set; }

        public SpatialChannelFrictionDefinition SpatialChannelFrictionDefinition { get; private set; }

        # region Implementation of ICloneable

        public object Clone()
        {
            throw new NotImplementedException();
        }

        # endregion

        # region Implementation of IFeature

        public IGeometry Geometry
        {
            get => Channel.Geometry;
            set => Channel.Geometry = value;
        }

        public IFeatureAttributeCollection Attributes
        {
            get => Channel.Attributes;
            set => Channel.Attributes = value;
        }

        # endregion

        public override string ToString()
        {
            return "1D Roughness";
        }
    }
}