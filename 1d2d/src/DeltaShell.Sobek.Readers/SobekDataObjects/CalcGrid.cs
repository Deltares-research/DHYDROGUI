using System.Collections.Generic;

namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class CalcGrid
    {
        private readonly IList<SobekCalcGridPoint> gridPoints;
        private string branchID;
        private string id;

        public CalcGrid()
        {
            gridPoints = new List<SobekCalcGridPoint>();
        }

        public string ID
        {
            get { return id; }
            set { id = value; }
        }

        public string BranchID
        {
            get { return branchID; }
            set { branchID = value; }
        }

        public IList<SobekCalcGridPoint> GridPoints
        {
            get { return gridPoints; }
        }
    }
}