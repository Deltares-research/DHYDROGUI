using System;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Roughness;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;

namespace DeltaShell.NGHS.IO.DataObjects.Friction
{
    /// <summary>
    /// Spatial friction definition for a <see cref="IChannel"/>.
    /// </summary>
    [Entity]
    public class SpatialChannelFrictionDefinition
    {
        private RoughnessFunction functionType;

        public SpatialChannelFrictionDefinition()
        {
            FunctionType = RoughnessFunction.Constant;
        }

        public RoughnessType Type { get; set; }

        public RoughnessFunction FunctionType
        {
            get => functionType;
            set
            {
                functionType = value;

                Function = null;
                ConstantSpatialChannelFrictionDefinitions = null;

                switch (functionType)
                {
                    case RoughnessFunction.Constant:
                        ConstantSpatialChannelFrictionDefinitions = new EventedList<ConstantSpatialChannelFrictionDefinition>();
                        break;
                    case RoughnessFunction.FunctionOfQ:
                        Function = RoughnessSection.DefineFunctionOfQ();
                        break;
                    case RoughnessFunction.FunctionOfH:
                        Function = RoughnessSection.DefineFunctionOfH();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public IEventedList<ConstantSpatialChannelFrictionDefinition> ConstantSpatialChannelFrictionDefinitions { get; private set; }

        public IFunction Function { get; private set; }
    }
}
