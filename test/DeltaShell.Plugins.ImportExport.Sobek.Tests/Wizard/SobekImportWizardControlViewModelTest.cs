using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.ImportExport.Sobek.Wizard;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.Wizard
{
    [TestFixture]
    public class SobekImportWizardControlViewModelTest
    {
        [Test]
        public void GivenSobekImportWizardControlViewModel_SettingFilePath_ShouldWorkWithEmptyString()
        {
            //Arrange
            var viewmodel = new SobekImportWizardControlViewModel();

            // Act
            viewmodel.FilePath = "";

            // Assert
            Assert.IsFalse(viewmodel.HasFileSet);
            Assert.AreEqual("", viewmodel.FilePath);
            Assert.IsFalse(viewmodel.IsCaseList);
            Assert.AreEqual(0, viewmodel.ImportersWaterFlow1d.Count());
            Assert.AreEqual(0, viewmodel.ImportersRainfallRunoff.Count());
            Assert.AreEqual(0, viewmodel.ImportersRtc.Count());
            Assert.IsFalse(viewmodel.CanImportFlowRtc);
            Assert.IsFalse(viewmodel.CanImportRr);
        }

        [Test]
        public void GivenSobekImportWizardControlViewModel_SettingFilePath_ShouldWorkWithNetworkFile()
        {
            //Arrange
            // RR only case
            var path = TestHelper.GetTestFilePath(@"019_011.lit\2\network.tp");
            var viewmodel = new SobekImportWizardControlViewModel();

            // Act
            viewmodel.FilePath = path;

            // Assert
            Assert.IsTrue(viewmodel.HasFileSet);
            Assert.AreEqual(path, viewmodel.FilePath);
            Assert.IsFalse(viewmodel.IsCaseList);
            Assert.AreEqual(0, viewmodel.ImportersWaterFlow1d.Count());
            Assert.AreEqual(11, viewmodel.ImportersRainfallRunoff.Count());
            Assert.AreEqual(0, viewmodel.ImportersRtc.Count());
            Assert.IsFalse(viewmodel.CanImportFlowRtc);
            Assert.IsTrue(viewmodel.CanImportRr);
        }

        [Test]
        public void GivenSobekImportWizardControlViewModel_SettingFilePath_ShouldWorkWithSobekReFile()
        {
            //Arrange
            var path = TestHelper.GetTestFilePath(@"ReModels\J_10BANK.sbk\4\deftop.1");
            var viewmodel = new SobekImportWizardControlViewModel();

            // Act
            viewmodel.FilePath = path;

            // Assert
            Assert.IsTrue(viewmodel.HasFileSet);
            Assert.AreEqual(path, viewmodel.FilePath);
            Assert.IsFalse(viewmodel.IsCaseList);
            Assert.AreEqual(12, viewmodel.ImportersWaterFlow1d.Count());
            Assert.AreEqual(0, viewmodel.ImportersRainfallRunoff.Count());
            Assert.AreEqual(1, viewmodel.ImportersRtc.Count());
            Assert.IsTrue(viewmodel.CanImportFlowRtc);
            Assert.IsFalse(viewmodel.CanImportRr);
        }

        [Test]
        public void GivenSobekImportWizardControlViewModel_SettingFilePath_ShouldWorkWithCaseFile()
        {
            //Arrange
            var path = TestHelper.GetTestFilePath(@"compound.lit\CASELIST.CMT");
            var viewmodel = new SobekImportWizardControlViewModel();

            // Act
            viewmodel.FilePath = path;

            // Assert
            Assert.IsTrue(viewmodel.HasFileSet);
            Assert.AreEqual(path, viewmodel.FilePath);
            Assert.IsTrue(viewmodel.IsCaseList);
            Assert.AreEqual("2 'compound'", viewmodel.SelectedCase);
            Assert.AreEqual(1, viewmodel.Cases.Length);
            Assert.AreEqual(12, viewmodel.ImportersWaterFlow1d.Count());
            Assert.AreEqual(0, viewmodel.ImportersRainfallRunoff.Count());
            Assert.AreEqual(1, viewmodel.ImportersRtc.Count());
            Assert.IsTrue(viewmodel.CanImportFlowRtc);
            Assert.IsFalse(viewmodel.CanImportRr);
        }
        
        [Test]
        public void GivenSobekImportWizardControlViewModel_GetFilepathCommand_ShouldCallDelegate()
        {
            //Arrange
            var path = TestHelper.GetTestFilePath(@"compound.lit\CASELIST.CMT");
            var viewmodel = new SobekImportWizardControlViewModel
            {
                GetFilePath = ()=> path
            };

            // Act
            viewmodel.GetFilepathCommand.Execute(null);

            // Assert
            Assert.AreEqual(path, viewmodel.FilePath);
            Assert.IsTrue(viewmodel.IsCaseList);
            Assert.AreEqual("2 'compound'", viewmodel.SelectedCase);
            Assert.AreEqual(1, viewmodel.Cases.Length);
        }

        [Test, Apartment(ApartmentState.MTA), Ignore("Hangs on buildserver")]
        public void GivenSobekImportWizardControlViewModel_ExecuteCommand_ShouldCallExecuteProjectTemplate()
        {
            //Arrange
            var path = TestHelper.GetTestFilePath(@"ReducedModel\3\NETWORK.TP");
            bool startingImportCalled = false;
            bool finishedImportCalled = false;
            bool executeProjectTemplateCalled = false;

            var viewmodel = new SobekImportWizardControlViewModel
            {
                StartingImport = ()=> startingImportCalled = true,
                FinishedImport = ()=> finishedImportCalled = true,
                ExecuteProjectTemplate = m =>
                {
                    executeProjectTemplateCalled = true;
                    Assert.IsInstanceOf<HydroModel>(m);
                },
                FilePath = path
            };

            // Act
            viewmodel.ExecuteCommand.Execute(null);

            // Assert
            while (viewmodel.IsRunning)
            {
                Thread.Sleep(50);
            }

            Assert.IsTrue(startingImportCalled, "StartingImport not called");
            Assert.IsTrue(finishedImportCalled, "FinishedImport not called");
            Assert.IsTrue(executeProjectTemplateCalled, "ExecuteProjectTemplate not called");
        }

        [Test]
        public void GivenSobekImportWizardControlViewModelTest_CancelCommand_ShouldCallCancelProjectTemplate()
        {
            //Arrange
            var cancelProjectTemplateCalled = false;
            var viewmodel = new SobekImportWizardControlViewModel
            {
                CancelProjectTemplate = () => cancelProjectTemplateCalled = true
            };

            // Act
            viewmodel.CancelCommand.Execute(null);

            // Assert
            Assert.IsTrue(cancelProjectTemplateCalled);
        }

        [Test]
        public void GivenSobekImportWizardControlViewModel_EnableDisableFlow1DImport_ShouldBeReflectedInImporters()
        {
            //Arrange
            var path = TestHelper.GetTestFilePath(@"groesbeek.lit\network.tp");
            var viewmodel = new SobekImportWizardControlViewModel {FilePath = path};

            // Act & Assert
            Assert.IsTrue(viewmodel.CanImportFlowRtc);
            Assert.IsTrue(viewmodel.ImportFlow);
            
            Assert.AreEqual(12, viewmodel.ImportersWaterFlow1d.Count());
            Assert.AreEqual(11, viewmodel.ImportersRainfallRunoff.Count());
            Assert.AreEqual(1, viewmodel.ImportersRtc.Count());

            viewmodel.ImportFlow = false;

            Assert.AreEqual(0, viewmodel.ImportersWaterFlow1d.Count());
            Assert.AreEqual(11, viewmodel.ImportersRainfallRunoff.Count());
            Assert.AreEqual(1, viewmodel.ImportersRtc.Count());
        }
        
        [Test]
        public void GivenSobekImportWizardControlViewModel_EnableDisableRtcImport_ShouldBeReflectedInImporters()
        {
            //Arrange
            var path = TestHelper.GetTestFilePath(@"groesbeek.lit\network.tp");
            var viewmodel = new SobekImportWizardControlViewModel {FilePath = path};

            // Act & Assert
            Assert.IsTrue(viewmodel.CanImportFlowRtc);
            Assert.IsTrue(viewmodel.ImportRtc);
        
            Assert.AreEqual(12, viewmodel.ImportersWaterFlow1d.Count());
            Assert.AreEqual(11, viewmodel.ImportersRainfallRunoff.Count());
            Assert.AreEqual(1, viewmodel.ImportersRtc.Count());

            viewmodel.ImportRtc = false;

            Assert.AreEqual(12, viewmodel.ImportersWaterFlow1d.Count());
            Assert.AreEqual(11, viewmodel.ImportersRainfallRunoff.Count());
            Assert.AreEqual(0, viewmodel.ImportersRtc.Count());
        }
        
        [Test]
        public void GivenSobekImportWizardControlViewModel_EnableDisableRRImport_ShouldBeReflectedInImporters()
        {
            //Arrange
            var path = TestHelper.GetTestFilePath(@"groesbeek.lit\network.tp");
            var viewmodel = new SobekImportWizardControlViewModel {FilePath = path};

            // Act & Assert
            Assert.IsTrue(viewmodel.CanImportRr);
            Assert.IsTrue(viewmodel.ImportRr);

            Assert.AreEqual(12, viewmodel.ImportersWaterFlow1d.Count());
            Assert.AreEqual(11, viewmodel.ImportersRainfallRunoff.Count());
            Assert.AreEqual(1, viewmodel.ImportersRtc.Count());

            viewmodel.ImportRr = false;

            Assert.AreEqual(12, viewmodel.ImportersWaterFlow1d.Count());
            Assert.AreEqual(0, viewmodel.ImportersRainfallRunoff.Count());
            Assert.AreEqual(1, viewmodel.ImportersRtc.Count());
        }

        [Test]
        public void GivenSobekImportWizardControlViewModel_ProgressProperties_ShouldFirePropertyChangedEvents()
        {
            //Arrange
            var propertyChangedEvents = new List<string>();
            var viewmodel = new SobekImportWizardControlViewModel();
            viewmodel.PropertyChanged += (s, a) => propertyChangedEvents.Add(a.PropertyName);

            // Act & Assert
            viewmodel.ProgressText = "test";
            Assert.Contains(nameof(viewmodel.ProgressText), propertyChangedEvents);
            Assert.AreEqual("test",viewmodel.ProgressText);

            viewmodel.ProgressCurrentStep = 1;
            Assert.Contains(nameof(viewmodel.ProgressCurrentStep), propertyChangedEvents);
            Assert.AreEqual(1, viewmodel.ProgressCurrentStep);

            viewmodel.ProgressTotalTotalSteps = 1;
            Assert.Contains(nameof(viewmodel.ProgressTotalTotalSteps), propertyChangedEvents);
            Assert.AreEqual(1, viewmodel.ProgressTotalTotalSteps);
        }
    }
}