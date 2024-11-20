using GeoAPI.Geometries;

namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekBranchLocation
    {
        private string branchID;
        private IGeometry geometry;
        private string id;
        private string name;
        private double offset;

        public string BranchID
        {
            get { return branchID; }
            set { branchID = value; }
        }

        /// <summary>
        /// Unique identifier of branch.
        /// </summary>
        public string ID
        {
            get { return id; }
            set { id = value; }
        }

        /// <summary>
        /// Geometry of the cross section (ie yz values.)
        /// </summary>
        public IGeometry Geometry
        {
            get { return geometry; }
            set { geometry = value; }
        }

        public double Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
    }
}