namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    /// <summary>
    /// Nodes in sobek are stored with a string id
    /// </summary>
    public class SobekNode
    {
        private string id;
        private string name;
        private double x;
        private double y;
        private bool isLinkageNode;


        /// <summary>
        /// Unique identifier of node.
        /// </summary>
        public string ID
        {
            get { return id; }
            set { id = value; }
        }

        /// <summary>
        /// X-coordinate of node (usually in RD coordinate reference system)
        /// </summary>
        public double X
        {
            get { return x; }
            set { x = value; }
        }

        /// <summary>
        /// Y-coordinate of node (usually in RD coordinate reference system)
        /// </summary>
        public double Y
        {
            get { return y; }
            set { y = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        //interpolation over node
        public bool InterpolationOverNode{ get; set; }

        //interpolation from reach
        public string InterpolationFrom { get; set; }

        //interpolation to reach
        public string InterpolationTo { get; set; }

        /// <summary>
        /// Unique identifier of node.
        /// </summary>
        public bool IsLinkageNode
        {
            get { return isLinkageNode; }
        }

        protected void SetIsLinkageNode(bool isLN)
        {
            isLinkageNode = isLN;
        }
    }
}