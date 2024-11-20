namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekRRHbvFlow
    {
        public double BaseFlowReservoirConstant { get; set; }

        public double InterflowReservoirConstant { get; set; }

        public double QuickFlowReservoirConstant { get; set; }

        public double UpperZoneThreshold { get; set; }

        public double MaximumPercolation { get; set; }

        public string Id { get; set; }
    }
}
