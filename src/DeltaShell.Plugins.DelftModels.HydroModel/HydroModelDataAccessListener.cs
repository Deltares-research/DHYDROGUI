using System.Collections.Generic;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.NGHS.Common.Extensions;
using DeltaShell.NGHS.Utils.Extensions;

namespace DeltaShell.Plugins.DelftModels.HydroModel
{
    public class HydroModelDataAccessListener : DataAccessListenerBase
    {
        public override void OnPostLoad(object entity, object[] state, string[] propertyNames)
        {
            if (!(entity is HydroModel hydroModel))
                return;

            IList<bool> suspendClearOutput = new List<bool>();
            
            try
            {
                SuspendClearOutput(hydroModel, suspendClearOutput);

                // Relink after loading
                using (hydroModel.InEditMode("Linking items after loading"))
                {
                    hydroModel.RelinkDataItems();
                    hydroModel.RelinkHydroRegionLinks();
                }
            }
            finally
            {
                ResetSuspendClearOutput(hydroModel, suspendClearOutput);
            }
        }
        
        public override object Clone()
        {
            return new HydroModelDataAccessListener { ProjectRepository = ProjectRepository };
        }
        
        private static void SuspendClearOutput(HydroModel hydroModel, ICollection<bool> suspendClearOutput)
        {
            foreach (IModel model in hydroModel.Models)
            {
                suspendClearOutput.Add(model.SuspendClearOutputOnInputChange);
                model.SuspendClearOutputOnInputChange = true;
            }
        }
        
        private static void ResetSuspendClearOutput(HydroModel hydroModel, IList<bool> suspendClearOutput)
        {
            (hydroModel.Models, suspendClearOutput).ForEach((m, s) => m.SuspendClearOutputOnInputChange = s);
        }
    }
}