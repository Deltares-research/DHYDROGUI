namespace DelftTools.Hydro.Structures
{
    public class Orifice : Gate
    {
        public Orifice() : this("Orifice")
        {
            
        }

        public Orifice(string name)
        {
            Name = name;
        }

        public double BottomLevel { get; set; }
        public double ContractionCoefficent { get; set; }
        public double MaxDischarge { get; set; }
    }
}