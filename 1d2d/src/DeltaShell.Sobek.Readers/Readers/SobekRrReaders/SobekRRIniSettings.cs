using System;

namespace DeltaShell.Sobek.Readers.Readers.SobekRrReaders
{
    public struct SobekRRIniSettings
    {
        // General settings
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan TimestepSize { get; set; }
        public bool PeriodFromEvent { get; set; }
        public double OutputTimestepMultiplier { get; set; }
        public int UnsaturatedZone { get; set; }
        public short GreenhouseYear { get; set; }
        public int InitCapsimOption { get; set; }
        public bool CapsimPerCropAreaIsDefined { get; set; }
        public int CapsimPerCropArea { get; set; }

        // Output settings
        public int AggregationOptions { get; set; }
        public bool OutputRRPaved { get; set; }
        public bool OutputRRUnpaved { get; set; }
        public bool OutputRRGreenhouse { get; set; }
        public bool OutputRROpenWater { get; set; }
        public bool OutputRRStructure { get; set; }
        public bool OutputRRBoundary { get; set; }
        public bool OutputRRNWRW { get; set; }
        public bool OutputRRWWTP { get; set; }
        public bool OutputRRIndustry { get; set; }
        public bool OutputRRSacramento { get; set; }
        public bool OutputRRRunoff { get; set; }
        public bool OutputRRLinkFlows { get; set; }
        public bool OutputRRBalance { get; set; }
    }
}