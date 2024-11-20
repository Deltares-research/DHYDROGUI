using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.Laterals;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors.Laterals.TimeSeriesGeneration;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors.Laterals.ViewModels;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Editors.Laterals.ViewModels
{
    [TestFixture]
    public class LateralDefinitionViewModelTest
    {
        [Test]
        [TestCaseSource(nameof(Constructor_ArgNullCases))]
        public void Constructor_ArgNull_ThrowsArgumentNullException(LateralDefinition lateralDefinition,
                                                                    ITimeSeriesGeneratorDialogService timeSeriesGeneratorDialogService)
        {
            // Call
            void Call() => new LateralDefinitionViewModel(lateralDefinition, timeSeriesGeneratorDialogService);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_FunctionsContainsDischargeTimeSeries()
        {
            // Setup
            var lateralDefinition = new LateralDefinition();
            var timeSeriesGeneratorDialogService = Substitute.For<ITimeSeriesGeneratorDialogService>();
            var viewModel = new LateralDefinitionViewModel(lateralDefinition, timeSeriesGeneratorDialogService);

            // Assert
            Assert.That(viewModel.Functions.Single(), Is.SameAs(lateralDefinition.Discharge.TimeSeries));
            Assert.That(viewModel.GenerateTimeSeriesCommand, Is.Not.Null);
        }

        [Test]
        public void GenerateTimeSeriesCommand_OnExecute_ExecutesTheTimeSeriesGeneratorDialogService()
        {
            var lateralDefinition = new LateralDefinition();
            var timeSeriesGeneratorDialogService = Substitute.For<ITimeSeriesGeneratorDialogService>();
            var viewModel = new LateralDefinitionViewModel(lateralDefinition, timeSeriesGeneratorDialogService);

            // Call
            viewModel.GenerateTimeSeriesCommand.Execute(null);

            // Assert
            DateTime start = DateTime.Today;
            TimeSpan timeStep = TimeSpan.FromDays(1);
            DateTime stop = start.Add(timeStep);
            timeSeriesGeneratorDialogService.Received(1).Execute(start, stop, timeStep, lateralDefinition.Discharge.TimeSeries);
        }

        [Test]
        [TestCase(ViewLateralDischargeType.TimeSeries, LateralDischargeType.TimeSeries)]
        [TestCase(ViewLateralDischargeType.Constant, LateralDischargeType.Constant)]
        [TestCase(ViewLateralDischargeType.RealTime, LateralDischargeType.RealTime)]
        public void SetDischargeType_SetsTheCorrespondingDischargeTypeOnTheModel(ViewLateralDischargeType viewLateralDischargeType,
                                                                                 LateralDischargeType expLateralDischargeType)
        {
            // Setup
            var lateralDefinition = new LateralDefinition();
            var timeSeriesGeneratorDialogService = Substitute.For<ITimeSeriesGeneratorDialogService>();
            var viewModel = new LateralDefinitionViewModel(lateralDefinition, timeSeriesGeneratorDialogService);

            // Call
            viewModel.DischargeType = viewLateralDischargeType;

            // Assert
            Assert.That(lateralDefinition.Discharge.Type, Is.EqualTo(expLateralDischargeType));
        }

        [Test]
        [TestCase(LateralDischargeType.TimeSeries, ViewLateralDischargeType.TimeSeries)]
        [TestCase(LateralDischargeType.Constant, ViewLateralDischargeType.Constant)]
        [TestCase(LateralDischargeType.RealTime, ViewLateralDischargeType.RealTime)]
        public void GetDischargeType_GetsCorrespondingDischargeTypeFromTheModel(LateralDischargeType lateralDischargeType,
                                                                                ViewLateralDischargeType expViewLateralDischargeType)
        {
            // Setup
            var lateralDefinition = new LateralDefinition();
            var timeSeriesGeneratorDialogService = Substitute.For<ITimeSeriesGeneratorDialogService>();
            var viewModel = new LateralDefinitionViewModel(lateralDefinition, timeSeriesGeneratorDialogService);

            lateralDefinition.Discharge.Type = lateralDischargeType;

            // Assert
            Assert.That(viewModel.DischargeType, Is.EqualTo(expViewLateralDischargeType));
        }

        [Test]
        public void SetConstantDischarge_SetsTheConstantDischargeOnTheModel()
        {
            // Setup
            var lateralDefinition = new LateralDefinition();
            var timeSeriesGeneratorDialogService = Substitute.For<ITimeSeriesGeneratorDialogService>();
            var viewModel = new LateralDefinitionViewModel(lateralDefinition, timeSeriesGeneratorDialogService);

            // Call
            viewModel.ConstantDischarge = 1.23;

            // Assert
            Assert.That(lateralDefinition.Discharge.Constant, Is.EqualTo(1.23));
        }

        [Test]
        public void GetConstantDischarge_GetsConstantDischargeFromTheModel()
        {
            // Setup
            var lateralDefinition = new LateralDefinition();
            var timeSeriesGeneratorDialogService = Substitute.For<ITimeSeriesGeneratorDialogService>();
            var viewModel = new LateralDefinitionViewModel(lateralDefinition, timeSeriesGeneratorDialogService);

            lateralDefinition.Discharge.Constant = 1.23;

            // Assert
            Assert.That(viewModel.ConstantDischarge, Is.EqualTo(1.23));
        }

        private static IEnumerable<TestCaseData> Constructor_ArgNullCases()
        {
            var lateralDefinition = new LateralDefinition();
            var timeSeriesGeneratorDialogService = Substitute.For<ITimeSeriesGeneratorDialogService>();

            yield return new TestCaseData(null,
                                          timeSeriesGeneratorDialogService);
            yield return new TestCaseData(lateralDefinition,
                                          null);
        }
    }
}