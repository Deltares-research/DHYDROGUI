using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Editing;
using DelftTools.Utils.Reflection;

namespace DeltaShell.Plugins.DelftModels.HydroModel
{
    public static class HydroModelExtensions
    {
        [InvokeRequired]
        public static void MoveModelIntoIntegratedModel(this IHydroModel sourceModel, Folder rootFolder, HydroModel targetHydroModel)
        {
            var editAction = new DefaultEditAction("Move model " + sourceModel.Name + " into " + targetHydroModel.Name);
            targetHydroModel.BeginEdit(editAction);

            MoveActivity(sourceModel, rootFolder, targetHydroModel);
            targetHydroModel.EndEdit();
        }

        public static void UpgradeModelIntoIntegratedModel(this IHydroModel sourceModel, Folder folder, IApplication application)
        {
            var editAction = new DefaultEditAction("Upgrade model " + sourceModel.Name + " into an integrated model.");
            ((IEditableObject)sourceModel).BeginEdit(editAction);

            var hydroModelBuilder = new HydroModelBuilder();
            var newHydroModel = hydroModelBuilder.BuildModel(ModelGroup.Empty);
            newHydroModel.CoordinateSystem = sourceModel.Region?.CoordinateSystem;
            newHydroModel.WorkingDirectoryPathFunc = () => application?.WorkDirectory;

            folder.Items.Add(newHydroModel);
            MoveActivity(sourceModel, folder, newHydroModel);

            ((IEditableObject)sourceModel).EndEdit();
        }

        internal static void ReplaceHydroModelRegion(this IHydroModel sourceModel, HydroModel targetHydroModel)
        {
            if (sourceModel.Region != null)
            {
                // Replace the region of the integrated model by the one in the source model. 
                var hydroRegions = targetHydroModel.Region.SubRegions;
                var integratedModelHydroRegion =
                    hydroRegions.FirstOrDefault(region => region.GetType().Implements(sourceModel.Region.GetType()));

                if (integratedModelHydroRegion != null)
                {
                    hydroRegions.Remove(integratedModelHydroRegion);
                }

                if (sourceModel.Region.SubRegions.Any())
                {
                    foreach (var subRegion in sourceModel.Region.SubRegions)
                    {
                        hydroRegions.Add(subRegion);
                    }
                }
                else
                {
                    hydroRegions.Add(sourceModel.Region);
                }
            }

            // Move (overwrite) model itself to target HydroModel. 
            var targetFlowModel =
                targetHydroModel.Activities.FirstOrDefault(a => a.GetType().Implements(sourceModel.GetType()));
            if (targetFlowModel != null)
            {
                targetHydroModel.Activities.Remove(targetFlowModel);
            }

            targetHydroModel.Migrating = true;
            targetHydroModel.Activities.Add(sourceModel);
            targetHydroModel.Migrating = false;
        }

        private static void MoveActivity(IHydroModel sourceModel, Folder rootFolder, HydroModel targetHydroModel)
        {
            if (rootFolder != null)
            {
                // Remove source model from root folder
                rootFolder.Items.Remove(sourceModel);
            }

            sourceModel.ReplaceHydroModelRegion(targetHydroModel);
        }
    }
}
