using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Editing;
using DelftTools.Utils.Reflection;
using GeoAPI.Extensions.Feature;

namespace DelftTools.Hydro
{
    /// <summary>
    /// Action used for importing a full model
    /// This can be used to skip specific logic that is not necessary during full model import
    /// </summary>
    public class MovingModelAction : EditActionBase
    {
        public MovingModelAction(string name) : base(name) {}
    }

    public static class IHydroModelExtensions
    {
        [InvokeRequired]
        public static void MoveModelIntoIntegratedModel(this IHydroModel sourceModel, Folder rootFolder,
                                                        ICompositeActivity targetHydroModel)
        {
            var editAction = new MovingModelAction("Move model " + sourceModel.Name + " into " + targetHydroModel.Name);
            var eobjectmodel = targetHydroModel as IEditableObject;
            if (eobjectmodel == null)
            {
                return;
            }

            eobjectmodel.BeginEdit(editAction);

            MoveActivity(sourceModel, rootFolder, targetHydroModel);
            eobjectmodel.EndEdit();
        }

        public static void MoveActivity(this IHydroModel sourceModel, Folder rootFolder,
                                        ICompositeActivity targetHydroModel)
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

                IEventedList<IRegion> hydroRegions = hmodel.Region.SubRegions;
                IRegion integratedModelHydroRegion =
                    hydroRegions.FirstOrDefault(region => region.GetType().Implements(sourceModel.Region.GetType()));

                if (integratedModelHydroRegion != null)
                {
                    hydroRegions.Remove(integratedModelHydroRegion);
                }

                hydroRegions.Add(sourceModel.Region);
            }

            // Move (overwrite) model itself to target CompositeModel. 
            IActivity targetFlowModel = targetHydroModel.Activities
                                                        .FirstOrDefault(
                                                            a => a.GetType().Implements(sourceModel.GetType()));
            if (targetFlowModel != null)
            {
                targetHydroModel.Activities.Remove(targetFlowModel);
            }

            targetHydroModel.Activities.Add(sourceModel);
        }
    }
}