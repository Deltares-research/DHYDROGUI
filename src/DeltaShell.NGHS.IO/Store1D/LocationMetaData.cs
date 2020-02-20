namespace DeltaShell.NGHS.IO.Store1D
{
    public class LocationMetaData
    {
        public string Id { get; set; }
        public int BranchId { get; set; }
        public double Chainage { get; set; }
        public double XCoordinate { get; set; }
        public double YCoordinate { get; set; }

        public LocationMetaData()
        {
            
        }
        public LocationMetaData(string id, int branchId, double chainage, double xCoordinate, double yCoordinate)
        {
            Id = id;
            BranchId = branchId;
            Chainage = chainage;
            XCoordinate = xCoordinate;
            YCoordinate = yCoordinate;
        }
    }
}