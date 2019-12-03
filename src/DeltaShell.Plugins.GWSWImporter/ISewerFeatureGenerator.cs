using DelftTools.Hydro.SewerFeatures;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    public interface ISewerFeatureGenerator
    {
        ISewerFeature Generate(GwswElement gwswElement);
    }
}