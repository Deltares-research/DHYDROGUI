using System;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using DelftTools.Utils.Reflection;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary
{
    /// <summary>
    /// Substance
    /// </summary>
    [Serializable]
    [Entity(FireOnCollectionChange = false)]
    public class WaterQualitySubstance : Unique<long>, INameable, ICloneable, IComparable
    {
        /// <summary>
        /// The description of the substance
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Whether or not the substance is active
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// The initial value of the substance
        /// </summary>
        public double InitialValue { get; set; }

        /// <summary>
        /// The concentration unit of the substance
        /// </summary>
        public string ConcentrationUnit { get; set; }

        /// <summary>
        /// The waste load unit of the substance
        /// </summary>
        public string WasteLoadUnit { get; set; }

        /// <summary>
        /// The name of the substance
        /// </summary>
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public object Clone()
        {
            return TypeUtils.MemberwiseClone(this);
        }

        public int CompareTo(object obj)
        {
            return Name.CompareTo(obj.ToString());
        }
    }
}