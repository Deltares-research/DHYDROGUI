using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Utils.Editing;

namespace DeltaShell.Plugins.DelftModels.HydroModel
{
    public static class HydroModelExtensions
    {
        public static void UpgradeModelIntoIntegratedModel(this IHydroModel sourceModel, Folder folder)
        {
            var editAction = new MovingModelAction("Upgrade model " + sourceModel.Name + " into an integrated model.");

            ((IEditableObject) sourceModel).BeginEdit(editAction);
            try
            {
                var hydroModelBuilder = new HydroModelBuilder();
                HydroModel newHydroModel = hydroModelBuilder.BuildModel(ModelGroup.Empty);

                newHydroModel.BeginEdit(editAction);

                sourceModel.MoveActivity(folder, newHydroModel);

                newHydroModel.EndEdit();

                folder.Items.Add(newHydroModel);
            }
            finally
            {
                ((IEditableObject) sourceModel).EndEdit();
            }
        }
    }
}