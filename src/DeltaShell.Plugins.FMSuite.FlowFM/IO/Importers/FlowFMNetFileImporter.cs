using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using SharpMap.CoordinateSystems;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public class FlowFMNetFileImporter: IFileImporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FlowFMNetFileImporter));
        
        public Func<UnstructuredGrid,WaterFlowFMModel> GetModelForGrid { get; set; }

        #region IFileImporter 

        public string Name { get { return "Unstructured Grid"; } }

        public string Category { get { return "2D / 3D"; } }

        public Bitmap Image { get { return Properties.Resources.unstruc; } }

        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof (UnstructuredGrid);
            }
        }

        public bool CanImportOn(object targetObject)
        {
            if (targetObject == null) return true; //import on root level

            UnstructuredGrid grid = null;
            var unstructuredGrid = targetObject as UnstructuredGrid;
            if (unstructuredGrid != null)
            {
                grid = unstructuredGrid;
            }
            return grid != null && GetModelForGrid != null && GetModelForGrid(grid) != null;
        }

        public bool CanImportOnRootLevel { get { return true; }}

        public string FileFilter { get { return "Net file|*.nc"; }}
        
        public string TargetDataDirectory { get; set; }
        
        public bool ShouldCancel { get; set; }
        
        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public bool OpenViewAfterImport
        {
            get { return true; }
        }

        public object ImportItem(string path, object target = null)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(string.Format("Could not find file {0}", path));
            }

            if (target == null)
            {
                return new DataItem {Value = new ImportedFMNetFile(path), Name = Path.GetFileName(path)};
            }

            var flowModel = target as WaterFlowFMModel;
            
            var grid = target as UnstructuredGrid;

            if (grid != null && GetModelForGrid != null)
            {
                flowModel = GetModelForGrid(grid);
                /* DELFT3DFM-453 */
                Log.WarnFormat("Importing bathymetry. Existing grid will be overwritten with grid from {0}.", path);
                flowModel = GetModelForGrid(flowModel.Bathymetry.Grid);
            }
            if (flowModel != null && flowModel.NetFilePath != null)
            {
                var netfile = new ImportedFMNetFile(path);
                var coordinates = netfile.Grid.Vertices;
                if (flowModel.CoordinateSystem != null && 
                    !CoordinateSystemValidator.CanAssignCoordinateSystem(coordinates, flowModel.CoordinateSystem))
                {
                    throw new Exception("Grid coordinates are incompatible with current model coordinate system: {0}, canceling import.");
                }

                var destFileName = Path.Combine(Path.GetDirectoryName(flowModel.NetFilePath), Path.GetFileName(path));
                if (Path.GetFullPath(destFileName) != Path.GetFullPath(path))
                {
                    File.Copy(path, destFileName, true);
                    flowModel.ModelDefinition.GetModelProperty(KnownProperties.NetFile).SetValueAsString(Path.GetFileName(destFileName));
                }
                flowModel.ReloadGrid(false, true);

                return flowModel.Grid;
            }
            return null;
        }

        #endregion
    }
}
