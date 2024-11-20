namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekRRHbvSnow
    {
        public double SnowFallTemperature { get; set; }

        public double SnowMeltTemperature { get; set; }
       
        public double SnowMeltingConstant { get; set; }
        
        public double TemperatureAltitudeConstant { get; set; }
        
        public double FreezingEfficiency { get; set; }
        
        public double FreeWaterFraction { get; set; }

        public string Id { get; set; }
    }
}
