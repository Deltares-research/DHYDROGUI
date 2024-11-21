namespace DHYDRO.Common.IO.BndExtForce
{
    /// <summary>
    /// Defines the type of discharge value.
    /// </summary>
    public enum BndExtForceDischargeType
    {
        /// <summary>
        /// The discharge is used as a time-constant value.
        /// </summary>
        TimeConstant,

        /// <summary>
        /// The discharge is used as a time-varying value.
        /// </summary>
        TimeVarying,

        /// <summary>
        /// The value is driven by some external force.
        /// </summary>
        External
    }
}