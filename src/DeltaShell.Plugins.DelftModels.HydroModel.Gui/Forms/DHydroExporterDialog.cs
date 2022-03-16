using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core;
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

        public DelftDialogResult ShowModal()
        {
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

            switch (SelectedModel)
            {
                case ICompositeActivity integratedModel:
                    return ExportSubDimrModels(integratedModel);
                case IDimrModel dimrModel:
                {
                    var dimrModelExporter = (IFileExporter) Activator.CreateInstance(dimrModel.ExporterType);
                    return DimrSubModelsExportDialogResult(dimrModelExporter, dimrModel);
                }
                default:
                    return DelftDialogResult.Cancel;
            }
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

        private IModel SelectedModel => Gui?.SelectedModel;

        private DHydroConfigXmlExporter Exporter
        {
            get
            {
                return Gui?.Application.FileExporters.OfType<DHydroConfigXmlExporter>().FirstOrDefault();
            }
        }

        private DelftDialogResult ExportSubDimrModels(ICompositeActivity hydroModel)
        {
            List<IDimrModel> dimrModels = hydroModel.CurrentWorkflow.Activities.GetActivitiesOfType<IDimrModel>()
                                                    .Plus(hydroModel.CurrentWorkflow as IDimrModel).Where(dm => dm != null).ToList();

            WarnForModelsWhichCannotBeExportedByDimr(hydroModel, dimrModels);

            foreach (IDimrModel dimrModel in dimrModels)
            {
                var dimrModelExporter = (IFileExporter) Activator.CreateInstance(dimrModel.ExporterType);
                DelftDialogResult resultOfSubModelExportDialog = DimrSubModelsExportDialogResult(dimrModelExporter, dimrModel);
                if (resultOfSubModelExportDialog == DelftDialogResult.Cancel)
                {
                    return DelftDialogResult.Cancel;
                }
            }

            return DelftDialogResult.OK;
        }

        private static void WarnForModelsWhichCannotBeExportedByDimr(ICompositeActivity hydroModel, IEnumerable<IDimrModel> dimrModels)
        {
            IEnumerable<IActivity> remainingActivities = hydroModel.Activities.Select(UnwrapActivity).Except(dimrModels);

            foreach (IActivity remainingActivity in remainingActivities)
            {
                Log.WarnFormat(Resources.DHydroExporterDialog_WarnForModelsWhichCannotBeExportedByDimr_Activity_of_type__0__cannot_be_exported_to_DIMR_file_tree_and_shall_be_ignored_,
                               remainingActivity.GetType());
            }
        }

        private DelftDialogResult DimrSubModelsExportDialogResult(IFileExporter dimrModelExporter, IDimrModel dimrModel)
        {
            var configureDialog = Gui.DocumentViewsResolver.CreateViewForData(dimrModelExporter) as IPartitionDialog;
            if (configureDialog != null)
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
            while (result is ActivityWrapper)
            {
                result = ((ActivityWrapper) result).Activity;
            }

            return result;
        }
    }
}