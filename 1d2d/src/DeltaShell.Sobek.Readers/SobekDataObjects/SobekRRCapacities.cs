namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekRRCapacities
    {
        public string Id { get; set; }

        public double UpperZoneTensionWaterStorageCapacity { get; set; }
       
        public double UpperZoneTensionWaterInitialContent { get; set; }

        public double UpperZoneFreeWaterStorageCapacity { get; set; }

        public double UpperZoneFreeWaterInitialContent { get; set; }

        public double UpperZoneFreeWaterDrainageRate { get; set; }

        public double LowerZoneTensionWaterStorageCapacity { get; set; }

        public double LowerZoneTensionWaterInitialContent { get; set; }

        public double LowerZoneSupplementalFreeWaterStorageCapacity { get; set; }

        public double LowerZoneSupplementalFreeWaterInitialContent { get; set; }

        public double LowerZoneSupplementalFreeWaterDrainageRate { get; set; }

        public double LowerZonePrimaryFreeWaterStorageCapacity { get; set; }

        public double LowerZonePrimaryFreeWaterInitialContent { get; set; }

        public double LowerZonePrimaryFreeWaterDrainageRate { get; set; }
    }
}
