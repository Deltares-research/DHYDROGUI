namespace DHYDRO.Common.IO.BndExtForce
{
    /// <summary>
    /// Represents a meteo definition in the new style external forcings file (*_bnd.ext).
    /// </summary>
    public sealed class BndExtForceMeteoData
    {
        /// <summary>
        /// Gets or sets the line number where the meteo data is located.
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Gets or sets the quantity name of the meteo data.
        /// </summary>
        public string Quantity { get; set; }

        /// <summary>
        /// Gets or sets the name of the forcing file for the meteo data.
        /// </summary>
        /// <remarks>
        /// The type of forcing file is specified by <see cref="ForcingFileType"/>.
        /// </remarks>
        public string ForcingFile { get; set; }

        /// <summary>
        /// Gets or sets the type of the meteo forcing file.
        /// </summary>
        public BndExtForceDataFileType ForcingFileType { get; set; }

        /// <summary>
        /// Gets or sets the name of the polygon file (*.pol) to use as a mask.
        /// </summary>
        /// <remarks>
        /// Grid parts inside any polygon will receive the meteo forcing.
        /// This is an optional value.
        /// </remarks>
        public string TargetMaskFile { get; set; }

        /// <summary>
        /// Gets or sets whether the target mask should be inverted.
        /// </summary>
        /// <remarks>
        /// This is an optional value.
        /// </remarks>
        public bool TargetMaskInvert { get; set; }

        /// <summary>
        /// Gets or sets the type of interpolation method.
        /// </summary>
        public BndExtForceInterpolationMethod InterpolationMethod { get; set; }

        /// <summary>
        /// Gets or sets the type of operand.
        /// </summary>
        /// <remarks>
        /// Specifies how the meteo data is combined with existing data of the same quantity.
        /// </remarks>
        public BndExtForceOperand Operand { get; set; }
    }
}