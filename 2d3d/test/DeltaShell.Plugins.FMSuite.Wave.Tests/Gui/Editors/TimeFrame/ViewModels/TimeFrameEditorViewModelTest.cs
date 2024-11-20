using System;
using System.Collections.Generic;
using System.ComponentModel;
using DelftTools.Functions;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.Helpers;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.ViewModels;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.ViewModels.TimeFrameEditor;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame.DeltaShell.Plugins.FMSuite.Wave.TimeFrame;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.TimeFrame.ViewModels
{
    [TestFixture]
    public class TimeFrameEditorViewModelTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            const WindInputDataType expectedWindInputDataType = WindInputDataType.FileBased;
            const HydrodynamicsInputDataType expectedHydrodynamicsInputDataType = HydrodynamicsInputDataType.TimeVarying;

            var function = Substitute.For<IFunction>();
            ITimeFrameData timeFrameData = Substitute.For<ITimeFrameData, INotifyPropertyChanged>();
            timeFrameData.TimeVaryingData.Returns(function);

            timeFrameData.WindInputDataType = expectedWindInputDataType;
            timeFrameData.HydrodynamicsInputDataType = expectedHydrodynamicsInputDataType;

            var hydrodynamicsViewModel = new HydrodynamicsConstantsViewModel(new HydrodynamicsConstantData());
            var windViewModel = new WindConstantsViewModel(new WindConstantData());

            var helper = Substitute.For<ITimeFrameEditorFileImportHelper>();
            var windFilesViewModel = new WindFilesViewModel(new WaveMeteoData(), helper);

            // Call
            using (var viewModel = new TimeFrameEditorViewModel(timeFrameData,
                                                                hydrodynamicsViewModel,
                                                                windViewModel,
                                                                windFilesViewModel))
            {
                Assert.That(viewModel, Is.InstanceOf<INotifyPropertyChanged>());
                Assert.That(viewModel, Is.InstanceOf<IDisposable>());

                Assert.That(viewModel.HydrodynamicsInputDataType, Is.EqualTo(expectedHydrodynamicsInputDataType));
                Assert.That(viewModel.WindInputDataType, Is.EqualTo(expectedWindInputDataType));
                Assert.That(viewModel.HydrodynamicsConstantsViewModel, Is.SameAs(hydrodynamicsViewModel));
                Assert.That(viewModel.WindConstantsViewModel, Is.SameAs(windViewModel));
                Assert.That(viewModel.DataFunctionBindingList, Is.Not.Null);
                Assert.That(viewModel.DataFunctionBindingList.Function, Is.SameAs(function));
            }
        }

        private static IEnumerable<TestCaseData> GetConstructorParameterNullData()
        {
            ITimeFrameData timeFrameData = Substitute.For<ITimeFrameData, INotifyPropertyChanged>();
            var hydrodynamicsViewModel = new HydrodynamicsConstantsViewModel(new HydrodynamicsConstantData());
            var windViewModel = new WindConstantsViewModel(new WindConstantData());

            var helper = Substitute.For<ITimeFrameEditorFileImportHelper>();
            var windFilesViewModel = new WindFilesViewModel(new WaveMeteoData(), helper);

            yield return new TestCaseData(null, hydrodynamicsViewModel, windViewModel, windFilesViewModel, "timeFrameData");
            yield return new TestCaseData(timeFrameData, null, windViewModel, windFilesViewModel, "hydrodynamicsConstantsViewModel");
            yield return new TestCaseData(timeFrameData, hydrodynamicsViewModel, null, windFilesViewModel, "windConstantsViewModel");
            yield return new TestCaseData(timeFrameData, hydrodynamicsViewModel, windViewModel, null, "windFilesViewModel");
        }

        [Test]
        [TestCaseSource(nameof(GetConstructorParameterNullData))]
        public void Constructor_ParameterNull_ThrowsArgumentNullException(ITimeFrameData timeFrameData,
                                                                          HydrodynamicsConstantsViewModel hydrodynamicsConstantsViewModel,
                                                                          WindConstantsViewModel windConstantsViewModel,
                                                                          WindFilesViewModel windFilesViewModel,
                                                                          string expectedParameterName)
        {
            void Call() => new TimeFrameEditorViewModel(timeFrameData,
                                                        hydrodynamicsConstantsViewModel,
                                                        windConstantsViewModel,
                                                        windFilesViewModel);
            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParameterName));
        }

        [Test]
        public void HydrodynamicsInputDataType_SetValue_RaisesNotifyPropertyChangedAndUpdatesTimeFrameData()
        {
            // Setup
            ITimeFrameData timeFrameData = Substitute.For<ITimeFrameData, INotifyPropertyChanged>();
            timeFrameData
                .WhenForAnyArgs(x => x.HydrodynamicsInputDataType = HydrodynamicsInputDataType.Constant)
                .Do(_ => (timeFrameData as INotifyPropertyChanged).PropertyChanged += Raise.Event<PropertyChangedEventHandler>(timeFrameData,
                                                                                                                                   new PropertyChangedEventArgs(nameof(ITimeFrameData.HydrodynamicsInputDataType))));

            var hydrodynamicsViewModel = new HydrodynamicsConstantsViewModel(new HydrodynamicsConstantData());
            var windViewModel = new WindConstantsViewModel(new WindConstantData());

            var helper = Substitute.For<ITimeFrameEditorFileImportHelper>();
            var windFilesViewModel = new WindFilesViewModel(new WaveMeteoData(), helper);

            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();

            const HydrodynamicsInputDataType hydrodynamicsInputDataTypeValue = HydrodynamicsInputDataType.TimeVarying;

            using (var viewModel = new TimeFrameEditorViewModel(timeFrameData,
                                                                hydrodynamicsViewModel,
                                                                windViewModel,
                                                                windFilesViewModel))
            {
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                viewModel.HydrodynamicsInputDataType = hydrodynamicsInputDataTypeValue;

                // Assert
                Assert.That(viewModel.HydrodynamicsInputDataType, Is.EqualTo(hydrodynamicsInputDataTypeValue));
                timeFrameData.Received(1).HydrodynamicsInputDataType = hydrodynamicsInputDataTypeValue;

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1));
                Assert.That(propertyChangedObserver.Senders[0], Is.EqualTo(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[0].PropertyName, Is.EqualTo(nameof(TimeFrameEditorViewModel.HydrodynamicsInputDataType)));
            }
        }

        [Test]
        public void WindInputDataType_SetValue_RaisesNotifyPropertyChangedAndUpdatesTimeFrameData()
        {
            // Setup
            ITimeFrameData timeFrameData = Substitute.For<ITimeFrameData, INotifyPropertyChanged>();
            timeFrameData
                .WhenForAnyArgs(x => x.WindInputDataType = WindInputDataType.Constant)
                .Do(_ => (timeFrameData as INotifyPropertyChanged).PropertyChanged += Raise.Event<PropertyChangedEventHandler>(timeFrameData,
                                                                                                                                   new PropertyChangedEventArgs(nameof(ITimeFrameData.WindInputDataType))));

            var hydrodynamicsViewModel = new HydrodynamicsConstantsViewModel(new HydrodynamicsConstantData());
            var windViewModel = new WindConstantsViewModel(new WindConstantData());

            var helper = Substitute.For<ITimeFrameEditorFileImportHelper>();
            var windFilesViewModel = new WindFilesViewModel(new WaveMeteoData(), helper);

            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();

            const WindInputDataType windInputDataTypeValue = WindInputDataType.TimeVarying;

            using (var viewModel = new TimeFrameEditorViewModel(timeFrameData,
                                                                hydrodynamicsViewModel,
                                                                windViewModel,
                                                                windFilesViewModel))
            {
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                viewModel.WindInputDataType = windInputDataTypeValue;

                // Assert
                Assert.That(viewModel.WindInputDataType, Is.EqualTo(windInputDataTypeValue));
                timeFrameData.Received(1).WindInputDataType = windInputDataTypeValue;

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1));
                Assert.That(propertyChangedObserver.Senders[0], Is.EqualTo(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[0].PropertyName, Is.EqualTo(nameof(TimeFrameEditorViewModel.WindInputDataType)));
            }
        }
    }
}