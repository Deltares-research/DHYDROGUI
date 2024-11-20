namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekRRNode
    {
        public string Id
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string ReachId
        {
            get;
            set;
        }

        public SobekRRNodeType NodeType
        {
            get;
            set;
        }

        public int NetterType
        {
            get;
            set;
        }

        public string ObjectTypeName
        {
            get;
            set;
        }

        public double X
        {
            get;
            set;
        }

        public double Y
        {
            get;
            set;
        }
    }

    public enum SobekRRNodeType
    {
        None = 0,
        PavedArea = 1,
        UnpavedArea = 2,
        Greenhouse = 3,
        InternallyReservedForAllStructures = 5,
        Boundary = 6,
        NWRW = 7, //???
        Pump = 8,
        Weir = 9,
        Orifice = 10,
        ManningResistance = 11,
        QHRelation = 12,
        Culvert = 13,
        WWTP = 14,
        Industry = 15,
        Sacramento = 16,
        ExternalRunoff = 18,
        HBV = 19,
        SCS = 20,
        OpenWater = 21,
        NAM = 31
    }
}
