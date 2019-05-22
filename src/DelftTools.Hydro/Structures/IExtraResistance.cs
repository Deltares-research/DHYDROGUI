using System.ComponentModel;
using DelftTools.Functions;

namespace DelftTools.Hydro.Structures
{
    public enum ExtraResistanceType
    {
        [Description("Ksi in delta")]
        KsiInDelta,
        [Description("Eta in delta")]
        EtaInDelta
    }

    ///<summary>
    ///</summary>
    public interface IExtraResistance : IStructure1D
    {
        ///<summary>
        /// Used formulate to calculate the extra friction
        ///</summary>
        ExtraResistanceType ExtraResistanceType { get; set; }

        /// <summary>
        /// friction table
        /// Water level and extra resistance
        /// </summary>
        IFunction FrictionTable { get; set; }
    }
}
