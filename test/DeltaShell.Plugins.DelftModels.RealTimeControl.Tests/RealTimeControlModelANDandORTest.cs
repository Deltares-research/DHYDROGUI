using System;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests
{
    [TestFixture]
    [Category("DIMR_Introduction")]
    [Category(TestCategory.WorkInProgress)]
    public class RealTimeControlModelANDandORTest
    {
        [Test]
        [Category(TestCategory.Integration)]
        public void rtcDocumentJaco_632_C1_And_C2OrC3_IsInInitailzedStateAfterInitialize()
        {
            RealTimeControlModel rtcModel = GetRtcModelWithHydraulicRule();

            ControlGroup controlGroup = rtcModel.ControlGroups.First();

            var c1 = (StandardCondition) rtcModel.ControlGroups[0].Conditions[0];
            IInput input = c1.Input;
            RuleBase rule = rtcModel.ControlGroups[0].Rules[0];

            c1.Name = "C1";
            c1.Operation = Operation.Greater;
            c1.Value = 1.1;
            c1.TrueOutputs.Clear();
            c1.FalseOutputs.Clear();

            var c2 = new StandardCondition
            {
                Name = "C2",
                Input = input,
                Operation = Operation.Greater,
                Value = 2.2
            };

            var c3 = new StandardCondition
            {
                Name = "C3",
                Input = input,
                Operation = Operation.Less,
                Value = 0.5
            };

            c1.TrueOutputs.Add(c2);

            c2.TrueOutputs.Add(rule);
            c2.FalseOutputs.Add(c3);

            c3.TrueOutputs.Add(rule);

            controlGroup.Conditions.Add(c2);
            controlGroup.Conditions.Add(c3);

            rtcModel.Initialize();
            Assert.AreEqual(ActivityStatus.Initialized, rtcModel.Status);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void Execute_C1AndC2_Or_C3ANDC4_TriggerForHydraulicRule()
        {
            // RTC model
            var hydraulicRuleInput = new Input
            {
                Feature = new RtcTestFeature {Name = "f1"},
                ParameterName = "hydraulicRuleInput",
                UnitName = "none"
            };
            var hydraulicRuleOutput = new Output
            {
                Feature = new RtcTestFeature {Name = "f2"},
                ParameterName = "hydraulicRuleOutput",
                UnitName = "none"
            };
            var conditionInput = new Input
            {
                Feature = new RtcTestFeature {Name = "f3"},
                ParameterName = "conditionInput",
                UnitName = "none"
            };

            var hydraulicRule = new HydraulicRule
            {
                Name = "HydraulicRule",
                Inputs = {hydraulicRuleInput},
                Outputs = {hydraulicRuleOutput}
            };

            hydraulicRule.Function[-100.0] = 1.0; // rule on
            hydraulicRule.Function[100.0] = 1.0;  // rule on

            var c4 = new StandardCondition
            {
                Name = "C4",
                Input = conditionInput,
                Operation = Operation.Less,
                Value = -2.0,
                TrueOutputs = {hydraulicRule}
            };
            var c3 = new StandardCondition
            {
                Name = "C3",
                Input = conditionInput,
                Operation = Operation.Less,
                Value = -1.0,
                TrueOutputs = {c4}
            };
            var c2 = new StandardCondition
            {
                Name = "C2",
                Input = conditionInput,
                Operation = Operation.Greater,
                Value = 2.0,
                TrueOutputs = {hydraulicRule},
                FalseOutputs = {c3}
            };
            var c1 = new StandardCondition
            {
                Name = "C1",
                Input = conditionInput,
                Operation = Operation.Greater,
                Value = 1.0,
                TrueOutputs = {c2},
                FalseOutputs = {c3}
            };

            var controlGroup = new ControlGroup
            {
                Name = "Control group",
                Inputs =
                {
                    hydraulicRuleInput,
                    conditionInput
                },
                Outputs = {hydraulicRuleOutput},
                Rules = {hydraulicRule},
                Conditions =
                {
                    c1,
                    c2,
                    c3,
                    c4
                }
            };

            var rtcModel = new RealTimeControlModel
            {
                ControlGroups = {controlGroup},
                StartTime = new DateTime(2000, 1, 1),
                StopTime = new DateTime(2000, 1, 2),
                TimeStep = new TimeSpan(0, 0, 1)
            };

            LogHelper.ConfigureLogging(Level.Error);
            rtcModel.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, rtcModel.Status);

            //(C1 and C2) or (C3 and C4)
            //C1 > 1
            //C2 > 2
            //C3 < -1
            //C4 < -2

            //Case C1=false, C2=false, C3=false, C4=false -> rule Off
            conditionInput.Value = 0;
            hydraulicRuleOutput.Value = -99.0;
            rtcModel.Execute(); //one timestep
            Assert.AreEqual(-99.0, hydraulicRuleOutput.Value, "rule is off, value is unchanged");

            //Case C1=true, C2=false, C3=false, C4=false -> rule Off
            conditionInput.Value = 1.5;
            hydraulicRuleOutput.Value = -99.0;
            rtcModel.Execute(); //one timestep
            Assert.AreEqual(-99.0, hydraulicRuleOutput.Value, "rule is off, value is unchanged");

            //Case C1=true, C2=true, C3=false, C4=false -> rule On
            conditionInput.Value = 3;
            hydraulicRuleOutput.Value = -99.0;
            rtcModel.Execute(); //one timestep
            Assert.AreEqual(1.0, hydraulicRuleOutput.Value, "rule is on");

            //Case C1=false, C2=false, C3=true, C4=false -> rule Off
            conditionInput.Value = -1.5;
            hydraulicRuleOutput.Value = -99.0;
            rtcModel.Execute(); //one timestep
            Assert.AreEqual(-99.0, hydraulicRuleOutput.Value, "rule is off, value is unchanged");

            //Case C1=false, C2=false, C3=true, C4=true -> rule On
            conditionInput.Value = -3.0;
            hydraulicRuleOutput.Value = -99.0;
            rtcModel.Execute(); //one timestep
            Assert.AreEqual(1.0, hydraulicRuleOutput.Value, "rule is off, value is unchanged");
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void rtcDocumentJaco_632_C1AndC2_Or_C3_IsInInitailzedStateAfterInitialize()
        {
            RealTimeControlModel rtcModel = GetRtcModelWithHydraulicRule();

            ControlGroup controlGroup = rtcModel.ControlGroups.First();

            var c1 = (StandardCondition) rtcModel.ControlGroups[0].Conditions[0];
            IInput input = c1.Input;
            RuleBase rule = rtcModel.ControlGroups[0].Rules[0];

            c1.Name = "C1";
            c1.Operation = Operation.Greater;
            c1.Value = 1.1;
            c1.TrueOutputs.Clear();
            c1.FalseOutputs.Clear();

            var c2 = new StandardCondition
            {
                Name = "C2",
                Input = input,
                Operation = Operation.Greater,
                Value = 2.2
            };

            var c3 = new StandardCondition
            {
                Name = "C3",
                Input = input,
                Operation = Operation.Less,
                Value = 0.5
            };

            c1.TrueOutputs.Add(c2);
            c1.FalseOutputs.Add(c3);

            c2.TrueOutputs.Add(rule);
            c2.FalseOutputs.Add(c3);

            c3.TrueOutputs.Add(rule);

            controlGroup.Conditions.Add(c2);
            controlGroup.Conditions.Add(c3);

            rtcModel.Initialize();
            Assert.AreEqual(ActivityStatus.Initialized, rtcModel.Status);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void rtcDocumentJaco_632_C1AndC2_Or_NotC1AndC3_IsInInitailzedStateAfterInitialize()
        {
            RealTimeControlModel rtcModel = GetRtcModelWithHydraulicRule();

            ControlGroup controlGroup = rtcModel.ControlGroups.First();

            var c1 = (StandardCondition) rtcModel.ControlGroups[0].Conditions[0];
            IInput input = c1.Input;
            RuleBase rule = rtcModel.ControlGroups[0].Rules[0];

            c1.Name = "C1";
            c1.Operation = Operation.Greater;
            c1.Value = 1.1;
            c1.TrueOutputs.Clear();
            c1.FalseOutputs.Clear();

            var c2 = new StandardCondition
            {
                Name = "C2",
                Input = input,
                Operation = Operation.Greater,
                Value = 2.2
            };

            var c3 = new StandardCondition
            {
                Name = "C3",
                Input = input,
                Operation = Operation.Less,
                Value = 0.5
            };

            c1.TrueOutputs.Add(c2);
            c1.FalseOutputs.Add(c3);

            c2.TrueOutputs.Add(rule);

            c3.TrueOutputs.Add(rule);

            controlGroup.Conditions.Add(c2);
            controlGroup.Conditions.Add(c3);

            rtcModel.Initialize();
            Assert.AreEqual(ActivityStatus.Initialized, rtcModel.Status);
        }

        [Obsolete("Don't use these helpers, construct everything in test to keep code readable")]
        private RealTimeControlModel GetRtcModelWithHydraulicRule()
        {
            var outputFeature = new RtcTestFeature {Name = "output_feature"};
            var inputFeature = new RtcTestFeature {Name = "input_feature"};

            var controlledModel = new ControlledTestModel
            {
                StartTime = new DateTime(2000, 1, 1, 0, 0, 0),
                StopTime = new DateTime(2000, 1, 1, 6, 0, 0),
                TimeStep = new TimeSpan(0, 1, 0, 0),
                OutputFeatures = {outputFeature},
                InputFeatures = {inputFeature}
            };

            ControlGroup controlGroup = RealTimeControlModelHelper.CreateGroupHydraulicRule(true);
            var rtcModel = new RealTimeControlModel {ControlGroups = {controlGroup}};

            rtcModel.GetDataItemByValue(controlGroup.Inputs[0]).LinkTo(controlledModel.GetChildDataItems(outputFeature).First());
            rtcModel.GetDataItemByValue(controlGroup.Inputs[1]).LinkTo(controlledModel.GetChildDataItems(outputFeature).First());
            controlledModel.GetChildDataItems(inputFeature).First().LinkTo(rtcModel.GetDataItemByValue(controlGroup.Outputs[0]));

            //RealTimeControlTestHelper.AddDummyLinksToGroup(controlledModel, controlGroup);
            ((HydraulicRule) controlGroup.Rules[0]).Function[0.0] = 1.0; // empy lookupTable is not allowed

            var comp = new CompositeModel();
            comp.Activities.Add(rtcModel);
            comp.Activities.Add(controlledModel);
            return rtcModel;
        }
    }
}