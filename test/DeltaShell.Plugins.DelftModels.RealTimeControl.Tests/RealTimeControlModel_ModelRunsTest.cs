using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests
{
    [TestFixture]
    [Category("DIMR_Introduction")]
    [Category(TestCategory.WorkInProgress)]
    public class RealTimeControlModel_ModelRunsTest
    {
        # region Model runs

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void TestLinkedDataItemsWorksIndependentOfActivityOrderInWorkflow()
        {
            // Setup models and control group
            ControlledTestModel controlledModel;
            RealTimeControlModel realTimeControlModel;

            RealTimeControlTestHelper.SetupControlledTestModel(out controlledModel, out realTimeControlModel);
            realTimeControlModel.ControlGroups.Add(RealTimeControlTestHelper.SetupHydraulicRuleControlGroup(controlledModel, realTimeControlModel, true));

            // Initialise models: (RTC before controlledModel)
            realTimeControlModel.Initialize();
            var linkedDataItems = TypeUtils.GetField<RealTimeControlModel, IList<IDataItem>>(realTimeControlModel, "linkedDataItemsOriginalValues");
            Assert.AreEqual(linkedDataItems.Count, 0);

            controlledModel.Initialize();
            Assert.AreEqual(linkedDataItems.Count, 1, "RealTimeControlModel's collection of linked DataItems should be updated after initialisation of controlled Model");

            realTimeControlModel.Finish();
            controlledModel.Finish();

            // reset for next run
            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();
            Assert.AreEqual(linkedDataItems.Count, 0, "RealTimeControlModel's collection of linked DataItems should be cleared after cleanup");

            // Initialise models: (controlledModel before RTC)
            controlledModel.Initialize();
            Assert.AreEqual(linkedDataItems.Count, 1, "RealTimeControlModel's collection of linked DataItems should be updated after initialisation of controlled Model");

            realTimeControlModel.Initialize();
            controlledModel.Finish();

            realTimeControlModel.Finish();

            realTimeControlModel.Cleanup();
            Assert.AreEqual(linkedDataItems.Count, 0, "RealTimeControlModel's collection of linked DataItems should be cleared after cleanup");
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void ExecuteHydraulicRule()
        {
            ControlledTestModel controlledModel;
            RealTimeControlModel realTimeControlModel;

            RealTimeControlTestHelper.SetupControlledTestModel(out controlledModel, out realTimeControlModel);

            var controlGroup = RealTimeControlTestHelper.SetupHydraulicRuleControlGroup(controlledModel, realTimeControlModel, true);

            // Initialize
            realTimeControlModel.Initialize();
            controlledModel.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            // Run both models
            var timeStepsCount = 0;
            var weirValues = new List<double>();

            while (realTimeControlModel.Status != ActivityStatus.Done)
            {
                controlledModel.GetChildDataItems(controlledModel.OutputFeatures[0]).First().Value = -1.0; // Set output value of controlled model (which is input for RTC)

                Assert.AreEqual(new DateTime(2000, 1, 1, 0 + timeStepsCount, 0, 0), realTimeControlModel.CurrentTime);

                realTimeControlModel.Execute();
                controlledModel.Execute();

                weirValues.Add(controlGroup.Outputs[0].Value);

                timeStepsCount++;
            }

            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();

            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, controlledModel.Status);
            Assert.AreEqual(6, timeStepsCount);
            Assert.AreEqual(new[] { 33.0, 33.0, 33.0, 33.0, 33.0, 33.0 }, weirValues.ToArray());
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void ExecuteHydraulicRuleBasedOnFlowDirection()
        {
            ControlledTestModel controlledModel;
            RealTimeControlModel realTimeControlModel;
            RealTimeControlTestHelper.SetupControlledTestModel(out controlledModel, out realTimeControlModel);

            var controlGroup = RealTimeControlTestHelper.SetupHydraulicRuleControlGroup(controlledModel, realTimeControlModel, true);
            var hydraulicRule = (HydraulicRule)controlGroup.Rules[0];

            hydraulicRule.Function.Clear();
            hydraulicRule.Function[-1.0] = 40.0;
            hydraulicRule.Function[0.0] = 50.0;

            var inputValues = new[] { -2.0, -0.5, 0.0, 0.5, 10.0, 2.0 };

            // Initialize
            realTimeControlModel.Initialize();
            controlledModel.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            // Run the models
            var timeStepsCount = 0;
            var weirValues = new List<double>();
            while (realTimeControlModel.Status != ActivityStatus.Done)
            {
                controlledModel.GetChildDataItems(controlledModel.OutputFeatures[0]).First().Value = inputValues[timeStepsCount]; // Set output value of controlled model (which is input for RTC)

                Assert.AreEqual(new DateTime(2000, 1, 1, 0 + timeStepsCount, 0, 0), realTimeControlModel.CurrentTime);

                realTimeControlModel.Execute();
                controlledModel.Execute();

                weirValues.Add(controlGroup.Outputs[0].Value);

                timeStepsCount++;
            }
            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();

            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, controlledModel.Status);
            Assert.AreEqual(6, timeStepsCount);
            Assert.AreEqual(new[] { 40.0, 40.0, 50.0, 50.0, 50.0, 50.0 }, weirValues.ToArray());
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void ExecuteHydraulicRuleWithTimeLag()
        {
            var results = new List<double>();
            ControlledTestModel controlledModel;
            RealTimeControlModel realTimeControlModel;
            IList<double> observationPointValues = new List<double>();

            RealTimeControlTestHelper.SetupControlledTestModel(out controlledModel, out realTimeControlModel);
            controlledModel.StopTime = controlledModel.StartTime.AddSeconds(12 * controlledModel.TimeStep.TotalSeconds);

            var controlGroup = RealTimeControlTestHelper.SetupHydraulicRuleControlGroup(controlledModel, realTimeControlModel, true);
            var hydraulicRule = (HydraulicRule)controlGroup.Rules[0];
            var output = hydraulicRule.Outputs.First();

            hydraulicRule.Interpolation = InterpolationType.Linear;
            hydraulicRule.TimeLag = 3 * Convert.ToInt32(realTimeControlModel.TimeStep.TotalSeconds); // TimeLag as 3 timesteps (back in time)

            // Input is output -> handy for this test
            hydraulicRule.Function.Clear();
            hydraulicRule.Function[-20.0] = -20.0;
            hydraulicRule.Function[20.0] = 20.0;

            // Initialize
            realTimeControlModel.Initialize();
            controlledModel.Initialize();

            Assert.AreEqual(3, hydraulicRule.TimeLagInTimeSteps);
            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            // Run the models
            var timeStepsCount = 0;
            while (realTimeControlModel.Status != ActivityStatus.Done)
            {
                controlledModel.GetChildDataItems(controlledModel.OutputFeatures[0]).First().Value = Double.Parse(timeStepsCount.ToString()); // Set output value of controlled model (which is input for RTC)
                observationPointValues.Add(timeStepsCount);

                output.Value = -1.0; // Reset to check when value is actually set

                Assert.AreEqual(new DateTime(2000, 1, 1, 0 + timeStepsCount, 0, 0), realTimeControlModel.CurrentTime);
                realTimeControlModel.Execute();
                controlledModel.Execute();

                var delayedIndex = timeStepsCount - hydraulicRule.TimeLagInTimeSteps + 1;

                if (delayedIndex >= 0 && timeStepsCount < 12 - 1) // -1 is not a valid index, the last timestep never executes
                {
                    results.Add(output.Value);
                    Assert.AreEqual(observationPointValues[delayedIndex], output.Value, "TimeLag is not working properly");
                }

                timeStepsCount++;
            }
            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();

            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, controlledModel.Status);
            Assert.AreEqual(12, timeStepsCount);
            Assert.Less(hydraulicRule.TimeLagInTimeSteps, timeStepsCount);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void ExecuteInvertorRule()
        {
            ControlledTestModel controlledModel;
            RealTimeControlModel realTimeControlModel;
            RealTimeControlTestHelper.SetupControlledTestModel(out controlledModel, out realTimeControlModel);

            controlledModel.StopTime += controlledModel.TimeStep;
            controlledModel.StopTime += controlledModel.TimeStep;
            var controlGroup = RealTimeControlTestHelper.SetupInvertorRule(controlledModel, realTimeControlModel, null);

            // Initialize
            realTimeControlModel.Initialize();
            controlledModel.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            // Run the models
            var timeStepsCount = 0;
            var inputValues = new[] { 10.0, 1.0, 0.5, 0.0, -0.5, -1.0, -10.0, -100 };
            var resultValues = new List<double>();

            while (realTimeControlModel.Status != ActivityStatus.Done)
            {
                controlledModel.GetChildDataItems(controlledModel.OutputFeatures[0]).First().Value = inputValues[timeStepsCount]; // Set output value of controlled model (which is input for RTC)

                controlGroup.Outputs[0].Value = -1.0; // Reset to check when value is actually set

                Assert.AreEqual(new DateTime(2000, 1, 1, 0 + timeStepsCount, 0, 0), realTimeControlModel.CurrentTime);

                realTimeControlModel.Execute();
                controlledModel.Execute();

                resultValues.Add(controlGroup.Outputs[0].Value);

                timeStepsCount++;
            }
            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();

            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, controlledModel.Status);
            Assert.AreEqual(8, timeStepsCount);
            Assert.AreEqual(new[] { -10.0, -1.0, -0.5, 0.0, 0.5, 1.0, 10.0, 100.0 }, resultValues.ToArray());
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Performance)]
        public void InvertorRulePerformance()
        {
            ControlledTestModel controlledModel;
            RealTimeControlModel realTimeControlModel;
            RealTimeControlTestHelper.SetupControlledTestModel(out controlledModel, out realTimeControlModel);

            controlledModel.StopTime = controlledModel.StartTime + new TimeSpan(30, 0, 0, 0);
            controlledModel.TimeStep = new TimeSpan(0, 0, 10, 0);

            RealTimeControlTestHelper.SetupInvertorRule(controlledModel, realTimeControlModel, null);

            TestHelper.AssertIsFasterThan(3500, () =>
            {
                // Initialize
                realTimeControlModel.Initialize();
                controlledModel.Initialize();

                Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
                Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

                // Run the models
                while (realTimeControlModel.Status != ActivityStatus.Done)
                {
                    realTimeControlModel.Execute();
                    controlledModel.Execute();
                }
            });
            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();

            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, controlledModel.Status);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void ExecuteTwoHydraulicRules()
        {
            ControlGroup controlGroup2;
            ControlGroup controlGroup1;
            RealTimeControlModel realTimeControlModel;
            var controlledModel = RealTimeControlTestHelper.SetupTwoIdenticalHydraulicRuleControlGroups(out realTimeControlModel, out controlGroup2, out controlGroup1);

            // Update names for rule and condition to force unique names in xml
            controlGroup2.Rules[0].Name = "rule2";
            controlGroup2.Conditions[0].Name = "condition2";

            // Initialize
            realTimeControlModel.Initialize();
            controlledModel.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            // Run the models
            var timeStepsCount = 0;
            var weir1Values = new List<double>();
            var weir2Values = new List<double>();

            while (realTimeControlModel.Status != ActivityStatus.Done)
            {
                controlGroup1.Outputs[0].Value = -1.0; // Reset to check when value is actually set
                controlGroup2.Outputs[0].Value = -2.0; // Reset to check when value is actually set

                Assert.AreEqual(new DateTime(2000, 1, 1, 0 + timeStepsCount, 0, 0), realTimeControlModel.CurrentTime);

                controlledModel.GetChildDataItems(controlledModel.OutputFeatures[1]).First().Value = (timeStepsCount == 1 || timeStepsCount == 2) ? -1.0 : 1.0; // -1.0 != active | 1.0 == active
                controlledModel.GetChildDataItems(controlledModel.OutputFeatures[2]).First().Value = (timeStepsCount == 2 || timeStepsCount == 4) ? -1.0 : 1.0; // -1.0 != active | 1.0 == active

                realTimeControlModel.Execute();
                controlledModel.Execute();

                Assert.AreNotEqual(ActivityStatus.Failed, realTimeControlModel.Status);

                weir1Values.Add(controlGroup1.Outputs[0].Value);
                weir2Values.Add(controlGroup2.Outputs[0].Value);

                timeStepsCount++;
            }
            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();

            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, controlledModel.Status);
            Assert.AreEqual(6, timeStepsCount);
            Assert.AreEqual(new[] { 33.0, -1.0, -1.0, 33.0, 33.0, 33.0 }, weir1Values.ToArray());
            Assert.AreEqual(new[] { 66.0, 66.0, -2.0, 66.0, -2.0, 66.0 }, weir2Values.ToArray());
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void ExecuteWithTwoFeaturesOnSameOutputCoverage()
        {
            ControlGroup controlGroup1;
            ControlGroup controlGroup2;
            RealTimeControlModel realTimeControlModel;

            var controlledModel = RealTimeControlTestHelper.SetupTwoIdenticalHydraulicRuleControlGroups(out realTimeControlModel, out controlGroup2, out controlGroup1);

            // Update names for rule and condition to force unique names in xml
            controlGroup2.Rules[0].Name = "rule2";
            controlGroup2.Conditions[0].Name = "condition2";

            var inputDataItem1 = controlledModel.GetChildDataItems(controlledModel.InputFeatures.First()).First();
            inputDataItem1.LinkTo(realTimeControlModel.GetDataItemByValue(controlGroup1.Outputs[0]));

            var inputDataItem2 = controlledModel.GetChildDataItems(controlledModel.InputFeatures.Skip(1).First()).First();
            inputDataItem2.LinkTo(realTimeControlModel.GetDataItemByValue(controlGroup2.Outputs[0]));

            // Initialize
            realTimeControlModel.Initialize();
            controlledModel.Initialize();

            Assert.AreEqual(2, realTimeControlModel.OutputFeatureCoverages.SelectMany(o => o.Features).Distinct().Count());
            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            // Run the models
            var timeStepsCount = 0;
            var weir1Values = new List<double>();
            var weir2Values = new List<double>();
            while (realTimeControlModel.Status != ActivityStatus.Done)
            {
                controlGroup1.Outputs[0].Value = -1.0; // Reset to check when value is actually set
                controlGroup2.Outputs[0].Value = -2.0; // Reset to check when value is actually set
                Assert.AreEqual(new DateTime(2000, 1, 1, 0 + timeStepsCount, 0, 0), realTimeControlModel.CurrentTime);

                controlledModel.GetChildDataItems(controlledModel.OutputFeatures[1]).First().Value = (timeStepsCount == 1 || timeStepsCount == 2) ? -1.0 : 1.0; // -1.0 != active | 1.0 == active
                controlledModel.GetChildDataItems(controlledModel.OutputFeatures[2]).First().Value = (timeStepsCount == 2 || timeStepsCount == 4) ? -1.0 : 1.0; // -1.0 != active | 1.0 == active

                realTimeControlModel.Execute();
                controlledModel.Execute();

                weir1Values.Add(controlGroup1.Outputs[0].Value);
                weir2Values.Add(controlGroup2.Outputs[0].Value);

                timeStepsCount++;
            }
            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();

            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, controlledModel.Status);
            Assert.AreEqual(1, realTimeControlModel.OutputFeatureCoverages.Count());
            Assert.AreEqual(6, timeStepsCount);
            Assert.AreEqual(new[] { 33.0, -1.0, -1.0, 33.0, 33.0, 33.0 }, weir1Values.ToArray());
            Assert.AreEqual(new[] { 66.0, 66.0, -2.0, 66.0, -2.0, 66.0 }, weir2Values.ToArray());
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void Execute2HydraulicRulesWithIdenticalNames()
        {
            ControlGroup controlGroup1;
            ControlGroup controlGroup2;
            RealTimeControlModel realTimeControlModel;
            var controlledModel = RealTimeControlTestHelper.SetupTwoIdenticalHydraulicRuleControlGroups(out realTimeControlModel, out controlGroup2, out controlGroup1);

            // Do not update names for rule and condition; names only have to be unique within controlgroup

            // Initialize
            realTimeControlModel.Initialize();
            controlledModel.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            // Run the models
            var timeStepsCount = 0;
            var weir1Values = new List<double>();
            var weir2Values = new List<double>();
            while (realTimeControlModel.Status != ActivityStatus.Done)
            {
                controlGroup1.Outputs[0].Value = -1.0; // Reset to check when value is actually set
                controlGroup2.Outputs[0].Value = -2.0; // Reset to check when value is actually set

                Assert.AreEqual(new DateTime(2000, 1, 1, 0 + timeStepsCount, 0, 0), realTimeControlModel.CurrentTime);

                controlledModel.GetChildDataItems(controlledModel.OutputFeatures[1]).First().Value = (timeStepsCount == 1 || timeStepsCount == 2) ? -1.0 : 1.0; // -1.0 != active | 1.0 == active
                controlledModel.GetChildDataItems(controlledModel.OutputFeatures[2]).First().Value = (timeStepsCount == 2 || timeStepsCount == 4) ? -1.0 : 1.0; // -1.0 != active | 1.0 == active

                realTimeControlModel.Execute();
                controlledModel.Execute();

                Assert.AreNotEqual(ActivityStatus.Failed, realTimeControlModel.Status);

                weir1Values.Add(controlGroup1.Outputs[0].Value);
                weir2Values.Add(controlGroup2.Outputs[0].Value);

                timeStepsCount++;
            }
            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();

            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, controlledModel.Status);
            Assert.AreEqual(6, timeStepsCount);
            CollectionAssert.AreEqual(new[] { 33.0, -1.0, -1.0, 33.0, 33.0, 33.0 }, weir1Values.ToArray());
            CollectionAssert.AreEqual(new[] { 66.0, 66.0, -2.0, 66.0, -2.0, 66.0 }, weir2Values.ToArray());
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void ExecuteHydraulicRuleTrafficLight()
        {
            ControlledTestModel controlledModel;
            RealTimeControlModel realTimeControlModel;

            RealTimeControlTestHelper.SetupControlledTestModel(out controlledModel, out realTimeControlModel);

            var controlGroup = RealTimeControlTestHelper.SetupHydraulicRuleControlGroup(controlledModel, realTimeControlModel, true);

            var intputDataItem1 = controlledModel.GetChildDataItems(controlledModel.OutputFeatures[1]).First();
            intputDataItem1.Value = 2.0; // Rule active
            realTimeControlModel.GetDataItemByValue(controlGroup.Conditions[0].Input).LinkTo(intputDataItem1);

            // Initialize
            realTimeControlModel.Initialize();
            controlledModel.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            // Run the models
            var timeStepsCount = 0;
            var weirValues = new List<double>();
            while (realTimeControlModel.Status != ActivityStatus.Done)
            {
                controlGroup.Outputs[0].Value = -1.0; // Reset to check when value is actually set

                Assert.AreEqual(new DateTime(2000, 1, 1, 0 + timeStepsCount, 0, 0), realTimeControlModel.CurrentTime);

                controlledModel.GetChildDataItems(controlledModel.OutputFeatures[1]).First().Value = (timeStepsCount == 1 || timeStepsCount == 2) ? -1.0 : 1.0; // -1.0 != active | 1.0 == active

                realTimeControlModel.Execute();
                controlledModel.Execute();

                weirValues.Add(controlGroup.Outputs[0].Value);

                timeStepsCount++;
            }
            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();

            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, controlledModel.Status);
            Assert.AreEqual(6, timeStepsCount);

            Assert.AreEqual(new[] { 33.0, -1.0, -1.0, 33.0, 33.0, 33.0 }, weirValues.ToArray());
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void ExecuteDirectionalCondition()
        {
            var realTimeControlModel = new RealTimeControlModel();
            var controlledModel = new ControlledTestModel
            {
                StartTime = new DateTime(2000, 1, 1, 0, 0, 0),
                StopTime = new DateTime(2000, 1, 1, 6, 0, 0),
                TimeStep = new TimeSpan(0, 1, 0, 0)
            };

            var directionalCondition = new DirectionalCondition { Name = "Directional Condition" };

            var hydraulicRule = new HydraulicRule();
            hydraulicRule.Function[-2.0] = 0.0;
            hydraulicRule.Function[+2.0] = 4.0;

            var controlGroup = RealTimeControlModelHelper.CreateGroupRuleWithOneInputOneConditionInput(hydraulicRule, directionalCondition);
            realTimeControlModel.ControlGroups.Add(controlGroup);

            var compositeActivity = new TestCompositeActivity { Activities = { realTimeControlModel, controlledModel } };

            var outputDataItem1 = controlledModel.GetChildDataItems(controlledModel.OutputFeatures[0]).First();
            outputDataItem1.Value = 1.0;
            realTimeControlModel.GetDataItemByValue(controlGroup.Inputs[0]).LinkTo(outputDataItem1);

            var outputDataItem2 = controlledModel.GetChildDataItems(controlledModel.OutputFeatures[1]).First();
            outputDataItem2.Value = 2.0;
            realTimeControlModel.GetDataItemByValue(controlGroup.Inputs[1]).LinkTo(outputDataItem2);

            var intputDataItem1 = controlledModel.GetChildDataItems(controlledModel.InputFeatures[0]).First();
            intputDataItem1.Value = 12.0;
            intputDataItem1.LinkTo(realTimeControlModel.GetDataItemByValue(controlGroup.Outputs[0]));

            directionalCondition.Operation = Operation.Greater; //increasing

            // Initialize
            compositeActivity.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            // Run the models
            var timeStepsCount = 0;
            var weirValues = new List<double>();
            var inputValues = new double[] { 0, 1, 2, 2, 1, 0 }; // Nothing, increasing, increasing, unchanged, decreasing, decreasing
            while (realTimeControlModel.Status != ActivityStatus.Done)
            {
                outputDataItem2.Value = inputValues[timeStepsCount];

                controlGroup.Outputs[0].Value = -99.0; // Reset to check when value is actually set

                Assert.AreEqual(new DateTime(2000, 1, 1, 0 + timeStepsCount, 0, 0), realTimeControlModel.CurrentTime);

                realTimeControlModel.Execute();
                controlledModel.Execute();

                weirValues.Add(controlGroup.Outputs[0].Value);

                timeStepsCount++;
            }
            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();

            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, controlledModel.Status);
            Assert.AreEqual(6, timeStepsCount);
            Assert.AreEqual(new[] { -99.0, 0.0, 0.0, -99.0, -99.0, -99.0 }, weirValues.ToArray()); // Condition is only active on time step 1 and 2. Other time steps it does nothing!
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void ExecuteTimeRule()
        {
            var realTimeControlModel = new RealTimeControlModel();
            var controlledModel = new ControlledTestModel
            {
                StartTime = new DateTime(2000, 1, 1, 0, 0, 0),
                StopTime = new DateTime(2000, 1, 1, 6, 0, 0),
                TimeStep = new TimeSpan(0, 1, 0, 0)
            };

            var controlGroup = RealTimeControlModelHelper.CreateGroupTimeRuleWithCondition();
            realTimeControlModel.ControlGroups.Add(controlGroup);

            new TestCompositeActivity { Activities = { realTimeControlModel, controlledModel } };

            var outputDataItem1 = controlledModel.GetChildDataItems(controlledModel.OutputFeatures[0]).First();
            outputDataItem1.Value = 2.0;
            realTimeControlModel.GetDataItemByValue(controlGroup.Inputs[0]).LinkTo(outputDataItem1);

            var intputDataItem1 = controlledModel.GetChildDataItems(controlledModel.InputFeatures[0]).First();
            intputDataItem1.Value = 12.0;
            intputDataItem1.LinkTo(realTimeControlModel.GetDataItemByValue(controlGroup.Outputs[0]));

            ((TimeRule)controlGroup.Rules[0]).InterpolationOptionsTime = InterpolationType.Constant;
            ((TimeRule)controlGroup.Rules[0]).TimeSeries[new DateTime(2000, 1, 1, 0, 0, 0)] = 11.0;
            ((TimeRule)controlGroup.Rules[0]).TimeSeries[new DateTime(2000, 1, 1, 1, 0, 0)] = 12.0;
            ((TimeRule)controlGroup.Rules[0]).TimeSeries[new DateTime(2000, 1, 1, 2, 0, 0)] = 13.0;
            ((TimeRule)controlGroup.Rules[0]).TimeSeries[new DateTime(2000, 1, 1, 3, 0, 0)] = 14.0;
            ((TimeRule)controlGroup.Rules[0]).TimeSeries[new DateTime(2000, 1, 1, 4, 0, 0)] = 15.0;
            ((StandardCondition)controlGroup.Conditions[0]).Operation = Operation.Greater;
            controlGroup.Conditions[0].Value = 0; // 2 > 0 -> condition true : rule active

            // Initialize
            realTimeControlModel.Initialize();
            controlledModel.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            // Run the models
            var timeStepsCount = 0;
            var weirValues = new List<double>();
            while (realTimeControlModel.Status != ActivityStatus.Done)
            {
                controlGroup.Outputs[0].Value = -1.0; // Reset to check when value is actually set

                Assert.AreEqual(new DateTime(2000, 1, 1, 0 + timeStepsCount, 0, 0), realTimeControlModel.CurrentTime);

                realTimeControlModel.Execute();
                controlledModel.Execute();

                weirValues.Add(controlGroup.Outputs[0].Value);

                timeStepsCount++;
            }
            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();

            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, controlledModel.Status);
            Assert.AreEqual(6, timeStepsCount);
            Assert.AreEqual(new[] { 12.0, 13.0, 14.0, 15.0, 15.0, 15.0 }, weirValues.ToArray());
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void ExecuteTwoTimeRuleControlGroups()
        {
            var realTimeControlModel = new RealTimeControlModel();
            var controlledModel = new ControlledTestModel
            {
                StartTime = new DateTime(2000, 1, 1, 0, 0, 0),
                StopTime = new DateTime(2000, 1, 1, 6, 0, 0),
                TimeStep = new TimeSpan(0, 1, 0, 0)
            };

            var controlGroup1 = RealTimeControlTestHelper.CreateControlGroupWithTimeRule("group1", controlledModel, realTimeControlModel);
            var controlGroup2 = RealTimeControlTestHelper.CreateControlGroupWithTimeRule("group2", controlledModel, realTimeControlModel, 1);

            // Validate
            realTimeControlModel.Validate();

            // Initialize
            realTimeControlModel.Initialize();
            controlledModel.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            // Run the models
            var timeStepsCount = 0;
            var weirValues1 = new List<double>();
            var weirValues2 = new List<double>();
            while (realTimeControlModel.Status != ActivityStatus.Done)
            {
                controlGroup1.Outputs[0].Value = -1.0; // Reset to check when value is actually set
                controlGroup2.Outputs[0].Value = -1.0; // Reset to check when value is actually set

                Assert.AreEqual(new DateTime(2000, 1, 1, 0 + timeStepsCount, 0, 0), realTimeControlModel.CurrentTime);

                realTimeControlModel.Execute();
                controlledModel.Execute();

                weirValues1.Add(controlGroup1.Outputs[0].Value);
                weirValues2.Add(controlGroup2.Outputs[0].Value);

                timeStepsCount++;
            }
            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();

            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, controlledModel.Status);
            Assert.AreEqual(6, timeStepsCount);

            // Expolation constant thus at end 2 times 16
            Assert.AreEqual(new[] { 12.0, 13.0, 14.0, 15.0, 16.0, 16.0 }, weirValues1.ToArray());
            Assert.AreEqual(new[] { 12.0, 13.0, 14.0, 15.0, 16.0, 16.0 }, weirValues2.ToArray());

        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void ExecuteTimeRuleOneDayTimeStep1HourCalculate1Minute()
        {
            var realTimeControlModel = new RealTimeControlModel();
            var controlledModel = new ControlledTestModel
            {
                StartTime = new DateTime(2000, 1, 1, 0, 0, 0),
                StopTime = new DateTime(2000, 1, 2, 0, 0, 0),
                TimeStep = new TimeSpan(0, 0, 1, 0)
            };

            var controlGroup = RealTimeControlModelHelper.CreateGroupTimeRuleWithCondition();
            realTimeControlModel.ControlGroups.Add(controlGroup);

            new TestCompositeActivity { Activities = { realTimeControlModel, controlledModel } };

            var outputDataItem1 = controlledModel.GetChildDataItems(controlledModel.OutputFeatures[0]).First();
            outputDataItem1.Value = 2.0;
            realTimeControlModel.GetDataItemByValue(controlGroup.Inputs[0]).LinkTo(outputDataItem1);

            var intputDataItem1 = controlledModel.GetChildDataItems(controlledModel.InputFeatures[0]).First();
            intputDataItem1.Value = 12.0;
            intputDataItem1.LinkTo(realTimeControlModel.GetDataItemByValue(controlGroup.Outputs[0]));

            var timeRule = (TimeRule)controlGroup.Rules[0];
            timeRule.InterpolationOptionsTime = InterpolationType.Linear;

            for (var i = 0; i < 24; i++)
            {
                timeRule.TimeSeries[new DateTime(2000, 1, 1, i, 0, 0)] = (double)i;
            }

            timeRule.TimeSeries[new DateTime(2000, 1, 2, 0, 0, 0)] = (double)24;
            ((StandardCondition)controlGroup.Conditions[0]).Operation = Operation.Greater;
            controlGroup.Conditions[0].Value = 0; // 2 > 0 -> condition true : rule active

            // Initialize
            realTimeControlModel.Initialize();
            controlledModel.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            // Run the models
            var timeStepsCount = 0;
            var weirValues = new List<double>();
            while (realTimeControlModel.Status != ActivityStatus.Done)
            {
                controlGroup.Outputs[0].Value = -1.0; // Reset to check when value is actually set

                realTimeControlModel.Execute();
                controlledModel.Execute();

                weirValues.Add(controlGroup.Outputs[0].Value);

                timeStepsCount++;
            }
            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();

            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, controlledModel.Status);
            Assert.AreEqual(1440, timeStepsCount);

            // One calculation timestep earlier to start flow model at given time
            Assert.AreEqual(1.0, weirValues[59], 1.0e-4);
            Assert.AreEqual(2.0, weirValues[119], 1.0e-4);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void ExecuteRelativeTimeRuleAbsolute()
        {
            ControlledTestModel controlledModel;
            RealTimeControlModel realTimeControlModel;
            var controlGroup = RealTimeControlTestHelper.SetupRelativeTimeRule(out controlledModel, out realTimeControlModel, false);

            // Initialize
            realTimeControlModel.Initialize();
            controlledModel.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            // Run the models
            var timeStepsCount = 0;
            var weirValues = new List<double>();
            while (realTimeControlModel.Status != ActivityStatus.Done)
            {
                controlGroup.Outputs[0].Value = -999.0; // Reset to check when value is actually set

                Assert.AreEqual(new DateTime(2000, 1, 1, 0 + timeStepsCount, 0, 0), realTimeControlModel.CurrentTime);

                realTimeControlModel.Execute();
                controlledModel.Execute();

                weirValues.Add(controlGroup.Outputs[0].Value);

                timeStepsCount++;
            }
            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();

            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, controlledModel.Status);
            Assert.AreEqual(6, timeStepsCount);

            // RelativeTimeRule with <valueOption>ABSOLUTE</valueOption>
            // time series is 33 -> 66 in 10000 seconds
            // condition always active -> each time step of 1hour = 3600 add (3600/10000) * (66-33) = 11.88
            // t = 0 -> 33 = 33.0
            // t = 1 -> 33 + 11.88 = 44.88
            // t = 2 -> 44.88 + 11.88 = 56.76
            // t = 3 -> 56.76 + 11.88 = 68.64 > 66 (should stop at 66)
            // t = 4 -> 66
            // t = 5 -> 66 (last step is not set to waterflowmodel)
            Assert.IsTrue(RealTimeControlTestHelper.CompareArray(new[] { 33.0, 44.88, 56.76, 66.0, 66.0, 66.0 }, weirValues.ToArray(), 1.0e-6), "Arrays are not equal");
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void ExecuteRelativeTimeRuleAbsoluteWithTimeCondition()
        {
            var startTime = new DateTime(2000, 1, 1, 0, 0, 0);
            var timeStep = new TimeSpan(0, 1, 0, 0);
            var realTimeControlModel = new RealTimeControlModel();
            var controlledModel = new ControlledTestModel
            {
                StartTime = startTime,
                StopTime = new DateTime(2000, 1, 1, 6, 0, 0),
                TimeStep = timeStep
            };

            var controlGroup = new ControlGroup { Name = "Control group" };
            var ruleOutput = new Output();
            controlGroup.Outputs.Add(ruleOutput);

            realTimeControlModel.ControlGroups.Add(controlGroup);
            var comp = new TestCompositeActivity { Activities = { realTimeControlModel, controlledModel } };
            comp.Activities.Add(realTimeControlModel);
            comp.Activities.Add(controlledModel);

            var relativeTimeRule = new RelativeTimeRule("TimeControl", false) { FromValue = false };
            relativeTimeRule.Function[0.0] = 33.0;
            relativeTimeRule.Function[10000.0] = 66.0;
            relativeTimeRule.Outputs.Add(ruleOutput);
            controlGroup.Rules.Add(relativeTimeRule);

            // Add time condition
            var timeCondition = new TimeCondition { Extrapolation = ExtrapolationType.Constant };
            timeCondition.TimeSeries[startTime] = false;
            timeCondition.TimeSeries[startTime.AddTicks(2 * timeStep.Ticks)] = true;
            controlGroup.Conditions.Add(timeCondition);
            timeCondition.TrueOutputs.Add(relativeTimeRule);

            var dataItem = controlledModel.GetChildDataItems(controlledModel.InputFeatures[0]).First();
            dataItem.Value = -999.0;
            realTimeControlModel.GetDataItemByValue(controlGroup.Outputs[0]).LinkTo(dataItem);

            // Initialize
            realTimeControlModel.Initialize();
            controlledModel.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            // Run the models
            var timeStepsCount = 0;
            var weirValues = new List<double>();
            while (realTimeControlModel.Status != ActivityStatus.Done)
            {
                controlGroup.Outputs[0].Value = -999.0; // Reset to check when value is actually set

                Assert.AreEqual(startTime.AddTicks(new TimeSpan(timeStep.Ticks * timeStepsCount).Ticks), realTimeControlModel.CurrentTime);

                realTimeControlModel.Execute();
                controlledModel.Execute();

                weirValues.Add(controlGroup.Outputs[0].Value);

                timeStepsCount++;
            }
            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();

            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, controlledModel.Status);
            Assert.AreEqual(6, timeStepsCount);

            // RelativeTimeRule with <valueOption>ABSOLUTE</valueOption>
            // time series is 33 -> 66 in 10000 seconds
            // condition always active -> each time step of 1hour = 3600 add (3600/10000) * (66-33) = 11.88
            // t = 0 -> -999 not on jet 
            // t = 1 -> 33 
            // t = 2 -> 33 + 11.88 = 44.88
            // t = 3 -> 44.88 + 11.88 = 56.76
            // t = 4 -> 56.76 + 11.88 = 68.64 > 66 (should stop at 66)
            // t = 5 -> 66
            Assert.IsTrue(RealTimeControlTestHelper.CompareArray(new[] { -999.0, 33.0, 44.88, 56.76, 66.0, 66.0 }, weirValues.ToArray(), 1.0e-6), "Arrays are not equal");
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void ExecuteRelativeTimeRuleFromValue()
        {
            ControlledTestModel controlledModel;
            RealTimeControlModel realTimeControlModel;
            var controlGroup = RealTimeControlTestHelper.SetupRelativeTimeRule(out controlledModel, out realTimeControlModel, true);

            // The initial value of the output must be set in the state vector
            controlGroup.Outputs[0].Value = 38;

            // Initialize
            realTimeControlModel.Initialize();
            controlledModel.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            // Run the models
            var timeStepsCount = 0;
            var weirValues = new List<double>();

            // For the RelativeTimeFormValue the output is also used as input for RTCTools
            // Relative series is from 33 at t=0 to 66 at t=10000

            // If relative time rule is RELATIVE state vector (12) should be ignored
            controlGroup.Outputs[0].Value = 37;
            while (realTimeControlModel.Status != ActivityStatus.Done)
            {
                Assert.AreEqual(new DateTime(2000, 1, 1, 0 + timeStepsCount, 0, 0), realTimeControlModel.CurrentTime);

                realTimeControlModel.Execute();
                controlledModel.Execute();

                weirValues.Add(controlGroup.Outputs[0].Value);

                timeStepsCount++;
            }
            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();

            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, controlledModel.Status);
            Assert.AreEqual(6, timeStepsCount);

            // RelativeTimeRule with <valueOption>RELATIVE</valueOption>
            // time series is 33 -> 66 in 10000 seconds
            // condition always active -> each time step of 1hour = 3600 add (3600/10000) * (66-33) = 11.88
            // t = 1 -> 37.00 + 11.88 = 48.88
            // t = 2 -> 48.88 + 11.88 = 60.76
            // t = 3 -> 60.76 + 11.88 = 72.64 > 66 (should stop at 66)
            // t = 4 -> 66
            // t = 5 -> 66 (last step is not set to waterflowmodel; but value is not reset -> hold)
            Assert.IsTrue(RealTimeControlTestHelper.CompareArray(new[] { 48.88, 60.76, 66.0, 66.0, 66.0, 66.0 }, weirValues.ToArray(), 1.0e-6), "Arrays are not equal");
        }

        // dvalue/dt 0	
        // start period 518400; ie the rule is reset after 518400 seconds (3 days)
        // Output parameter	Crest level	
        // Condition:    Waterlevel at observation point > 0.9
        // Value t=0 controlled parameter 1	
        // relative time table
        // time     crestlevel
        //      0        0				
        // 172800       -1				ExecuteRelativeTimeRuleRelativeFromSobekExample
        // 345600       -2				
        // 518400       -3				
        //                                          Relative from                 Relative from 
        //                                          time                          value
        // T(day      Rel time   Input measurement  Output RTC t 	Rel time 	  Output RTC t 
        //         (from time)     station at t     (input voor    (from value)   (input voor 
        //                                           flow t+1)                      flow t+1)
        //                                               1                               1
        //     0        86400        1.00   1         -0.5              86400         -0.5
        //     1       172800        0.91   1         -1.0             172800         -1.0
        //     2       259200        0.80   0         -1.0             172800         -1.0
        //     3       345600        0.70   0         -1.0             172800         -1.0
        //     4       432000        0.70   0         -1.0             172800         -1.0
        //     5       518400        0.70   0         -1.0             172800         -1.0
        //     6       518400        0.70   0         -1.0             172800         -1.0
        //     7        86400        1.00   1  restart-0.5             259200         -1.5
        //     8       172800        1.00   1         -1.0             345600         -2.0
        //     9       259200        0.90   1         -1.5             432000         -2.5
        //    10       345600        0.80   0         -1.5             432000         -2.5
        //    11       432000        0.80   0         -1.5             432000         -2.5
        //    12       518400        1.00   1 continue-3.0             518400         -3.0
        //    13        86400        1.00   1         -0.5             518400         -3.0
        //    14       172800        1.00   1         -1.0             518400         -3.0
        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void ExecuteRelativeTimeRuleRelativeFromSobekExample()
        {
            List<double> rtcOutput;
            var timeStepsCount = RealTimeControlTestHelper.SetupAndExecuteRelativeTimeRuleSobekExample(out rtcOutput);

            Assert.AreEqual(15 /* 840 / 60 */, timeStepsCount);

            Assert.IsTrue(RealTimeControlTestHelper.CompareArray(new[]
            { //  expected   observ.   cond.  actual values
                //             point            from rtcTools
                //                              [thus 0 will set to 
                //---------------------------todo fix with extrapolation
                -0.5, //    1.00     1     [0] -0.5 [crest level at t=1
                -1.0, //    0.91     1     [1] -1.0
                -1.0, //    0.80     0     [2] -1.0
                -1.0, //    0.70     0     [3] -1.0
                -1.0, //    0.70     0     [4] -1.0
                -1.0, //    0.70     0     [5] -1.0
                -1.0, //    0.70     0     [6] -1.0
                -1.5, //    1.00     1     [7] -1.5 
                -2.0, //    1.00     1     [8] -2.0
                -2.5, //    0.90     1     [9] -2.5
                -2.5, //    0.80     0    [10] -2.5
                -2.5, //    0.80     0    [11] -2.5
                -3.0, //    1.00     1    [12] -3.0 
                -3.0, //    1.00     1    [13] -3.0
                -3.0  //    1.00     1    [14] -3.0
            }, rtcOutput.ToArray(), 1.0e-6), "Arrays are not equal");
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void ExecuteRelativeTimeRuleTrafficLightAbsolute()
        {
            ControlledTestModel controlledModel;
            RealTimeControlModel realTimeControlModel;
            var controlGroup = RealTimeControlTestHelper.SetupRelativeTimeRule(out controlledModel, out realTimeControlModel, false);

            var intputDataItem1 = controlledModel.GetChildDataItems(controlledModel.OutputFeatures[1]).First();
            intputDataItem1.Value = 2.0; // Rule active
            realTimeControlModel.GetDataItemByValue(controlGroup.Conditions[0].Input).LinkTo(intputDataItem1);

            // Initialize
            realTimeControlModel.Initialize();
            controlledModel.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            // Run the models
            var timeStepsCount = 0;
            var weirValues = new List<double>();
            while (realTimeControlModel.Status != ActivityStatus.Done)
            {
                controlGroup.Outputs[0].Value = -999.0; // Reset to check when value is actually set

                Assert.AreEqual(new DateTime(2000, 1, 1, 0 + timeStepsCount, 0, 0), realTimeControlModel.CurrentTime);

                controlledModel.GetChildDataItems(controlledModel.OutputFeatures[1]).First().Value = timeStepsCount == 3 ? 0.0 : 10.0; //-> time step 3 off

                realTimeControlModel.Execute();
                controlledModel.Execute();

                weirValues.Add(controlGroup.Outputs[0].Value);

                timeStepsCount++;
            }
            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();

            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, controlledModel.Status);
            Assert.AreEqual(6, timeStepsCount);

            // RelativeTimeRule with <valueOption>ABSOLUTE</valueOption>
            // time series is 33 -> 66 in 10000 seconds
            // condition always active -> each time step of 1hour = 3600 add (3600/10000) * (66-33) = 11.88
            // t = 0 -> 33
            // t = 1 -> 33 + 11.88 = 44.88
            // t = 2 -> 44.88 + 11.88 = 56.76
            // t = 3 -> disabled -> 56.76 (Value fixed/not changed)
            // t = 4 -> 33 = 33.0
            // t = 5 -> 33 + 11.88 = 44.88
            // actual values are 33.0, 44.88, 56.76, -->0.0<--, 33.0, 56.76
            Assert.IsTrue(RealTimeControlTestHelper.CompareArray(new[] { 33.0, 44.88, 56.76, 56.76, 33.0, 44.88 }, weirValues.ToArray(), 1.0e-6), "Arrays are not equal");
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void ExecuteRelativeTimeRuleTrafficLightRelative()
        {
            ControlledTestModel controlledModel;
            RealTimeControlModel realTimeControlModel;
            var controlGroup = RealTimeControlTestHelper.SetupRelativeTimeRule(out controlledModel, out realTimeControlModel, true);

            // The initialvalue of the output must be set in the state vector
            controlGroup.Outputs[0].Value = 12;

            var intputDataItem1 = controlledModel.GetChildDataItems(controlledModel.OutputFeatures[1]).First();
            intputDataItem1.Value = 2.0; // Rule active
            realTimeControlModel.GetDataItemByValue(controlGroup.Conditions[0].Input).LinkTo(intputDataItem1);

            // Initialize
            realTimeControlModel.Initialize();
            controlledModel.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            // Run the models
            var timeStepsCount = 0;
            var weirValues = new List<double>();

            // If relative time rule is RELATIVE state vector (12) should be ignored

            controlGroup.Outputs[0].Value = 0; // Lower than 33

            while (realTimeControlModel.Status != ActivityStatus.Done)
            {
                Assert.AreEqual(new DateTime(2000, 1, 1, 0 + timeStepsCount, 0, 0), realTimeControlModel.CurrentTime);

                controlledModel.GetChildDataItems(controlledModel.OutputFeatures[1]).First().Value = timeStepsCount == 2 ? 0.0 : 10.0;

                realTimeControlModel.Execute();
                controlledModel.Execute();

                weirValues.Add(controlGroup.Outputs[0].Value);

                timeStepsCount++;
            }
            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();

            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, controlledModel.Status);
            Assert.AreEqual(6, timeStepsCount);

            // RelativeTimeRule with <valueOption>RELATIVE</valueOption>
            // time series is 33 -> 66 in 10000 seconds
            // condition always active -> each time step of 1hour = 3600 add (3600/10000) * (66-33) = 11.88
            // t = 0 -> 33.00 + 11.88 = 44.88  -> start is always at 33 is first part of time series
            // t = 1 -> 44.88 + 11.88 = 56.76
            // t = 2 -> 56.76; condition disables rule -> hold
            // t = 3 -> 56.76 + 11.88 = 68.64 > 66 ; reactivated 
            // t = 4 -> 66.00 + 11.88 > 66
            // t = 5 -> 66.00 + 11.88 > 66
            Assert.IsTrue(RealTimeControlTestHelper.CompareArray(new[] { 44.88, 56.76, 56.76, 66.00, 66.00, 66.00 }, weirValues.ToArray(), 1.0e-6), "Arrays are not equal");
        }

        /// <summary>
        /// test based on example provided for TOOLS-3639
        /// Parameters                Values			
        /// Setting.above.deadband    7			
        /// Setting.below.deadband    3			
        /// Setpoint                  800
        /// Deadband                  25
        /// Velocity                  0.01
        /// Timestep                  60
        /// 
        /// Timestep:  Input.Value   Input value  direction    Value          Comment
        ///            Measurement   controlled              controlled
        ///            station       parameter                parameter
        /// 1           810          5              0            5            Value within deadband of setpoint: 
        ///                                                                       nothing happens
        /// 2           810          5              0            5	          
        /// 3           900          5              +            5.6          Value above deadband of setpoint; 
        ///                                                                      controlled parameter moves towards 
        ///                                                                      setting above deadband with velocity
        /// 4           900          5.6      	    +    	   	 6.2         
        /// 5           900          6.2      	    +    	   	 6.8         
        /// 6           900          6.8            0            7            Maximum value is reached!
        /// 7           700          7              -            6.4          Value below deadband of setpoint: 
        ///                                                                       controlled parameter moves towards 
        ///                                                                       setting below deadband with velocity
        /// 8           700          6.4      	    -    	   	 5.8         
        /// 9           700          5.8      	    -    	   	 5.2         
        /// 10          700          5.2      	    -    	   	 4.6         
        /// 11          700          4.6            -            4	          
        /// 12          700          4        	    -    	   	 3.4         
        /// 13          700          3.4            0            3            Minimum value is reached
        /// 14          700          3              0            3	          
        /// 15          800          3              0            3            Value within deadband of setpoint: 
        ///                                                                       nothing happens
        /// 16          810          3              0            3	          
        /// 17          900          3              +            3.6          etc.
        /// </summary>

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        [NUnit.Framework.Category(TestCategory.WorkInProgress)] // this test fails too frequently
        public void ExecuteIntervalRule()
        {
            var realTimeControlModel = new RealTimeControlModel();
            var controlledModel = new ControlledTestModel
            {
                StartTime = new DateTime(2000, 1, 1, 0, 0, 0),
                StopTime = new DateTime(2000, 1, 1, 0, 17, 0),
                TimeStep = new TimeSpan(0, 0, 1, 0)
            };

            var observationPointValues = new[]
            {
                810.0, 810.0, 900.0, 900.0, 900.0, 900.0, 700.0, 700.0,
                700.0, 700.0, 700.0, 700.0, 700.0, 700.0, 800.0, 810.0,
                900.0
            };

            const double setPoint = 800.0;
            const double velocity = 0.01;
            const double deadband = 25.0;
            const double settingAbove = 7.0;
            const double settingBelow = 3.0;

            var controlGroup = RealTimeControlModelHelper.CreateGroupIntervalRule();
            realTimeControlModel.ControlGroups.Add(controlGroup);

            new TestCompositeActivity { Activities = { realTimeControlModel, controlledModel } };

            var conditionInput = controlGroup.Conditions[0].Input;
            var intervalRule = (IntervalRule)controlGroup.Rules[0];
            var ruleInput = intervalRule.Inputs[0];

            var outputDataItem1 = controlledModel.GetChildDataItems(controlledModel.OutputFeatures[0]).First();
            outputDataItem1.Value = 2.0;
            realTimeControlModel.GetDataItemByValue(ruleInput).LinkTo(outputDataItem1);

            var outputDataItem2 = controlledModel.GetChildDataItems(controlledModel.OutputFeatures[1]).First();
            outputDataItem2.Value = 3.0;
            realTimeControlModel.GetDataItemByValue(conditionInput).LinkTo(outputDataItem2);

            var intputDataItem1 = controlledModel.GetChildDataItems(controlledModel.InputFeatures[0]).First();
            intputDataItem1.Value = 5.0;
            intputDataItem1.LinkTo(realTimeControlModel.GetDataItemByValue(controlGroup.Outputs[0]));

            // Set setPoint for the complete period -> = constant
            intervalRule.TimeSeries[new DateTime(1900, 1, 1, 0, 0, 0)] = setPoint;
            intervalRule.TimeSeries[new DateTime(2100, 1, 1, 0, 0, 0)] = setPoint;
            intervalRule.DeadbandAroundSetpoint = deadband;
            intervalRule.Setting.Above = settingAbove;
            intervalRule.Setting.Below = settingBelow;
            intervalRule.IntervalType = IntervalRule.IntervalRuleIntervalType.Variable;
            intervalRule.Setting.MaxSpeed = velocity;
            ((StandardCondition)controlGroup.Conditions[0]).Operation = Operation.Greater;
            conditionInput.Value = 2; // Value 
            controlGroup.Conditions[0].Value = 0; // 2 > 0 -> condition true : rule active

            realTimeControlModel.LogLevel = 4; // Logging on to see what goes on on build server (often fails)

            // Initialize
            realTimeControlModel.Initialize();
            controlledModel.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            // Run the models
            var timeStepsCount = 0;
            var weirValues = new List<double>();
            while (realTimeControlModel.Status != ActivityStatus.Done)
            {
                controlGroup.Outputs[0].Value = -1.0; // Reset to check when value is actually set
                controlledModel.GetChildDataItems(controlledModel.OutputFeatures[0]).First().Value = observationPointValues[timeStepsCount];

                Assert.AreEqual(new DateTime(2000, 1, 1, 0, 0 + timeStepsCount, 0), realTimeControlModel.CurrentTime);

                realTimeControlModel.Execute();
                controlledModel.Execute();

                weirValues.Add(controlGroup.Outputs[0].Value);

                timeStepsCount++;
            }
            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();

            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, controlledModel.Status);
            Assert.AreEqual(17, timeStepsCount);

            var expectedValues = new[]
            {
                5.0, 5.0, 5.6, 6.2, 6.8, 7.0, 6.4, 5.8,
                5.2, 4.6, 4.0, 3.4, 3.0, 3.0, 3.0, 3.0,
                3.6
            };

            //    0    0    +    +    +    0    -    -
            //    -    -    -    -    0    0    0    0
            //    +
            Assert.IsTrue(RealTimeControlTestHelper.CompareArray(expectedValues, weirValues.ToArray(), 1.0e-6), "Arrays are not equal");
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void ExecutePidRule()
        {
            Input ruleInput;
            ControlledTestModel controlledModel;
            RealTimeControlModel realTimeControlModel;

            var controlGroup = RealTimeControlTestHelper.SetupPidRule(new DateTime(2000, 1, 1, 0, 0, 0), new DateTime(2000, 1, 1, 6, 0, 0), new TimeSpan(0, 1, 0, 0), out controlledModel, out realTimeControlModel, out ruleInput, true);

            // Initialize
            realTimeControlModel.Initialize();
            controlledModel.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            // Run the models
            var timeStepsCount = 0;
            var weirValues = new List<double>();
            while (realTimeControlModel.Status != ActivityStatus.Done)
            {
                controlGroup.Outputs[0].Value = -1.0; // Reset to check when value is actually set

                ruleInput.Value = 32 + timeStepsCount;

                Assert.AreEqual(new DateTime(2000, 1, 1, 0 + timeStepsCount, 0, 0), realTimeControlModel.CurrentTime);

                realTimeControlModel.Execute();
                controlledModel.Execute();

                weirValues.Add(controlGroup.Outputs[0].Value);

                timeStepsCount++;
            }
            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();

            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, controlledModel.Status);
            Assert.AreEqual(6, timeStepsCount);
            Assert.AreNotEqual(0, weirValues[0]); // TODO: Manually determine valid values
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void ExecutePidRuleWithTimeTrigger_Tools7828()
        {
            // Setup rtc model and controlled model
            var realTimeControlModel = new RealTimeControlModel();
            var controlledModel = new ControlledTestModel
            {
                StartTime = new DateTime(2000, 1, 1, 0, 0, 0),
                StopTime = new DateTime(2000, 1, 1, 8, 0, 0),
                TimeStep = new TimeSpan(0, 1, 0, 0),
                InputFeatures = { new RtcTestFeature { Name = "input1" } },
                OutputFeatures = { new RtcTestFeature { Name = "output1" } }
            };

            // Create control group
            var input = new Input();
            var output = new Output();
            var controlGroup = new ControlGroup { Name = "Control group" };

            controlGroup.Inputs.Add(input);
            controlGroup.Outputs.Add(output);
            realTimeControlModel.ControlGroups.Add(controlGroup);

            var compositeActivity = new TestCompositeActivity { Activities = { realTimeControlModel, controlledModel } };

            var outputDataItem1 = controlledModel.GetChildDataItems(controlledModel.OutputFeatures[0]).First();
            realTimeControlModel.GetDataItemByValue(controlGroup.Inputs[0]).LinkTo(outputDataItem1);

            var inputDataItem = controlledModel.GetChildDataItems(controlledModel.InputFeatures[0]).First();
            inputDataItem.Value = 0.0;
            inputDataItem.LinkTo(realTimeControlModel.GetDataItemByValue(controlGroup.Outputs[0]));

            // PID controller
            var pidRule = new PIDRule("p-only controller")
            {
                Kd = 0,
                Ki = 0,
                Kp = 0.5,
                Setting = { Max = 50, MaxSpeed = 20, Min = -50 },
                PidRuleSetpointType = PIDRule.PIDRuleSetpointType.Constant,
                ConstantValue = 1.0
            };

            pidRule.Inputs.Add(input);
            pidRule.Outputs.Add(output);
            controlGroup.Rules.Add(pidRule);

            // Time trigger
            var timeTrigger = new TimeCondition();
            timeTrigger.TimeSeries[new DateTime(2000, 1, 1, 0, 0, 0)] = false;
            timeTrigger.TimeSeries[new DateTime(2000, 1, 1, 5, 0, 0)] = true;

            controlGroup.Conditions.Add(timeTrigger);
            timeTrigger.TrueOutputs.Add(pidRule);

            // Initialize
            compositeActivity.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            // Run the models
            var inputValue = 0.0;
            var timeStepsCount = 0;
            var outputValues = new List<double>();

            while (realTimeControlModel.Status != ActivityStatus.Done)
            {
                controlGroup.Outputs[0].Value = -999.0; // Reset to check when value is actually set

                controlledModel.GetChildDataItems(controlledModel.OutputFeatures[0]).First().Value = inputValue;

                Assert.AreEqual(new DateTime(2000, 1, 1, 0 + timeStepsCount, 0, 0), realTimeControlModel.CurrentTime);

                realTimeControlModel.Execute();
                controlledModel.Execute();

                inputValue = controlGroup.Outputs[0].Value; // Controlled variable is also observed variable
                outputValues.Add(inputValue);

                timeStepsCount++;
            }
            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();

            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, controlledModel.Status);
            Assert.AreEqual(8, timeStepsCount);

            // Expected value can be calculated by (when active): output[t+1] = output[t] - (output[t] - pidRule.ConstantValue) * Kp 
            // (Ki and Kd are 0, so ignore those PID terms)
            // Controller is active from 5th hour
            var expectedOutput = new[] { 0.0, 0.0, 0.0, 0.0, 0.5, 0.75, 0.875, 0.9375 };
            for (var i = 0; i < 8; i++)
            {
                Assert.AreEqual(expectedOutput[i], outputValues[i], 0.0000001d);
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void ExecutePidRuleAndHydraulicRuleWithSameControlledParameter()
        {
            Input input;
            ControlledTestModel controlledModel;
            RealTimeControlModel realTimeControlModel;

            var controlGroup = RealTimeControlTestHelper.SetupPidRule(new DateTime(2000, 1, 1, 0, 0, 0), new DateTime(2000, 1, 1, 6, 0, 0), new TimeSpan(0, 1, 0, 0), out controlledModel, out realTimeControlModel, out input, true);
            var output = controlGroup.Outputs.First();
            var condition = controlGroup.Conditions.First();

            // Setup hydraulic rule
            var hydraulicRule = new HydraulicRule();
            hydraulicRule.Function[0.0] = 0.0;
            hydraulicRule.Function[10.0] = 10.0;
            hydraulicRule.Inputs.Add(input);
            hydraulicRule.Outputs.Add(output);
            controlGroup.Rules.Add(hydraulicRule);

            // Connect condition and hydraulic rule
            condition.FalseOutputs.Add(hydraulicRule);

            // Initialize
            realTimeControlModel.Initialize();
            controlledModel.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            // Run the models
            var timeStepsCount = 0;
            var weirValues = new List<double>();
            while (realTimeControlModel.Status != ActivityStatus.Done)
            {
                controlGroup.Outputs[0].Value = -1.0; // Reset to check when value is actually set

                input.Value = 32 + timeStepsCount;

                Assert.AreEqual(new DateTime(2000, 1, 1, 0 + timeStepsCount, 0, 0), realTimeControlModel.CurrentTime);

                realTimeControlModel.Execute();
                controlledModel.Execute();

                weirValues.Add(controlGroup.Outputs[0].Value);

                timeStepsCount++;
            }
            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();

            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, controlledModel.Status);
            Assert.AreEqual(6, timeStepsCount);
            Assert.AreNotEqual(0, weirValues[0]); // TODO: Manually determine valid values
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void ExecuteControlGroupWithTwoConditionsOnePidAndOneTimeRuleFromRijntakkenModel()
        {
            // Setup rtc model and controlled model
            var realTimeControlModel = new RealTimeControlModel();
            var controlledModel = new ControlledTestModel
            {
                StartTime = new DateTime(2000, 1, 1, 0, 0, 0),
                StopTime = new DateTime(2000, 1, 1, 11, 0, 0),
                TimeStep = new TimeSpan(0, 1, 0, 0),
                InputFeatures = { new RtcTestFeature { Name = "input1" } },
                OutputFeatures = { new RtcTestFeature { Name = "output1" } }
            };

            var input = new Input();
            var output = new Output();
            var controlGroup = new ControlGroup { Name = "Control group" };

            controlGroup.Inputs.Add(input);
            controlGroup.Outputs.Add(output);
            realTimeControlModel.ControlGroups.Add(controlGroup);

            var compositeActivity = new TestCompositeActivity { Activities = { realTimeControlModel, controlledModel } };

            var outputDataItem1 = controlledModel.GetChildDataItems(controlledModel.OutputFeatures[0]).First();
            realTimeControlModel.GetDataItemByValue(controlGroup.Inputs[0]).LinkTo(outputDataItem1);

            var intputDataItem1 = controlledModel.GetChildDataItems(controlledModel.InputFeatures[0]).First();
            intputDataItem1.Value = 12.0;
            intputDataItem1.LinkTo(realTimeControlModel.GetDataItemByValue(controlGroup.Outputs[0]));

            var condition1 = new StandardCondition
            {
                Name = "condition1",
                Operation = Operation.Greater,
                Value = 4.5,
                Input = input
            };
            // value 
            controlGroup.Conditions.Add(condition1);

            var condition2 = new StandardCondition
            {
                Name = "condition2",
                Operation = Operation.Less,
                Value = 3.5,
                Input = input
            };
            // value 
            controlGroup.Conditions.Add(condition2);

            var pidRule = new PIDRule("pid")
            {
                Kd = 0.32,
                Ki = 0.2,
                Kp = 0.5,
                Setting =
                {
                    Max = 10,
                    MaxSpeed = 0.002,
                    Min = 0
                },
                PidRuleSetpointType = PIDRule.PIDRuleSetpointType.Constant,
                ConstantValue = 5.0
            };

            pidRule.Inputs.Add(input);
            pidRule.Outputs.Add(output);
            controlGroup.Rules.Add(pidRule);

            var timeRule = new TimeRule("timerule");
            timeRule.TimeSeries[new DateTime(1999, 1, 1, 0, 0, 0)] = 2.0;
            timeRule.Outputs.Add(output);
            controlGroup.Rules.Add(timeRule);

            condition1.TrueOutputs.Add(pidRule);
            condition1.FalseOutputs.Add(condition2);
            condition2.TrueOutputs.Add(timeRule);

            // Check the XML
            var xDocument = RealTimeControlXmlWriter.GetToolsConfigXml(DimrApiDataSet.RtcToolsDllPath, realTimeControlModel.ControlGroups);
            Assert.IsNotNull(xDocument);

            const string fewsXmlheader = " xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"" +
                                         " xmlns:rtc=\"http://www.wldelft.nl/fews\"" +
                                         " xmlns=\"http://www.wldelft.nl/fews\"" +
                                         " xsi:schemaLocation=\"" +
                                         @"http://www.wldelft.nl/fews ";

            var rtcToolsConfigxsd = DimrApiDataSet.RtcToolsDllPath + Path.DirectorySeparatorChar + "rtcToolsConfig.xsd\"";

            var expectedXml =
                @"<rtcToolsConfig" + fewsXmlheader + rtcToolsConfigxsd + ">" +
                "<general>" +
                "<description>RTC Model DeltaShell</description>" +
                "<poolRoutingScheme>Theta</poolRoutingScheme>" +
                "<theta>0.5</theta>" +
                "</general>" +
                "<rules>" +
                "<rule>" +
                "<unitDelay id='pid_unitDelay'>" +
                "<input>" +
                "<x>output_input feature 1_Value</x>" +
                "</input>" +
                "<output>" +
                "<y>output_input feature 1_Value</y>" +
                "</output>" +
                "</unitDelay>" +
                "</rule>" +
                "<rule>" +
                "<pid id='Control grouppid'>" +
                "<mode>PIDVEL</mode>" +
                "<settingMin>0</settingMin>" +
                "<settingMax>10</settingMax>" +
                "<settingMaxSpeed>0.002</settingMaxSpeed>" +
                "<kp>0.5</kp>" +
                "<ki>0.2</ki>" +
                "<kd>0.32</kd>" +
                "<input>" +
                "<x>input_output feature 1_Value</x>" +
                "<setpointValue>5</setpointValue>" +
                "</input>" +
                "<output>" +
                "<y>output_input feature 1_Value</y>" +
                "<integralPart>Control grouppid_IP</integralPart>" +
                "<differentialPart>Control grouppid_DP</differentialPart>" +
                "</output>" +
                "</pid>" +
                "</rule>" +
                "<rule>" +
                "<timeAbsolute id='Control grouptimerule'>" +
                "<input>" +
                "<x>Control grouptimerule_TimeSeries</x>" +
                "</input>" +
                "<output>" +
                "<y>output_input feature 1_Value</y>" +
                "</output>" +
                "</timeAbsolute>" +
                "</rule>" +
                "</rules>" +
                "<triggers>" +
                "<trigger>" +
                "<standard id='Control groupcondition1'>" +
                "<condition>" +
                "<x1Series ref='EXPLICIT'>input_output feature 1_Value</x1Series>" +
                "<relationalOperator>Greater</relationalOperator>" +
                "<x2Value>4.5</x2Value>" +
                "</condition>" +
                "<true>" +
                "<trigger>" +
                "<ruleReference>Control grouppid</ruleReference>" +
                "</trigger>" +
                "</true>" +
                "<false>" +
                "<trigger>" +
                "<standard id='Control groupcondition2'>" +
                "<condition>" +
                "<x1Series ref='EXPLICIT'>input_output feature 1_Value</x1Series>" +
                "<relationalOperator>Less</relationalOperator>" +
                "<x2Value>3.5</x2Value>" +
                "</condition>" +
                "<true>" +
                "<trigger>" +
                "<ruleReference>Control grouptimerule</ruleReference>" +
                "</trigger>" +
                "</true>" +
                "<output>" +
                "<status>Control groupStatus_condition2</status>" +
                "</output>" +
                "</standard>" +
                "</trigger>" +
                "</false>" +
                "<output>" +
                "<status>Control groupStatus_condition1</status>" +
                "</output>" +
                "</standard>" +
                "</trigger>" +
                "</triggers>" +
                "</rtcToolsConfig>";

            Assert.AreEqual(expectedXml.Replace("'", "\""), xDocument.ToString(SaveOptions.DisableFormatting));

            // Initialize
            compositeActivity.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            // Run the models
            var timeStepsCount = 0;
            var inputValues = new[] { 7.0, 6.0, 5.0, 4.0, 3.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0 };
            var outputValues = new List<double>();
            while (realTimeControlModel.Status != ActivityStatus.Done)
            {
                controlGroup.Outputs[0].Value = -999.0; // Reset to check when value is actually set

                controlledModel.GetChildDataItems(controlledModel.OutputFeatures[0]).First().Value = inputValues[timeStepsCount];

                Assert.AreEqual(new DateTime(2000, 1, 1, 0 + timeStepsCount, 0, 0), realTimeControlModel.CurrentTime);

                realTimeControlModel.Execute();
                controlledModel.Execute();

                outputValues.Add(controlGroup.Outputs[0].Value);

                timeStepsCount++;
            }
            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();

            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, controlledModel.Status);
            Assert.AreEqual(11, timeStepsCount);

            var expectedOutput = new[] { 10.0, 10.0, 10.0, 10.0, 2.0, 2.0, 2.0, 2.0, 2.0, 0.98, 0.0 };
            for (var i = 0; i < 11; i++)
            {
                Assert.AreEqual(expectedOutput[i], outputValues[i], 0.0000001d);
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void ExecutePidRuleConstantTimeSeries()
        {
            var realTimeControlModel = new RealTimeControlModel();
            var controlledModel = new ControlledTestModel
            {
                StartTime = new DateTime(2000, 1, 1, 0, 0, 0),
                StopTime = new DateTime(2000, 1, 1, 6, 0, 0),
                TimeStep = new TimeSpan(0, 1, 0, 0)
            };

            var controlGroup = RealTimeControlModelHelper.CreateGroupPidRule(false);
            realTimeControlModel.ControlGroups.Add(controlGroup);

            new TestCompositeActivity { Activities = { realTimeControlModel, controlledModel } };

            var outputDataItem1 = controlledModel.GetChildDataItems(controlledModel.OutputFeatures[0]).First();
            outputDataItem1.Value = 2.0;
            realTimeControlModel.GetDataItemByValue(controlGroup.Inputs[0]).LinkTo(outputDataItem1);

            var intputDataItem1 = controlledModel.GetChildDataItems(controlledModel.InputFeatures[0]).First();
            intputDataItem1.Value = 12.0;
            intputDataItem1.LinkTo(realTimeControlModel.GetDataItemByValue(controlGroup.Outputs[0]));

            var pidRule = (PIDRule)controlGroup.Rules[0];
            pidRule.Kd = 0.0;
            pidRule.Ki = 0.2;
            pidRule.Kp = 0.5;
            pidRule.Setting.Max = 123.6;
            pidRule.Setting.MaxSpeed = 0.2;
            pidRule.Setting.Min = 116.0;

            // Set timeseries; this is the series the interval rule will try to satisfy
            pidRule.TimeSeries.Time.ExtrapolationType = ExtrapolationType.Constant;
            pidRule.TimeSeries.Components[0].DefaultValue = 150.0;

            var ruleInput = pidRule.Inputs[0];

            // Initialize
            realTimeControlModel.Initialize();
            controlledModel.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            // Run the models
            var timeStepsCount = 0;
            var weirValues = new List<double>();
            while (realTimeControlModel.Status != ActivityStatus.Done)
            {
                controlGroup.Outputs[0].Value = -1.0; // Reset to check when value is actually set

                ruleInput.Value = 32 + timeStepsCount;

                Assert.AreEqual(new DateTime(2000, 1, 1, 0 + timeStepsCount, 0, 0), realTimeControlModel.CurrentTime);

                realTimeControlModel.Execute();
                controlledModel.Execute();

                weirValues.Add(controlGroup.Outputs[0].Value);

                timeStepsCount++;
            }
            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();

            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, controlledModel.Status);
            Assert.AreEqual(6, timeStepsCount);
            Assert.AreNotEqual(0, weirValues[0]); // TODO: Manually determine valid values
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void ExecuteTimeCondition()
        {
            ControlledTestModel controlledModel;
            RealTimeControlModel realTimeControlModel;
            RealTimeControlTestHelper.SetupControlledTestModel(out controlledModel, out realTimeControlModel);

            controlledModel.StopTime += controlledModel.TimeStep;
            controlledModel.StopTime += controlledModel.TimeStep;

            var timeCondition = new TimeCondition { Name = "timer" };
            timeCondition.TimeSeries[controlledModel.StartTime] = true;
            timeCondition.TimeSeries[controlledModel.StartTime + controlledModel.TimeStep] = true;
            timeCondition.TimeSeries[controlledModel.StartTime + new TimeSpan(3 * controlledModel.TimeStep.Ticks)] = false;
            timeCondition.TimeSeries[controlledModel.StartTime + new TimeSpan(4 * controlledModel.TimeStep.Ticks)] = true;
            timeCondition.TimeSeries[controlledModel.StartTime + new TimeSpan(5 * controlledModel.TimeStep.Ticks)] = true;
            timeCondition.TimeSeries[controlledModel.StartTime + new TimeSpan(6 * controlledModel.TimeStep.Ticks)] = false;
            timeCondition.TimeSeries[controlledModel.StartTime + new TimeSpan(7 * controlledModel.TimeStep.Ticks)] = true;
            timeCondition.TimeSeries[controlledModel.StartTime + new TimeSpan(8 * controlledModel.TimeStep.Ticks)] = true;

            var controlGroup = RealTimeControlTestHelper.SetupInvertorRule(controlledModel, realTimeControlModel, timeCondition);

            // Initialize
            realTimeControlModel.Initialize();
            controlledModel.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            // Run the models
            var timeStepsCount = 0;
            var inputValues = new[] { 10.0, 1.0, 0.5, 0.0, -0.5, -1.0, -10.0, -100 };
            var resultValues = new List<double>();
            while (realTimeControlModel.Status != ActivityStatus.Done)
            {
                controlGroup.Outputs[0].Value = -999.0; // Reset to check when value is actually set

                controlledModel.GetChildDataItems(controlledModel.OutputFeatures[0]).First().Value = inputValues[timeStepsCount];

                Assert.AreEqual(new DateTime(2000, 1, 1, timeStepsCount, 0, 0), realTimeControlModel.CurrentTime);

                realTimeControlModel.Execute();
                controlledModel.Execute();

                timeStepsCount++;

                Assert.AreEqual(new DateTime(2000, 1, 1, timeStepsCount, 0, 0), realTimeControlModel.CurrentTime);

                resultValues.Add(controlGroup.Outputs[0].Value);
            }
            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();

            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, controlledModel.Status);
            Assert.AreEqual(8, timeStepsCount);
            Assert.AreEqual(new[] { -10.0, -1.0, -999.0, 0.0, 0.5, -999.0, 10.0, 100.0 }, resultValues.ToArray());
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void ExecuteTimeConditionWithTrueAndFalsePath()
        {
            ControlledTestModel controlledModel;
            RealTimeControlModel realTimeControlModel;
            RealTimeControlTestHelper.SetupControlledTestModel(out controlledModel, out realTimeControlModel);

            controlledModel.StopTime += controlledModel.TimeStep;
            controlledModel.StopTime += controlledModel.TimeStep;

            var timeCondition = new TimeCondition { Name = "TimeCondition1" };
            timeCondition.TimeSeries[controlledModel.StartTime] = true;
            timeCondition.TimeSeries[controlledModel.StartTime + controlledModel.TimeStep] = true;
            timeCondition.TimeSeries[controlledModel.StartTime + new TimeSpan(3 * controlledModel.TimeStep.Ticks)] = false;
            timeCondition.TimeSeries[controlledModel.StartTime + new TimeSpan(4 * controlledModel.TimeStep.Ticks)] = true;
            timeCondition.TimeSeries[controlledModel.StartTime + new TimeSpan(5 * controlledModel.TimeStep.Ticks)] = true;
            timeCondition.TimeSeries[controlledModel.StartTime + new TimeSpan(6 * controlledModel.TimeStep.Ticks)] = false;
            timeCondition.TimeSeries[controlledModel.StartTime + new TimeSpan(7 * controlledModel.TimeStep.Ticks)] = true;

            var controlGroup = RealTimeControlTestHelper.SetupInvertorRule(controlledModel, realTimeControlModel, timeCondition);

            // Add false line
            var hydraulicRule = new HydraulicRule { Name = "HydraulicRule" };
            hydraulicRule.Inputs.Add(controlGroup.Inputs.First());
            hydraulicRule.Outputs.Add(controlGroup.Outputs.First());
            hydraulicRule.Extrapolation = ExtrapolationType.Constant;
            hydraulicRule.Interpolation = InterpolationType.Constant;
            hydraulicRule.Function[-100.0] = 3.33;
            hydraulicRule.Function[100.0] = 3.33;
            controlGroup.Rules.Add(hydraulicRule);

            timeCondition.FalseOutputs.Add(hydraulicRule);

            // Initialize
            realTimeControlModel.Initialize();
            controlledModel.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            // Run the models
            var timeStepsCount = 0;
            var inputValues = new[] { 10.0, 10.0, 10.0, 10.0, 10.0, 10.0, 10.0, 10.0 };
            var resultValues = new List<double>();
            while (realTimeControlModel.Status != ActivityStatus.Done)
            {
                controlGroup.Outputs[0].Value = -999.0; // Reset to check when value is actually set

                controlledModel.GetChildDataItems(controlledModel.OutputFeatures[0]).First().Value = inputValues[timeStepsCount];

                Assert.AreEqual(new DateTime(2000, 1, 1, 0 + timeStepsCount, 0, 0), realTimeControlModel.CurrentTime);

                realTimeControlModel.Execute();
                controlledModel.Execute();

                resultValues.Add(controlGroup.Outputs[0].Value);

                timeStepsCount++;
            }
            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();

            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, controlledModel.Status);
            Assert.AreEqual(8, timeStepsCount);
            Assert.AreEqual(new[] { -10.0, -10.0, 3.33, -10.0, -10.0, 3.33, -10.0, -10.0 }, resultValues.ToArray());
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void ExecuteTwoTimeConditions()
        {
            ControlledTestModel controlledModel;
            RealTimeControlModel realTimeControlModel;
            RealTimeControlTestHelper.SetupControlledTestModel(out controlledModel, out realTimeControlModel);

            controlledModel.StopTime += controlledModel.TimeStep;
            controlledModel.StopTime += controlledModel.TimeStep;

            var timeCondition = new TimeCondition { Name = "TimeCondition1" };
            timeCondition.TimeSeries[controlledModel.StartTime] = true;
            timeCondition.TimeSeries[controlledModel.StartTime + controlledModel.TimeStep] = true;
            timeCondition.TimeSeries[controlledModel.StartTime + new TimeSpan(3 * controlledModel.TimeStep.Ticks)] = false;
            timeCondition.TimeSeries[controlledModel.StartTime + new TimeSpan(4 * controlledModel.TimeStep.Ticks)] = true;
            timeCondition.TimeSeries[controlledModel.StartTime + new TimeSpan(5 * controlledModel.TimeStep.Ticks)] = true;
            timeCondition.TimeSeries[controlledModel.StartTime + new TimeSpan(6 * controlledModel.TimeStep.Ticks)] = false;
            timeCondition.TimeSeries[controlledModel.StartTime + new TimeSpan(7 * controlledModel.TimeStep.Ticks)] = true;
            timeCondition.TimeSeries[controlledModel.StartTime + new TimeSpan(8 * controlledModel.TimeStep.Ticks)] = true;

            var controlGroup = RealTimeControlTestHelper.SetupInvertorRule(controlledModel, realTimeControlModel, timeCondition);

            // Add false line with time condition
            var timeCondition2 = new TimeCondition { Name = "TimeCondition2" };
            timeCondition2.TimeSeries[controlledModel.StartTime] = false;
            timeCondition2.TimeSeries[controlledModel.StartTime + new TimeSpan(3 * controlledModel.TimeStep.Ticks)] = true;
            timeCondition2.TimeSeries[controlledModel.StartTime + new TimeSpan(6 * controlledModel.TimeStep.Ticks)] = false;
            controlGroup.Conditions.Add(timeCondition2);

            var hydraulicRule = new HydraulicRule { Name = "HydraulicRule" };
            hydraulicRule.Inputs.Add(controlGroup.Inputs.First());
            hydraulicRule.Outputs.Add(controlGroup.Outputs.First());
            hydraulicRule.Extrapolation = ExtrapolationType.Constant;
            hydraulicRule.Interpolation = InterpolationType.Constant;
            hydraulicRule.Function[-100.0] = 3.33;
            hydraulicRule.Function[100.0] = 3.33;
            controlGroup.Rules.Add(hydraulicRule);

            timeCondition.FalseOutputs.Add(timeCondition2);
            timeCondition2.TrueOutputs.Add(hydraulicRule);

            // Initialize
            realTimeControlModel.Initialize();
            controlledModel.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            // Run the models
            var timeStepsCount = 0;
            var inputValues = new[] { 10.0, 10.0, 10.0, 10.0, 10.0, 10.0, 10.0, 10.0 };
            var resultValues = new List<double>();
            while (realTimeControlModel.Status != ActivityStatus.Done)
            {
                controlGroup.Outputs[0].Value = -999.0; // Reset to check when value is actually set

                controlledModel.GetChildDataItems(controlledModel.OutputFeatures[0]).First().Value = inputValues[timeStepsCount];

                Assert.AreEqual(new DateTime(2000, 1, 1, 0 + timeStepsCount, 0, 0), realTimeControlModel.CurrentTime);

                realTimeControlModel.Execute();
                controlledModel.Execute();

                resultValues.Add(controlGroup.Outputs[0].Value);

                timeStepsCount++;
            }
            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();

            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, controlledModel.Status);
            Assert.AreEqual(8, timeStepsCount);
            Assert.AreEqual(new[] { -10.0, -10.0, 3.33, -10.0, -10.0, -999.0, -10.0, -10.0 }, resultValues.ToArray());
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void ExecuteTwoTimeConditionsAndTwoConditionsFromRijntakkenModel() // Validate results from engine
        {
            ControlledTestModel controlledModel;
            RealTimeControlModel realTimeControlModel;

            RealTimeControlTestHelper.SetupControlledTestModel(out controlledModel, out realTimeControlModel);

            controlledModel.StopTime += controlledModel.TimeStep;
            controlledModel.StopTime += controlledModel.TimeStep;

            var controlGroup = new ControlGroup();
            realTimeControlModel.ControlGroups.Add(controlGroup);

            var input = new Input();
            controlGroup.Inputs.Add(input);
            var output = new Output();
            controlGroup.Outputs.Add(output);

            var inputDataItem1 = controlledModel.GetChildDataItems(controlledModel.InputFeatures.First()).First();
            inputDataItem1.LinkTo(realTimeControlModel.GetDataItemByValue(controlGroup.Outputs[0]));

            var outputDataItem1 = controlledModel.GetChildDataItems(controlledModel.OutputFeatures.First()).First();
            outputDataItem1.LinkTo(realTimeControlModel.GetDataItemByValue(controlGroup.Inputs[0]));

            new TestCompositeActivity { Activities = { realTimeControlModel, controlledModel } };

            var timeCondition1 = new TimeCondition { Name = "2454" };
            timeCondition1.TimeSeries[new DateTime(1900, 1, 1)] = true;
            timeCondition1.Extrapolation = ExtrapolationType.Constant;
            controlGroup.Conditions.Add(timeCondition1);

            var timeCondition2 = new TimeCondition { Name = "2455" };
            timeCondition2.TimeSeries[new DateTime(1900, 1, 1)] = true;
            timeCondition2.Extrapolation = ExtrapolationType.Constant;
            controlGroup.Conditions.Add(timeCondition2);

            var condition1 = new StandardCondition
            {
                Name = "2454_1",
                Operation = Operation.Less,
                Value = 60,
                Input = input
            };
            controlGroup.Conditions.Add(condition1);

            var condition2 = new StandardCondition
            {
                Name = "2455_1",
                Operation = Operation.Greater,
                Value = 70,
                Input = input
            };
            controlGroup.Conditions.Add(condition2);

            var hydraulicRule1 = new HydraulicRule { Name = "HydraulicRule1" };
            hydraulicRule1.Inputs.Add(input);
            hydraulicRule1.Outputs.Add(output);
            hydraulicRule1.Extrapolation = ExtrapolationType.Constant;
            hydraulicRule1.Interpolation = InterpolationType.Constant;
            hydraulicRule1.Function[-100.0] = 1.00;
            hydraulicRule1.Function[100.0] = 1.00;
            controlGroup.Rules.Add(hydraulicRule1);

            var hydraulicRule2 = new HydraulicRule { Name = "HydraulicRule2" };
            hydraulicRule2.Inputs.Add(input);
            hydraulicRule2.Outputs.Add(output);
            hydraulicRule2.Extrapolation = ExtrapolationType.Constant;
            hydraulicRule2.Interpolation = InterpolationType.Constant;
            hydraulicRule2.Function[-100.0] = 2.00;
            hydraulicRule2.Function[100.0] = 2.00;
            controlGroup.Rules.Add(hydraulicRule2);

            timeCondition1.TrueOutputs.Add(condition1);
            condition1.TrueOutputs.Add(hydraulicRule1);

            timeCondition1.FalseOutputs.Add(timeCondition2);
            condition1.FalseOutputs.Add(timeCondition2);

            timeCondition2.TrueOutputs.Add(condition2);
            condition2.TrueOutputs.Add(hydraulicRule2);

            // Initialize
            realTimeControlModel.Initialize();
            controlledModel.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            // Run the models
            var timeStepsCount = 0;
            var inputValues = new[] { 80.0, 80.0, 80.0, 80.0, 65.0, 50.0, 50.0, 50.0 };
            var resultValues = new List<double>();
            while (realTimeControlModel.Status != ActivityStatus.Done)
            {
                controlGroup.Outputs[0].Value = -999.0; // Reset to check when value is actually set

                controlGroup.Inputs[0].Value = inputValues[timeStepsCount];

                Assert.AreEqual(new DateTime(2000, 1, 1, 0 + timeStepsCount, 0, 0), realTimeControlModel.CurrentTime);

                realTimeControlModel.Execute();
                controlledModel.Execute();

                resultValues.Add(controlGroup.Outputs[0].Value);

                timeStepsCount++;
            }
            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();

            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, controlledModel.Status);
            Assert.AreEqual(8, timeStepsCount);
            Assert.AreEqual(new[] { 2.0, 2.0, 2.0, 2.0, -999.0, 1.0, 1.0, 1.0 }, resultValues.ToArray());
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void ExecuteARelativeTimeRuleAndWithANotActivePIDRule()
        {
            ControlledTestModel controlledModel;
            RealTimeControlModel realTimeControlModel;

            RealTimeControlTestHelper.SetupControlledTestModel(out controlledModel, out realTimeControlModel);

            new TestCompositeActivity { Activities = { realTimeControlModel, controlledModel } };

            var controlGroup = new ControlGroup();
            realTimeControlModel.ControlGroups.Add(controlGroup);

            var input = new Input();
            controlGroup.Inputs.Add(input);
            var output = new Output();
            controlGroup.Outputs.Add(output);

            var inputDataItem1 = controlledModel.GetChildDataItems(controlledModel.InputFeatures.First()).First();
            inputDataItem1.LinkTo(realTimeControlModel.GetDataItemByValue(controlGroup.Outputs[0]));

            var outputDataItem1 = controlledModel.GetChildDataItems(controlledModel.OutputFeatures.First()).First();
            outputDataItem1.LinkTo(realTimeControlModel.GetDataItemByValue(controlGroup.Inputs[0]));

            var timeCondition = new TimeCondition { Name = "TimeConditionAlwaysOn" };
            timeCondition.TimeSeries[new DateTime(1900, 1, 1)] = true;
            timeCondition.TimeSeries[new DateTime(2200, 1, 1)] = false;
            timeCondition.Extrapolation = ExtrapolationType.Constant;
            controlGroup.Conditions.Add(timeCondition);

            var relativeTimeRule = new RelativeTimeRule { Name = "RelativeTimeRule" };
            relativeTimeRule.Outputs.Add(output);
            relativeTimeRule.FromValue = false;
            relativeTimeRule.Function[0.0] = 2.0;
            relativeTimeRule.Function[3600.0] = 3.00;
            relativeTimeRule.Function[7200.0] = 4.00;
            relativeTimeRule.Function[10800.0] = 5.00;
            controlGroup.Rules.Add(relativeTimeRule);

            var pidRuleNotActive = new PIDRule { Name = "PIDRuleNotActive" };
            pidRuleNotActive.Inputs.Add(input);
            pidRuleNotActive.Outputs.Add(output);
            pidRuleNotActive.Kp = 0.3;
            pidRuleNotActive.Ki = 0.3;
            pidRuleNotActive.Kd = 0.3;
            pidRuleNotActive.PidRuleSetpointType = PIDRule.PIDRuleSetpointType.Constant;
            pidRuleNotActive.ConstantValue = 1.0;
            controlGroup.Rules.Add(pidRuleNotActive);

            timeCondition.TrueOutputs.Add(relativeTimeRule);
            timeCondition.FalseOutputs.Add(pidRuleNotActive);

            // Initialize
            controlledModel.Initialize();
            realTimeControlModel.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);

            // Run the models
            var timeStepsCount = 0;
            var inputValuesForNotActivePID = new[] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
            var resultValues = new List<double>();

            while (realTimeControlModel.Status != ActivityStatus.Done)
            {
                controlGroup.Outputs[0].Value = -999.0; // Reset to check when value is actually set

                controlGroup.Inputs[0].Value = inputValuesForNotActivePID[timeStepsCount];

                Assert.AreEqual(new DateTime(2000, 1, 1, 0 + timeStepsCount, 0, 0), realTimeControlModel.CurrentTime);

                controlledModel.Execute();
                realTimeControlModel.Execute();

                resultValues.Add(controlGroup.Outputs[0].Value);

                timeStepsCount++;
            }
            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();

            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, controlledModel.Status);
            Assert.AreEqual(6, timeStepsCount);
            Assert.AreEqual(new[] { 2.0, 3.0, 4.0, 5.0, 5.0, 5.0 }, resultValues.ToArray());
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void TimeSeriesIdsInXmlShouldBeUniqueTools9625()
        {
            ControlledTestModel controlledModel;
            RealTimeControlModel realTimeControlModel;

            RealTimeControlTestHelper.SetupControlTestmodelWithFourInputs(out controlledModel, out realTimeControlModel);
            realTimeControlModel.LimitMemory = false;

            new TestCompositeActivity { Activities = { realTimeControlModel, controlledModel } };

            var controlGroup = RealTimeControlTestHelper.CreateControlGroupWithDiverseRulesAndConditions();
            controlGroup.Name = "cg1";
            realTimeControlModel.ControlGroups.Add(controlGroup);

            var inputDataItem1 = controlledModel.GetChildDataItems(controlledModel.InputFeatures[0]).First();
            inputDataItem1.LinkTo(realTimeControlModel.GetDataItemByValue(controlGroup.Outputs[0]));

            var inputDataItem2 = controlledModel.GetChildDataItems(controlledModel.InputFeatures[1]).First();
            inputDataItem2.LinkTo(realTimeControlModel.GetDataItemByValue(controlGroup.Outputs[1]));

            var outputDataItem1 = controlledModel.GetChildDataItems(controlledModel.OutputFeatures[0]).First();
            outputDataItem1.LinkTo(realTimeControlModel.GetDataItemByValue(controlGroup.Inputs[0]));

            // Create the exact same controlgroup with the same names
            var controlGroup2 = RealTimeControlTestHelper.CreateControlGroupWithDiverseRulesAndConditions();
            controlGroup2.Name = "cg2";
            realTimeControlModel.ControlGroups.Add(controlGroup2);

            var inputDataItem3 = controlledModel.GetChildDataItems(controlledModel.InputFeatures[2]).First();
            inputDataItem3.LinkTo(realTimeControlModel.GetDataItemByValue(controlGroup2.Outputs[0]));

            var inputDataItem4 = controlledModel.GetChildDataItems(controlledModel.InputFeatures[3]).First();
            inputDataItem4.LinkTo(realTimeControlModel.GetDataItemByValue(controlGroup2.Outputs[1]));

            var outputDataItem2 = controlledModel.GetChildDataItems(controlledModel.OutputFeatures[1]).First();
            outputDataItem2.LinkTo(realTimeControlModel.GetDataItemByValue(controlGroup2.Inputs[0]));

            // Initialize
            realTimeControlModel.Initialize();

            try
            {
                Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            }
            finally
            {
                realTimeControlModel.Cleanup();
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void ExecuteTimeConditionWhereInterpolationIsNeeded()
        {
            ControlledTestModel controlledModel;
            RealTimeControlModel realTimeControlModel;
            RealTimeControlTestHelper.SetupControlledTestModel(out controlledModel, out realTimeControlModel);

            controlledModel.StopTime += controlledModel.TimeStep;
            controlledModel.StopTime += controlledModel.TimeStep;

            var timeCondition = new TimeCondition { Name = "timer" };
            timeCondition.TimeSeries[controlledModel.StartTime] = true;
            timeCondition.TimeSeries[controlledModel.StartTime + new TimeSpan(3 * controlledModel.TimeStep.Ticks)] = false;
            timeCondition.TimeSeries[controlledModel.StartTime + new TimeSpan(4 * controlledModel.TimeStep.Ticks)] = true;
            timeCondition.TimeSeries[controlledModel.StartTime + new TimeSpan(6 * controlledModel.TimeStep.Ticks)] = false;
            timeCondition.TimeSeries[controlledModel.StartTime + new TimeSpan(7 * controlledModel.TimeStep.Ticks)] = true;

            var controlGroup = RealTimeControlTestHelper.SetupInvertorRule(controlledModel, realTimeControlModel, timeCondition);

            // Initialize
            realTimeControlModel.Initialize();
            controlledModel.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            // Run the models
            var timeStepsCount = 0;
            var inputValues = new[] { 10.0, 1.0, 0.5, 0.0, -0.5, -1.0, -10.0, -100 };
            var resultValues = new List<double>();
            while (realTimeControlModel.Status != ActivityStatus.Done)
            {
                controlGroup.Outputs[0].Value = -999.0; // Reset to check when value is actually set

                controlledModel.GetChildDataItems(controlledModel.OutputFeatures[0]).First().Value = inputValues[timeStepsCount];

                Assert.AreEqual(new DateTime(2000, 1, 1, 0 + timeStepsCount, 0, 0), realTimeControlModel.CurrentTime);

                realTimeControlModel.Execute();
                controlledModel.Execute();

                resultValues.Add(controlGroup.Outputs[0].Value);

                timeStepsCount++;
            }
            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();

            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, controlledModel.Status);
            Assert.AreEqual(8, timeStepsCount);
            Assert.AreEqual(new[] { -10.0, -1.0, -999.0, 0.0, 0.5, -999.0, 10.0, 100.0 }, resultValues.ToArray());
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void ExecuteTimeConditionWithPeriodicExtrapolation()
        {
            ControlledTestModel controlledModel;
            RealTimeControlModel realTimeControlModel;
            RealTimeControlTestHelper.SetupControlledTestModel(out controlledModel, out realTimeControlModel);

            controlledModel.StopTime += controlledModel.TimeStep;
            controlledModel.StopTime += controlledModel.TimeStep;

            var timeCondition = new TimeCondition { Name = "timer" };
            timeCondition.TimeSeries[controlledModel.StartTime] = false;
            timeCondition.TimeSeries[controlledModel.StartTime + new TimeSpan(1 * controlledModel.TimeStep.Ticks)] = true;
            timeCondition.Extrapolation = ExtrapolationType.Periodic;

            var controlGroup = RealTimeControlTestHelper.SetupInvertorRule(controlledModel, realTimeControlModel, timeCondition);

            // Initialize
            realTimeControlModel.Initialize();
            controlledModel.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            // Run the models
            var timeStepsCount = 0;
            var inputValues = new[] { 10.0, 1.0, 0.5, 0.0, -0.5, -1.0, -10.0, -100 };
            var resultValues = new List<double>();
            while (realTimeControlModel.Status != ActivityStatus.Done)
            {
                controlGroup.Outputs[0].Value = -999.0; // Reset to check when value is actually set

                controlledModel.GetChildDataItems(controlledModel.OutputFeatures[0]).First().Value = inputValues[timeStepsCount];

                Assert.AreEqual(new DateTime(2000, 1, 1, 0 + timeStepsCount, 0, 0), realTimeControlModel.CurrentTime);

                realTimeControlModel.Execute();
                controlledModel.Execute();

                resultValues.Add(controlGroup.Outputs[0].Value);

                timeStepsCount++;
            }
            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();

            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, controlledModel.Status);
            Assert.AreEqual(8, timeStepsCount);
            Assert.AreEqual(new[] { -10.0, -999.0, -0.5, -999.0, 0.5, -999.0, 10.0, -999.0 }, resultValues.ToArray());
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void ExecuteTimeConditionWithOneTimeStep()
        {
            ControlledTestModel controlledModel;
            RealTimeControlModel realTimeControlModel;
            RealTimeControlTestHelper.SetupControlledTestModel(out controlledModel, out realTimeControlModel);

            controlledModel.StopTime += controlledModel.TimeStep;
            controlledModel.StopTime += controlledModel.TimeStep;

            var timeCondition = new TimeCondition { Name = "timer" };
            timeCondition.TimeSeries[new DateTime(1900, 1, 1)] = true;
            timeCondition.Extrapolation = ExtrapolationType.Constant;

            var controlGroup = RealTimeControlTestHelper.SetupInvertorRule(controlledModel, realTimeControlModel, timeCondition);

            // Initialize
            realTimeControlModel.Initialize();
            controlledModel.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            // Run the models
            var timeStepsCount = 0;
            var inputValues = new[] { 10.0, 1.0, 0.5, 0.0, -0.5, -1.0, -10.0, -100 };
            var resultValues = new List<double>();
            while (realTimeControlModel.Status != ActivityStatus.Done)
            {
                controlGroup.Outputs[0].Value = -999.0; // Reset to check when value is actually set

                controlledModel.GetChildDataItems(controlledModel.OutputFeatures[0]).First().Value = inputValues[timeStepsCount];

                Assert.AreEqual(new DateTime(2000, 1, 1, 0 + timeStepsCount, 0, 0), realTimeControlModel.CurrentTime);

                realTimeControlModel.Execute();
                controlledModel.Execute();

                resultValues.Add(controlGroup.Outputs[0].Value);

                timeStepsCount++;
            }
            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();

            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, controlledModel.Status);
            Assert.AreEqual(8, timeStepsCount);
            Assert.AreEqual(new[] { -10.0, -1.0, -0.5, 0.0, 0.5, 1.0, 10.0, 100.0 }, resultValues.ToArray());
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void ExecuteControlGroupFromNDBModelWithInterferenceTimeConditionProblem()
        {
            ControlledTestModel controlledModel;
            RealTimeControlModel realTimeControlModel;
            RealTimeControlTestHelper.SetupControlledTestModel(out controlledModel, out realTimeControlModel);

            var controlGroup1 = new ControlGroup();
            realTimeControlModel.ControlGroups.Add(controlGroup1);

            new TestCompositeActivity { Activities = { realTimeControlModel, controlledModel } };

            var input = new Input();
            var output = new Output();

            controlGroup1.Inputs.Add(input);
            controlGroup1.Outputs.Add(output);

            controlGroup1.Inputs[0].ParameterName = "Water level";
            controlGroup1.Inputs[0].Feature = new RtcTestFeature { Name = "location1" };

            controlGroup1.Outputs[0].ParameterName = "Crest level";
            controlGroup1.Outputs[0].Feature = new RtcTestFeature { Name = "weir" };

            var hydraulicRule1 = new HydraulicRule { Name = "HydraulicRule1" };
            hydraulicRule1.Inputs.Add(controlGroup1.Inputs.First());
            hydraulicRule1.Outputs.Add(controlGroup1.Outputs.First());
            hydraulicRule1.Extrapolation = ExtrapolationType.Constant;
            hydraulicRule1.Interpolation = InterpolationType.Constant;
            hydraulicRule1.Function[-100.0] = 1.0;
            hydraulicRule1.Function[100.0] = 1.0;
            hydraulicRule1.Inputs[0] = input;
            hydraulicRule1.Outputs[0] = output;

            controlGroup1.Rules.Add(hydraulicRule1);

            var hydraulicRule2 = new HydraulicRule { Name = "HydraulicRule2" };
            hydraulicRule2.Inputs.Add(controlGroup1.Inputs.First());
            hydraulicRule2.Outputs.Add(controlGroup1.Outputs.First());
            hydraulicRule2.Extrapolation = ExtrapolationType.Constant;
            hydraulicRule2.Interpolation = InterpolationType.Constant;
            hydraulicRule2.Function[-100.0] = 2.0;
            hydraulicRule2.Function[100.0] = 2.0;
            hydraulicRule2.Inputs[0] = input;
            hydraulicRule2.Outputs[0] = output;

            controlGroup1.Rules.Add(hydraulicRule2);

            var timeCondition1 = new TimeCondition { Name = "timer1" };
            timeCondition1.TimeSeries[new DateTime(1900, 1, 1)] = true;
            timeCondition1.Extrapolation = ExtrapolationType.Constant;
            controlGroup1.Conditions.Add(timeCondition1);

            var timeCondition2 = new TimeCondition { Name = "timer2" };
            timeCondition2.TimeSeries[new DateTime(1900, 1, 1)] = true;
            timeCondition2.Extrapolation = ExtrapolationType.Constant;
            controlGroup1.Conditions.Add(timeCondition2);

            var condition1 = new StandardCondition
            {
                Name = "condition1",
                Operation = Operation.Greater,
                Value = 0,
                Input = input
            };
            controlGroup1.Conditions.Add(condition1);

            var condition2 = new StandardCondition
            {
                Name = "condition2",
                Operation = Operation.Less,
                Value = 0,
                Input = input
            };
            controlGroup1.Conditions.Add(condition2);

            timeCondition1.TrueOutputs.Add(condition1);
            timeCondition1.FalseOutputs.Add(timeCondition2);

            condition1.TrueOutputs.Add(hydraulicRule1);
            condition1.FalseOutputs.Add(timeCondition2);

            timeCondition2.TrueOutputs.Add(condition2);
            condition2.TrueOutputs.Add(hydraulicRule2);

            // Duplicate controlgroup
            var controlGroup2 = (ControlGroup)controlGroup1.Clone();
            controlGroup2.Name += "_duplicate";
            realTimeControlModel.ControlGroups.Add(controlGroup2);

            // Initialize
            realTimeControlModel.Initialize();
            controlledModel.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            // Run the models
            LogHelper.ConfigureLogging(Level.Debug);
            var timeStepsCount = 0;
            var inputValues = new[] { 1.0, 1.0, -1.0, 1.0, -1.0, -1.0 };
            var resultValues = new List<double>();

            while (realTimeControlModel.Status != ActivityStatus.Done)
            {
                controlGroup1.Outputs[0].Value = -999.0; // Reset to check when value is actually set
                controlGroup1.Inputs[0].Value = inputValues[timeStepsCount];

                Assert.AreEqual(new DateTime(2000, 1, 1, 0 + timeStepsCount, 0, 0), realTimeControlModel.CurrentTime);

                realTimeControlModel.Execute();
                controlledModel.Execute();

                Assert.AreNotEqual(ActivityStatus.Failed, realTimeControlModel.Status);

                resultValues.Add(controlGroup1.Outputs[0].Value);

                timeStepsCount++;
            }

            LogHelper.ResetLogging();
            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();

            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, controlledModel.Status);
            Assert.AreEqual(6, timeStepsCount);
            Assert.That(resultValues, Is.EquivalentTo(new[] { 1.0, 1.0, 2.0, 1.0, 2.0, 2.0 }));
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void ExecuteTimeConditionAndRelativeTimeRuleFromTestbenchTest179() // Expectations checked by Thieu, Issue assigned to Dirk. Wait for response
        {
            var timeStep = new TimeSpan(0, 1, 0, 0);
            var startTime = new DateTime(2000, 1, 1, 0, 0, 0);
            var realTimeControlModel = new RealTimeControlModel();
            var controlledModel = new ControlledTestModel
            {
                StartTime = startTime,
                StopTime = new DateTime(2000, 1, 1, 12, 0, 0),
                TimeStep = timeStep
            };

            var controlGroup = new ControlGroup { Name = "Control group" };
            var output = new Output();

            controlGroup.Outputs.Add(output);
            realTimeControlModel.ControlGroups.Add(controlGroup);

            new TestCompositeActivity { Activities = { realTimeControlModel, controlledModel } };

            var intputDataItem1 = controlledModel.GetChildDataItems(controlledModel.InputFeatures[0]).First();
            intputDataItem1.Value = -999.0;
            intputDataItem1.LinkTo(realTimeControlModel.GetDataItemByValue(controlGroup.Outputs[0]));

            var relativeTimeRule = new RelativeTimeRule("relative time rule", false)
            {
                Interpolation = InterpolationType.Linear
            };
            relativeTimeRule.Function[0.0] = 40.0;
            relativeTimeRule.Function[new TimeSpan(2 * timeStep.Ticks).TotalSeconds] = 60.0;
            relativeTimeRule.Outputs.Add(output);
            controlGroup.Rules.Add(relativeTimeRule);

            // Add time condition
            var timeCondition = new TimeCondition { Extrapolation = ExtrapolationType.Periodic };
            timeCondition.TimeSeries[startTime] = false;
            timeCondition.TimeSeries[startTime.Add(new TimeSpan(3 * timeStep.Ticks))] = true;
            timeCondition.TimeSeries[startTime.Add(new TimeSpan(5 * timeStep.Ticks))] = false;
            timeCondition.TimeSeries[startTime.Add(new TimeSpan(6 * timeStep.Ticks))] = false;
            controlGroup.Conditions.Add(timeCondition);
            timeCondition.TrueOutputs.Add(relativeTimeRule);

            // The initial value of the output must be set in the state vector
            controlGroup.Outputs[0].Value = -999.0;

            // Initialize
            realTimeControlModel.Initialize();
            controlledModel.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            // Run the models
            var timeStepsCount = 0;
            var weirValues = new List<double>();

            while (realTimeControlModel.Status != ActivityStatus.Done)
            {
                Assert.AreEqual(new DateTime(2000, 1, 1, 0 + timeStepsCount, 0, 0), realTimeControlModel.CurrentTime);

                controlGroup.Outputs[0].Value = -999.0; //to be sure it will be set

                realTimeControlModel.Execute();
                controlledModel.Execute();

                timeStepsCount++;

                Assert.AreEqual(new DateTime(2000, 1, 1, 0 + timeStepsCount, 0, 0), realTimeControlModel.CurrentTime);

                weirValues.Add(controlGroup.Outputs[0].Value);
            }
            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();

            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, controlledModel.Status);
            Assert.AreEqual(12, timeStepsCount);

            Assert.IsTrue(RealTimeControlTestHelper.CompareArray(new[] { -999.0, -999.0, 40.0, 50.0, 50.0, 50.0, 50.0, 50.0, 50.0, 40.0, 50.0, 50.0 }, weirValues.ToArray(), 1.0e-6), "Arrays are not equal");
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void ExecuteTimeConditionAndCheckLinkedDataItemsWereUpdated()
        {
            ControlledTestModel controlledModel;
            RealTimeControlModel realTimeControlModel;
            RealTimeControlTestHelper.SetupControlledTestModel(out controlledModel, out realTimeControlModel);

            controlledModel.StopTime += controlledModel.TimeStep;
            controlledModel.StopTime += controlledModel.TimeStep;

            var timeCondition = new TimeCondition { Name = "timer" };
            timeCondition.TimeSeries[new DateTime(1900, 1, 1)] = true;
            timeCondition.Extrapolation = ExtrapolationType.Constant;
            var controlGroup = RealTimeControlTestHelper.SetupInvertorRule(controlledModel, realTimeControlModel, timeCondition);

            var timeStepsCount = 0;
            var inputValues = new[] { 10.0, 1.0, 0.5, 0.0, -0.5, -1.0, -10.0, -100 };
            var resultValuesOutput = new List<double>();
            var resultValuesDataItem = new List<double>();
            var resultValuesLinkedDataItem = new List<double>();

            var output = controlGroup.Outputs[0];
            var outputDataItem = realTimeControlModel.GetDataItemByValue(output);
            var dataItemLinkedToOutput = controlledModel.AllDataItems.First(di => di.LinkedTo == outputDataItem);

            // Initialize
            realTimeControlModel.Initialize();
            controlledModel.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            // Execute
            while (realTimeControlModel.Status != ActivityStatus.Done)
            {
                output.Value = -999.0; // Reset to check when value is actually set

                controlledModel.GetChildDataItems(controlledModel.OutputFeatures[0]).First().Value = inputValues[timeStepsCount];

                realTimeControlModel.Execute();
                controlledModel.Execute();

                resultValuesOutput.Add(output.Value);
                resultValuesDataItem.Add((double)outputDataItem.Value);
                resultValuesLinkedDataItem.Add((double)dataItemLinkedToOutput.Value);

                timeStepsCount++;
            }
            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();

            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, controlledModel.Status);

            var expected = new[] { -10.0, -1.0, -0.5, 0.0, 0.5, 1.0, 10.0, 100.0 };
            Assert.AreEqual(expected, resultValuesOutput.ToArray()); // Set by RTC
            Assert.AreEqual(expected, resultValuesDataItem.ToArray(), "data item"); // Pull based
            Assert.AreEqual(expected, resultValuesLinkedDataItem.ToArray(), "linked data item"); // Event based
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void ExecuteLookupSignal()
        {
            ControlledTestModel controlledModel;
            RealTimeControlModel realTimeControlModel;

            RealTimeControlTestHelper.SetupControlledTestModel(out controlledModel, out realTimeControlModel);

            var controlGroup = RealTimeControlTestHelper.CreateControlGroupWithLookupSignalAndPIDRule();

            realTimeControlModel = new RealTimeControlModel { ControlGroups = { controlGroup } };

            var outputDataItem = controlledModel.GetChildDataItems(controlledModel.OutputFeatures[0]).First();
            outputDataItem.Value = 2.0;
            realTimeControlModel.GetDataItemByValue(controlGroup.Inputs[0]).LinkTo(outputDataItem);

            var inputDataItem = controlledModel.GetChildDataItems(controlledModel.InputFeatures[0]).First();
            inputDataItem.Value = 2.0;
            realTimeControlModel.GetDataItemByValue(controlGroup.Outputs[0]).LinkTo(inputDataItem);

            // Initialize
            realTimeControlModel.Initialize();
            controlledModel.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            // Execute
            realTimeControlModel.Execute();
            controlledModel.Execute();
            realTimeControlModel.Execute();
            controlledModel.Execute();
        }

        #endregion

        #region Other
        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void RemoveOutputWhenControlledModelPropertyChangesAfterCancelledRun()
        {
            ControlledTestModel controlledModel;
            RealTimeControlModel realTimeControlModel;
            RealTimeControlTestHelper.SetupControlledTestModel(out controlledModel, out realTimeControlModel);
            RealTimeControlTestHelper.SetupHydraulicRuleControlGroup(controlledModel, realTimeControlModel, true);

            realTimeControlModel.LimitMemory = false;
            
            realTimeControlModel.DataItems.Add(new DataItem { Role = DataItemRole.Output });

            realTimeControlModel.Initialize();
            controlledModel.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            realTimeControlModel.Execute();
            controlledModel.Execute();

            var outputItemsCount = realTimeControlModel.DataItems.Count(di => (di.Role & DataItemRole.Output) == DataItemRole.Output);

            realTimeControlModel.Cancel();
            controlledModel.Cancel();

            Assert.AreEqual(outputItemsCount, realTimeControlModel.DataItems.Count(di => (di.Role & DataItemRole.Output) == DataItemRole.Output));

            // Change something in the controller model
            ((ICompositeActivity)realTimeControlModel.Owner).Activities.Remove(realTimeControlModel.ControlledModels.First());

            Assert.AreEqual(0, realTimeControlModel.DataItems.Count(di => (di.Role & DataItemRole.Output) == DataItemRole.Output));
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void RealTimeControlModelMarksOutputOutOfSyncWhenAFeatureIsDeletedFromTheSourceModel()
        {
            var controlledModel = new ControlledTestModel
            {
                StartTime = new DateTime(2000, 1, 1, 0, 0, 0),
                StopTime = new DateTime(2000, 1, 1, 6, 0, 0),
                TimeStep = new TimeSpan(0, 1, 0, 0),
                OutputFeatures = { new RtcTestFeature { Name = "output" } },
                InputFeatures = { new RtcTestFeature { Name = "input" } }
            };

            var realTimeControlModel = new RealTimeControlModel();

            var rule = RealTimeControlTestHelper.GetHydraulicRule();
            rule.Function[0.0d] = -1.0d;

            var controlGroup = RealTimeControlTestHelper.GetControlGroupForRule(rule);
            realTimeControlModel.ControlGroups.Add(controlGroup);

            // Hook the control group to some data items; input for rtc == output for controlled test model
            var outputDataItem1 = controlledModel.GetChildDataItems(controlledModel.OutputFeatures[0]).First();
            realTimeControlModel.GetDataItemByValue(controlGroup.Inputs[0]).LinkTo(outputDataItem1);

            var intputDataItem1 = controlledModel.GetChildDataItems(controlledModel.InputFeatures[0]).First();
            intputDataItem1.LinkTo(realTimeControlModel.GetDataItemByValue(controlGroup.Outputs[0]));

            new TestCompositeActivity { Activities = { realTimeControlModel, controlledModel } };

            var validationResult = realTimeControlModel.Validate();
            Assert.AreEqual(0, validationResult.ErrorCount);
            Assert.AreEqual(0, validationResult.WarningCount);
            Assert.AreEqual(0, validationResult.InfoCount);

            realTimeControlModel.Initialize();
            controlledModel.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            while (realTimeControlModel.Status != ActivityStatus.Done && realTimeControlModel.Status != ActivityStatus.Failed)
            {
                realTimeControlModel.Execute();
                controlledModel.Execute();
            }

            realTimeControlModel.Finish();
            realTimeControlModel.Cleanup();

            Assert.IsFalse(realTimeControlModel.OutputOutOfSync);
            Assert.AreEqual(1, realTimeControlModel.OutputFeatureCoverages.Count());

            controlledModel.OutputFeatures.Remove(controlledModel.OutputFeatures[0]);

            Assert.IsTrue(realTimeControlModel.OutputOutOfSync);
            Assert.AreEqual(1, realTimeControlModel.OutputFeatureCoverages.Count());
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void LastRTCTimeShouldNotBeNull()
        {
            ControlledTestModel controlledModel;
            RealTimeControlModel realTimeControlModel;

            RealTimeControlTestHelper.SetupControlledTestModel(out controlledModel, out realTimeControlModel);
            realTimeControlModel.LogLevel = 4;
            realTimeControlModel.FlushLogEveryStep = true;

            var controlGroup = RealTimeControlModelHelper.CreateGroupHydraulicRule(false);
            realTimeControlModel.ControlGroups.Add(controlGroup);

            new TestCompositeActivity { Activities = { realTimeControlModel, controlledModel } };

            var outputDataItem1 = controlledModel.GetChildDataItems(controlledModel.OutputFeatures[0]).First();
            realTimeControlModel.GetDataItemByValue(controlGroup.Inputs[0]).LinkTo(outputDataItem1);

            var intputDataItem1 = controlledModel.GetChildDataItems(controlledModel.InputFeatures[0]).First();
            intputDataItem1.LinkTo(realTimeControlModel.GetDataItemByValue(controlGroup.Outputs[0]));

            var hydraulicRule = (HydraulicRule)controlGroup.Rules[0];
            hydraulicRule.Function[-1.0] = 1.0;
            hydraulicRule.Function[1.0] = -1.0;
            hydraulicRule.Interpolation = InterpolationType.Linear;
            hydraulicRule.Extrapolation = ExtrapolationType.Linear;

            realTimeControlModel.Initialize();
            controlledModel.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            // Run the models
            var timeStepsCount = 0;
            var inputValues = new[] { 11.0, 12.0, 13.0, 14.0, 15.0, 16.0 };
            var resultValues = new List<double>();
            while (realTimeControlModel.Status != ActivityStatus.Done)
            {
                controlGroup.Outputs[0].Value = -999.0; // Reset to check when value is actually set

                controlledModel.GetChildDataItems(controlledModel.OutputFeatures[0]).First().Value = inputValues[timeStepsCount];

                Assert.AreEqual(new DateTime(2000, 1, 1, 0 + timeStepsCount, 0, 0), realTimeControlModel.CurrentTime);

                realTimeControlModel.Execute();
                controlledModel.Execute();

                resultValues.Add(controlGroup.Outputs[0].Value);

                timeStepsCount++;
            }
            realTimeControlModel.Cleanup();
            controlledModel.Cleanup();

            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Cleaned, controlledModel.Status);
            Assert.AreEqual(6, timeStepsCount);
            Assert.AreEqual(new[] { -11.0, -12.0, -13.0, -14.0, -15.0, -16.0 }, resultValues.ToArray());
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void RealTimeControlModelRunShouldProduceStateExportFile()
        {
            var rtcRunDir = Path.Combine(Environment.CurrentDirectory, "RtcRunForCheckingStateExportFile");
            FileUtils.DeleteIfExists(rtcRunDir);
            Directory.CreateDirectory(rtcRunDir);
            Assert.IsTrue(Directory.Exists(rtcRunDir));

            ControlledTestModel controlledModel;
            RealTimeControlModel realTimeControlModel;
            RealTimeControlTestHelper.SetupControlledTestModel(out controlledModel, out realTimeControlModel);
            RealTimeControlTestHelper.SetupHydraulicRuleControlGroup(controlledModel, realTimeControlModel, true);
            realTimeControlModel.ExplicitWorkingDirectory = rtcRunDir;

            // Run the models
            realTimeControlModel.Initialize();
            controlledModel.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, realTimeControlModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            while (realTimeControlModel.Status != ActivityStatus.Done)
            {
                realTimeControlModel.Execute();
                controlledModel.Execute();
            }

            realTimeControlModel.Finish();
            realTimeControlModel.Cleanup();

            Assert.AreEqual(ActivityStatus.Cleaned, realTimeControlModel.Status);

            var stateExportFile = Path.Combine(rtcRunDir, RealTimeControlXMLFiles.XmlExportState);
            Assert.IsTrue(File.Exists(stateExportFile));
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void RealTimeControlModelCanProduceStateFiles()
        {
            var rtcRunDir = Path.Combine(Environment.CurrentDirectory, "realTimeControlModelCanProduceStateFile");
            FileUtils.DeleteIfExists(rtcRunDir);
            Directory.CreateDirectory(rtcRunDir);
            Assert.IsTrue(Directory.Exists(rtcRunDir));

            ControlledTestModel controlledModel;
            RealTimeControlModel realTimeControlModel;

            RealTimeControlTestHelper.SetupControlledTestModel(out controlledModel, out realTimeControlModel);
            RealTimeControlTestHelper.SetupHydraulicRuleControlGroup(controlledModel, realTimeControlModel, true);
            realTimeControlModel.ExplicitWorkingDirectory = rtcRunDir;

            // Setup model to generate statefiles            
            realTimeControlModel.WriteRestart = true;
            realTimeControlModel.SaveStateStartTime = realTimeControlModel.StartTime;
            realTimeControlModel.SaveStateStopTime = realTimeControlModel.StopTime;
            realTimeControlModel.SaveStateTimeStep = realTimeControlModel.TimeStep;

            // Run the models
            ActivityRunner.RunActivity(controlledModel);
            ActivityRunner.RunActivity(realTimeControlModel);

            Assert.AreEqual(6, realTimeControlModel.GetRestartOutputStates().Count());
        }

        #endregion

    }
}