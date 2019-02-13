using System.Collections.Generic;
using System.Linq;
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
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Forms
{
    [TestFixture]
    public class DHydroExporterDialogTest
    {
        private readonly MockRepository mocks = new MockRepository();

        private class DHydroExporterDialogStub : DHydroExporterDialog
        {
            protected override DialogResult ShowSaveFileDialog()
            {
                return DialogResult.OK;
            }
        }

        [TearDown]
        public void TearDown()
        {
            mocks.VerifyAll();
        }

        [Test]
        public void GivenHydroModelWithDimrModelAsActivity_WhenShowingModal_ThenNoWarningMessageIsShownToUser()
        {
            // Given
            var dimrModel = GetMockedDimrModel();
            var currentWorkFlow = GetMockedCurrentWorkFlow(dimrModel);

            var integratedModel = mocks.DynamicMultiMock<IHydroModel>(typeof(ICompositeActivity));
            integratedModel.Expect(m => ((ICompositeActivity)m).Activities).Return(new EventedList<IActivity> { dimrModel }).Repeat.Any();
            integratedModel.Expect(m => ((ICompositeActivity)m).CurrentWorkflow).Return(currentWorkFlow).Repeat.Any();

            var hydroExporterDialog = GetDHydroExporterDialog(integratedModel);

            // When - Then
            TestHelper.AssertLogMessagesCount(() => hydroExporterDialog.ShowModal(), 0);
        }

        private IDimrModel GetMockedDimrModel()
        {
            var waterFlowModel1D = mocks.DynamicMock<IDimrModel>();
            waterFlowModel1D.Expect(m => m.ExporterType).Return(typeof(DHydroConfigXmlExporter)).Repeat.Any();
            return waterFlowModel1D;
        }

        private ICompositeActivity GetMockedCurrentWorkFlow(IDimrModel dimrModel)
        {
            var currentWorkFlow = mocks.DynamicMock<ICompositeActivity>();
            currentWorkFlow.Expect(w => w.Activities).Return(new EventedList<IActivity> {dimrModel}).Repeat.Any();
            return currentWorkFlow;
        }

        [Test]
        public void GivenHydroModelWithNonDimrModelAsActivity_WhenShowingModal_ThenWarningMessageIsShownToUser()
        {
            // Given
            var hydroModel = new HydroModel();
            var waterQualityModel = new WaterQualityModel.WaterQualityModel();
            hydroModel.Activities.Add(waterQualityModel);
            hydroModel.Activities.Add(new WaterFlowModel1D());
            hydroModel.CurrentWorkflow = hydroModel.Workflows.FirstOrDefault();

            var hydroExporterDialog = GetDHydroExporterDialog(hydroModel);

            // When - Then
            var expectedWarningMessage = string.Format(Resources.DHydroExporterDialog_WarnForModelsWhichCannotBeExportedByDimr_Activity_of_type__0__cannot_be_exported_to_DIMR_file_tree_and_shall_be_ignored_,
                waterQualityModel.GetType());
            TestHelper.AssertLogMessageIsGenerated(() => hydroExporterDialog.ShowModal(), expectedWarningMessage, 1);
        }

        [Test]
        public void GivenIHydroModelAsSelectedModel_WhenShowingModal_ThenDialogResultIsEqualToCancel()
        {
            // Given
            var gui = mocks.DynamicMock<IGui>();
            var application = mocks.DynamicMock<IApplication>();

            gui.Expect(g => g.SelectedModel).Return(mocks.DynamicMock<IHydroModel>()).Repeat.Any();
            gui.Expect(g => g.Application).Return(application).Repeat.Any();
            application.Expect(a => a.FileExporters).Return(new IFileExporter[]{new DHydroConfigXmlExporter()}).Repeat.Any();

            mocks.ReplayAll();

            var hydroExporterDialog = new DHydroExporterDialogStub
            {
                Gui = gui
            };

            // When
            var dialogResult = hydroExporterDialog.ShowModal();

            // Then
            Assert.That(dialogResult, Is.EqualTo(DelftDialogResult.Cancel));
        }

        [Test]
        public void GivenIDimrModelAsSelectedModel_WhenShowingModalWithoutDHydroConfigXmlExporter_ThenDialogResultIsEqualToCancel()
        {
            // Given
            var gui = mocks.DynamicMock<IGui>();
            var application = mocks.DynamicMock<IApplication>();

            gui.Expect(g => g.SelectedModel).Return(mocks.DynamicMock<IDimrModel>()).Repeat.Any();
            gui.Expect(g => g.Application).Return(application).Repeat.Any();
            application.Expect(a => a.FileExporters).Return(new IFileExporter[0]).Repeat.Any();

            mocks.ReplayAll();

            var hydroExporterDialog = new DHydroExporterDialogStub
            {
                Gui = gui
            };

            // When
            var dialogResult = hydroExporterDialog.ShowModal();

            // Then
            Assert.That(dialogResult, Is.EqualTo(DelftDialogResult.Cancel));
        }

        private DHydroExporterDialogStub GetDHydroExporterDialog(IHydroModel hydroModel)
        {
            var gui = mocks.DynamicMock<IGui>();
            var application = mocks.DynamicMock<IApplication>();
            var fileExporters = new List<IFileExporter> {new DHydroConfigXmlExporter()};
            var viewResolver = mocks.DynamicMock<IViewResolver>();

            gui.Expect(g => g.SelectedModel).Return(hydroModel).Repeat.Any();
            gui.Expect(g => g.Application).Return(application).Repeat.Any();
            gui.Expect(g => g.DocumentViewsResolver).Return(viewResolver).Repeat.Any();
            application.Expect(a => a.FileExporters).Return(fileExporters).Repeat.Any();
            viewResolver.Expect(vr => vr.CreateViewForData(typeof(WaterFlowModel1DExporter))).IgnoreArguments().Return(null)
                .Repeat.Any();

            mocks.ReplayAll();

            var hydroExporterDialog = new DHydroExporterDialogStub
            {
                Gui = gui
            };
            return hydroExporterDialog;
        }
    }
}