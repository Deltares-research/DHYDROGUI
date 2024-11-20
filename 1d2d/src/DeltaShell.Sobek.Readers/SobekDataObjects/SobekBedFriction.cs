using System.Data;
using System.Linq;
using DelftTools.Functions.Generic;

namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public enum SobekFrictionFunctionType
    {
        Constant,
        FunctionOfH, // friction value on different locations along the branch for every h
        FunctionOfQ, // friction value on different locations along the branch for every Q
        FunctionOfLocation // friction as a constant or as a function of the location on the branch)
    }

    /// 0 = Chezy
    /// 1 = Manning
    /// 2 = Strickler Kn
    /// 3 = Strickler Ks
    /// 4 = White-Colebrook
    /// 7 = De Bos and Bijkerk
    public enum SobekBedFrictionType
    {
        Chezy = 0,
        Mannning = 1,
        StricklerKn = 2,
        StricklerKs = 3,
        WhiteColebrook = 4,
        CopyOfMain = 6,
        DeBosAndBijkerk = 7
    }

    public class SobekBedFrictionData
    {
        public SobekBedFrictionType FrictionType
        {
            get; set;
        }
        public SobekBedFrictionDirectionData Positive { get; private set; }
        public SobekBedFrictionDirectionData Negative { get; private set; }

        public SobekBedFrictionData()
        {
            Positive = new SobekBedFrictionDirectionData();
            Negative = new SobekBedFrictionDirectionData();
        }
    }

    public class SobekBedFrictionDirectionData
    {
        public SobekBedFrictionDirectionData()
        {
            InterpolationNotSetValue = InterpolationType.None;
            Interpolation = InterpolationNotSetValue;
        }
        public double FrictionConst { get; set; }
        public SobekFrictionFunctionType FunctionType { get; set; }
        public DataTable HTable { get; set; }
        public DataTable QTable { get; set; }
        public DataTable LocationTable { get; set; }
        public InterpolationType Interpolation { get; set; }
        public static InterpolationType InterpolationNotSetValue { get; private set; }

        public bool Equals(SobekBedFrictionDirectionData other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.FrictionConst.Equals(FrictionConst) 
                && Equals(other.FunctionType, FunctionType)
                && TablesEqual(other.HTable, HTable)
                && TablesEqual(other.QTable, QTable)
                && TablesEqual(other.LocationTable, LocationTable) 
                && Equals(other.Interpolation, Interpolation);
        }

        private bool TablesEqual(DataTable one, DataTable other)
        {
            if (one == null ^ other == null) return false;
            if (ReferenceEquals(one, other)) return true;
            if (one.Rows.Count != other.Rows.Count) return false;

            for (int i = 0; i < one.Rows.Count; i++ )
            {
                if (!other.Rows[i].ItemArray.SequenceEqual(one.Rows[i].ItemArray))
                {
                    return false;
                }
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (SobekBedFrictionDirectionData)) return false;
            return Equals((SobekBedFrictionDirectionData) obj);
        }
    }
   
    /// <summary>
    /// Class to hold the imported values as found in the Sobek file friction.dat
    /// Currently 20100319 only a subsection seems to be supported
    /// </summary>
    public class SobekBedFriction
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string BranchId { get; set; }

        public static SobekBedFrictionType SobekFrictionDefaultType = SobekBedFrictionType.Chezy;
        public static double SobekFrictionDefaultValue = 40.0;

        public SobekBedFrictionData MainFriction { get; set; }
        public SobekBedFrictionData FloodPlain1Friction { get; set; }
        public SobekBedFrictionData FloodPlain2Friction { get; set; }

        public SobekBedFriction()
        {
            MainFriction = new SobekBedFrictionData();
            FloodPlain1Friction = new SobekBedFrictionData();
            FloodPlain2Friction = new SobekBedFrictionData();
        }
    }
}