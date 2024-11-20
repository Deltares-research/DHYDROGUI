namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekCrossSectionMapping
    {
        private string definitionId;
        private double downstreamSlope;
        private string locationId;
        private double refLevel1;
        private double refLevel2;
        private double surfaceLevelLeft;
        private double surfaceLevelRight;
        private double upstreamSlope;

        public string LocationId
        {
            get { return locationId; }
            set { locationId = value; }
        }

        public string DefinitionId
        {
            get { return definitionId; }
            set { definitionId = value; }
        }

        public double RefLevel1
        {
            get { return refLevel1; }

            set { refLevel1 = value; }
        }

        public double RefLevel2
        {
            get { return refLevel2; }

            set { refLevel2 = value; }
        }

        public double UpstreamSlope
        {
            get { return upstreamSlope; }
            set { upstreamSlope = value; }
        }

        public double DownstreamSlope
        {
            get { return downstreamSlope; }

            set { downstreamSlope = value; }
        }

        public double SurfaceLevelLeft
        {
            get { return surfaceLevelLeft; }

            set { surfaceLevelLeft = value; }
        }

        public double SurfaceLevelRight
        {
            get { return surfaceLevelRight; }

            set { surfaceLevelRight = value; }
        }
    }
}