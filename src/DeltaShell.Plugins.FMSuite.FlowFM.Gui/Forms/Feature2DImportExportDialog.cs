using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO.ImportExport;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Extensions.CoordinateSystems;
using SharpMap;
using SharpMap.Extensions.CoordinateSystems;
using SharpMap.UI.Forms;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    public partial class Feature2DImportExportDialog : Form, IConfigureDialog, IView
    {
        public Feature2DImportExportDialog()
        {
            InitializeComponent();
        }

        public bool ImportMode { get; set; }

        public ICoordinateSystem ModelCoordinateSystem { get; set; }

        public string FileFilter { get; set; }

        private ICoordinateTransformation CoordinateTransformation { get; set; }

        #region IConfigureDialog

        public string Title { get; set; }

        public DelftDialogResult ShowModal()
        {
            if (ImportMode)
            {
                importFileNames = new FileDialogService().SelectFiles(FileFilter);
                if (importFileNames == null)
                {
                    importFileNames = new string[0];
                    return DelftDialogResult.Cancel;
                }

                var coordinateDialog = new CoordinateConversionDialog(ModelCoordinateSystem, ModelCoordinateSystem,
                                                                      Map.CoordinateSystemFactory.SupportedCoordinateSystems,
                                                                      (f, t) =>
                                                                          new OgrCoordinateSystemFactory()
                                                                              .CreateTransformation(f, t));
                if (coordinateDialog.ShowDialog() != DialogResult.OK)
                {
                    return DelftDialogResult.Cancel;
                }

                CoordinateTransformation = coordinateDialog.ResultTransformation;

                return DelftDialogResult.OK;
            }
            else
            {
                saveFileDialog.Filter = FileFilter;
                saveFileDialog.FileName = null;
                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return DelftDialogResult.Cancel;
                }

                var coordinateDialog = new CoordinateConversionDialog(ModelCoordinateSystem, ModelCoordinateSystem,
                                                                      Map.CoordinateSystemFactory.SupportedCoordinateSystems,
                                                                      (f, t) =>
                                                                          new OgrCoordinateSystemFactory()
                                                                              .CreateTransformation(f, t));
                coordinateDialog.SwitchToExportDialog();
                if (coordinateDialog.ShowDialog() != DialogResult.OK)
                {
                    return DelftDialogResult.Cancel;
                }

                CoordinateTransformation = coordinateDialog.ResultTransformation;

                return DelftDialogResult.OK;
            }
        }

        public DelftDialogResult ShowModal(object owner)
        {
            return ShowModal();
        }

        public void Configure(object item)
        {
            var feature2DImporterExporter = item as IFeature2DImporterExporter;
            if (feature2DImporterExporter != null)
            {
                feature2DImporterExporter.Files = ImportMode
                                                      ? importFileNames
                                                      : new[]
                                                      {
                                                          saveFileDialog.FileName
                                                      };
                feature2DImporterExporter.CoordinateTransformation = CoordinateTransformation;
                feature2DImporterExporter.ShouldReplace = (originalFeature, newFeature) =>
                {
                    var boundaryConditionSet = originalFeature as BoundaryConditionSet;
                    if (boundaryConditionSet == null || !boundaryConditionSet.ContainsData())
                    {
                        return true;
                    }

                    return MessageBox.Show($"Overwrite boundary condition set {boundaryConditionSet.Name} and loose all data?",
                                           "Overwrite feature data", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
                };
            }
        }

        #endregion

        #region IView

        public object Data { get; set; }

        public Image Image { get; set; }

        public void EnsureVisible(object item)
        {
            // Nothing to be done, enforced through IView
        }

        public ViewInfo ViewInfo { get; set; }

        #endregion
    }
}