using DelftTools.Hydro.SewerFeatures;

namespace DeltaShell.Plugins.ImportExport.Gwsw
{
    public interface ISewerFeatureGenerator
    {
        ISewerFeature Generate(GwswElement gwswElement);
    }
}