using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.Common.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.Common.Gui.RgfGrid;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using NetTopologySuite.Extensions.Grids;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters
{
    public class FmModelTreeShortcutNodePresenter : ModelTreeShortcutNodePresenterBase<FmModelTreeShortcut>
    {
        protected override void OpenGridEditor(FmModelTreeShortcut shortcut)
        {
            var flowFmModel = shortcut.FlowFmModel;
            
            // Write current state of land-boundaries to file
            var writer = new MduFile();
            var targetMduFilePath = flowFmModel.MduFilePath;
            writer.WriteLandBoundaries(targetMduFilePath, flowFmModel.ModelDefinition, flowFmModel.Area);

            // Get land-boundaries file names
            var modelLdbPaths = flowFmModel.ModelDefinition.GetModelProperty(KnownProperties.LandBoundaryFile).GetFileNames(".ldb", ' ');
            var paths = modelLdbPaths.Select(modelLdbPath => Path.Combine(Path.GetDirectoryName(targetMduFilePath), modelLdbPath));

            RgfGridEditor.OpenGrid(flowFmModel.NetFilePath, flowFmModel.Grid == null || flowFmModel.Grid.IsEmpty, paths, flowFmModel.CoordinateSystem);
            ReloadGrid(flowFmModel);
        }

        private void ReloadGrid(WaterFlowFMModel model)
        {
            try
            {
                Gui?.MainWindow?.SetWaitCursorOn();

                // D3DFMIQ-16: This if-statement should be removed after the fix in DELFT3DFM-1413, where the user should be 
                // prompted by RGFGRID if he/she wants to save the grid.
                if (File.Exists(model.NetFilePath) && new FileInfo(model.NetFilePath).Length == 0)
                {
                    throw new FileFormatException(new Uri(model.NetFilePath),
                        "Empty file detected. Changes in the grid were not saved.\nPlease save your project before exiting RGFGRID.");
                }

                if (!File.Exists(model.NetFilePath))
                {
                    model.RemoveGrid();
                    return;
                }

                var currentCoordinateSystem = model.CoordinateSystem;
                var targetCoordinateSystem = UnstructuredGridFileHelper.GetCoordinateSystem(model.NetFilePath);

                if (currentCoordinateSystem != targetCoordinateSystem &&
                    (currentCoordinateSystem == null ||
                     targetCoordinateSystem == null ||
                     currentCoordinateSystem.IsGeographic != targetCoordinateSystem.IsGeographic)
                )
                {
                    model.CoordinateSystem = targetCoordinateSystem;
                }

                model.ReloadGrid(false);
            }
            // D3DFMIQ-16: This catch block should be removed after the fix in DELFT3DFM-1413, where the user should be 
            // prompted by RGFGRID if he/she wants to save the grid.
            catch (FileFormatException exception)
            {
                DelftTools.Controls.Swf.MessageBox.Show(exception.Message, "Grid was not saved in RGFGRID", MessageBoxButtons.OK);
                model.Grid = NetFileImporter.ImportGrid(model.NetFilePath) ?? new UnstructuredGrid();
            }
            catch (Exception exception)
            {
                var dialogResult = DelftTools.Controls.Swf.MessageBox.Show(
                    "Failed to reload grid after RGFGrid edits: " + exception.Message + Environment.NewLine +
                    "Continue with new grid?", "Failed to reload grid.", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    model.Grid = NetFileImporter.ImportGrid(model.NetFilePath) ?? new UnstructuredGrid();
                }
                else
                {
                    if (File.Exists(model.NetFilePath)) File.Delete(model.NetFilePath);
                    model.WriteNetFile(model.NetFilePath);
                }
            }
            finally
            {
                var mapView = (Gui?.DocumentViews?.ActiveView as ProjectItemMapView)?.MapView;
                mapView?.Map.ZoomToExtents();
                Gui?.MainWindow?.SetWaitCursorOff();
            }
        }
    }
}