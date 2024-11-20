namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekLinkageNode : SobekNode
    {
        private string branchID;
        private double reachLocation;

        public SobekLinkageNode()
        {
            SetIsLinkageNode(true);
        }

        public string BranchID
        {
            get { return branchID; }
            set { branchID = value; }
        }

        public double ReachLocation
        {
            get { return reachLocation; }
            set { reachLocation = value; }
        }
    }
}