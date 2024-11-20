using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.Wind;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.Extensions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.Helpers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.ViewModels.TimeFrameEditor;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.TimeFrame.ViewModels.TimeFrameEditor
{
    [TestFixture]
    public class WindFilesViewModelTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            var data = new WaveMeteoData()
            {
                FileType = WindDefinitionType.SpiderWebGrid,
                XYVectorFilePath = "xyVectorFilePath",
                XComponentFilePath = "xComponentFilePath",
                YComponentFilePath = "yComponentFilePath",
                HasSpiderWeb = true,
                SpiderWebFilePath = "spiderWebFilePath",
            };

            var helper = Substitute.For<ITimeFrameEditorFileImportHelper>();

            // Call
            using (var viewModel = new WindFilesViewModel(data, helper))
            {
                // Assert
                Assert.That(viewModel, Is.InstanceOf<INotifyPropertyChanged>());
                Assert.That(viewModel, Is.InstanceOf<IDisposable>());

                Assert.That(viewModel.WindFileType, Is.EqualTo(data.FileType.ConvertToWindInputType()));
                Assert.That(viewModel.WindVelocityPath, Is.EqualTo(data.XYVectorFilePath));
                Assert.That(viewModel.XComponentPath, Is.EqualTo(data.XComponentFilePath));
                Assert.That(viewModel.YComponentPath, Is.EqualTo(data.YComponentFilePath));
                Assert.That(viewModel.UseSpiderWeb, Is.EqualTo(data.HasSpiderWeb));
                Assert.That(viewModel.SpiderWebPath, Is.EqualTo(data.SpiderWebFilePath));

                Assert.That(viewModel.WindVelocitySelectPathCommand, Is.Not.Null);
                Assert.That(viewModel.SpiderWebSelectPathCommand, Is.Not.Null);
                Assert.That(viewModel.XComponentSelectPathCommand, Is.Not.Null);
                Assert.That(viewModel.YComponentSelectPathCommand, Is.Not.Null);
            }
        }

        private static IEnumerable<TestCaseData> GetConstructorArgumentNullTestCaseData()
        {
            var meteoData = new WaveMeteoData();
            var importHelper = Substitute.For<ITimeFrameEditorFileImportHelper>();

            yield return new TestCaseData(null, importHelper, "waveMeteoData");
            yield return new TestCaseData(meteoData, null, "importHelper");
        }

        [Test]
        [TestCaseSource(nameof(GetConstructorArgumentNullTestCaseData))]
        public void Constructor_ArgumentNull_ThrowsArgumentNullException(WaveMeteoData meteoData,
                                                                         ITimeFrameEditorFileImportHelper helper,
                                                                         string expectedParamName)
        {
            void Call() => new WindFilesViewModel(meteoData, helper);
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParamName));
        }

        private static IEnumerable<TestCaseData> GetPropertyChangedData()
        {
            void UpdateFileType(WindFilesViewModel vm) => vm.WindFileType = WindInputType.SpiderWebGrid;
            yield return new TestCaseData((Action<WindFilesViewModel>)UpdateFileType,
                                          nameof(WindFilesViewModel.WindFileType));

            void UpdateXYVectorPath(WindFilesViewModel vm) => vm.WindVelocityPath = "somePath";
            yield return new TestCaseData((Action<WindFilesViewModel>)UpdateXYVectorPath,
                                          nameof(WindFilesViewModel.WindVelocityPath));

            void UpdateXComponentFilePath(WindFilesViewModel vm) => vm.XComponentPath = "somePath";
            yield return new TestCaseData((Action<WindFilesViewModel>)UpdateXComponentFilePath,
                                          nameof(WindFilesViewModel.XComponentPath));

            void UpdateYComponentFilePath(WindFilesViewModel vm) => vm.YComponentPath = "somePath";
            yield return new TestCaseData((Action<WindFilesViewModel>)UpdateYComponentFilePath,
                                          nameof(WindFilesViewModel.YComponentPath));


            void UpdateHasSpiderWeb(WindFilesViewModel vm) => vm.UseSpiderWeb = true;
            yield return new TestCaseData((Action<WindFilesViewModel>)UpdateHasSpiderWeb,
                                          nameof(WindFilesViewModel.UseSpiderWeb));

            void UpdateSpiderWebFilePath(WindFilesViewModel vm) => vm.SpiderWebPath = "somePath";
            yield return new TestCaseData((Action<WindFilesViewModel>)UpdateSpiderWebFilePath,
                                          nameof(WindFilesViewModel.SpiderWebPath));
        }

        [Test]
        [TestCaseSource(nameof(GetPropertyChangedData))]
        public void PropertyChanged_RaisesEvent(Action<WindFilesViewModel> updateProperty,
                                                string expectedParameterName)
        {
            // Setup
            var data = new WaveMeteoData();
            var importHelper = Substitute.For<ITimeFrameEditorFileImportHelper>();

            using (var viewModel = new WindFilesViewModel(data, importHelper))
            {
                var observer = new EventTestObserver<PropertyChangedEventArgs>();

                ((INotifyPropertyChanged)viewModel).PropertyChanged += observer.OnEventFired;

                // Call
                updateProperty.Invoke(viewModel);

                // Assert
                Assert.That(observer.NCalls, Is.EqualTo(1));
                Assert.That(observer.Senders[0], Is.SameAs(viewModel));
                Assert.That(observer.EventArgses[0].PropertyName,
                            Is.EqualTo(expectedParameterName));
            }
        }

        private static IEnumerable<TestCaseData> GetCommandsData()
        {
            ICommand GetSpiderWebCommand(WindFilesViewModel vm) => vm.SpiderWebSelectPathCommand;
            string GetSpiderWebValue(WindFilesViewModel vm) => vm.SpiderWebPath;
            yield return new TestCaseData((Func<WindFilesViewModel, ICommand>)GetSpiderWebCommand,
                                          (Func<WindFilesViewModel, string>)GetSpiderWebValue,
                                          "Spider Web (*.spw)|*.spw|All files (*.*)|*.*");

            ICommand GetYComponentCommand(WindFilesViewModel vm) => vm.YComponentSelectPathCommand;
            string GetYComponentValue(WindFilesViewModel vm) => vm.YComponentPath;
            yield return new TestCaseData((Func<WindFilesViewModel, ICommand>)GetYComponentCommand,
                                          (Func<WindFilesViewModel, string>)GetYComponentValue,
                                          "Uniform Y series (*.wnd;*.amv)|*.wnd;*.amv|All files (*.*)|*.*");


            ICommand GetXComponentCommand(WindFilesViewModel vm) => vm.XComponentSelectPathCommand;
            string GetXComponentValue(WindFilesViewModel vm) => vm.XComponentPath;
            yield return new TestCaseData((Func<WindFilesViewModel, ICommand>)GetXComponentCommand,
                                          (Func<WindFilesViewModel, string>)GetXComponentValue,
                                          "Uniform X series (*.wnd;*.amu)|*.wnd;*.amu|All files (*.*)|*.*");

            ICommand GetWindVelocityCommand(WindFilesViewModel vm) => vm.WindVelocitySelectPathCommand;
            string GetWindVelocityValue(WindFilesViewModel vm) => vm.WindVelocityPath;
            yield return new TestCaseData((Func<WindFilesViewModel, ICommand>)GetWindVelocityCommand,
                                          (Func<WindFilesViewModel, string>)GetWindVelocityValue,
                                          "Wind Velocity (*.wnd)|*.wnd|All files (*.*)|*.*");
        }

        [Test]
        [TestCaseSource(nameof(GetCommandsData))]
        public void SelectPathCommand_ReturnedPathNotNull_UpdatesCorrectly(Func<WindFilesViewModel, ICommand> getCommandFunc,
                                                                           Func<WindFilesViewModel, string> getResultFunc,
                                                                           string expectedFilter)
        {
            // Setup
            var data = new WaveMeteoData()
            {
                FileType = WindDefinitionType.SpiderWebGrid,
                XYVectorFilePath = "xyVectorFilePath",
                XComponentFilePath = "xComponentFilePath",
                YComponentFilePath = "yComponentFilePath",
                HasSpiderWeb = true,
                SpiderWebFilePath = "spiderWebFilePath",
            };

            var helper = Substitute.For<ITimeFrameEditorFileImportHelper>();

            const string expectedResult = "someResult";
            helper.HandleInputFileImport(expectedFilter).Returns(expectedResult);

            using (var viewModel = new WindFilesViewModel(data, helper))
            {
                // Call
                ICommand cmd = getCommandFunc.Invoke(viewModel);
                cmd.Execute(viewModel);

                // Assert
                string result = getResultFunc.Invoke(viewModel);
                Assert.That(result, Is.EqualTo(expectedResult));
            }
        }

        [Test]
        [TestCaseSource(nameof(GetCommandsData))]
        public void SelectPathCommand_ReturnedPathNull_RetainsValue(Func<WindFilesViewModel, ICommand> getCommandFunc,
                                                                    Func<WindFilesViewModel, string> getResultFunc,
                                                                    string expectedFilter)
        {
            // Setup
            var data = new WaveMeteoData()
            {
                FileType = WindDefinitionType.SpiderWebGrid,
                XYVectorFilePath = "xyVectorFilePath",
                XComponentFilePath = "xComponentFilePath",
                YComponentFilePath = "yComponentFilePath",
                HasSpiderWeb = true,
                SpiderWebFilePath = "spiderWebFilePath",
            };

            var helper = Substitute.For<ITimeFrameEditorFileImportHelper>();

            helper.HandleInputFileImport(expectedFilter).Returns((string)null);

            using (var viewModel = new WindFilesViewModel(data, helper))
            {
                string expectedResult = getResultFunc.Invoke(viewModel);

                // Call
                ICommand cmd = getCommandFunc.Invoke(viewModel);
                cmd.Execute(viewModel);

                // Assert
                string result = getResultFunc.Invoke(viewModel);
                Assert.That(result, Is.EqualTo(expectedResult));
            }
        }
    }
}