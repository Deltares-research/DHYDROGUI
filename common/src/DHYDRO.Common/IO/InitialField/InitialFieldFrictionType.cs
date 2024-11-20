namespace DHYDRO.Common.IO.InitialField
{
    /// <summary>
    /// Type of friction for friction coefficient initial fields.
    /// </summary>
    public enum InitialFieldFrictionType
    {
        /// <summary>
        /// Represents the Chezy friction type.
        /// </summary>
        Chezy,

        /// <summary>
        /// Represents the Manning friction type.
        /// </summary>
        Manning,

        /// <summary>
        /// Represents the Wall Law Nikuradse friction type.
        /// </summary>
        WallLawNikuradse,

        /// <summary>
        /// Represents the White-Colebrook friction type.
        /// </summary>
        WhiteColebrook,
    }
}