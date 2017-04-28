namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekWaqNumericalSettings
    {
        /// <summary>
        /// Integration method number
        /// </summary>
        public int NumericalScheme1D { get; set; }

        /// <summary>
        /// Use flows and dispersion as specified (false) or only if the flow is not zero (true)
        /// </summary>
        public bool NoDispersionIfFlowIsZero { get; set; }

        /// <summary>
        /// Use dispersion over open boundaries
        /// </summary>
        public bool NoDispersionOverOpenBoundaries { get; set; }

        /// <summary>
        /// Use first order (true) or second order (false) approximation over open boundaries
        /// </summary>
        public bool UseFirstOrder { get; set; }

        /// <summary>
        /// Mass balance output level number
        /// </summary>
        public int BalanceOutputLevel { get; set; }
    }
}
