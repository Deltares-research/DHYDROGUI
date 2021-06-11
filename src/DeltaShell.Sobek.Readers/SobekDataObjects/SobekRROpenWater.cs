namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekRROpenWater : ISobekCatchment
    {
        public SobekRROpenWater()
        {
            AreaAjustmentFactor = 1.0;
        }

        public string Id { get; set; }

        public RationalMethodType MethodType { get; set; }

        /// <summary>
        /// mm/s
        /// </summary>
        public double ConstantIntensity { get; set; }

        public string MeteoStationId { get; set; }

        /// <summary>
        /// mm/s
        /// </summary>
        public double InfiltrationItensity { get; set; }

        /// <summary>
        /// m2
        /// </summary>
        public double Area { get; set; }

        public double AreaAjustmentFactor { get; set; }

    }

    public enum RationalMethodType
    {
        None = 0, //No rational method -> read openwater file with new tag OWRR for D-RR -> will be used for testing!
        ConstantIntensity = 6,
        RainfallStation = 7
    }
}