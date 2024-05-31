using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Services;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Collections;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.HydroModel.Properties;
using log4net;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms
{
    public partial class DHydroExporterDialog : Form, IConfigureDialog, IView
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DHydroExporterDialog));

        public DHydroExporterDialog()
        {
            InitializeComponent();
        }

        public IGui Gui { get; set; }

        [ExcludeFromCodeCoverage]
        public string Title { get; set; }

        [ExcludeFromCodeCoverage]
        public object Data { get; set; }

        [ExcludeFromCodeCoverage]
        public Image Image { get; set; }

        [ExcludeFromCodeCoverage]
        public ViewInfo ViewInfo { get; set; }

        private IModel SelectedModel => Gui?.SelectedModel;

        private IFileExportService FileExportService
            => Gui.Application.FileExportService;

        private DHydroConfigXmlExporter Exporter
            => FileExportService.FileExporters.OfType<DHydroConfigXmlExporter>().FirstOrDefault();

        private IDimrModelFileExporter GetDimrModelExporter(IDimrModel dimrModel)
            => FileExportService.GetFileExportersFor(dimrModel).OfType<IDimrModelFileExporter>().FirstOrDefault();

        public DelftDialogResult ShowModal()
        {
            IModel model = SelectedModel;
            if (!(model is HydroModel || model is IDimrModel))
            {
                return DelftDialogResult.Cancel;
            }

            if (Exporter == null)
            {
                return DelftDialogResult.Cancel;
            }

            DialogResult dialogResult = ShowSaveFileDialog();
            if (dialogResult != DialogResult.OK)
            {
                return DelftDialogResult.Cancel;
            }
            
            if (Exporter.CoreCountDictionary == null)
            {
                Exporter.CoreCountDictionary = new ConcurrentDictionary<IDimrModel, int>();
            }

            var hydroModel = model as HydroModel;
            if (hydroModel != null)
            {
                List<IDimrModel> dimrModels = hydroModel.CurrentWorkflow.Activities
                                                        .GetActivitiesOfType<IDimrModel>()
                                                        .Plus(hydroModel.CurrentWorkflow as IDimrModel)
                                                        .Where(dm => dm != null)
                                                        .ToList();

                WarnForModelsWhichCannotBeExportedByDimr(hydroModel, dimrModels);

                foreach (IDimrModel dimrModel in dimrModels)
                {
                    IDimrModelFileExporter dimrModelExporter = GetDimrModelExporter(dimrModel);
                    DelftDialogResult resultOfSubModelExportDialog = DimrSubModelsExportDialogResult(dimrModelExporter, dimrModel);
                    if (resultOfSubModelExportDialog == DelftDialogResult.Cancel)
                    {
                        return resultOfSubModelExportDialog;
                    }
                }

                return DelftDialogResult.OK;
            }

            if (model is IDimrModel singleDimrModel)
            {
                IDimrModelFileExporter dimrModelExporter = GetDimrModelExporter(singleDimrModel);
                return DimrSubModelsExportDialogResult(dimrModelExporter, singleDimrModel);
            }

            return DelftDialogResult.OK;
        }

        public DelftDialogResult ShowModal(object owner)
        {
            return ShowModal();
        }

        public void Configure(object item)
        {
            //Everything already done in ShowModal
        }

        public void EnsureVisible(object item)
        {
            // Nothing to be done, enforced through IView.
        }
        
        protected virtual DialogResult ShowSaveFileDialog()
        {
            var saveFileDialog = new SaveFileDialog {Filter = "DIMR config files (*.xml)|*.xml"};
            DialogResult dialogResult = saveFileDialog.ShowDialog();

            Exporter.ExportFilePath = saveFileDialog.FileName;
            return dialogResult;
        }

        private static void WarnForModelsWhichCannotBeExportedByDimr(HydroModel hydroModel, List<IDimrModel> dimrModels)
        {
            IEnumerable<IActivity> remainingActivities = hydroModel.Activities.Select(UnwrapActivity).Except(dimrModels);

            foreach (IActivity remainingActivity in remainingActivities)
            {
                Log.WarnFormat(Resources.Activity_of_type__0__cannot_be_exported_to_DIMR_file_tree_and_shall_be_ignored,
                               remainingActivity.GetType());
            }
        }

        private DelftDialogResult DimrSubModelsExportDialogResult(IFileExporter dimrModelExporter, IDimrModel dimrModel)
        {
            if (dimrModelExporter == null)
            {
                Log.WarnFormat(Resources.No_file_exporter_found_for_model, dimrModel.Name);
                return DelftDialogResult.Cancel;
            }

            if (Gui.DocumentViewsResolver.CreateViewForData(dimrModelExporter) is IPartitionDialog configureDialog)
            {
                if (configureDialog.ShowPartitionModal() == DelftDialogResult.OK)
                {
                    configureDialog.ConfigurePartition(dimrModelExporter);
                    Exporter.CoreCountDictionary[dimrModel] = configureDialog.CoreCount;
                    return DelftDialogResult.OK;
                }

                return DelftDialogResult.Cancel;
            }

            Exporter.CoreCountDictionary[dimrModel] = 0;
            return DelftDialogResult.OK;
        }

        private static IActivity UnwrapActivity(IActivity activity)
        {
            IActivity result = activity;
            while (result is ActivityWrapper wrapper)
            {
                result = wrapper.Activity;
            }

            return result;
        }
    }
}