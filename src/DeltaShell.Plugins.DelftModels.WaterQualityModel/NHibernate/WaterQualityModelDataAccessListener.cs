using System;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Dao;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.Data.NHibernate.DataAccessListeners;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.BoundaryData;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.NHibernate
{
    public class WaterQualityModelDataAccessListener : DataAccessListenerBase
    {
        private readonly ProjectDataPathPersisterHelper projectDataPathPersisterHelper;
        private readonly string modelDataDirPropertyName;
        private readonly string dataTableManagerFolderPropertyName;
        private readonly string settingsOutputDirectoryPropertyName;
        private readonly string settingsWorkDirectoryPropertyName;
        private readonly string hydFileDataPathPropertyName;
        private static string waterQualityModelSettingsWorkDirectory;
        

        public WaterQualityModelDataAccessListener()
        {
            projectDataPathPersisterHelper = new ProjectDataPathPersisterHelper();
            modelDataDirPropertyName = TypeUtils.GetMemberName<WaterQualityModel>(m => m.ModelDataDirectory);
            dataTableManagerFolderPropertyName = TypeUtils.GetMemberName<DataTableManager>(dtm => dtm.FolderPath);
            settingsOutputDirectoryPropertyName = TypeUtils.GetMemberName<WaterQualityModelSettings>(s => s.OutputDirectory);
            settingsWorkDirectoryPropertyName = TypeUtils.GetMemberName<WaterQualityModelSettings>(s => s.WorkDirectory);
            hydFileDataPathPropertyName = TypeUtils.GetMemberName<HydFileData>(hydFileData => hydFileData.Path);
            waterQualityModelSettingsWorkDirectory = string.Empty;
        }

        public override IProjectRepository ProjectRepository
        {
            get { return projectDataPathPersisterHelper.ProjectRepository; }
            set { projectDataPathPersisterHelper.ProjectRepository = value; }
        }

        public override object Clone()
        {
            return new WaterQualityModelDataAccessListener();
        }

        #region Pre-Persist

        public override bool OnPreUpdate(object entity, object[] state, string[] propertyNames)
        {
            BeforePersist(entity, state, propertyNames);
            return false; //no veto
        }

        public override bool OnPreInsert(object entity, object[] state, string[] propertyNames)
        {
            BeforePersist(entity, state, propertyNames);
            return false; //no veto
        }

        public override bool OnPreDelete(object entity, object[] deletedState, string[] propertyNames)
        {
            BeforePersist(entity, deletedState, propertyNames); //commit the delete internally
            return false;
        }

        private void BeforePersist(object entity, object[] state, string[] propertyNames)
        {
            HandleBeforePersistWaterQualityModel(entity, state, propertyNames);

            HandleBeforePersistDataTableManager(entity, state, propertyNames);

            HandleBeforePersistWaterQualityModelSettings(entity, state, propertyNames);
        }

        private void HandleBeforePersistWaterQualityModelSettings(object entity, object[] state, string[] propertyNames)
        {
            MakeProjectDataDirectoryChildPathRelative<WaterQualityModelSettings>(entity, state, propertyNames, settingsOutputDirectoryPropertyName);
        }

        private void HandleBeforePersistDataTableManager(object entity, object[] state, string[] propertyNames)
        {
            MakeProjectDataDirectoryChildPathRelative<DataTableManager>(entity, state, propertyNames, dataTableManagerFolderPropertyName);
        }

        private void HandleBeforePersistWaterQualityModel(object entity, object[] state, string[] propertyNames)
        {
            MakeProjectDataDirectoryChildPathRelative<WaterQualityModel>(entity, state, propertyNames, modelDataDirPropertyName);
        }

        /// <summary>
        /// Handles rooted file- or folder-paths inside the project data directory path by
        /// making the rooted part to the project data directory relative.
        /// </summary>
        /// <typeparam name="T">Object type to handly</typeparam>
        /// <param name="entity">The instance to handle, doing nothing if not if type does not match <typeparamref name="T"/>.</param>
        /// <param name="state">The state for instance <paramref name="entity"/>.</param>
        /// <param name="propertyNames">The property names of the state variables in <paramref name="state"/>.</param>
        /// <param name="propertyName">Name of the property being handled.</param>
        private void MakeProjectDataDirectoryChildPathRelative<T>(object entity, object[] state, string[] propertyNames, string propertyName) where T : class
        {
            var instance = entity as T;
            if (instance == null) return;

            var propertyInfo = TypeUtils.GetPropertyInfo(typeof(T), propertyName);
            var originalPath = (string)propertyInfo.GetGetMethod().Invoke(instance, new object[] { });

            var path = projectDataPathPersisterHelper.MakePathRelativeToProjectDataDirectory(originalPath);
            if (string.IsNullOrEmpty(path)) return;

            // Update persister state for consistency with change:
            state[Array.IndexOf(propertyNames, propertyName)] = path;

            propertyInfo.GetSetMethod().Invoke(instance, new object[] { path });
        }

        #endregion

        #region Post-Persist

        public override void OnPostUpdate(object entity, object[] state, string[] propertyNames)
        {
            AfterPersist(entity, state, propertyNames);
        }

        public override void OnPostInsert(object entity, object[] state, string[] propertyNames)
        {
            AfterPersist(entity, state, propertyNames);
        }

        public override void OnPostLoad(object entity, object[] state, string[] propertyNames)
        {
            AfterPersist(entity, state, propertyNames);
        }

        private void AfterPersist(object entity, object[] state, string[] propertyNames)
        {
            HandleAfterPersistWaterQualityModel(entity, state, propertyNames);

            HandleAfterPersistDataTableManager(entity, state, propertyNames);

            HandleAfterPersistWaterQualityModelSettings(entity, state, propertyNames);

            HandleAfterPersistHydFileDataFilePathPath(entity, state, propertyNames);
        }

        private void HandleAfterPersistHydFileDataFilePathPath(object entity, object[] state, string[] propertyNames)
        {
            var instance = entity as HydFileData;
            if (instance == null) return;

            var propertyInfo = TypeUtils.GetPropertyInfo(typeof(HydFileData), hydFileDataPathPropertyName);
            if (propertyInfo.PropertyType != typeof(FileInfo)) return;

            var originalPath = (FileInfo)propertyInfo.GetGetMethod().Invoke(instance, new object[] { });
            if (originalPath.Exists) return;
            var filePath = originalPath.FullName;
            var startIndexWherePathsStartToBeDifferent = filePath.Zip(waterQualityModelSettingsWorkDirectory, (c1, c2) => c1 == c2).TakeWhile(b => b).Count();
            string relFilePath = filePath.Substring(startIndexWherePathsStartToBeDifferent);
            var currentDirectoryName = Path.GetDirectoryName(ProjectRepository.Path);
            string newAbsPath = string.Empty;
            if (currentDirectoryName != null && !filePath.StartsWith(currentDirectoryName))
                newAbsPath = Path.Combine(currentDirectoryName, relFilePath);
            
            if (string.IsNullOrWhiteSpace(newAbsPath)) return;

            var convertedPath = new FileInfo(newAbsPath);
            propertyInfo.GetSetMethod().Invoke(instance, new object[] {convertedPath});

            // Update persister state for consistency with change:
            state[Array.IndexOf(propertyNames, hydFileDataPathPropertyName)] = convertedPath;
        }

        private void HandleAfterPersistWaterQualityModelSettings(object entity, object[] state, string[] propertyNames)
        {
            HandleRelativeProjectDataDirectoryPath<WaterQualityModelSettings>(entity, state, propertyNames, settingsOutputDirectoryPropertyName);

            var waqModelSettings = entity as WaterQualityModelSettings;
            if (waqModelSettings == null || !string.IsNullOrWhiteSpace(waterQualityModelSettingsWorkDirectory)) return;
            
            var propertyInfo = TypeUtils.GetPropertyInfo(typeof(WaterQualityModelSettings), settingsWorkDirectoryPropertyName);
            if (propertyInfo.PropertyType != typeof(string)) return;

            waterQualityModelSettingsWorkDirectory = (string)propertyInfo.GetGetMethod().Invoke(waqModelSettings, new object[] { });
            
            // do we need to set the workdirectory?
            //HandleRelativeProjectDataDirectoryPath<WaterQualityModelSettings>(entity, state, propertyNames, settingsWorkDirectoryPropertyName);
        }

        private void HandleAfterPersistDataTableManager(object entity, object[] state, string[] propertyNames)
        {
            HandleRelativeProjectDataDirectoryPath<DataTableManager>(entity, state, propertyNames, dataTableManagerFolderPropertyName);
        }

        private void HandleAfterPersistWaterQualityModel(object entity, object[] state, string[] propertyNames)
        {
            HandleRelativeProjectDataDirectoryPath<WaterQualityModel>(entity, state, propertyNames, modelDataDirPropertyName);
        }

        /// <summary>
        /// Handles the relative project data directory path generated by <see cref="MakeProjectDataDirectoryChildPathRelative{T}"/>.
        /// </summary>
        /// <typeparam name="T">Object type to handle.</typeparam>
        /// <param name="entity">The instance to handle, doing nothing if not if type does not match T.</param>
        /// <param name="state">The state for instance entity.</param>
        /// <param name="propertyNames">The property names of the state variables in state.</param>
        /// <param name="propertyName">Name of the property being handled.</param>
        private void HandleRelativeProjectDataDirectoryPath<T>(object entity, object[] state, string[] propertyNames, string propertyName) where T : class
        {
            var instance = entity as T;

            if (instance == null)
            {
                return;
            }

            var propertyInfo = TypeUtils.GetPropertyInfo(typeof(T), propertyName);
            if (propertyInfo.PropertyType != typeof(string)) return;

            var originalPath = (string) propertyInfo.GetGetMethod().Invoke(instance, new object[]{});
            if (Path.IsPathRooted(originalPath)) return;

            var convertedPath = projectDataPathPersisterHelper.MakePathAbsolute(originalPath);
            if (Equals(convertedPath, originalPath)) return;

            propertyInfo.GetSetMethod().Invoke(instance, new object[] { convertedPath });
                
            // Update persister state for consistency with change:
            state[Array.IndexOf(propertyNames, propertyName)] = convertedPath;
        }
        #endregion
    }
}