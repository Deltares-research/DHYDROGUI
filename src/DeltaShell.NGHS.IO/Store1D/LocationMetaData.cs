namespace DeltaShell.NGHS.IO.Store1D
{
    public class LocationMetaData
    {
        public string Id { get; private set; }
        public int BranchId { get; private set; }
        public double Chainage { get; private set; }
        public double XCoordinate { get; private set; }
        public double YCoordinate { get; private set; }

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