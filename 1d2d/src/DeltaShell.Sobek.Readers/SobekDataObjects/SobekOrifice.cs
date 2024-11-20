namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekOrifice : ISobekStructureDefinition
    {
        /// <summary>
        /// Cl
        /// </summary>
        public double CrestLevel { get; set; }
        /// <summary>
        /// Cw
        /// </summary>
        public double CrestWidth { get; set; }
        /// <summary>
        /// Gh
        /// </summary>
        public double GateHeight { get; set; }
        /// <summary>
        /// Mu
        /// </summary>
        public double ContractionCoefficient { get; set; }

        /// <summary>
        /// Sc
        /// </summary>
        public double LateralContractionCoefficient { get; set; }
        
        /// <summary>
        /// Rt
        /// </summary>
        public int FlowDirection { get; set; }

        /// <summary>
        /// Mp. When null no maximum to be used
        /// </summary>
        public double MaximumFlowPos { get; set; }

        /// <summary>
        /// Mn. When null no maximum to be used
        /// </summary>
        public double MaximumFlowNeg { get; set; }

        /// <summary>
        /// Mp. When null no maximum to be used
        /// </summary>
        public bool UseMaximumFlowPos { get; set; }

        /// <summary>
        /// Mn. When null no maximum to be used
        /// </summary>
        public bool UseMaximumFlowNeg { get; set; }
    }
}