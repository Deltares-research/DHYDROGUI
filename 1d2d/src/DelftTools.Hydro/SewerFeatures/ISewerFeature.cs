namespace DelftTools.Hydro.SewerFeatures
{
    public interface ISewerFeature
    {
        void AddToHydroNetwork(IHydroNetwork hydroNetwork, SewerImporterHelper helper);
    }
}