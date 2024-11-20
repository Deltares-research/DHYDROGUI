using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Domain
{
    [TestFixture]
    public class ControlGroupTest
    {
        [Test]
        public void Clone()
        {
            var input = new Input
            {
                ParameterName = "parameetr",
                Feature = new RtcTestFeature {Name = "Inlocation"}
            };
            var output = new Output
            {
                ParameterName = "parameetr",
                Feature = new RtcTestFeature {Name = "Outlocation"}
            };
            var rule = new PIDRule
            {
                Name = "noot",
                Inputs = {input},
                Outputs = {output}
            };
            var condition = new StandardCondition
            {
                Name = "aap",
                Input = input,
                TrueOutputs = {rule},
                FalseOutputs = {rule}
            };
            var signal = new LookupSignal
            {
                Name = "signal",
                Inputs = {input},
                RuleBases = {rule}
            };
            var controlGroup = new ControlGroup
            {
                Name = "test",
                Conditions = {condition},
                Rules = {rule},
                Inputs = {input},
                Outputs = {output},
                Signals = {signal}
            };

            var controlGroupClone = (ControlGroup) controlGroup.Clone();

            Assert.AreEqual(controlGroup.Name, controlGroupClone.Name);
            Assert.AreEqual(controlGroup.Inputs[0].Name, controlGroupClone.Inputs[0].Name);
            Assert.AreEqual(controlGroup.Outputs[0].Name, controlGroupClone.Outputs[0].Name);
            Assert.AreEqual(controlGroup.Rules[0].Name, controlGroupClone.Rules[0].Name);
            Assert.AreEqual(controlGroup.Conditions[0].Name, controlGroupClone.Conditions[0].Name);
            Assert.AreEqual(controlGroup.Signals[0].Name, controlGroupClone.Signals[0].Name);

            Assert.AreNotEqual(controlGroup.Inputs[0], controlGroupClone.Inputs[0]);

            // check if all inputs / outputs were re-wired correctly
            Assert.AreEqual(controlGroupClone.Rules[0], controlGroupClone.Conditions[0].TrueOutputs[0]);
            Assert.AreEqual(controlGroupClone.Rules[0], controlGroupClone.Conditions[0].FalseOutputs[0]);
            Assert.AreEqual(controlGroupClone.Inputs[0], controlGroupClone.Conditions[0].Input);

            Assert.AreEqual(controlGroupClone.Inputs[0], controlGroupClone.Rules[0].Inputs[0]);
            Assert.AreEqual(controlGroupClone.Outputs[0], controlGroupClone.Rules[0].Outputs[0]);

            Assert.AreEqual(controlGroupClone.Inputs[0], controlGroupClone.Signals[0].Inputs[0]);
            Assert.AreEqual(controlGroupClone.Rules[0], controlGroupClone.Signals[0].RuleBases[0]);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void CleanUpWhenFeaturesAreRemoved()
        {
            // see TOOLS-3543
            var inputFeature = new RtcTestFeature();
            var outputFeature = new RtcTestFeature();
            var controlledModel = new ControlledTestModel
            {
                InputFeatures = {inputFeature},
                OutputFeatures = {outputFeature}
            };

            ControlGroup controlGroup = RealTimeControlModelHelper.CreateGroupInvertorRule();
            var comp = new CompositeModel();

            var realTimeControlModel = new RealTimeControlModel {ControlGroups = {controlGroup}};

            comp.Activities.Add(realTimeControlModel);
            comp.Activities.Add(controlledModel);

            // connect
            IDataItem outputDataItem = controlledModel.GetChildDataItems(outputFeature).First();
            realTimeControlModel.GetDataItemByValue(controlGroup.Inputs[0]).LinkTo(outputDataItem);

            IDataItem inputDataItem = controlledModel.GetChildDataItems(inputFeature).First();
            inputDataItem.LinkTo(realTimeControlModel.GetDataItemByValue(controlGroup.Outputs[0]));

            Assert.IsNotNull(controlGroup.Inputs[0].Feature);
            Assert.IsNotNull(controlGroup.Outputs[0].Feature);

            // remove features from controlled model - will result in unlinking of consumer items and this should result in clearing of input/output items in RTC
            controlledModel.InputFeatures.Remove(inputFeature);
            controlledModel.OutputFeatures.Remove(outputFeature);

            // RTC connections are cleared
            Assert.IsNull(controlGroup.Inputs[0].Feature);
            Assert.IsNull(controlGroup.Outputs[0].Feature);
        }

        [Test]
        public void CollectionChanged()
        {
            var controlGroup = new ControlGroup();
            var rule = new PIDRule();
            var condition = new StandardCondition();
            var input = new Input();
            var output = new Output();

            controlGroup.Name = "Test group";
            rule.Name = "Test rule";
            condition.Name = "Test condition";
            input.Name = "Test input";
            output.Name = "Test output";

            var count = 0;

            ((INotifyCollectionChanged) controlGroup).CollectionChanged += (s, e) =>
            {
                count++;
                Console.WriteLine("Property = " + ((INameable) e.GetRemovedOrAddedItem()).Name + " (" + count + ")");
            };

            controlGroup.Rules.Add(rule);
            controlGroup.Conditions.Add(condition);
            controlGroup.Inputs.Add(input);
            controlGroup.Outputs.Add(output);

            Assert.AreEqual(4, count);
        }

        [Test]
        public void PropertyChanged()
        {
            var controlGroup = new ControlGroup();
            var rule = new PIDRule();
            var condition = new StandardCondition();
            var input = new Input();
            var output = new Output();

            controlGroup.Rules.Add(rule);
            controlGroup.Conditions.Add(condition);
            controlGroup.Inputs.Add(input);
            controlGroup.Outputs.Add(output);

            var count = 0;

            ((INotifyPropertyChanged) controlGroup).PropertyChanged += (s, e) =>
            {
                count++;
                Console.WriteLine("Property = " + e.PropertyName + " (" + count + ")");
            };

            controlGroup.Name = "Test group";
            rule.Name = "Test rule";
            condition.Name = "Test condition";
            input.Name = "Test input";
            output.Name = "Test output";

            Assert.AreEqual(5, count);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("    ")]
        public void Name_SettingInvalidValue_LogsErrorAndMaintainsOriginalName(string invalidValue)
        {
            // Setup
            var controlGroup = new ControlGroup();
            string originalName = controlGroup.Name;

            // Precondition
            Assert.IsNotEmpty(originalName);

            // Call
            Action testAction = () => controlGroup.Name = invalidValue;

            // Assert
            const string expectedErrorMessage =
                "Error changing the Name. The field cannot be empty. Please only use alphanumeric, spaces, underscores and dashes.";
            TestHelper.AssertLogMessageIsGenerated(testAction, expectedErrorMessage, 1);

            Assert.AreEqual(originalName, controlGroup.Name);
        }

        [Test]
        public void Name_SettingValidValue_ErrorNotLoggedAndNameUpdated()
        {
            // Setup
            const string validName = "ControlGroupName";
            var controlGroup = new ControlGroup();

            // Call
            Action testAction = () => controlGroup.Name = validName;

            // Assert
            TestHelper.AssertLogMessagesCount(testAction, 0);
            Assert.AreEqual(validName, controlGroup.Name);
        }
    }
}