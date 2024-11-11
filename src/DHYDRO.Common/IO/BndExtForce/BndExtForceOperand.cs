using System.ComponentModel;

namespace DHYDRO.Common.IO.BndExtForce
{
    /// <summary>
    /// Type of external forcing operand describing how the data is combined with existing data for the same quantity.
    /// </summary>
    public enum BndExtForceOperand
    {
        /// <summary>
        /// No operand defined
        /// </summary>
        [Description("")]
        None,

        /// <summary>
        /// Overwrites any previous data.
        /// </summary>
        [Description("O")]
        Overwrite,

        /// <summary>
        /// Appends where data is still missing.
        /// </summary>
        [Description("A")]
        Append,

        /// <summary>
        /// Adds the provided values to the existing values.
        /// </summary>
        [Description("+")]
        Add,

        /// <summary>
        /// Multiplies the existing values by the provided values.
        /// </summary>
        [Description("*")]
        Multiply,

        /// <summary>
        /// Takes the maximum of the existing values and the provided values.
        /// </summary>
        [Description("X")]
        Maximum,

        /// <summary>
        /// Takes the minimum of the existing values and the provided values.
        /// </summary>
        [Description("N")]
        Minimum
    }
}