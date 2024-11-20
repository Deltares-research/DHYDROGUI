using System.Collections.Generic;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.DomainSpecificDataEditor.ViewModels
{
    [TestFixture]
    public class HydroDynamicsSettingsViewModelTest
    {
        private HydroFromFlowSettings settings;
        private HydroDynamicsSettingsViewModel viewModel;

        [SetUp]
        public void Setup()
        {
            settings = new HydroFromFlowSettings();
            viewModel = new HydroDynamicsSettingsViewModel(settings);
        }

        [TestCaseSource(nameof(GetUsageTypeTestCases))]
        public void GetBedLevelUsage_ReturnsCorrectValue(UsageFromFlowType usageFromFlowType, HydroDynamicsUseParameterType expectedHydroDynamicsUseParameterType)
        {
            // Setup
            settings.BedLevelUsage = usageFromFlowType;

            // Call
            HydroDynamicsUseParameterType resultedInputType = viewModel.BedLevelUsage;

            // Assert
            Assert.That(resultedInputType, Is.EqualTo(expectedHydroDynamicsUseParameterType));
        }

        [TestCaseSource(nameof(SetUsageTypeTestCases))]
        public void SetBedLevelUsage_SetsCorrectPropertyValueOnModel(HydroDynamicsUseParameterType originalValue,
                                                                     HydroDynamicsUseParameterType setValue,
                                                                     UsageFromFlowType expectedUsageFromFlowType,
                                                                     int expectedPropertyChangedCount)
        {
            // Setup
            viewModel.BedLevelUsage = originalValue;

            // Call
            void Call() => viewModel.BedLevelUsage = setValue;

            // Assert
            viewModel.AssertPropertyChangedFired(Call, expectedPropertyChangedCount, nameof(viewModel.BedLevelUsage));
            Assert.That(settings.BedLevelUsage, Is.EqualTo(expectedUsageFromFlowType));
        }

        [TestCaseSource(nameof(GetUsageTypeTestCases))]
        public void GetWaterLevelUsage_ReturnsCorrectValue(UsageFromFlowType usageFromFlowType, HydroDynamicsUseParameterType expectedHydroDynamicsUseParameterType)
        {
            // Setup
            settings.WaterLevelUsage = usageFromFlowType;

            // Call
            HydroDynamicsUseParameterType resultedInputType = viewModel.WaterLevelUsage;

            // Assert
            Assert.That(resultedInputType, Is.EqualTo(expectedHydroDynamicsUseParameterType));
        }

        [TestCaseSource(nameof(SetUsageTypeTestCases))]
        public void SetWaterLevelUsage_SetsCorrectPropertyValueOnModel(HydroDynamicsUseParameterType originalValue,
                                                                       HydroDynamicsUseParameterType setValue,
                                                                       UsageFromFlowType expectedUsageFromFlowType,
                                                                       int expectedPropertyChangedCount)
        {
            // Setup
            viewModel.WaterLevelUsage = originalValue;

            // Call
            void Call() => viewModel.WaterLevelUsage = setValue;

            // Assert
            viewModel.AssertPropertyChangedFired(Call, expectedPropertyChangedCount, nameof(viewModel.WaterLevelUsage));
            Assert.That(settings.WaterLevelUsage, Is.EqualTo(expectedUsageFromFlowType));
        }

        [TestCaseSource(nameof(GetUsageTypeTestCases))]
        public void GetVelocityUsage_ReturnsCorrectValue(UsageFromFlowType usageFromFlowType, HydroDynamicsUseParameterType expectedHydroDynamicsUseParameterType)
        {
            // Setup
            settings.VelocityUsage = usageFromFlowType;

            // Call
            HydroDynamicsUseParameterType resultedInputType = viewModel.VelocityUsage;

            // Assert
            Assert.That(resultedInputType, Is.EqualTo(expectedHydroDynamicsUseParameterType));
        }

        [TestCaseSource(nameof(SetUsageTypeTestCases))]
        public void SetVelocityUsage_SetsCorrectPropertyValueOnModel(HydroDynamicsUseParameterType originalValue,
                                                                     HydroDynamicsUseParameterType setValue,
                                                                     UsageFromFlowType expectedUsageFromFlowType,
                                                                     int expectedPropertyChangedCount)
        {
            // Setup
            viewModel.VelocityUsage = originalValue;

            // Call
            void Call() => viewModel.VelocityUsage = setValue;

            // Assert
            viewModel.AssertPropertyChangedFired(Call, expectedPropertyChangedCount, nameof(viewModel.VelocityUsage));
            Assert.That(settings.VelocityUsage, Is.EqualTo(expectedUsageFromFlowType));
        }

        [TestCase(VelocityComputationType.SurfaceLayer, VelocityType.SurfaceLevel)]
        [TestCase(VelocityComputationType.DepthAveraged, VelocityType.DepthAveraged)]
        [TestCase(VelocityComputationType.WaveDependent, VelocityType.WaveDependent)]
        public void GetVelocityType_ReturnsCorrectValue(VelocityComputationType velocityComputationType, VelocityType expectedVelocityType)
        {
            // Setup
            settings.VelocityUsageType = velocityComputationType;

            // Call
            VelocityType resultedInputType = viewModel.VelocityType;

            // Assert
            Assert.That(resultedInputType, Is.EqualTo(expectedVelocityType));
        }

        [TestCase(VelocityType.WaveDependent, VelocityType.WaveDependent, VelocityComputationType.WaveDependent, 0)]
        [TestCase(VelocityType.WaveDependent, VelocityType.DepthAveraged, VelocityComputationType.DepthAveraged, 1)]
        [TestCase(VelocityType.WaveDependent, VelocityType.SurfaceLevel, VelocityComputationType.SurfaceLayer, 1)]
        [TestCase(VelocityType.DepthAveraged, VelocityType.WaveDependent, VelocityComputationType.WaveDependent, 1)]
        [TestCase(VelocityType.DepthAveraged, VelocityType.DepthAveraged, VelocityComputationType.DepthAveraged, 0)]
        [TestCase(VelocityType.DepthAveraged, VelocityType.SurfaceLevel, VelocityComputationType.SurfaceLayer, 1)]
        [TestCase(VelocityType.SurfaceLevel, VelocityType.WaveDependent, VelocityComputationType.WaveDependent, 1)]
        [TestCase(VelocityType.SurfaceLevel, VelocityType.DepthAveraged, VelocityComputationType.DepthAveraged, 1)]
        [TestCase(VelocityType.SurfaceLevel, VelocityType.SurfaceLevel, VelocityComputationType.SurfaceLayer, 0)]
        public void SetVelocityType_SetsCorrectPropertyValueOnModel(VelocityType originalValue,
                                                                    VelocityType setValue,
                                                                    VelocityComputationType expectedFileType,
                                                                    int expectedPropertyChangedCount)
        {
            // Setup
            viewModel.VelocityType = originalValue;

            // Call
            void Call() => viewModel.VelocityType = setValue;

            // Assert
            viewModel.AssertPropertyChangedFired(Call, expectedPropertyChangedCount, nameof(viewModel.VelocityType));
            Assert.That(settings.VelocityUsageType, Is.EqualTo(expectedFileType));
        }

        [TestCaseSource(nameof(GetUsageTypeTestCases))]
        public void GetWindUsage_ReturnsCorrectValue(UsageFromFlowType usageFromFlowType, HydroDynamicsUseParameterType expectedHydroDynamicsUseParameterType)
        {
            // Setup
            settings.WindUsage = usageFromFlowType;

            // Call
            HydroDynamicsUseParameterType resultedInputType = viewModel.WindUsage;

            // Assert
            Assert.That(resultedInputType, Is.EqualTo(expectedHydroDynamicsUseParameterType));
        }

        [TestCaseSource(nameof(SetUsageTypeTestCases))]
        public void SetWindUsage_SetsCorrectPropertyValueOnModel(HydroDynamicsUseParameterType originalValue,
                                                                 HydroDynamicsUseParameterType setValue,
                                                                 UsageFromFlowType expectedUsageFromFlowType,
                                                                 int expectedPropertyChangedCount)
        {
            // Setup
            viewModel.WindUsage = originalValue;

            // Call
            void Call() => viewModel.WindUsage = setValue;

            // Assert
            viewModel.AssertPropertyChangedFired(Call, expectedPropertyChangedCount, nameof(viewModel.WindUsage));
            Assert.That(settings.WindUsage, Is.EqualTo(expectedUsageFromFlowType));
        }

        private static IEnumerable<TestCaseData> SetUsageTypeTestCases()
        {
            yield return new TestCaseData(HydroDynamicsUseParameterType.DoNotUse, HydroDynamicsUseParameterType.DoNotUse, UsageFromFlowType.DoNotUse, 0);
            yield return new TestCaseData(HydroDynamicsUseParameterType.DoNotUse, HydroDynamicsUseParameterType.Use, UsageFromFlowType.UseDoNotExtend, 1);
            yield return new TestCaseData(HydroDynamicsUseParameterType.DoNotUse, HydroDynamicsUseParameterType.UseExtend, UsageFromFlowType.UseAndExtend, 1);
            yield return new TestCaseData(HydroDynamicsUseParameterType.Use, HydroDynamicsUseParameterType.DoNotUse, UsageFromFlowType.DoNotUse, 1);
            yield return new TestCaseData(HydroDynamicsUseParameterType.Use, HydroDynamicsUseParameterType.Use, UsageFromFlowType.UseDoNotExtend, 0);
            yield return new TestCaseData(HydroDynamicsUseParameterType.Use, HydroDynamicsUseParameterType.UseExtend, UsageFromFlowType.UseAndExtend, 1);
            yield return new TestCaseData(HydroDynamicsUseParameterType.UseExtend, HydroDynamicsUseParameterType.DoNotUse, UsageFromFlowType.DoNotUse, 1);
            yield return new TestCaseData(HydroDynamicsUseParameterType.UseExtend, HydroDynamicsUseParameterType.Use, UsageFromFlowType.UseDoNotExtend, 1);
            yield return new TestCaseData(HydroDynamicsUseParameterType.UseExtend, HydroDynamicsUseParameterType.UseExtend, UsageFromFlowType.UseAndExtend, 0);
        }

        private static IEnumerable<TestCaseData> GetUsageTypeTestCases()
        {
            yield return new TestCaseData(UsageFromFlowType.DoNotUse, HydroDynamicsUseParameterType.DoNotUse);
            yield return new TestCaseData(UsageFromFlowType.UseDoNotExtend, HydroDynamicsUseParameterType.Use);
            yield return new TestCaseData(UsageFromFlowType.UseAndExtend, HydroDynamicsUseParameterType.UseExtend);
        }
    }
}