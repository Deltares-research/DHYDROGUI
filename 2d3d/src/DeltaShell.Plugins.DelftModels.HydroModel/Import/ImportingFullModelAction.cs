using DelftTools.Utils.Editing;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Import
{
    /// <summary>
    /// Action used for importing a full model
    /// This can be used to skip specific logic that is not necessary during full model import
    /// </summary>
    public class ImportingFullModelAction : EditActionBase
    {
        public ImportingFullModelAction(string name) : base(name) {}
    }
}