using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        public string Title { get; set; }

        public IGui Gui { get; set; }

        IModel SelectedModel
        {
            get
            {
                if (Gui == null) return null;
                return Gui.SelectedModel;
            }
        }

        private DHydroConfigXmlExporter Exporter
        {
            get { return Gui == null ? null : Gui.Application.FileExporters.OfType<DHydroConfigXmlExporter>().FirstOrDefault(); }
        }

        public DelftDialogResult ShowModal()
        {
            var model = SelectedModel;
            if (!(model is HydroModel || model is IDimrModel))
            {
                return DelftDialogResult.Cancel;
            }

            if (Exporter == null)
            {
                return DelftDialogResult.Cancel;
            }

            var dialogResult = ShowSaveFileDialog();
            if (dialogResult != DialogResult.OK)
            {
                return DelftDialogResult.Cancel;
            }

            if (Exporter.CoreCountDictionary == null)
            {
                Exporter.CoreCountDictionary = new ConcurrentDictionary<IDimrModel, int>();
            }
            var hydroModel = model as HydroModel;
            if ( hydroModel != null )
            {
                var dimrModels = hydroModel.CurrentWorkflow.Activities.GetActivitiesOfType<IDimrModel>()
                    .Plus(hydroModel.CurrentWorkflow as IDimrModel).Where(dm => dm != null).ToList();

                WarnForModelsWhichCannotBeExportedByDimr(hydroModel, dimrModels);

                foreach (var dimrModel in dimrModels)
                {
                    var dimrModelExporter = (IFileExporter)Activator.CreateInstance(dimrModel.ExporterType);
                    var resultOfSubModelExportDialog = DimrSubModelsExportDialogResult(dimrModelExporter, dimrModel);
                    if (resultOfSubModelExportDialog == DelftDialogResult.Cancel)
                    {
                        return resultOfSubModelExportDialog;
                    }
                }
                return DelftDialogResult.OK;
            }
            var singleDimrModel = model as IDimrModel;
            if (singleDimrModel != null)
            {
                var dimrModelExporter = (IFileExporter)Activator.CreateInstance(singleDimrModel.ExporterType);
                return DimrSubModelsExportDialogResult(dimrModelExporter, singleDimrModel);
            }

            return DelftDialogResult.OK;
        }

        protected virtual DialogResult ShowSaveFileDialog()
        {
            var saveFileDialog = new SaveFileDialog { Filter = "DIMR config files (*.xml)|*.xml" };
            var dialogResult = saveFileDialog.ShowDialog();

            Exporter.ExportFilePath = saveFileDialog.FileName;
            return dialogResult;
        }

        private static void WarnForModelsWhichCannotBeExportedByDimr(ICompositeActivity hydroModel, IEnumerable<IDimrModel> dimrModels)
        {
            var remainingActivities = hydroModel.Activities.Select(UnwrapActivity).Except(dimrModels);

            foreach (var remainingActivity in remainingActivities)
            {
                Log.WarnFormat("Activity of type {0} cannot be exported to DIMR file tree and shall be ignored.",
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
            /*  Sobek3-641
            *  Disabled until it's clear what to do.
            */
            Exporter.CoreCountDictionary[dimrModel] = 0;
            return DelftDialogResult.OK;
            if (dimrModel.CanRunParallel)
            {
                var dialog = new InputTextDialog
                {
                    InitialText = "1",
                    Text = dimrModel.ShortName + " nr. of cores",
                    ValidationMethod = ValidateCoreCount,
                    ValidationErrorMsg = "Enter a valid number of cores",
                    CausesValidation = true,
                    StartPosition = FormStartPosition.CenterScreen
                };
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    int cores;
                    if (int.TryParse(dialog.EnteredText, out cores))
                    {
                        Exporter.CoreCountDictionary[dimrModel] = cores;
                    }
                }
                else
                {
                    return DelftTools.Controls.DelftDialogResult.Cancel;
                }
            }
        }

        private static IActivity UnwrapActivity(IActivity activity)
        {
            var result = activity;
            while (result is ActivityWrapper)
            {
                result = ((ActivityWrapper)result).Activity;
            }

            return result;
        }
        private static bool ValidateCoreCount(string text)
        {
            int coreCount;
            if (int.TryParse(text, out coreCount))
            {
                return coreCount > 0;
            }
            return false;
        }

        public DelftDialogResult ShowModal(object owner)
        {
            return ShowModal();
        }

        public void Configure(object model)
        {
            //Everything already done in ShowModal
        }

        public object Data { get; set; }
        
        public Image Image { get; set; }
        
        public void EnsureVisible(object item)
        {    
        }

        public ViewInfo ViewInfo { get; set; }
    }
}
