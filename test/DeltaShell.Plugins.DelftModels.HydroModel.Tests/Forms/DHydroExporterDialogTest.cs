using System;
using System.Collections.Generic;
using System.Windows.Forms;
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
            DHydroExporterDialogStub hydroExporterDialog = GetDHydroExporterDialog(integratedModel, fileExporters);

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
            DHydroExporterDialogStub hydroExporterDialog = GetDHydroExporterDialog(integratedModel, fileExporters);

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
            DHydroExporterDialogStub hydroExporterDialog = GetDHydroExporterDialog(integratedModel, fileExporters);

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
            DHydroExporterDialogStub hydroExporterDialog = GetDHydroExporterDialog(selectedModel, fileExporters);

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
            DHydroExporterDialogStub hydroExporterDialog = GetDHydroExporterDialog(selectedModel, fileExporters);

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
            DHydroExporterDialogStub hydroExporterDialog = GetDHydroExporterDialog(null, fileExporters);

            // When
            DelftDialogResult dialogResult = hydroExporterDialog.ShowModal();

            // Then
            Assert.That(dialogResult, Is.EqualTo(DelftDialogResult.Cancel));
        }

        private static IDimrModel GetMockedDimrModel()
        {
            var waterFlowModel1D = Substitute.For<IDimrModel>();
            return waterFlowModel1D;
        }

        private static ICompositeActivity GetMockedCurrentWorkFlow(params IModel[] models)
        {
            var currentWorkFlow = Substitute.For<ICompositeActivity>();
            currentWorkFlow.Activities.Returns(new EventedList<IActivity>(models));
            return currentWorkFlow;
        }

        private static IHydroModel GetMockedIntegratedModel(ICompositeActivity currentWorkFlow, params IActivity[] modelActivities)
        {
            IHydroModel integratedModel = Substitute.For<IHydroModel, ICompositeActivity>();
            ((ICompositeActivity)integratedModel).Activities.Returns(new EventedList<IActivity>(modelActivities));
            ((ICompositeActivity)integratedModel).CurrentWorkflow.Returns(currentWorkFlow);

            return integratedModel;
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

        private static DHydroExporterDialogStub GetDHydroExporterDialog(IModel selectedModel, IReadOnlyList<IFileExporter> fileExporters)
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

            var hydroExporterDialog = new DHydroExporterDialogStub { Gui = gui };
            return hydroExporterDialog;
        }

        private class DHydroExporterDialogStub : DHydroExporterDialog
        {
            protected override DialogResult ShowSaveFileDialog()
            {
                return DialogResult.OK;
            }
        }
    }
}