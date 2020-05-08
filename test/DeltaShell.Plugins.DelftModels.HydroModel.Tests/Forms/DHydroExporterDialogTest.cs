using System.Collections.Generic;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms;
using DeltaShell.Plugins.DelftModels.HydroModel.Properties;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Forms
{
    [TestFixture]
    public class DHydroExporterDialogTest
    {
        private readonly MockRepository mocks = new MockRepository();

        [TearDown]
        public void TearDown()
        {
            mocks.VerifyAll();
        }

        [Test]
        public void GivenIntegratedModelWithDimrModelAsActivity_WhenShowingModal_ThenNoWarningMessageIsShownToUser()
        {
            // Given
            IDimrModel dimrModel = GetMockedDimrModel();
            ICompositeActivity currentWorkFlow = GetMockedCurrentWorkFlow(dimrModel);
            IHydroModel integratedModel = GetMockedIntegratedModel(currentWorkFlow, dimrModel);

            var fileExporters = new List<IFileExporter> {new DHydroConfigXmlExporter()};
            DHydroExporterDialogStub hydroExporterDialog = GetDHydroExporterDialog(integratedModel, fileExporters);

            // When - Then
            TestHelper.AssertLogMessagesCount(() => hydroExporterDialog.ShowModal(), 0);
        }

        [Test]
        public void GivenIntegratedModelWithNonDimrModelAsActivity_WhenShowingModal_ThenWarningMessageIsShownToUser()
        {
            // Given
            IDimrModel dimrModel = GetMockedDimrModel();
            var nonDimrModel = mocks.DynamicMock<IModel>();
            ICompositeActivity currentWorkFlow = GetMockedCurrentWorkFlow(dimrModel, nonDimrModel);
            IHydroModel integratedModel = GetMockedIntegratedModel(currentWorkFlow, dimrModel, nonDimrModel);

            var fileExporters = new List<IFileExporter> {new DHydroConfigXmlExporter()};
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
            var selectedModel = mocks.DynamicMock<IModel>();
            var fileExporters = new IFileExporter[]
            {
                new DHydroConfigXmlExporter()
            };
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
            var selectedModel = mocks.DynamicMock<IDimrModel>();
            var fileExporters = new IFileExporter[0];
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
            var fileExporters = new IFileExporter[]
            {
                new DHydroConfigXmlExporter()
            };
            DHydroExporterDialogStub hydroExporterDialog = GetDHydroExporterDialog(null, fileExporters);

            // When
            DelftDialogResult dialogResult = hydroExporterDialog.ShowModal();

            // Then
            Assert.That(dialogResult, Is.EqualTo(DelftDialogResult.Cancel));
        }

        private class DHydroExporterDialogStub : DHydroExporterDialog
        {
            protected override DialogResult ShowSaveFileDialog()
            {
                return DialogResult.OK;
            }
        }

        private IDimrModel GetMockedDimrModel()
        {
            var waterFlowModel1D = mocks.DynamicMock<IDimrModel>();
            waterFlowModel1D.Expect(m => m.ExporterType).Return(typeof(DHydroConfigXmlExporter)).Repeat.Any();
            return waterFlowModel1D;
        }

        private ICompositeActivity GetMockedCurrentWorkFlow(params IModel[] models)
        {
            var currentWorkFlow = mocks.DynamicMock<ICompositeActivity>();
            currentWorkFlow.Expect(w => w.Activities).Return(new EventedList<IActivity>(models)).Repeat.Any();
            return currentWorkFlow;
        }

        private IHydroModel GetMockedIntegratedModel(ICompositeActivity currentWorkFlow, params IActivity[] modelActivities)
        {
            var integratedModel = mocks.DynamicMultiMock<IHydroModel>(typeof(ICompositeActivity));
            integratedModel.Expect(m => ((ICompositeActivity) m).Activities).Return(new EventedList<IActivity>(modelActivities)).Repeat.Any();
            integratedModel.Expect(m => ((ICompositeActivity) m).CurrentWorkflow).Return(currentWorkFlow).Repeat.Any();

            return integratedModel;
        }

        private DHydroExporterDialogStub GetDHydroExporterDialog(IModel selectedModel, IEnumerable<IFileExporter> fileExporters)
        {
            var gui = mocks.DynamicMock<IGui>();
            var application = mocks.DynamicMock<IApplication>();
            var viewResolver = mocks.DynamicMock<IViewResolver>();

            gui.Expect(g => g.SelectedModel).Return(selectedModel).Repeat.Any();
            gui.Expect(g => g.Application).Return(application).Repeat.Any();
            gui.Expect(g => g.DocumentViewsResolver).Return(viewResolver).Repeat.Any();
            application.Expect(a => a.FileExporters).Return(fileExporters).Repeat.Any();

            mocks.ReplayAll();

            var hydroExporterDialog = new DHydroExporterDialogStub {Gui = gui};
            return hydroExporterDialog;
        }
    }
}