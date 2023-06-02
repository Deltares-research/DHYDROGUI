using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Exporters;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff
{
    public class RainfallRunoffDataAccessListener : DataAccessListenerBase
    {
        private readonly IBasinGeometrySerializer serializer;

        public RainfallRunoffDataAccessListener(IBasinGeometrySerializer serializer)
        {
            Ensure.NotNull(serializer, nameof(serializer));
            this.serializer = serializer;
        }

        public override object Clone()
        {
            return new RainfallRunoffDataAccessListener(new BasinGeometryShapeFileSerializer()) {ProjectRepository = ProjectRepository};
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
            
            if (!rainfallRunoffModel.OutputIsEmpty)
            {
                rainfallRunoffModel.ConnectOutput(Path.GetDirectoryName(rainfallRunoffModel.Path));
            }

            var basinShapeFilePath = Path.Combine(Path.GetDirectoryName(rainfallRunoffModel.Path), "basinGeometry.shp");
            if (File.Exists(basinShapeFilePath))
            {
                serializer.ReadCatchmentGeometry(rainfallRunoffModel.Basin, basinShapeFilePath);
            }            
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
                ProjectRepository.PreLoad<UnpavedData>(up => up.BoundarySettings.BoundaryData);
                ProjectRepository.PreLoad<UnpavedData>(up => up.DrainageFormula);
                firstRRModel = false;
            }
            else if (firstBasin && entity is DrainageBasin || entity is IDrainageBasin)
            {
                ProjectRepository.PreLoad<Catchment>(c => c.Links);
                ProjectRepository.PreLoad<RunoffBoundary>(rb => rb.Links);
                ProjectRepository.PreLoad<WasteWaterTreatmentPlant>(wwtp => wwtp.Links);
                firstBasin = false;
            }
        }
    }
}