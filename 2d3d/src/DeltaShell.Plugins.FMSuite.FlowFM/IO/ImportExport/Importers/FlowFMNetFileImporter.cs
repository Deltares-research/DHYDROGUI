using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Grids;
using SharpMap.CoordinateSystems;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers
{
    /// <summary>
    /// <see cref="FlowFMNetFileImporter"/> implements the <see cref="IFileImporter"/>
    /// interface to import Net files onto <see cref="UnstructuredGrid"/> objects and
    /// as root level objects.
    /// </summary>
    /// <seealso cref="IFileImporter" />
    public class FlowFMNetFileImporter : IFileImporter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FlowFMNetFileImporter));

        /// <summary>
        /// Gets or sets the function to obtain the
        /// <see cref="IWaterFlowFMModel"/> corresponding with the provided
        /// <see cref="UnstructuredGrid"/>.
        /// </summary>
        public Func<UnstructuredGrid, IWaterFlowFMModel> GetModelForGrid { get; set; }

        public string Name => "Unstructured Grid (UGRID)";

        public string Category => 
            Resources.FMImporters_Category_D_Flow_FM_2D_3D;

        public string Description => string.Empty;

        [ExcludeFromCodeCoverage]
        public Bitmap Image => Resources.unstruc;

        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(UnstructuredGrid);
            }
        }

        public bool CanImportOn(object targetObject) =>
            (targetObject == null) || // import on root level
            (targetObject is UnstructuredGrid grid && GetModelForGrid?.Invoke(grid) != null);

        public bool CanImportOnRootLevel => true;

        public string FileFilter => 
            $"Net file|*{FileConstants.NetCdfFileExtension}";

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public bool OpenViewAfterImport => true;

        public object ImportItem(string path, object target = null)
        {
            ValidatePath(path);

            if (IsRootLevelTarget(target))
            {
                return GetRootLevelImport(path);
            }

            IWaterFlowFMModel flowModel = GetModel(target, path);
            
            if (flowModel != null && flowModel.NetFilePath != null)
            {
                return GetModelLevelImport(flowModel, path);
            }

            return null;
        }

        private static void ValidatePath(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Could not find file {path}");
            }
        }

        private static void ValidateCoordinates(ImportedFMNetFile netFile, IWaterFlowFMModel model)
        {
            IList<Coordinate> coordinates = netFile.Grid.Vertices;

            if (model.CoordinateSystem != null &&
                !CoordinateSystemValidator.CanAssignCoordinateSystem(coordinates, model.CoordinateSystem))
            {
                throw new InvalidOperationException("Grid coordinates are incompatible with current model coordinate system: {0}, canceling import.");
            }
        }
        private static object GetRootLevelImport(string path) =>
            new DataItem
            {
                Value = new ImportedFMNetFile(path),
                Name = Path.GetFileName(path)
            };

        private static object GetModelLevelImport(IWaterFlowFMModel model, string path)
        {
            var fmNetFile = new ImportedFMNetFile(path);
            
            ValidateCoordinates(fmNetFile, model);
            UpdatePersistedGrid(model, path);
            model.ReloadGrid(false, true);

            return model.Grid;
        }

        private static bool IsRootLevelTarget(object target) => target == null;

        private IWaterFlowFMModel GetModel(object target, string path)
        {
            switch (target)
            {
                case WaterFlowFMModel targetAsModel:
                    return targetAsModel;
                case UnstructuredGrid grid when GetModelForGrid != null:
                {
                    IWaterFlowFMModel flowModel = GetModelForGrid(grid); 
                    /* DELFT3DFM-453 */ 
                    log.WarnFormat("Importing bathymetry. Existing grid will be overwritten with grid from {0}.", path);
                    return GetModelForGrid(flowModel.SpatialData.Bathymetry.Grid);
                }
                default:
                    return null;
            }
        }

        private static void UpdatePersistedGrid(IWaterFlowFMModel model,
                                          string newGridPath)
        {
            string destFileName = Path.Combine(Path.GetDirectoryName(model.NetFilePath), 
                                               Path.GetFileName(newGridPath));

            if (Path.GetFullPath(destFileName) == Path.GetFullPath(newGridPath))
            {
                return;
            }

            File.Copy(newGridPath, destFileName, true);
            model.ModelDefinition
                 .GetModelProperty(KnownProperties.NetFile)
                 .SetValueFromString(Path.GetFileName(destFileName));

            model.MarkOutputOutOfSync();
        }
    }
}