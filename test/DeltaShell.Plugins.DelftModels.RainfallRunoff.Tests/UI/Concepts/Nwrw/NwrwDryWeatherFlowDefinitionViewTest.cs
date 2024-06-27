using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts.Nwrw;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI.Concepts.Nwrw
{
    [TestFixture]
    public class NwrwDryWeatherFlowDefinitionViewTest
    {
        private NwrwDryWeatherFlowDefinitionView flowDefinitionView;

        [SetUp]
        public void SetUp()
        {
            flowDefinitionView = CreateFlowDefinitionView();
        }

        [TearDown]
        public void TearDown()
        {
            flowDefinitionView.Dispose();
        }

        [Test]
        public void OnDistributionTypeChanged_DistributionTypeIsSetToConstant_UpdatesDailyVolumeVariable()
        {
            var flowDefinition = new NwrwDryWeatherFlowDefinition()
            {
                DistributionType = DryweatherFlowDistributionType.Daily,
                DailyVolumeConstant = 10,
                DailyVolumeVariable = 2
            };

            SetFlowDefinitionViewData(flowDefinition);

            flowDefinition.DistributionType = DryweatherFlowDistributionType.Constant;

            Assert.AreEqual(10, flowDefinition.DailyVolumeVariable);
        }

        [Test]
        public void OnDailyVolumeConstantChanged_DistributionTypeIsConstant_UpdatesDailyVolumeVariable()
        {
            var flowDefinition = new NwrwDryWeatherFlowDefinition()
            {
                DistributionType = DryweatherFlowDistributionType.Constant,
                DailyVolumeConstant = 5,
                DailyVolumeVariable = 1
            };

            SetFlowDefinitionViewData(flowDefinition);

            flowDefinition.DailyVolumeConstant = 40;

            Assert.AreEqual(40, flowDefinition.DailyVolumeVariable);
        }

        [Test]
        public void OnDailyVolumeConstantChanged_DistributionTypeIsDaily_DoesNotUpdateDailyVolumeVariable()
        {
            var flowDefinition = new NwrwDryWeatherFlowDefinition()
            {
                DistributionType = DryweatherFlowDistributionType.Daily,
                DailyVolumeConstant = 10,
                DailyVolumeVariable = 30
            };

            SetFlowDefinitionViewData(flowDefinition);

            flowDefinition.DailyVolumeConstant = 120;

            Assert.AreEqual(30, flowDefinition.DailyVolumeVariable);
        }

        private NwrwDryWeatherFlowDefinitionView CreateFlowDefinitionView()
        {
            return new NwrwDryWeatherFlowDefinitionView();
        }

        private void SetFlowDefinitionViewData(params NwrwDryWeatherFlowDefinition[] flowDefinitions)
        {
            flowDefinitionView.Data = new EventedList<NwrwDryWeatherFlowDefinition>(flowDefinitions);
        }
    }
}