namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekRiverAdvancedWeir : ISobekStructureDefinition
    {
        /// <summary>
        /// Cl
        /// </summary>
        public float CrestLevel { get; set; }

        /// <summary>
        /// Si
        /// </summary>
        public float SillWidth { get; set; }

        /// <summary>
        /// Ni
        /// </summary>
        public int NumberOfPiers { get; set; }

        /// <summary>
        /// Ph
        /// </summary>
        public float PositiveUpstreamFaceHeight { get; set; }

        /// <summary>
        /// Nh
        /// </summary>
        public float NegativeUpstreamHeight { get; set; }

        /// <summary>
        /// Pw
        /// </summary>
        public float PositiveWeirDesignHead { get; set; }

        /// <summary>
        /// Nh
        /// </summary>
        public float NegativeWeirDesignHead { get; set; }

        /// <summary>
        /// Pp
        /// </summary>
        public float PositivePierContractionCoefficient { get; set; }

        /// <summary>
        /// Np
        /// </summary>
        public float NegativePierContractionCoefficient { get; set; }

        /// <summary>
        /// Pa
        /// </summary>
        public float PositiveAbutmentContractionCoefficient { get; set; }

        /// <summary>
        /// Na
        /// </summary>
        public float NegativeAbutmentContractionCoefficient { get; set; }
    }
}