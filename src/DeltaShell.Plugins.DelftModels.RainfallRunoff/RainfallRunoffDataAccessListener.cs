using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff
{
    public class RainfallRunoffDataAccessListener : DataAccessListenerBase
    {
        public override object Clone()
        {
            return new RainfallRunoffDataAccessListener {ProjectRepository = ProjectRepository};
        }

        private bool firstBasin = true;
        private bool firstRRModel = true;

        public override void OnPostLoad(object entity, object[] state, string[] propertyNames)
        {
            if (!(entity is RainfallRunoffModel rainfallRunoffModel)) 
                return;

            var importer = Sobek2ModelImporters.GetImportersForType(typeof(RainfallRunoffModel)).FirstOrDefault();
            importer?.ImportItem(rainfallRunoffModel.Path, rainfallRunoffModel);
            rainfallRunoffModel.RestoreOutputSettings();
            rainfallRunoffModel.ConnectOutput(Path.GetDirectoryName(rainfallRunoffModel.Path));
        }

        public override void OnPreLoad(object entity, object[] loadedState, string[] propertyNames)
        {
            if (entity is Project)
            {
                firstBasin = true;
                firstRRModel = true;
            }
            else if (firstRRModel && entity is RainfallRunoffModel)
            {
                ProjectRepository.PreLoad<UnpavedData>(up => up.BoundaryData);
                ProjectRepository.PreLoad<UnpavedData>(up => up.DrainageFormula);
                ProjectRepository.PreLoad<CatchmentModelData>(cmd => cmd.SubCatchmentModelData);
                firstRRModel = false;
            }
            else if (firstBasin && entity is DrainageBasin || entity is IDrainageBasin)
            {
                ProjectRepository.PreLoad<Catchment>(c => c.SubCatchments);
                ProjectRepository.PreLoad<Catchment>(c => c.Links);
                ProjectRepository.PreLoad<RunoffBoundary>(rb => rb.Links);
                ProjectRepository.PreLoad<WasteWaterTreatmentPlant>(wwtp => wwtp.Links);
                firstBasin = false;
            }
        }
    }
}