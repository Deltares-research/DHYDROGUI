using DelftTools.Shell.Core.Dao;
using DeltaShell.NGHS.Common.Extensions;

namespace DeltaShell.Plugins.DelftModels.HydroModel
{
    public class HydroModelDataAccessListener : DataAccessListenerBase
    {
        public override void OnPostLoad(object entity, object[] state, string[] propertyNames)
        {
            if (!(entity is HydroModel hydroModel))
                return;

            // Relink after loading
            using (hydroModel.InEditMode($"Linking items after loading")) ;
            {
                hydroModel.RelinkDataItems();
                hydroModel.RelinkHydroRegionLinks();
            }
        }

        public override object Clone()
        {
            return new HydroModelDataAccessListener { ProjectRepository = ProjectRepository };
        }
    }
}