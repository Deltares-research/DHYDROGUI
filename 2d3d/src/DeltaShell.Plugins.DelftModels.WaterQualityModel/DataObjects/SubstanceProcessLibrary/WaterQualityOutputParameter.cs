using System;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using DelftTools.Utils.Reflection;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary
{
    /// <summary>
    /// Output parameter
    /// </summary>
    [Entity(FireOnCollectionChange = false)]
    public class WaterQualityOutputParameter : Unique<long>, INameable, ICloneable
    {
        /// <summary>
        /// The description of the output parameter
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Whether or not the output parameter should be shown in the HIS output
        /// </summary>
        public bool ShowInHis { get; set; }

        /// <summary>
        /// Whether or not the output parameter should be shown in the MAP output
        /// </summary>
        public bool ShowInMap { get; set; }

        /// <summary>
        /// THe name of the output parameter
        /// </summary>
        public string Name { get; set; }

        public object Clone()
        {
            return TypeUtils.MemberwiseClone(this);
        }
    }
}