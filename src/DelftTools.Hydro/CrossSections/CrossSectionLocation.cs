namespace DelftTools.Hydro.CrossSections
{
    public class CrossSectionLocation : ICrossSectionLocation
    {
        public CrossSectionLocation(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public string BranchName { get; set; }

        public double Chainage { get; set; }

        public double Shift { get; set; }

        public string Definition { get; set; }

        public string LongName { get; set; }
    }
}