namespace DelftTools.Hydro.Structures
{
    public interface IOrifice : ISewerFeature
    {
        double BottomLevel { get; set; }
        double ContractionCoefficent { get; set; }
        double MaxDischarge { get; set; }
    }
}