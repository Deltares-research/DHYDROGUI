namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekBranch
    {
        private string endNodeID;
        private double length;
        private string name;
        private string startNodeID;
        private string textID;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string TextID
        {
            get { return textID; }
            set { textID = value; }
        }

        public string StartNodeID
        {
            get { return startNodeID; }
            set { startNodeID = value; }
        }

        public string EndNodeID
        {
            get { return endNodeID; }
            set { endNodeID = value; }
        }

        public double Length
        {
            get { return length; }
            set { length = value; }
        }
    }
}