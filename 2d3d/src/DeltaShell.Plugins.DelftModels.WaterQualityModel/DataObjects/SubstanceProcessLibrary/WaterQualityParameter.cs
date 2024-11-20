using System;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using DelftTools.Utils.Reflection;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary
{
    /// <summary>
    /// Parameter
    /// </summary>
    [Entity(FireOnCollectionChange = false)]
    public class WaterQualityParameter : Unique<long>, INameable, ICloneable
    {
        /// <summary>
        /// The description of the parameter
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The unit of the parameter
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// The default value of the parameter
        /// </summary>
        public double DefaultValue { get; set; }

        /// <summary>
        /// The name of the parameter
        /// </summary>
        public string Name { get; set; }

        public object Clone()
        {
            return TypeUtils.MemberwiseClone(this);
        }
    }
}