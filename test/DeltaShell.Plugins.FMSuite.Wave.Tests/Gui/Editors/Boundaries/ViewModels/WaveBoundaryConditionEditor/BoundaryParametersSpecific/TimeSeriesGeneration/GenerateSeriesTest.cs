using System;
using System.Collections.Generic;
using System.Windows.Forms;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific.TimeSeriesGeneration;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Forms;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParametersSpecific.TimeSeriesGeneration
{
    [TestFixture(typeof(DegreesDefinedSpreading))]
    [TestFixture(typeof(PowerDefinedSpreading))]
    public class GenerateSeriesTest<TSpreading> where TSpreading : class, IBoundaryConditionSpreading, new()
    {
        [Test]
        public void Constructor_DialogHelperNull_ThrowsArgumentNullException()
        {
            void Call() => new GenerateSeries(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("dialogHelper"));
        }

        [Test]
        public void Execute_OwnerNull_ThrowsArgumentNullException()
        {
            // Setup
            var generateSeries = new GenerateSeries(Substitute.For<IGenerateSeriesDialogHelper>());
            
            // Call | Assert
            void Call() => generateSeries.Execute(null, Substitute.For<IWaveEnergyFunction<TSpreading>>());
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("owner"));
        }

        [Test]
        public void Execute_SelectedFunctionNull_ThrowsArgumentNullException()
        {
            // Setup
            var generateSeries = new GenerateSeries(Substitute.For<IGenerateSeriesDialogHelper>());
            
            // Call | Assert
            void Call() => generateSeries.Execute<TSpreading>(Substitute.For<IWin32Window>(), null);
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("selectedFunction"));
        }

        [Test]
        public void Execute_DialogResultNotOk_FunctionIsLeftUnchanged()
        {
            // Setup
            using (var timeSeriesDialog = new TimeSeriesGeneratorDialog())
            {
                var windowOwner = Substitute.For<IWin32Window>();
                timeSeriesDialog.DialogResult = DialogResult.Cancel;

                var dialogHelper = Substitute.For<IGenerateSeriesDialogHelper>();
                dialogHelper.GetTimeSeriesGeneratorResponse(Arg.Is(windowOwner), 
                                                            Arg.Any<DateTime>(),
                                                            Arg.Any<DateTime>(),
                                                            Arg.Any<TimeSpan>())
                            .Returns(timeSeriesDialog);

                var generateSeries = new GenerateSeries(dialogHelper);

                var selectedFunction = Substitute.For<IWaveEnergyFunction<TSpreading>>();

                // Call
                generateSeries.Execute(windowOwner, selectedFunction);

                // Assert
                VerifyNotCalled(selectedFunction);
            }
        }

        private static void VerifyNotCalled(IWaveEnergyFunction<TSpreading> functionMock)
        {
                IVariable<DateTime> _0 = functionMock.DidNotReceiveWithAnyArgs().TimeArgument;
                IVariable<double> _1 = functionMock.DidNotReceiveWithAnyArgs().SpreadingComponent;
                IVariable<double> _2 = functionMock.DidNotReceiveWithAnyArgs().PeriodComponent;
                IVariable<double> _3 = functionMock.DidNotReceiveWithAnyArgs().DirectionComponent;
                IVariable<double> _4 = functionMock.DidNotReceiveWithAnyArgs().HeightComponent;
        }

        private static void OnlySelectedCalledWithoutOthers(IWaveEnergyFunction<TSpreading> selectedFunction, IEnumerable<IWaveEnergyFunction<TSpreading>> others)
        {
            Assert.That(selectedFunction.TimeArgument.AllValues.Count, Is.EqualTo(25));
        }

        private static void OnlySelectedCalled(IWaveEnergyFunction<TSpreading> selectedFunction, IEnumerable<IWaveEnergyFunction<TSpreading>> others)
        {
            Assert.That(selectedFunction.TimeArgument.AllValues.Count, Is.EqualTo(25));

            foreach (IWaveEnergyFunction<TSpreading> waveEnergyFunction in others)
                VerifyNotCalled(waveEnergyFunction);
        }

        private static void AllCalled(IWaveEnergyFunction<TSpreading> selectedFunction, IEnumerable<IWaveEnergyFunction<TSpreading>> others)
        {
            Assert.That(selectedFunction.TimeArgument.AllValues.Count, Is.EqualTo(25));

            foreach (IWaveEnergyFunction<TSpreading> waveEnergyFunction in others)
                Assert.That(waveEnergyFunction.TimeArgument.AllValues.Count, Is.EqualTo(25));
        }

        private static void NoCalled(IWaveEnergyFunction<TSpreading> selectedFunction,
                                     IEnumerable<IWaveEnergyFunction<TSpreading>> others)
        {
            VerifyNotCalled(selectedFunction);

            foreach (IWaveEnergyFunction<TSpreading> waveEnergyFunction in others)
                VerifyNotCalled(waveEnergyFunction);
        }


        private static IEnumerable<TestCaseData> GetExecuteGenerateTimeSeriesData()
        {
            yield return new TestCaseData(new WaveEnergyFunction<TSpreading>(), 
                                          null, 
                                          WaveSupportPointMode.SelectedActiveSupportPoint, 
                                          (Action<IWaveEnergyFunction<TSpreading>, IEnumerable<IWaveEnergyFunction<TSpreading>>>)OnlySelectedCalledWithoutOthers);

            IWaveEnergyFunction<TSpreading>[] GetOtherFunctionsNotCalled() =>
            new [] {
                Substitute.For<IWaveEnergyFunction<TSpreading>>(),
                Substitute.For<IWaveEnergyFunction<TSpreading>>(),
                Substitute.For<IWaveEnergyFunction<TSpreading>>(),
                Substitute.For<IWaveEnergyFunction<TSpreading>>(),
            };

            yield return new TestCaseData(new WaveEnergyFunction<TSpreading>(), 
                                          GetOtherFunctionsNotCalled(), 
                                          WaveSupportPointMode.SelectedActiveSupportPoint, 
                                          (Action<IWaveEnergyFunction<TSpreading>, IEnumerable<IWaveEnergyFunction<TSpreading>>>)OnlySelectedCalled);

            IWaveEnergyFunction<TSpreading>[] othersCalled =
            {
                new WaveEnergyFunction<TSpreading>(),
                new WaveEnergyFunction<TSpreading>(),
                new WaveEnergyFunction<TSpreading>(),
                new WaveEnergyFunction<TSpreading>(),
            };

            yield return new TestCaseData(new WaveEnergyFunction<TSpreading>(), 
                                          othersCalled, 
                                          WaveSupportPointMode.AllActiveSupportPoints, 
                                          (Action<IWaveEnergyFunction<TSpreading>, IEnumerable<IWaveEnergyFunction<TSpreading>>>)AllCalled);

            yield return new TestCaseData(Substitute.For<IWaveEnergyFunction<TSpreading>>(), 
                                          GetOtherFunctionsNotCalled(), 
                                          WaveSupportPointMode.NoSupportPoints, 
                                          (Action<IWaveEnergyFunction<TSpreading>, IEnumerable<IWaveEnergyFunction<TSpreading>>>)NoCalled);
        }

        [Test]
        [TestCaseSource(nameof(GetExecuteGenerateTimeSeriesData))]
        public void Execute_GeneratesTimeSeries(IWaveEnergyFunction<TSpreading> selectedFunction, 
                                                IWaveEnergyFunction<TSpreading>[] otherFunctions, 
                                                WaveSupportPointMode mode, 
                                                Action<IWaveEnergyFunction<TSpreading>, IEnumerable<IWaveEnergyFunction<TSpreading>>> assertAction)
        {
            // Setup
            using (var timeSeriesDialog = new TimeSeriesGeneratorDialog() { ApplyOnAccept = false})
            {
                timeSeriesDialog.SetData(null, 
                                         DateTime.Today, 
                                         DateTime.Today + TimeSpan.FromDays(1), 
                                         TimeSpan.FromHours(1));

                var windowOwner = Substitute.For<IWin32Window>();
                timeSeriesDialog.DialogResult = DialogResult.OK;

                var dialogHelper = Substitute.For<IGenerateSeriesDialogHelper>();
                dialogHelper.GetTimeSeriesGeneratorResponse(Arg.Is(windowOwner), 
                                                            Arg.Any<DateTime>(),
                                                            Arg.Any<DateTime>(),
                                                            Arg.Any<TimeSpan>())
                            .Returns(timeSeriesDialog);
                dialogHelper.GetSupportPointSelectionMode(windowOwner)
                            .Returns(mode);

                var generateSeries = new GenerateSeries(dialogHelper);


                // Call
                generateSeries.Execute(windowOwner, selectedFunction, otherFunctions);

                // Assert
                assertAction.Invoke(selectedFunction, otherFunctions);
            }
        }
    }
}