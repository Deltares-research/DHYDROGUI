using System;
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
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.HydroModel.Properties;
using log4net;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms
{
    public partial class DHydroExporterDialog : Form, IConfigureDialog, IView
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DHydroExporterDialog));
        private IFolderDialogService folderDialogService;

        public DHydroExporterDialog()
        {
            InitializeComponent();
        }

        public IGui Gui { get; set; }

        /// <summary>
        /// The folder dialog service for selecting the DIMR export folder.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="value"/> is <c>null</c>.
        /// </exception>
        public IFolderDialogService FolderDialogService
        {
            get => folderDialogService;
            set
            {
                Ensure.NotNull(value, nameof(value));
                folderDialogService = value;
            }
        }

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

            var folderDialogOptions = new FolderDialogOptions();
            string selectedPath = FolderDialogService.ShowSelectFolderDialog(folderDialogOptions);
            
            if (string.IsNullOrEmpty(selectedPath))
            {
                return DelftDialogResult.Cancel;
            }

            Exporter.ExportDirectoryPath = selectedPath;

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
                    IDimrModelFileExporter dimrModelExporter = GetDimrModelExporter(dimrModel);
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

        private IModel SelectedModel => Gui?.SelectedModel;

        private IFileExportService FileExportService
            => Gui.Application.FileExportService;

        private DHydroConfigXmlExporter Exporter
            => FileExportService.FileExporters.OfType<DHydroConfigXmlExporter>().FirstOrDefault();

        private IDimrModelFileExporter GetDimrModelExporter(IDimrModel dimrModel)
            => FileExportService.GetFileExportersFor(dimrModel).OfType<IDimrModelFileExporter>().FirstOrDefault();

        private DelftDialogResult ExportSubDimrModels(ICompositeActivity hydroModel)
        {
            List<IDimrModel> dimrModels = hydroModel.CurrentWorkflow.Activities.GetActivitiesOfType<IDimrModel>()
                                                    .Plus(hydroModel.CurrentWorkflow as IDimrModel).Where(dm => dm != null).ToList();

            WarnForModelsWhichCannotBeExportedByDimr(hydroModel, dimrModels);

            foreach (IDimrModel dimrModel in dimrModels)
            {
                IDimrModelFileExporter dimrModelExporter = GetDimrModelExporter(dimrModel);
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
            if (dimrModelExporter == null)
            {
                Log.WarnFormat(Resources.DHydroExporterDialog_DimrSubModelsExportDialogResult_No_file_exporter_found_for_model___0___, dimrModel.Name);
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
            while (result is ActivityWrapper)
            {
                result = ((ActivityWrapper) result).Activity;
            }

            return result;
        }
    }
}