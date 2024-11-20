using System;
using DelftTools.Hydro;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;

namespace DeltaShell.NGHS.IO.DataObjects.InitialConditions
{
    /// <summary>
    /// Initial Condition definition for a <see cref="IChannel"/>.
    /// </summary>
    [Entity]
    public class ChannelInitialConditionDefinition : Unique<long>, IFeature
    {
        private ChannelInitialConditionSpecificationType specificationType;

        public ChannelInitialConditionDefinition(IChannel channel)
        {
            Channel = channel;
            SpecificationType = ChannelInitialConditionSpecificationType.ModelSettings;
        }

        private ChannelInitialConditionDefinition()
        {
        }

        public IChannel Channel { get; private set; }

        public ChannelInitialConditionSpecificationType SpecificationType
        {
            get => specificationType;
            set
            {
                specificationType = value;
                ConstantChannelInitialConditionDefinition = null;
                SpatialChannelInitialConditionDefinition = null;

                switch (specificationType)
                {
                    case ChannelInitialConditionSpecificationType.ConstantChannelInitialConditionDefinition:
                        ConstantChannelInitialConditionDefinition = new ConstantChannelInitialConditionDefinition();
                        break;
                    case ChannelInitialConditionSpecificationType.SpatialChannelInitialConditionDefinition:
                        SpatialChannelInitialConditionDefinition = new SpatialChannelInitialConditionDefinition();
                        break;
                    case ChannelInitialConditionSpecificationType.ModelSettings:
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        public ConstantChannelInitialConditionDefinition ConstantChannelInitialConditionDefinition { get; private set; }
        public SpatialChannelInitialConditionDefinition SpatialChannelInitialConditionDefinition { get; private set; }

        #region Implementation of ICloneable

        public object Clone()
        {
            var clone = new ChannelInitialConditionDefinition();
            clone.Channel = Channel;
            clone.SpecificationType = specificationType;
            clone.ConstantChannelInitialConditionDefinition = ConstantChannelInitialConditionDefinition;
            clone.SpatialChannelInitialConditionDefinition = SpatialChannelInitialConditionDefinition;
            return clone;
        }
        # endregion

        #region Implementation of IFeature

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

        #endregion

        /// <summary>
        /// Copies the properties from another definition to this definition.
        /// </summary>
        /// <param name="otherDefinition">The definition to copy from.</param>
        public void CopyFrom(ChannelInitialConditionDefinition otherDefinition)
        {
            Channel = otherDefinition.Channel;
            SpecificationType = otherDefinition.SpecificationType;
            Attributes = otherDefinition.Attributes;
            ConstantChannelInitialConditionDefinition?.CopyFrom(otherDefinition.ConstantChannelInitialConditionDefinition);
            SpatialChannelInitialConditionDefinition?.CopyFrom(otherDefinition.SpatialChannelInitialConditionDefinition);
        }

        public override string ToString()
        {
            return "1D Initial Conditions";
        }

        
    }
}
