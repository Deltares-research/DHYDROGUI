using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using DelftTools.Functions;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Utils;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using DeltaShell.Plugins.FMSuite.FlowFM.Grid;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessBuilders;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.IniReaders;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using DHYDRO.Common.IO.Ini;
using GeoAPI.Extensions.Coverages;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using SharpMap;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    /// <summary>
    /// Legacy loader for <see cref="FlowFMApplicationPlugin"/> version 1.3.0.
    /// This legacy loader removes the data items for the spatial coverages from the data base.
    /// </summary>
    /// <seealso cref="LegacyLoader"/>
    public class WaterFlowFMModel130LegacyLoader : LegacyLoader
    {
        private const string bedLevelCoverageName = "Bed Level";
        private const string tracerQuantityName = "initialtracer";
        private static readonly ILog log = LogManager.GetLogger(typeof(WaterFlowFMModel130LegacyLoader));

        private static readonly IDictionary<string, string> quantities = new Dictionary<string, string>()
        {
            {"Initial Water Level", "initialwaterlevel"},
            {"Initial Salinity", "initialsalinity"},
            {"Initial Temperature", "initialtemperature"},
            {"Roughness", "frictioncoefficient"},
            {"Viscosity", "horizontaleddyviscositycoefficient"},
            {"Diffusivity", "horizontaleddydiffusivitycoefficient"},
        };

        /// <summary>
        /// Called after initializing the migration.
        /// Recovers the original values of a value converter (spatial operation).
        /// These values were never exported to file, but are still stored in the database.
        /// </summary>
        /// <param name="entity"> The WaterFlow FM model. </param>
        /// <param name="dbConnection">The database connection.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="entity"/> or <paramref name="dbConnection"/> is <c>null</c>.
        /// </exception>
        public override void OnAfterInitialize(object entity, IDbConnection dbConnection)
        {
            Ensure.NotNull(entity, nameof(entity));
            Ensure.NotNull(dbConnection, nameof(dbConnection));

            var model = (WaterFlowFMModel) entity;

            if (!MigrationHelper.TryParseDatabasePath(dbConnection.ConnectionString,
                                                      out string dbPath))
            {
                log.ErrorFormat(Resources.WaterFlowFMModel130LegacyLoader_Could_not_determine_dsproj_location, dbConnection.ConnectionString);
                return;
            }

            RecoverCoverageValues(model, dbPath);

            base.OnAfterInitialize(entity, dbConnection);
        }

        /// <summary>
        /// Called after the project migrated.
        /// Removes the data items that store an <see cref="UnstructuredGridCoverage"/>.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="project"/> is <c>null</c>.
        /// </exception>
        public override void OnAfterProjectMigrated(Project project)
        {
            Ensure.NotNull(project, nameof(project));

            foreach (WaterFlowFMModel model in GetModelsFromProject(project))
            {
                RemoveAllSpatialCoverageDataItems(model);
            }

            base.OnAfterProjectMigrated(project);
        }

        private static void RecoverCoverageValues(WaterFlowFMModel model, string dbPath)
        {
            string relMduFilePath = Path.Combine(model.Name, "input", $"{model.Name}.mdu");
            string mduFilePath = Path.Combine($"{dbPath}_data", relMduFilePath);
            string extFilePath = Path.ChangeExtension(mduFilePath, "ext");
            string inputDirectory = Path.GetDirectoryName(mduFilePath);

            var uniqueFileNameProvider = new UniqueFileNameProvider();
            uniqueFileNameProvider.AddFiles(GetExistingFileNames(inputDirectory));

            UnstructuredGrid grid = null;
            List<string> extFileContent = null;

            foreach (UnstructuredGridCoverage coverage in GetRelevantCoverages(model.DataItems))
            {
                if (extFileContent == null)
                {
                    if (TryGetFileContent(extFilePath, out List<string> content))
                    {
                        extFileContent = content;
                    }

                    else
                    {
                        return;
                    }
                }

                if (!TryGetQuantity(coverage.Name, extFileContent, out string quantity))
                {
                    continue;
                }

                if (grid == null)
                {
                    if (TryGetGrid(mduFilePath, out UnstructuredGrid loadedGrid))
                    {
                        grid = loadedGrid;
                    }
                    else
                    {
                        return;
                    }
                }

                coverage.Grid = grid;

                string fileName = uniqueFileNameProvider.GetUniqueFileNameFor(quantity + "_samples.xyz");
                WriteSamples(coverage, inputDirectory, fileName);
                UpdateExtFileContent(extFileContent, quantity, fileName);
            }

            UpdateFile(extFilePath, extFileContent);
        }

        private static IEnumerable<WaterFlowFMModel> GetModelsFromProject(Project project) =>
            project.GetAllItemsRecursive().OfType<WaterFlowFMModel>();

        private static void RemoveAllSpatialCoverageDataItems(WaterFlowFMModel model)
        {
            model.DataItems.RemoveAllWhere(d => typeof(UnstructuredGridCoverage).IsAssignableFrom(d.ValueType));
        }

        private static bool TryGetQuantity(string coverageName, List<string> extFileContent, out string quantity)
        {
            if (quantities.TryGetValue(coverageName, out string value))
            {
                quantity = value;
                return true;
            }

            var tracerQuantity = $"{tracerQuantityName}{coverageName}";
            if (extFileContent.Any(l => ContainsQuantity(l, tracerQuantity)))
            {
                quantity = tracerQuantity;
                return true;
            }

            quantity = null;
            return false;
        }

        private static bool ContainsQuantity(string line, string value)
        {
            string[] splitKey = line.Split('=');
            if (splitKey.Length == 1 || !string.Equals(splitKey[0].Trim(), "quantity",
                                                       StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string[] splitValue = splitKey[1].Split('#', '!');
            return string.Equals(value, splitValue[0].Trim(),
                                 StringComparison.OrdinalIgnoreCase);
        }

        private static IEnumerable<UnstructuredGridCoverage> GetRelevantCoverages(IEnumerable<IDataItem> dataItems)
        {
            foreach (SpatialOperationSetValueConverter converter in GetValueConverters(dataItems))
            {
                var originalCoverage = (UnstructuredGridCoverage) converter.OriginalValue;
                if (originalCoverage.Name == bedLevelCoverageName)
                {
                    continue;
                }

                IVariable component = originalCoverage.Components[0];
                if (HasValues(component))
                {
                    yield return originalCoverage;
                }
            }
        }

        private static void WriteSamples(UnstructuredGridCoverage originalCoverage, string inputDirectory, string fileName)
        {
            IPointCloud pointCloud = originalCoverage.ToPointCloud(skipMissingValues: true);

            string filePath = Path.Combine(inputDirectory, fileName);

            XyzFile.Write(filePath, pointCloud.PointValues);
        }

        private static bool TryGetFileContent(string filePath, out List<string> content)
        {
            content = null;
            if (!FileExists(filePath))
            {
                return false;
            }

            try
            {
                content = File.ReadAllLines(filePath).ToList();
            }
            catch (Exception e) when (e is IOException ||
                                      e is UnauthorizedAccessException ||
                                      e is SecurityException)
            {
                log.ErrorFormat(Resources.WaterFlowFMModel130LegacyLoader_Error_occurred_while_reading_file, filePath, e.Message);
                return false;
            }

            return true;
        }

        private static bool FileExists(string filePath)
        {
            if (File.Exists(filePath))
            {
                return true;
            }

            log.ErrorFormat(Resources.WaterFlowFMModel130LegacyLoader_Could_not_find_file, filePath);
            return false;
        }

        private static bool HasValues(IVariable component) =>
            component.Values.OfType<double>().Any(value => !value.Equals(component.NoDataValue));

        private static IEnumerable<SpatialOperationSetValueConverter> GetValueConverters(IEnumerable<IDataItem> dataItems)
        {
            return dataItems.Select(d => d.ValueConverter).OfType<SpatialOperationSetValueConverter>();
        }

        private static bool TryGetGrid(string mduFilePath, out UnstructuredGrid grid)
        {
            grid = null;

            if (!FileExists(mduFilePath))
            {
                return false;
            }

            if (!TryGetGridFileName(mduFilePath, out string gridFileName))
            {
                log.ErrorFormat(Resources.WaterFlowFMModel130LegacyLoader_Could_not_determine_grid_file_location, mduFilePath);
                return false;
            }

            string gridFilePath = Path.Combine(Path.GetDirectoryName(mduFilePath), gridFileName);
            if (!FileExists(gridFilePath))
            {
                return false;
            }

            grid = UnstructuredGridFileHelper.LoadFromFile(gridFilePath, callCreateCells: true);
            grid.GenerateFlowLinks();
            return true;
        }

        private static bool TryGetGridFileName(string mduFilePath, out string gridFileName)
        {
            gridFileName = null;
            try
            {
                using (var fileStream = new FileStream(mduFilePath, FileMode.Open, FileAccess.Read))
                {
                    var reader = new MduIniReader();
                    IniData iniData = reader.ReadIniFile(fileStream, mduFilePath);

                    IniSection geometrySection = iniData.FindSection("geometry");
                    gridFileName = geometrySection?.GetPropertyValue(KnownProperties.NetFile);

                    return gridFileName != null;
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat(Resources.WaterFlowFMModel130LegacyLoader_Error_occurred_while_reading_file, mduFilePath, e.Message);
                return false;
            }
        }

        private static void UpdateExtFileContent(List<string> fileContent, string quantity, string fileName)
        {
            int index = fileContent.FindIndex(l => ContainsQuantity(l, quantity));
            var lines = new List<string>
            {
                $"{ExtForceFileConstants.QuantityKey}={quantity}",
                $"{ExtForceFileConstants.FileNameKey}={fileName}",
                $"{ExtForceFileConstants.FileTypeKey}={AddSamplesDefaults.FileType}",
                $"{ExtForceFileConstants.MethodKey}={AddSamplesDefaults.Method}",
                $"{ExtForceFileConstants.OperandKey}={ExtForceQuantNames.OperatorToStringMapping[AddSamplesDefaults.Operand]}",
                $"{ExtForceFileConstants.AveragingTypeKey}={(int) AddSamplesDefaults.AveragingType}",
                $"{ExtForceFileConstants.RelSearchCellSizeKey}={AddSamplesDefaults.RelSearchCellSize.ToString(CultureInfo.InvariantCulture)}",
                ""
            };
            fileContent.InsertRange(index, lines);
        }

        private static void UpdateFile(string filePath, IEnumerable<string> content)
        {
            if (content == null)
            {
                return;
            }

            try
            {
                File.WriteAllLines(filePath, content);
            }
            catch (Exception e) when (e is IOException ||
                                      e is UnauthorizedAccessException ||
                                      e is SecurityException)
            {
                log.ErrorFormat(Resources.WaterFlowFMModel130LegacyLoader_An_error_occurred_while_updating_file, filePath, e.Message);
            }
        }

        private static IEnumerable<string> GetExistingFileNames(string inputDirectory) => Directory.GetFiles(inputDirectory).Select(Path.GetFileName);
    }
}