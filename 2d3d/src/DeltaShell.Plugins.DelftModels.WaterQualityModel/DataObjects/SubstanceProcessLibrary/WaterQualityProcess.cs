using System;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using DelftTools.Utils.Reflection;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary
{
    /// <summary>
    /// Process
    /// </summary>
    [Entity(FireOnCollectionChange = false)]
    public class WaterQualityProcess : Unique<long>, INameable, ICloneable
    {
        /// <summary>
        /// The description of the process
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The name of the process
        /// </summary>
        public string Name { get; set; }

        public object Clone()
        {
            return TypeUtils.MemberwiseClone(this);
        }
    }
}