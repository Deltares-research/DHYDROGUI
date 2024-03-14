using System;
using System.IO;
using System.Linq;
using System.Reflection;
using DelftTools.Shell.Core.Dao;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.Data.NHibernate.DataAccessListeners;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.BoundaryData;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.NHibernate
{
    public class WaterQualityModelDataAccessListener : IDataAccessListener
    {
        private static string waterQualityModelSettingsWorkDirectory = string.Empty;

        private const string modelDataDirPropertyName = nameof(WaterQualityModel.ModelDataDirectory);
        private const string dataTableManagerFolderPropertyName = nameof(DataTableManager.FolderPath);
        private const string settingsOutputDirectoryPropertyName = nameof(WaterQualityModelSettings.OutputDirectory);
        private const string settingsWorkDirectoryPropertyName = nameof(WaterQualityModelSettings.WorkDirectory);
        private const string hydFileDataPathPropertyName = nameof(HydFileData.Path);

        private IProjectRepository projectRepository;

        public WaterQualityModelDataAccessListener(IProjectRepository repository)
        {
            projectRepository = repository;
        }

        public void SetProjectRepository(IProjectRepository repository)
        {
            projectRepository = repository;
        }

        public IDataAccessListener Clone()
        {
            return new WaterQualityModelDataAccessListener(projectRepository);
        }

        #region Pre-Persist

        public bool OnPreUpdate(object entity, object[] state, string[] propertyNames)
        {
            return BeforePersist(entity, state, propertyNames);
        }

        public bool OnPreInsert(object entity, object[] state, string[] propertyNames)
        {
            return BeforePersist(entity, state, propertyNames);
        }

        public bool OnPreDelete(object entity, object[] deletedState, string[] propertyNames)
        {
            return BeforePersist(entity, deletedState, propertyNames); //commit the delete internally
        }

        public void OnPreLoad(object entity, object[] loadedState, string[] propertyNames)
        {
        }

        private bool BeforePersist(object entity, object[] state, string[] propertyNames)
        {
            HandleBeforePersistWaterQualityModel(entity, state, propertyNames);

            HandleBeforePersistDataTableManager(entity, state, propertyNames);

            HandleBeforePersistWaterQualityModelSettings(entity, state, propertyNames);

            return false;
        }

        private void HandleBeforePersistWaterQualityModelSettings(object entity, object[] state, string[] propertyNames)
        {
            MakeProjectDataDirectoryChildPathRelative<WaterQualityModelSettings>(
                entity, state, propertyNames, settingsOutputDirectoryPropertyName);
        }

        private void HandleBeforePersistDataTableManager(object entity, object[] state, string[] propertyNames)
        {
            MakeProjectDataDirectoryChildPathRelative<DataTableManager>(entity, state, propertyNames,
                                                                        dataTableManagerFolderPropertyName);
        }

        private void HandleBeforePersistWaterQualityModel(object entity, object[] state, string[] propertyNames)
        {
            MakeProjectDataDirectoryChildPathRelative<WaterQualityModel>(
                entity, state, propertyNames, modelDataDirPropertyName);
        }

        /// <summary>
        /// Handles rooted file- or folder-paths inside the project data directory path by
        /// making the rooted part to the project data directory relative.
        /// </summary>
        /// <typeparam name="T"> Object type to handly </typeparam>
        /// <param name="entity"> The instance to handle, doing nothing if not if type does not match <typeparamref name="T"/>. </param>
        /// <param name="state"> The state for instance <paramref name="entity"/>. </param>
        /// <param name="propertyNames"> The property names of the state variables in <paramref name="state"/>. </param>
        /// <param name="propertyName"> Name of the property being handled. </param>
        private void MakeProjectDataDirectoryChildPathRelative<T>(object entity, object[] state, string[] propertyNames,
                                                                  string propertyName) where T : class
        {
            var instance = entity as T;
            if (instance == null)
            {
                return;
            }

            PropertyInfo propertyInfo = TypeUtils.GetPropertyInfo(typeof(T), propertyName);
            var originalPath = (string) propertyInfo.GetGetMethod().Invoke(instance, new object[]
                                                                               {});

            string path = ProjectDataPathPersisterHelper.MakePathRelativeToProjectDataDirectory(projectRepository, originalPath);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            // Update persister state for consistency with change:
            state[Array.IndexOf(propertyNames, propertyName)] = path;

            propertyInfo.GetSetMethod().Invoke(instance, new object[]
            {
                path
            });
        }

        #endregion

        #region Post-Persist

        public void OnPostUpdate(object entity, object[] state, string[] propertyNames)
        {
            AfterPersist(entity, state, propertyNames);
        }

        public void OnPostInsert(object entity, object[] state, string[] propertyNames)
        {
            AfterPersist(entity, state, propertyNames);
        }

        public void OnPostDelete(object entity, object[] deletedState, string[] propertyNames)
        {
        }

        public void OnPostLoad(object entity, object[] state, string[] propertyNames)
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
            if (instance == null)
            {
                return;
            }

            PropertyInfo propertyInfo = TypeUtils.GetPropertyInfo(typeof(HydFileData), hydFileDataPathPropertyName);
            if (propertyInfo.PropertyType != typeof(FileInfo))
            {
                return;
            }

            var originalPath = (FileInfo) propertyInfo.GetGetMethod().Invoke(instance, new object[]
                                                                                 {});
            if (originalPath.Exists)
            {
                return;
            }

            string filePath = originalPath.FullName;
            int startIndexWherePathsStartToBeDifferent = filePath
                                                         .Zip(waterQualityModelSettingsWorkDirectory,
                                                              (c1, c2) => c1 == c2).TakeWhile(b => b).Count();
            string relFilePath = filePath.Substring(startIndexWherePathsStartToBeDifferent);
            string currentDirectoryName = Path.GetDirectoryName(projectRepository.Path);
            var newAbsPath = string.Empty;
            if (currentDirectoryName != null && !filePath.StartsWith(currentDirectoryName))
            {
                newAbsPath = Path.Combine(currentDirectoryName, relFilePath);
            }

            if (string.IsNullOrWhiteSpace(newAbsPath))
            {
                return;
            }

            var convertedPath = new FileInfo(newAbsPath);
            propertyInfo.GetSetMethod().Invoke(instance, new object[]
            {
                convertedPath
            });

            // Update persister state for consistency with change:
            state[Array.IndexOf(propertyNames, hydFileDataPathPropertyName)] = convertedPath;
        }

        private void HandleAfterPersistWaterQualityModelSettings(object entity, object[] state, string[] propertyNames)
        {
            HandleRelativeProjectDataDirectoryPath<WaterQualityModelSettings>(
                entity, state, propertyNames, settingsOutputDirectoryPropertyName);

            var waqModelSettings = entity as WaterQualityModelSettings;
            if (waqModelSettings == null || !string.IsNullOrWhiteSpace(waterQualityModelSettingsWorkDirectory))
            {
                return;
            }

            PropertyInfo propertyInfo =
                TypeUtils.GetPropertyInfo(typeof(WaterQualityModelSettings), settingsWorkDirectoryPropertyName);
            if (propertyInfo.PropertyType != typeof(string))
            {
                return;
            }

            waterQualityModelSettingsWorkDirectory = (string) propertyInfo.GetGetMethod().Invoke(waqModelSettings,
                                                                                                 new object[] {});
        }

        private void HandleAfterPersistDataTableManager(object entity, object[] state, string[] propertyNames)
        {
            HandleRelativeProjectDataDirectoryPath<DataTableManager>(entity, state, propertyNames,
                                                                     dataTableManagerFolderPropertyName);
        }

        private void HandleAfterPersistWaterQualityModel(object entity, object[] state, string[] propertyNames)
        {
            HandleRelativeProjectDataDirectoryPath<WaterQualityModel>(entity, state, propertyNames,
                                                                      modelDataDirPropertyName);
        }

        /// <summary>
        /// Handles the relative project data directory path generated by
        /// <see cref="MakeProjectDataDirectoryChildPathRelative{T}"/>.
        /// </summary>
        /// <typeparam name="T"> Object type to handle. </typeparam>
        /// <param name="entity"> The instance to handle, doing nothing if not if type does not match T. </param>
        /// <param name="state"> The state for instance entity. </param>
        /// <param name="propertyNames"> The property names of the state variables in state. </param>
        /// <param name="propertyName"> Name of the property being handled. </param>
        private void HandleRelativeProjectDataDirectoryPath<T>(object entity, object[] state, string[] propertyNames,
                                                               string propertyName) where T : class
        {
            var instance = entity as T;

            if (instance == null)
            {
                return;
            }

            PropertyInfo propertyInfo = TypeUtils.GetPropertyInfo(typeof(T), propertyName);
            if (propertyInfo.PropertyType != typeof(string))
            {
                return;
            }

            var originalPath = (string) propertyInfo.GetGetMethod().Invoke(instance, new object[]
                                                                               {});
            if (Path.IsPathRooted(originalPath))
            {
                return;
            }

            string convertedPath = ProjectDataPathPersisterHelper.MakePathAbsolute(projectRepository, originalPath);
            if (Equals(convertedPath, originalPath))
            {
                return;
            }

            propertyInfo.GetSetMethod().Invoke(instance, new object[]
            {
                convertedPath
            });

            // Update persister state for consistency with change:
            state[Array.IndexOf(propertyNames, propertyName)] = convertedPath;
        }

        #endregion
    }
}