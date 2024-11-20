namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class CurvingPoint
    {
        private readonly double angle;
        private readonly double location;

        public CurvingPoint(double location, double angle)
        {
            this.location = location;
            this.angle = angle;
        }

        /// <summary>
        /// angle (0 = north, 90= east)
        /// </summary>
        public double Angle
        {
            get { return angle; }
        }

        /// <summary>
        /// Location is at the center of a branchsegment (unit=meters)
        /// </summary>
        public double Location
        {
            get { return location; }
        }
    }
}