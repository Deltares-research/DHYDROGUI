namespace DelftTools.Hydro.Structures
{
    public interface IFrictionData
    {
        double Friction { get; set; }

        Friction FrictionDataType { get; set; }
    }
}