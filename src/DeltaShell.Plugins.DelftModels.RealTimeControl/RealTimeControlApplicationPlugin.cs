using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Import;
using log4net;
using Mono.Addins;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl
{
    [Extension(typeof(IPlugin))]
    public class RealTimeControlApplicationPlugin : ApplicationPlugin, IDataAccessListenersProvider
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RealTimeControlApplicationPlugin));

        public override string Name => "Real-Time Control";

        public override string DisplayName => "D-Real Time Control Plugin";

        public override string Description => 
            Properties.Resources.RealTimeControlApplicationPlugin_Description;

        public override string Version => 
            AssemblyUtils.GetAssemblyInfo(GetType().Assembly).Version;

        public override string FileFormatVersion => "3.8.0.0";

        public override IApplication Application
        {
            get => base.Application;
            set
            {
                if (Application != null)
                {
                    Application.ProjectOpened -= ApplicationProjectOpened;
                    Application.ProjectOpening -= ApplicationProjectOpening;
                }

                base.Application = value;

                if (Application != null)
                {
                    Application.ProjectOpened += ApplicationProjectOpened;
                    Application.ProjectOpening += ApplicationProjectOpening;
                }
            }
        }

        public override IEnumerable<ModelInfo> GetModelInfos()
        {
            yield return new ModelInfo
            {
                Name = "Real-Time Control Model",
                Category = "1D / 2D / 3D Standalone Models",
                GetParentProjectItem = owner =>
                {
                    Folder rootFolder = Application?.Project?.RootFolder;
                    return ApplicationPluginHelper.FindParentProjectItemInsideProject(rootFolder, owner) ?? rootFolder;
                },
                AdditionalOwnerCheck = owner =>
                    owner is ICompositeActivity // Only allow composite activities as target
                    && !(owner is ParallelActivity)
                    && !(owner is SequentialActivity)
                    && !((ICompositeActivity) owner).Activities.OfType<RealTimeControlModel>().Any(), // Don't allow multiple realtime control models in one composite activity
                CreateModel = owner => new RealTimeControlModel("Real_Time_Control")
            };
        }

        public override IEnumerable<Assembly> GetPersistentAssemblies()
        {
            yield return GetType().Assembly;
        }

        public override IEnumerable<IFileExporter> GetFileExporters()
        {
            yield return new RealTimeControlModelExporter();
            yield return new RealTimeControlRestartFileExporter();
        }

        public override IEnumerable<IFileImporter> GetFileImporters()
        {
            yield return new RealTimeControlModelImporter();
            yield return new RealTimeControlRestartFileImporter(GetRealTimeControlModels);
        }

        public IEnumerable<IDataAccessListener> CreateDataAccessListeners()
        {
            yield return new RtcDataAccessListener();
        }

        private IEnumerable<RealTimeControlModel> GetRealTimeControlModels() => 
            Application.GetAllModelsInProject().OfType<RealTimeControlModel>();

        private void ApplicationProjectOpening(string projectFilePath)
        {
            if (string.IsNullOrEmpty(projectFilePath) || !File.Exists(projectFilePath))
            {
                throw new FileNotFoundException($"File not found {projectFilePath}");
            }

            if (ShouldUpgradeDataBaseUsingSqlQueries(projectFilePath))
            {
                UpdateDataBase(projectFilePath);
            }
        }

        /// <summary>
        /// If the RTC loads a project where the <see cref="FileFormatVersion"/> is 3.5.0.0 or lower it should update the RTC.
        /// </summary>
        /// <param name="path">Rooted path to the dsproj file.</param>
        /// <returns><c>true</c> when the version of the database provided by <paramref name="path"/> is 3.5.0.0 or lower</returns>
        private bool ShouldUpgradeDataBaseUsingSqlQueries(string path)
        {
            IDictionary<string, Version> pluginVersions = Application.GetPluginFileFormatVersions(path);

            if (pluginVersions.TryGetValue(Name, out Version currentVersion))
            {
                var needsUpgradingVersion = new Version(3, 5, 0, 0);
                var maximumVersionThatNeedsUpgrading = new Version(3, 6, 0, 0);

                if (currentVersion <= needsUpgradingVersion && currentVersion < maximumVersionThatNeedsUpgrading)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Because of the current database table structure of RTC it is not possible to use the NHibernate LegacyLoader or
        /// DataAccessListener to update
        /// the objects / table to the new format.
        /// Only solution is to use SQL statements to create tables, moves objects from one table to the other. Because the objects
        /// have Foreign Keys
        /// we need use pragma statements to stop the database from trying to keep the database consistent by monitoring these FK
        /// relations.
        /// </summary>
        /// <param name="path">Rooted path to the dsproj file.</param>
        private static void UpdateDataBase(string path)
        {
            using (var dbConnection = new SQLiteConnection($"Data Source={path};"))
            {
                dbConnection.Open();

                try
                {
                    using (SQLiteCommand sqlCommand = dbConnection.CreateCommand())
                    {
                        /*
                         * Disable enforcing correct foreign key relations
                         * Create the table in which the IInput implementator objects are stored.
                         * Copy all the Input objects from the rtc_connection_points table to the table created above.
                         * This will include there foreign key relations. (because of this the pragma is needed.
                         * Enable enforcing correct foreign key relation
                         */
                        sqlCommand.CommandText =
                            @"
PRAGMA foreign_keys = off;
CREATE TABLE rtc_iinput_impl_objects (id BIGINT not null, type TEXT not null, name TEXT, LongName TEXT, Value DOUBLE, ParameterName TEXT, UnitName TEXT, feature_id BIGINT, mathExpressions TEXT, rtc_cg_input_id BIGINT, rtc_cg_input_list_index INT, rtc_cg_mathematical_expression_id BIGINT, rtc_cg_mathematical_expression_list_index INT, primary key (id), constraint FKEF1D96FBE8B2CFB9 foreign key (feature_id) references features, constraint FKEF1D96FB8A443375 foreign key (rtc_cg_input_id) references rtc_control_groups, constraint FKEF1D96FB86FA3C18 foreign key (rtc_cg_mathematical_expression_id) references rtc_control_groups);
INSERT INTO rtc_iinput_impl_objects (id, type, Value, ParameterName, UnitName, feature_id, rtc_cg_input_id, rtc_cg_input_list_index) SELECT id, type, Value, ParameterName, UnitName, feature_id, rtc_cg_input_id, rtc_cg_input_list_index 	FROM rtc_connection_points WHERE type = 'rtc_inputs';
DELETE FROM rtc_connection_points WHERE type = 'rtc_inputs';
PRAGMA foreign_keys = on;
";
                        sqlCommand.ExecuteNonQuery();
                    }
                }
                catch (SQLiteException exception)
                {
                    throw new SQLiteException("Loaded a project that has been migrated, but not saved. RTC database schema is in corrupted state.", exception);
                }
            }

            log.Info("RTC database schema updated to support mathematical expressions.");
        }

        private void ApplicationProjectOpened(Project project)
        {
            /*
                Note: it was not possible to do this in RtcDataAccessListener.OnPostLoad() 
                DataItems for Inputs and Outputs are not re-linked until the whole HydroModel has been imported
             */

            List<RealTimeControlModel> rtcModelsWithControlGroups = Application.GetAllModelsInProject()
                                                                               .OfType<RealTimeControlModel>().Where(m => m.ControlGroups.Any()).ToList();

            if (!rtcModelsWithControlGroups.Any())
            {
                return;
            }

            // DELFT3DFM-1441: Existing projects can have ControlGroups with the same names
            rtcModelsWithControlGroups.ForEach(m => m.MakeControlGroupNamesUnique());

            // DELFT3DFM-1441: Existing projects can have ControlGroup DataItems with ChildDataItems without the correct ControlGroup Name (as a prefix)
            rtcModelsWithControlGroups.ForEach(m => m.SyncControlGroupDataItemNames());
        }
    }
}