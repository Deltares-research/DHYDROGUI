using DelftTools.Hydro;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui
{
    public class ValidateMergeModelObjects
    {
        public IModelMerge DestinationModel { get; set; }
        public IModelMerge SourceModel { get; set; }
    }
}