using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;


namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public class RasterFileImporter : IFileImporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RasterFileImporter));
        private static readonly Dictionary<Type, Func<object, WaterFlowFMModel>> GetModelFunctions = new Dictionary<Type, Func<object, WaterFlowFMModel>>();
        private static readonly Dictionary<Type, Func<string, object, Func<object, WaterFlowFMModel>, Action<object>, object>> RasterImporterFunctions = new Dictionary<Type, Func<string, object, Func<object, WaterFlowFMModel>, Action<object>, object>>();

        public void RegisterGetModelFunction<T>(Func<T, WaterFlowFMModel> function)
        {
            if (GetModelFunctions.ContainsKey(typeof(T)))
                GetModelFunctions[typeof(T)] = o => function((T) o);
            else
                GetModelFunctions.Add(typeof(T), o => function((T) o));
        }
        private static void RegisterRasterImporterFunction<T>(Func<string, T, Func<T, WaterFlowFMModel>, Action<object>, T> function)
        {
            if (RasterImporterFunctions.ContainsKey(typeof(T)))
                RasterImporterFunctions[typeof(T)] = (path, target, getModelFunction, makeLayerVisibleAction) => (T) function(path, (T) target, typeToGetModelFor => getModelFunction((T) typeToGetModelFor), makeLayerVisibleAction);
            else
                RasterImporterFunctions.Add(typeof(T),
                    (path, target, getModelFunction, makeLayerVisibleAction) => (T) function(path, (T) target,
                        typeToGetModelFor => getModelFunction((T) typeToGetModelFor), makeLayerVisibleAction));
        }

        static RasterFileImporter()
        {
            RegisterRasterImporterFunction<UnstructuredGrid>(ImportRasterFileInGridOfFlowFMModel);
            RegisterRasterImporterFunction<UnstructuredGridCoverage>(ImportRasterFileInBathymetryOfFlowFMModel);
            RegisterRasterImporterFunction<WaterFlowFMModel>(ImportRasterFileInFlowFMModel);
        }

        private static WaterFlowFMModel ImportRasterFileInFlowFMModel(string path, WaterFlowFMModel flowModel, Func<WaterFlowFMModel, WaterFlowFMModel> getModelForFmModel, Action<object> makeLayerVisibleAction)
        {
            if (flowModel.Grid.Cells.Any())
            {
                Log.Error(Resources
                    .RasterFileImporter_ImportItem_There_is_already_a_grid_present__Remove_the_current_grid_before_importing_a_new_one_);
                return null;
            }

            SetGrid(path, flowModel);
            makeLayerVisibleAction?.Invoke(flowModel.Grid);
            SetBedLevel(path, flowModel);
            makeLayerVisibleAction?.Invoke(flowModel.Bathymetry);
            flowModel.ReloadGrid(false);
            return flowModel;
        }

        private static UnstructuredGridCoverage ImportRasterFileInBathymetryOfFlowFMModel(string netFilePath, UnstructuredGridCoverage bathymetry, Func<UnstructuredGridCoverage, WaterFlowFMModel> getModelForBathemetry, Action<object> makeLayerVisibleAction)
        {
            if (bathymetry == null || bathymetry.Name != WaterFlowFMModelDefinition.BathymetryDataItemName || getModelForBathemetry == null) return bathymetry;

            var flowModel = getModelForBathemetry(bathymetry);
            if (flowModel?.Grid != null && flowModel.Grid.IsEmpty)
            {
                Log.Error("Import of bed level canceled. You do not have a grid in your model to import bed levels on. Create or import a grid first.");
                return bathymetry;
            }

            bool verticesEqual;
            bool cellsEqual;
            bool linksEqual;
            var grid = RasterFile.ReadUnstructuredGrid(netFilePath);
            UnstructuredGridHelper.CompareGrids(flowModel.Grid, grid, out verticesEqual, out cellsEqual, out linksEqual);
            if (!verticesEqual || !cellsEqual)
            {
                Log.Error($"Import of bed level canceled. The file at '{netFilePath}' does not contain the same grid data as the grid you currently have in your model.\nImport this file as a spatial operation instead and interpolate.");
                return bathymetry;
            }

            SetBedLevel(netFilePath, flowModel);
            return flowModel.Bathymetry;
        }

        private static UnstructuredGrid ImportRasterFileInGridOfFlowFMModel(string path, UnstructuredGrid grid, Func<UnstructuredGrid, WaterFlowFMModel> getModelForGrid, Action<object> makeLayerVisibleAction)
        {
            if (grid != null && getModelForGrid != null)
            {
                var flowModel = getModelForGrid(grid);
                if (flowModel == null)
                {
                    Log.Error("There is no model present for the selected grid. Import canceled.");
                    return grid;
                }
                SetGrid(path, flowModel);
                makeLayerVisibleAction?.Invoke(flowModel.Grid);
                return flowModel.Grid;
            }
            return grid;
        }

        private static void SetGrid(string path, WaterFlowFMModel flowModel)
        {
            var grid = RasterFile.ReadUnstructuredGrid(path);
            if (grid != null)
            {
                flowModel.Grid = grid;
                flowModel.ReloadGrid();
            }
        }

        private static void SetBedLevel(string path, WaterFlowFMModel flowModel)
        {
            var bedlevels = RasterFile.ReadPointValues(path);
            if (bedlevels != null)
            {
                var bedLevelTypeProperty = flowModel.ModelDefinition.Properties.FirstOrDefault(p =>
                    p.PropertyDefinition != null &&
                    p.PropertyDefinition.MduPropertyName.ToLower() == KnownProperties.BedlevType);

                if (bedLevelTypeProperty == null)
                {
                    Log.WarnFormat("Cannot determine Bed level location, z-values will not be exported");
                }
                else
                {
                    var location = (UnstructuredGridFileHelper.BedLevelLocation)bedLevelTypeProperty.Value;
                    BathymetryFileWriter.Write(flowModel.NetFilePath, location, bedlevels.Select(bl => bl.Value).ToArray());
                    flowModel.Bathymetry.Arguments[0].Clear();
                    flowModel.Bathymetry.Components[0].Clear(); 
                    flowModel.UpdateBathymetryCoverage(location);
                    flowModel.ReloadGrid(false);
                    
                }
            }
        }
        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public object ImportItem(string path, object target = null)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(string.Format("Could not find file {0}", path));
            }

            if (target == null)
            {
                //just return a dataitem with the grid in it
                return new DataItem { Value = new ImportedFMNetFile(path), Name = Path.GetFileName(path) };
            }
            var getModelFunctionKey = GetModelFunctions.Keys.FirstOrDefault(k => k.IsInstanceOfType(target));
            var importerFunctionKey = RasterImporterFunctions.Keys.FirstOrDefault(k => k.IsInstanceOfType(target));
            if (importerFunctionKey == null) return null;
            return RasterImporterFunctions[importerFunctionKey](path, target, getModelFunctionKey != null ? GetModelFunctions[getModelFunctionKey] : null, MakeLayerVisibleAfterImport);
        }


        public string Name
        {
            get { return "Raster Grid Importer"; }
        }
        public string Description { get { return Name; } }
        public string Category
        {
            get {return "2D / 3D";}
        }

        public Bitmap Image { get; }

        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(WaterFlowFMModel);
                yield return typeof(UnstructuredGrid);
                yield return typeof(UnstructuredGridCoverage);
            }
        }
        public bool CanImportOnRootLevel { get; }

        public string FileFilter
        {
            get { string fileFilter = "";
                fileFilter += "All supported raster formats|*.asc;*.bil;*.tif;*.tiff;*.map";
                fileFilter += "|" + "Arc/Info ASCII Grid (*.asc)|*.asc";
                fileFilter += "|" + "ESRI .hdr Labelled (*.bil)|*.bil";
                fileFilter += "|" + "TIF Tagget Image File Format (*.tif)|*.tif;*.tiff";
                fileFilter += "|" + "PCRaster raster file format (*.map)|*.map"; ;
                return fileFilter;
            }
           
        }

        public string TargetDataDirectory { get; set; }
        public bool ShouldCancel { get; set; }
        public ImportProgressChangedDelegate ProgressChanged { get; set; }
        public bool OpenViewAfterImport { get; }
        public Action<object> MakeLayerVisibleAfterImport { get; set; }
    }
}
