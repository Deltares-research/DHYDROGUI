using System.Collections.Generic;

namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekRetention
    {
        private readonly IList<Storage> streetStorage;
        private readonly List<Storage> wellStorage;
        private double manHoleArea;
        private string nodeId;
        private double reservoirBedLevel;
        private double streetArea;
        private double streetLevel;
        private int type;

        public SobekRetention()
        {
            streetStorage = new List<Storage>();
            wellStorage = new List<Storage>();
        }

        public IList<Storage> StreetStorage
        {
            get { return streetStorage; }
        }

        public IList<Storage> WellStorage
        {
            get { return wellStorage; }
        }

        public string NodeID
        {
            get { return nodeId; }
            set { nodeId = value; }
        }

        /// <summary>
        /// Type water on street
        /// 1 = reservoir
        /// 2 = closed
        /// 3 = loss
        /// </summary>
        public int Type
        {
            get { return type; }
            set { type = value; }
        }

        public double StreetArea
        {
            get { return streetArea; }
            set { streetArea = value; }
        }

        public double ManHoleArea
        {
            get { return manHoleArea; }
            set { manHoleArea = value; }
        }

        public double StreetLevel
        {
            get { return streetLevel; }
            set { streetLevel = value; }
        }

        public double ReservoirBedLevel
        {
            get { return reservoirBedLevel; }
            set { reservoirBedLevel = value; }
        }

        public void AddWellStorage(double[] doubles)
        {
            wellStorage.Add(new Storage(doubles[0], doubles[1]));
        }

        public void AddStreetStorage(double[] doubles)
        {
            streetStorage.Add(new Storage(doubles[0], doubles[1]));
        }

        #region Nested type: Storage

        public class Storage

        {
            private readonly double a;
            private readonly double b;

            public Storage(double a, double b)
            {
                this.a = a;
                this.b = b;
            }

            public double B
            {
                get { return b; }
            }

            public double A
            {
                get { return a; }
            }
        }

        #endregion
    }
}