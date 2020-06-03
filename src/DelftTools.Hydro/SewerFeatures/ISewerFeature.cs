namespace DelftTools.Hydro.SewerFeatures
{
    public interface ISewerFeature
    {
        void AddToHydroNetwork(IHydroNetwork network, SewerImporterHelper helper);
    }
}