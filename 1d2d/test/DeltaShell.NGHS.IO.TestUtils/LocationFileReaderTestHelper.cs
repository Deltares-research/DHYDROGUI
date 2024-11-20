using DelftTools.Hydro;

namespace DeltaShell.NGHS.IO.TestUtils
{
    public static class LocationFileReaderTestHelper
    {
        #region Const TestValues

        public const string OBSERVATIONPOINT1_NAME = "ObservationPoint001";
        public const string OBSERVATIONPOINT1_LONGNAME = "ObservationPoint";
        public const double OBSERVATIONPOINT1_CHAINAGE = 50.0;

        public const string OBSERVATIONPOINT2_NAME = "ObservationPoint002";
        public const string OBSERVATIONPOINT2_LONGNAME = "";
        public const double OBSERVATIONPOINT2_CHAINAGE = 85.0;

        public const string LATERALDISCHARGE1_NAME = "LateralDischarge001";
        public const string LATERALDISCHARGE1_LONGNAME = "LateralDischarge";
        public const double LATERALDISCHARGE1_CHAINAGE = 65.0;

        public const string LATERALDISCHARGE2_NAME = "LateralDischarge002";
        public const string LATERALDISCHARGE2_LONGNAME = "";
        public const double LATERALDISCHARGE2_CHAINAGE = 90.0;

        #endregion

        public static bool CompareObservationPoints(IObservationPoint observationPoint1, IObservationPoint observationPoint2)
        {
            // Note: this comparison is not exhaustive
            var areEqual = true;

            areEqual &= observationPoint1.Name == observationPoint2.Name;
            //areEqual &= observationPoint1.LongName == observationPoint2.LongName; //we don't store obs pts longname
            areEqual &= observationPoint1.Chainage.Equals(observationPoint2.Chainage);

            areEqual &= observationPoint1.Description == observationPoint2.Description;
            areEqual &= observationPoint1.CanBeLinkSource == observationPoint2.CanBeLinkSource;
            areEqual &= observationPoint1.CanBeLinkTarget == observationPoint2.CanBeLinkTarget;
            areEqual &= observationPoint1.Attributes.Count == observationPoint2.Attributes.Count;

            areEqual &= observationPoint1.Network.Name == observationPoint2.Network.Name;    
            
            return areEqual;
        }

        public static bool CompareLateralDischargeLocations(ILateralSource lateralSource1, ILateralSource lateralSource2)
        {
            // Note: this comparison is not exhaustive
            var areEqual = true;

            areEqual &= lateralSource1.Name == lateralSource2.Name;
            areEqual &= lateralSource1.LongName == lateralSource2.LongName;
            areEqual &= lateralSource1.Chainage.Equals(lateralSource2.Chainage);

            areEqual &= lateralSource1.Description == lateralSource2.Description;
            areEqual &= lateralSource1.CanBeLinkSource == lateralSource2.CanBeLinkSource;
            areEqual &= lateralSource1.CanBeLinkTarget == lateralSource2.CanBeLinkTarget;
            areEqual &= lateralSource1.Attributes.Count == lateralSource2.Attributes.Count;

            areEqual &= lateralSource1.Network.Name == lateralSource2.Network.Name;
            areEqual &= lateralSource1.Description == lateralSource2.Description;

            areEqual &= lateralSource1.IsDiffuse == lateralSource2.IsDiffuse;

            return areEqual;
        }
    }
}
