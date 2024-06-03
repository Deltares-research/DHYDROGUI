using System;
using System.Collections.Generic;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Services;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms;
using DeltaShell.Plugins.DelftModels.HydroModel.Properties;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Forms
{
    [TestFixture]
    public class DHydroExporterDialogTest
    {
        [Test]
        public void GivenIntegratedModelWithDimrModelAsActivityAndNoFileExporterRegistered_WhenShowingModal_ThenWarningMessageIsShownToUser()
        {
            // Given
            IDimrModel dimrModel = GetMockedDimrModel();
            ICompositeActivity currentWorkFlow = GetMockedCurrentWorkFlow(dimrModel);
            IHydroModel integratedModel = GetMockedIntegratedModel(currentWorkFlow, dimrModel);

            var fileExporters = new List<IFileExporter> { GetDHydroConfigXmlExporter() };
            DHydroExporterDialog hydroExporterDialog = GetDHydroExporterDialog(integratedModel, fileExporters);

            // When - Then
            string expectedWarningMessage = string.Format(Resources.DHydroExporterDialog_DimrSubModelsExportDialogResult_No_file_exporter_found_for_model___0___, dimrModel.Name);
            TestHelper.AssertLogMessageIsGenerated(() => hydroExporterDialog.ShowModal(), expectedWarningMessage, 1);
        }
        
        [Test]
        public void GivenIntegratedModelWithDimrModelAsActivity_WhenShowingModal_ThenNoWarningMessageIsShownToUser()
        {
            // Given
            IDimrModel dimrModel = GetMockedDimrModel();
            ICompositeActivity currentWorkFlow = GetMockedCurrentWorkFlow(dimrModel);
            IHydroModel integratedModel = GetMockedIntegratedModel(currentWorkFlow, dimrModel);

            var fileExporters = new List<IFileExporter> { GetDHydroConfigXmlExporter(), GetDimrModelFileExporter() };
            DHydroExporterDialog hydroExporterDialog = GetDHydroExporterDialog(integratedModel, fileExporters);

            // When - Then
            TestHelper.AssertLogMessagesCount(() => hydroExporterDialog.ShowModal(), 0);
        }

        [Test]
        public void GivenIntegratedModelWithNonDimrModelAsActivity_WhenShowingModal_ThenWarningMessageIsShownToUser()
        {
            // Given
            IDimrModel dimrModel = GetMockedDimrModel();
            var nonDimrModel = Substitute.For<IModel>();
            ICompositeActivity currentWorkFlow = GetMockedCurrentWorkFlow(dimrModel, nonDimrModel);
            IHydroModel integratedModel = GetMockedIntegratedModel(currentWorkFlow, dimrModel, nonDimrModel);

            var fileExporters = new List<IFileExporter> { GetDHydroConfigXmlExporter(), GetDimrModelFileExporter() };
            DHydroExporterDialog hydroExporterDialog = GetDHydroExporterDialog(integratedModel, fileExporters);

            // When - Then
            string expectedWarningMessage = string.Format(Resources.DHydroExporterDialog_WarnForModelsWhichCannotBeExportedByDimr_Activity_of_type__0__cannot_be_exported_to_DIMR_file_tree_and_shall_be_ignored_,
                                                          nonDimrModel.GetType());
            TestHelper.AssertLogMessageIsGenerated(() => hydroExporterDialog.ShowModal(), expectedWarningMessage, 1);
        }

        [Test]
        public void GivenNonDimrOrIntegratedModelAsSelectedModel_WhenShowingModal_ThenDialogResultIsEqualToCancel()
        {
            // Given
            var selectedModel = Substitute.For<IModel>();
            var fileExporters = new IFileExporter[] { GetDHydroConfigXmlExporter(), GetDimrModelFileExporter() };
            DHydroExporterDialog hydroExporterDialog = GetDHydroExporterDialog(selectedModel, fileExporters);

            // When
            DelftDialogResult dialogResult = hydroExporterDialog.ShowModal();

            // Then
            Assert.That(dialogResult, Is.EqualTo(DelftDialogResult.Cancel));
        }

        [Test]
        public void GivenIDimrModelAsSelectedModel_WhenShowingModalWithoutDHydroConfigXmlExporter_ThenDialogResultIsEqualToCancel()
        {
            // Given
            var selectedModel = Substitute.For<IDimrModel>();
            IFileExporter[] fileExporters = Array.Empty<IFileExporter>();
            DHydroExporterDialog hydroExporterDialog = GetDHydroExporterDialog(selectedModel, fileExporters);

            // When
            DelftDialogResult dialogResult = hydroExporterDialog.ShowModal();

            // Then
            Assert.That(dialogResult, Is.EqualTo(DelftDialogResult.Cancel));
        }

        [Test]
        public void GivenWithoutSelectedModel_WhenShowingModalWithoutDHydroConfigXmlExporter_ThenDialogResultIsEqualToCancel()
        {
            // Given
            var fileExporters = new IFileExporter[] { GetDHydroConfigXmlExporter(), GetDimrModelFileExporter() };
            DHydroExporterDialog hydroExporterDialog = GetDHydroExporterDialog(null, fileExporters);

            // When
            DelftDialogResult dialogResult = hydroExporterDialog.ShowModal();

            // Then
            Assert.That(dialogResult, Is.EqualTo(DelftDialogResult.Cancel));
        }

        private static IDimrModel GetMockedDimrModel()
        {
            return Substitute.For<IDimrModel>();
        }

        private static ICompositeActivity GetMockedCurrentWorkFlow(params IModel[] models)
        {
            var currentWorkFlow = Substitute.For<ICompositeActivity>();
            currentWorkFlow.Activities.Returns(new EventedList<IActivity>(models));
            return currentWorkFlow;
        }

        private static IHydroModel GetMockedIntegratedModel(ICompositeActivity currentWorkFlow, params IActivity[] modelActivities)
        {
            ICompositeActivity integratedModel = Substitute.For<ICompositeActivity, IHydroModel>();
            integratedModel.Activities.Returns(new EventedList<IActivity>(modelActivities));
            integratedModel.CurrentWorkflow.Returns(currentWorkFlow);

            return (IHydroModel)integratedModel;
        }

        private static DHydroConfigXmlExporter GetDHydroConfigXmlExporter()
        {
            var fileExportService = Substitute.For<IFileExportService>();
            return new DHydroConfigXmlExporter(fileExportService);
        }

        private static IDimrModelFileExporter GetDimrModelFileExporter()
        {
            return Substitute.For<IDimrModelFileExporter>();
        }

        private static DHydroExporterDialog GetDHydroExporterDialog(IModel selectedModel, IReadOnlyList<IFileExporter> fileExporters)
        {
            var gui = Substitute.For<IGui>();
            var application = Substitute.For<IApplication>();
            var viewResolver = Substitute.For<IViewResolver>();
            var fileExportService = Substitute.For<IFileExportService>();

            gui.SelectedModel.Returns(selectedModel);
            gui.Application.Returns(application);
            gui.DocumentViewsResolver.Returns(viewResolver);
            
            fileExportService.FileExporters.Returns(fileExporters);
            fileExportService.GetFileExportersFor(Arg.Any<IDimrModel>()).Returns(fileExporters);
            application.FileExportService.Returns(fileExportService);

            var folderDialogService = Substitute.For<IFolderDialogService>();
            folderDialogService.ShowSelectFolderDialog(Arg.Any<FolderDialogOptions>()).Returns("some_dimr_export_dir");
            
            return new DHydroExporterDialog {Gui = gui, FolderDialogService = folderDialogService};
        }
    }
}