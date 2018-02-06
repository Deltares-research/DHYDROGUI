namespace DelftTools.Hydro
{
    public class SewerConnectionOrifice: SewerConnection
    {
        public double Bottom_Level;
        public double Contraction_Coefficent;
        public double Max_Discharge;

        public SewerConnectionOrifice() : base(null, null)
        {
        }

        public SewerConnectionOrifice(string name) : base(name)
        {
        }
    }
}