using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Editing;
using DelftTools.Utils.Reflection;

namespace DelftTools.Hydro
{
    public static class IHydroModelExtensions
    {
        [InvokeRequired]
        public static void MoveModelIntoIntegratedModel(this IHydroModel sourceModel, Folder rootFolder,
            ICompositeActivity targetHydroModel)
        {
            var editAction = new DefaultEditAction("Move model " + sourceModel.Name + " into " + targetHydroModel.Name);
            var eobjectmodel = targetHydroModel as IEditableObject;
            if (eobjectmodel == null) return;
            eobjectmodel.BeginEdit(editAction);

            MoveActivity(sourceModel, rootFolder, targetHydroModel);
            eobjectmodel.EndEdit();
        }

        private static void MoveActivity(IHydroModel sourceModel, Folder rootFolder, ICompositeActivity targetHydroModel)
        {
            if (rootFolder != null)
            {
                // Remove source model from root folder
                rootFolder.Items.Remove(sourceModel);
            }
            var hmodel = targetHydroModel as IHydroModel;
            if (sourceModel.Region != null && hmodel != null)
            {
                // Replace the region of the integrated model by the one in the source model. 
                
                var hydroRegions = hmodel.Region.SubRegions;
                var regionsToReplace = sourceModel.Region is HydroRegion hydroRegion
                    ? hydroRegion.SubRegions.ToArray()
                    : new[] { sourceModel.Region };
                

                foreach (var region in regionsToReplace)
                {
                    var integratedModelHydroRegion = hydroRegions.FirstOrDefault(r => region.GetType().Implements(r.GetType()));
                    if (integratedModelHydroRegion != null)
                    {
                        hydroRegions.Remove(integratedModelHydroRegion);
                    }
                    hydroRegions.Add(region);
                }
            }
            // Move (overwrite) model itself to target CompositeModel. 
            var targetFlowModel =
                targetHydroModel.Activities.FirstOrDefault(a => a.GetType().Implements(sourceModel.GetType()));
            if (targetFlowModel != null)
            {
                targetHydroModel.Activities.Remove(targetFlowModel);
            }
            targetHydroModel.Activities.Add(sourceModel);
        }
    }
}