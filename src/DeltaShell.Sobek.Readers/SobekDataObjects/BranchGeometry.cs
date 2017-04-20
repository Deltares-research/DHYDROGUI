using System.Collections.Generic;

namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class BranchGeometry
    {
        private readonly IList<CurvingPoint> curvingPoints;
        private string branchID;

        public BranchGeometry()
        {
            curvingPoints = new List<CurvingPoint>();
        }

        public IList<CurvingPoint> CurvingPoints
        {
            get { return curvingPoints; }
        }

        public string BranchID
        {
            get { return branchID; }
            set { branchID = value; }
        }

        public void AddPoint(string offset, string angle)
        {
            CurvingPoint curvingPoint = new CurvingPoint(NumUtils.ConvertToDouble(offset),
                                                         NumUtils.ConvertToDouble(angle));
            curvingPoints.Add(curvingPoint);
        }
    }
}