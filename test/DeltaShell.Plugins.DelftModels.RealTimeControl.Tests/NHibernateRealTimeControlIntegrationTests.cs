using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DeltaShell.IntegrationTestUtils.NHibernate;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;
using DeltaShell.Plugins.NetworkEditor;
using GeoAPI.Extensions.Feature;
using NUnit.Framework;
using SharpTestsEx;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    [Category(TestCategory.Slow)]
    public class NHibernateRealTimeControlIntegrationTests : NHibernateIntegrationTestBase
    {
        [OneTimeSetUp]
        public override void OneTimeSetUp()
        {
            var additionalPlugins = new List<IPlugin>
            {
                new RealTimeControlGuiPlugin(),
                new NetworkEditorApplicationPlugin(),
                new RealTimeControlApplicationPlugin()
            };
            factory = new NHibernateProjectRepositoryFactoryBuilder().AddPlugins(additionalPlugins).Build();
        }

        [Test]
        public void TestRtcOutputFileFunctionStoreIsPersisted()
        {
            var rtcModel = new RealTimeControlModel
            {
                OutputFileFunctionStore = new RealTimeControlOutputFileFunctionStore
                {
                    Features = new List<IFeature>(),
                    Path = @"C:\SomeDirectory\SomeFile.nc",
                }
            };

            var retrievedEntity = SaveAndRetrieveObject(rtcModel);

            Assert.IsNotNull(retrievedEntity);
            Assert.NotNull(retrievedEntity.OutputFileFunctionStore);
            Assert.AreEqual(rtcModel.OutputFileFunctionStore.Path, retrievedEntity.OutputFileFunctionStore.Path);
        }

        [Test]
        public void SaveAndLoadProjectWithRtcModelControlGroup()
        {
            var rtcModel = new RealTimeControlModel("real-time control");
            var retrievedEntity = SaveAndRetrieveObject(rtcModel);

            Assert.IsNotNull(retrievedEntity);
            Assert.AreEqual("real-time control", retrievedEntity.Name);
        }

        [Test]
        public void SaveAndLoadProjectWithRtcModelControlGroupAndATimeCondition()
        {
            var rtcModel = new RealTimeControlModel("Test RTC Model");

            var controlGroup = new ControlGroup { Name = "myFirstControlGroup" };
            rtcModel.ControlGroups.Add(controlGroup);

            var timeSeries = new TimeSeries();
            timeSeries.Components.Add(new Variable<bool> { DefaultValue = false });
            timeSeries[DateTime.Now] = true;
            timeSeries[DateTime.Now + new TimeSpan(1, 0, 0)] = false;
            timeSeries[DateTime.Now + new TimeSpan(2, 0, 0)] = true;
            timeSeries[DateTime.Now + new TimeSpan(3, 0, 0)] = false;
            timeSeries[DateTime.Now + new TimeSpan(4, 0, 0)] = true;

            var input = new Input { ParameterName = "InputParameterName" };

            var timeCondition = new TimeCondition
            {
                Name = "TimeCondition",
                LongName = "TimeConditionTimeCondition",
                Reference = "Implicit",
                Input = input,
                TimeSeries = timeSeries,
                Extrapolation = ExtrapolationType.Periodic,
                InterpolationOptionsTime = InterpolationType.Linear
            };

            controlGroup.Inputs.Add(input);
            controlGroup.Conditions.Add(timeCondition);

            var retrievedEntity = SaveAndRetrieveObject(rtcModel);
            Assert.IsNotNull(retrievedEntity);

            var retrievedTimeCondition = (TimeCondition)retrievedEntity.ControlGroups.First().Conditions.First();

            Assert.AreEqual(timeCondition.Name, retrievedTimeCondition.Name);
            Assert.AreEqual(timeCondition.LongName, retrievedTimeCondition.LongName);
            Assert.AreEqual(timeCondition.Reference, retrievedTimeCondition.Reference);
            Assert.AreEqual(timeCondition.Extrapolation, retrievedTimeCondition.Extrapolation);
            Assert.AreEqual(timeCondition.InterpolationOptionsTime, retrievedTimeCondition.InterpolationOptionsTime);
            Assert.AreEqual(timeCondition.TimeSeries.Arguments[0].Values.Count, retrievedTimeCondition.TimeSeries.Arguments[0].Values.Count);
        }

        [Test]
        public void SaveAndLoadProjectWithRtcModelControlGroupAndALookupSignal()
        {
            var rtcModel = new RealTimeControlModel("Test RTC Model");

            var controlGroup = new ControlGroup { Name = "myFirstControlGroup" };
            rtcModel.ControlGroups.Add(controlGroup);

            var input = new Input { ParameterName = "InputParameterName" };

            var lookupSignal = new LookupSignal();

            controlGroup.Signals.Add(lookupSignal);

            var retrievedEntity = SaveAndRetrieveObject(rtcModel);
            Assert.IsNotNull(retrievedEntity);

            var retrievedLookupSignal = (LookupSignal)retrievedEntity.ControlGroups.First().Signals.First();

            Assert.AreEqual(lookupSignal.Name, retrievedLookupSignal.Name);
        }

        [Test]
        public void SaveAndLoadProjectWithCustomRtcModel()
        {
            var retrievedModel = SaveAndRetrieveObject(RealTimeControlTestHelper.GenerateTestModel(false));
            Assert.IsNotNull(retrievedModel);
            var retrievedControlGroup = retrievedModel.ControlGroups.FirstOrDefault();
            Assert.IsNotNull(retrievedControlGroup);
            var resultControlGroup = RealTimeControlTestHelper.GenerateTestModel(false).ControlGroups.FirstOrDefault();

            Assert.AreEqual(RealTimeControlTestHelper.GenerateTestModel(false).Name, retrievedModel.Name);
            Assert.AreEqual(resultControlGroup.Name, retrievedControlGroup.Name);
            Assert.AreEqual(resultControlGroup.Inputs.FirstOrDefault().Name, retrievedControlGroup.Inputs.FirstOrDefault().Name);
            Assert.AreEqual(resultControlGroup.Outputs.FirstOrDefault().Name, retrievedControlGroup.Outputs.FirstOrDefault().Name);
            Assert.AreEqual(resultControlGroup.Rules.FirstOrDefault().Name, retrievedControlGroup.Rules.FirstOrDefault().Name);
            Assert.AreEqual(resultControlGroup.Conditions.FirstOrDefault().Name, retrievedControlGroup.Conditions.FirstOrDefault().Name);
        }

        [Test]
        public void SaveAndLoadProjectWithCustomRtcModelIncludingAllTheRules()
        {
            var retrievedModel = SaveAndRetrieveObject(RealTimeControlTestHelper.GenerateTestModel(true));
            Assert.IsNotNull(retrievedModel);
            var retrievedControlGroup = retrievedModel.ControlGroups.FirstOrDefault();
            Assert.IsNotNull(retrievedControlGroup);
            var resultControlGroup = RealTimeControlTestHelper.GenerateTestModel(true).ControlGroups.FirstOrDefault();

            Assert.AreEqual(RealTimeControlTestHelper.GenerateTestModel(true).Name, retrievedModel.Name);
            Assert.AreEqual(resultControlGroup.Name, retrievedControlGroup.Name);
            Assert.AreEqual(resultControlGroup.Inputs.FirstOrDefault().Name, retrievedControlGroup.Inputs.FirstOrDefault().Name);
            Assert.AreEqual(resultControlGroup.Outputs.FirstOrDefault().Name, retrievedControlGroup.Outputs.FirstOrDefault().Name);
            for (int i = 0; i < resultControlGroup.Rules.Count; i++)
            {
                Assert.IsTrue(RealTimeControlTestHelper.CompareEqualityOfRules(resultControlGroup.Rules[i], resultControlGroup.Rules[i]));
            }
            Assert.AreEqual(resultControlGroup.Conditions.FirstOrDefault().Name, retrievedControlGroup.Conditions.FirstOrDefault().Name);
        }

        [Test]
        public void SaveAndLoadProjectWithCustomRtcModelIncludingAllRulesAfterChangingOneRule()
        {
            var originalModel = RealTimeControlTestHelper.GenerateTestModel(true);
            var controlgroup = originalModel.ControlGroups.FirstOrDefault();
            var newRule = RealTimeControlTestHelper.GenerateTimeRule();
            var controller = new ControlGroupEditorController { ControlGroup = controlgroup };
            newRule.Name = "new Rule";
            var oldRule = controlgroup.Rules.Where(r => r.GetType() == typeof(PIDRule)).First();
            controller.ConvertRuleTypeTo(oldRule, typeof(TimeRule));

            var retrievedModel = SaveAndRetrieveObject(originalModel);

            Assert.IsNotNull(retrievedModel);
            var retrievedControlGroup = retrievedModel.ControlGroups.FirstOrDefault();
            Assert.IsNotNull(retrievedControlGroup);

            for (var i = 0; i < retrievedControlGroup.Rules.Count; i++)
            {
                Assert.IsTrue(RealTimeControlTestHelper.CompareEqualityOfRules(retrievedControlGroup.Rules[i], controlgroup.Rules[i]));
            }
            Assert.AreEqual(retrievedControlGroup.Conditions.FirstOrDefault().Name, retrievedControlGroup.Conditions.FirstOrDefault().Name);
        }

        [Test]
        public void SaveRetrieveModelConvertedRuleAndSaveAgain()
        {
            var model = new RealTimeControlModel("testModel");
            var controlGroup = new ControlGroup { Name = "myFirstControlGroup" };
            var hydroRule = new HydraulicRule() { Name = "rule name" };
            var input = new Input { ParameterName = "noot" };
            hydroRule.Inputs.Add(input);
            controlGroup.Rules.Add(hydroRule);
            controlGroup.Inputs.Add(input);
            model.ControlGroups.Add(controlGroup);
            var retrievedModel = SaveAndRetrieveObject(model);

            var retrievedControlGroup = retrievedModel.ControlGroups.FirstOrDefault();
            Assert.IsNotNull(retrievedControlGroup);
            var controller = new ControlGroupEditorController { ControlGroup = retrievedControlGroup };
            var retrievedHydroRule = retrievedControlGroup.Rules[0];
            controller.ConvertRuleTypeTo(retrievedHydroRule, typeof(TimeRule));
            RuleBase convertedRule = retrievedControlGroup.Rules.First();
            Assert.AreEqual(typeof(TimeRule), convertedRule.GetType());
            Assert.AreEqual(1, convertedRule.Inputs.Count);
            // resave the project that contains the model
            ProjectRepository.SaveOrUpdate(ProjectRepository.GetProject());
        }

        [Test]
        public void SaveAndRetrieveControlGroup()
        {
            var controlGroup = RealTimeControlTestHelper.GenerateControlGroup();
            var retrievedEntity = SaveAndRetrieveObject(controlGroup);
            Assert.IsNotNull(retrievedEntity);
            Assert.IsTrue(RealTimeControlTestHelper.CompareEqualityOfControlGroups(controlGroup, retrievedEntity));
        }

        [Test]
        public void SaveAndRetrieveControlGroup2Rules1Output()
        {
            var controlGroup = RealTimeControlTestHelper.GenerateControlGroup();
            var extraRule = new HydraulicRule();
            controlGroup.Rules.Add(extraRule);
            extraRule.Outputs.Add(controlGroup.Outputs[0]);
            var retrievedEntity = SaveAndRetrieveObject(controlGroup);
            Assert.IsNotNull(retrievedEntity);
            // mapoing error an Output could only be in 1 rule at the time
            Assert.IsTrue(RealTimeControlTestHelper.CompareEqualityOfControlGroups(controlGroup, retrievedEntity));
        }

        [Test]
        public void SaveAndRetrieveControlGroupWithLookupSignal()
        {
            var controlGroup = RealTimeControlTestHelper.GenerateControlGroup();
            var lookupSignal = new LookupSignal();
            controlGroup.Signals.Add(lookupSignal);
            var retrievedEntity = SaveAndRetrieveObject(controlGroup);
            Assert.IsNotNull(retrievedEntity);
            // mapoing error an Output could only be in 1 rule at the time
            Assert.IsTrue(RealTimeControlTestHelper.CompareEqualityOfControlGroups(controlGroup, retrievedEntity));
        }

        [Test]
        public void SaveAndRetrieveCondition()
        {
            var retrievedEntity = SaveAndRetrieveObject(RealTimeControlTestHelper.GenerateCondition());
            Assert.IsNotNull(retrievedEntity);
            Assert.IsTrue(RealTimeControlTestHelper.CompareEqualityOfStandardConditions(RealTimeControlTestHelper.GenerateCondition(), retrievedEntity));
        }

        [Test]
        public void SaveAndRetrieveDirectionalCondition()
        {
            var retrievedEntity = SaveAndRetrieveObject(RealTimeControlTestHelper.GenerateDirectionalCondition());
            Assert.IsNotNull(retrievedEntity);
            Assert.IsInstanceOf<DirectionalCondition>(retrievedEntity);
            Assert.IsTrue(RealTimeControlTestHelper.CompareEqualityOfConditions(RealTimeControlTestHelper.GenerateDirectionalCondition(), retrievedEntity));
        }

        [Test]
        public void SaveAndRetrievePidRule()
        {
            var retrievedEntity = SaveAndRetrieveObject(RealTimeControlTestHelper.GeneratePidRule());
            Assert.IsNotNull(retrievedEntity);
            Assert.IsTrue(RealTimeControlTestHelper.CompareEqualityOfPIDRules(RealTimeControlTestHelper.GeneratePidRule(), retrievedEntity));
        }

        [Test]
        public void SaveAndRetrieveTimeRule()
        {
            var retrievedEntity = SaveAndRetrieveObject(RealTimeControlTestHelper.GenerateTimeRule());
            Assert.IsNotNull(retrievedEntity);
            Assert.IsTrue(RealTimeControlTestHelper.CompareEqualityOfTimeRules(RealTimeControlTestHelper.GenerateTimeRule(), retrievedEntity));
        }

        [Test]
        public void SaveAndRetrieveHydraulicRuleRule()
        {
            var retrievedEntity = SaveAndRetrieveObject(RealTimeControlTestHelper.GenerateHydraulicRule());
            Assert.IsNotNull(retrievedEntity);
            Assert.IsTrue(RealTimeControlTestHelper.CompareEqualityOfHydraulicRules(RealTimeControlTestHelper.GenerateHydraulicRule(), retrievedEntity));
        }

        [Test]
        public void SaveAndRetrieveFactorRuleRule()
        {
            var retrievedEntity = SaveAndRetrieveObject(RealTimeControlTestHelper.GenerateFactorRule());
            Assert.IsNotNull(retrievedEntity);
            Assert.IsTrue(RealTimeControlTestHelper.CompareEqualityOfFactorRules(RealTimeControlTestHelper.GenerateFactorRule(), retrievedEntity));
        }

        [Test]
        public void SaveAndRetrieveIntervalRule()
        {
            var retrievedEntity = SaveAndRetrieveObject(RealTimeControlTestHelper.GenerateIntervalRule());
            Assert.IsNotNull(retrievedEntity);
            Assert.IsTrue(RealTimeControlTestHelper.CompareEqualityOfIntervalRules(RealTimeControlTestHelper.GenerateIntervalRule(), retrievedEntity));
        }

        [Test]
        public void SaveAndRetrieveRelativeTimeRule()
        {
            var rule = RealTimeControlTestHelper.GenerateRelativeTimeRule();
            rule.FromValue = !rule.FromValue; // do not use default
            var retrievedRule = SaveAndRetrieveObject(rule);
            Assert.IsNotNull(retrievedRule);
            Assert.IsTrue(RealTimeControlTestHelper.CompareEqualityOfRelativeTimeRules(rule, retrievedRule));
        }

        [Test]
        public void SaveAndRetrieveInputAndOutput()
        {
            var retrievedInput = SaveAndRetrieveObject(RealTimeControlTestHelper.GenerateInput());
            Assert.IsNotNull(retrievedInput);
            Assert.AreEqual(RealTimeControlTestHelper.GenerateInput().Name, retrievedInput.Name);

            var retrievedOutput = SaveAndRetrieveObject(RealTimeControlTestHelper.GenerateOutput());
            Assert.IsNotNull(retrievedOutput);
            Assert.AreEqual(RealTimeControlTestHelper.GenerateOutput().Name, retrievedOutput.Name);
        }

        [Test]
        public void SaveAndRetrieveViewContext()
        {
            const int x = 10;
            const int y = 11;
            var shapes = new[] { new RuleShape { X = x, Y = y, Rectangle = new RectangleF(x, y, 200, 30) } };
            var controlGroupEditorViewContext = new ControlGroupEditorViewContext
            {
                ShapeList = shapes,
                AutoSize = true
            };
            var retrievedInput = SaveAndRetrieveObject(controlGroupEditorViewContext);

            Assert.IsNotNull(retrievedInput);
            Assert.IsTrue(retrievedInput.AutoSize);

            var shapeBase = retrievedInput.ShapeList[0];

            Assert.AreEqual(x, shapeBase.X);
            Assert.AreEqual(y, shapeBase.Y);
            Assert.AreEqual(x, shapeBase.Rectangle.X);
            Assert.AreEqual(y, shapeBase.Rectangle.Y);
            Assert.AreEqual(200, shapeBase.Rectangle.Width);
            Assert.AreEqual(30, shapeBase.Rectangle.Height);
        }

        [Test]
        public void SaveAndLoadRuleShapeObject()
        {
            const int x = 2;
            const int y = 3;
            var shape = new RuleShape { X = x, Y = y };
            var retrievedEntity = SaveAndRetrieveObject(shape);
            Assert.IsNotNull(retrievedEntity);
            Assert.AreEqual(x, retrievedEntity.X);
            Assert.AreEqual(y, retrievedEntity.Y);
        }

        [Test]
        public void SaveAndRetrieveControlGroupWithTwoConditionsConnectedToRule()
        {
            // create control group with 1 rule and 2 conditions, linked to that rule using TrueOutputs
            var rule = new PIDRule();

            var condition1 = new StandardCondition { TrueOutputs = { rule } };
            var condition2 = new StandardCondition { TrueOutputs = { rule } };

            var controlGroup = new ControlGroup { Rules = { rule }, Conditions = { condition1, condition2 } };

            // save / load
            var controlGroupRetrieved = SaveAndRetrieveObject(controlGroup);

            // asserts
            var ruleRetrieved = controlGroupRetrieved.Rules[0];

            controlGroupRetrieved.Conditions[0].TrueOutputs.Contains(ruleRetrieved)
                .Should("condition1 has rule as TrueOutput after load").Be.True();

            controlGroupRetrieved.Conditions[1].TrueOutputs.Contains(ruleRetrieved)
                .Should("condition2 has rule as TrueOutput after load").Be.True();
        }

        [Test]
        public void SaveAndRetrieveLookupSignal()
        {
            var retrievedEntity = SaveAndRetrieveObject(RealTimeControlTestHelper.GenerateLookupSignal());
            Assert.IsNotNull(retrievedEntity);
            Assert.IsTrue(RealTimeControlTestHelper.CompareEqualityOfLookupSignals(RealTimeControlTestHelper.GenerateLookupSignal(), retrievedEntity));
        }
    }
}