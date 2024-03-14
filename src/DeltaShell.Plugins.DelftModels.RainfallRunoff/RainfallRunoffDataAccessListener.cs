using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Exporters;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff
{
    public class RainfallRunoffDataAccessListener : IDataAccessListener
    {
        private readonly IBasinGeometrySerializer serializer;
        private bool firstBasin = true;
        private IProjectRepository projectRepository;

        public RainfallRunoffDataAccessListener(IBasinGeometrySerializer serializer, IProjectRepository repository)
        {
            Ensure.NotNull(serializer, nameof(serializer));
            this.serializer = serializer;
            projectRepository = repository;
        }

        public void SetProjectRepository(IProjectRepository repository)
        {
            projectRepository = repository;
        }

        public IDataAccessListener Clone()
        {
            return new RainfallRunoffDataAccessListener(new BasinGeometryShapeFileSerializer(), projectRepository);
        }
        
        public void OnPostLoad(object entity, object[] state, string[] propertyNames)
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

        public bool OnPreUpdate(object entity, object[] state, string[] propertyNames)
        {
            return false;
        }

        public bool OnPreInsert(object entity, object[] state, string[] propertyNames)
        {
            return false;
        }

        public void OnPostUpdate(object entity, object[] state, string[] propertyNames)
        {
        }

        public void OnPostInsert(object entity, object[] state, string[] propertyNames)
        {
        }

        public bool OnPreDelete(object entity, object[] deletedState, string[] propertyNames)
        {
            return false;
        }

        public void OnPostDelete(object entity, object[] deletedState, string[] propertyNames)
        {
        }

        public void OnPreLoad(object entity, object[] loadedState, string[] propertyNames)
        {
            if (entity is Project)
            {
                firstBasin = true;
            }
            else if (firstBasin && entity is DrainageBasin || entity is IDrainageBasin)
            {
                projectRepository.PreLoad<RunoffBoundary>(rb => rb.Links);
                projectRepository.PreLoad<WasteWaterTreatmentPlant>(wwtp => wwtp.Links);
                firstBasin = false;
            }
        }
    }
}