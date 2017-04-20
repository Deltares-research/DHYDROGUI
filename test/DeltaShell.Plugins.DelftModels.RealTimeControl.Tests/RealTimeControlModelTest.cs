using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Core.Workflow.DataItems.ValueConverters;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Units.Generics;
using DelftTools.Utils.Reflection;
using DeltaShell.Gui;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using DeltaShell.Plugins.ProjectExplorer;
using NUnit.Framework;
using SharpTestsEx;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests
{
    [TestFixture]
    public class RealTimeControlModelTest
    {
        # region Syncing controlled models, control group items, model settings, etc.

        [Test]
        public void TestControlledModelsAreAddedAutomaticallyAfterOwnerIsSet()
        {
            var compositeActivity = new TestCompositeActivity();
            var realTimeControlModel = new RealTimeControlModel();

            Assert.AreEqual(0, realTimeControlModel.ControlledModels.Count());

            compositeActivity.Activities.Add(realTimeControlModel);

            Assert.AreEqual(0, realTimeControlModel.ControlledModels.Count());

            compositeActivity.Activities.Add(new ControlledTestModel());

            Assert.AreEqual(1, realTimeControlModel.ControlledModels.Count());
        }

        [Test]
        public void TestControlledModelsAreAddedAutomaticallyIfOwnerIsSet()
        {
            var compositeActivity = new TestCompositeActivity();
            var realTimeControlModel = new RealTimeControlModel();
            
            compositeActivity.Activities.Add(new ControlledTestModel());
            compositeActivity.Activities.Add(realTimeControlModel);
            
            Assert.AreEqual(1, realTimeControlModel.ControlledModels.Count());
        }

        [Test]
        public void TestControlledModelsAreRemovedAutomaticallyAfterOwnerIsUnset()
        {
            var compositeActivity = new TestCompositeActivity();
            var realTimeControlModel = new RealTimeControlModel();
            var controlledTestModel = new ControlledTestModel();

            compositeActivity.Activities.Add(controlledTestModel);
            compositeActivity.Activities.Add(realTimeControlModel);
            compositeActivity.Activities.Remove(controlledTestModel);

            Assert.AreEqual(0, realTimeControlModel.ControlledModels.Count());
        }

        [Test]
        public void TestControlledModelsAreRemovedAutomaticallyIfOwnerIsUnset()
        {
            var compositeActivity = new TestCompositeActivity();
            var realTimeControlModel = new RealTimeControlModel();
            
            compositeActivity.Activities.Add(new ControlledTestModel());
            compositeActivity.Activities.Add(realTimeControlModel);
            compositeActivity.Activities.Remove(realTimeControlModel);

            Assert.AreEqual(0, realTimeControlModel.ControlledModels.Count());
        }

        [Test]
        public void RemoveLinkedInputFromControlGroup()
        {
            var input = new Input();
            var controlGroup = new ControlGroup { Inputs = { input }, Outputs = { new Output() } };
            var realTimeControlModel = new RealTimeControlModel { ControlGroups = { controlGroup } };
            var outputParameter = new Parameter<double> { Name = "p1", Value = 1.0 };
            var outputDataItem = new DataItem(outputParameter)
                                 {
                                     ValueType = typeof (double),
                                     ValueConverter = new PropertyValueConverter(outputParameter, "Value")
                                 };

            var dataItemCount = realTimeControlModel.AllDataItems.Count();

            realTimeControlModel.GetDataItemByValue(input).LinkTo(outputDataItem);
             
            controlGroup.Inputs.Clear();

            Assert.AreEqual(0, controlGroup.Inputs.Count);
            Assert.AreEqual(dataItemCount - 1, realTimeControlModel.AllDataItems.Count());
        }

        [Test]
        public void RemoveControlGroupWithLinkedInputItem()
        {
            var input = new Input();
            var controlGroup = new ControlGroup { Inputs = { input }, Outputs = { new Output() } };
            var realTimeControlModel = new RealTimeControlModel { ControlGroups = { controlGroup } };
            var outputParameter = new Parameter<double> { Name = "p1", Value = 1.0 };
            var outputDataItem = new DataItem(outputParameter)
                                 {
                                     ValueType = typeof (double),
                                     ValueConverter = new PropertyValueConverter(outputParameter, "Value")
                                 };

            realTimeControlModel.GetDataItemByValue(input).LinkTo(outputDataItem);

            var dataItemCount = realTimeControlModel.AllDataItems.Count();

            realTimeControlModel.ControlGroups.Clear();

            Assert.AreEqual(0, realTimeControlModel.ControlGroups.Count);
            Assert.AreEqual(dataItemCount - 3, realTimeControlModel.AllDataItems.Count());
        }

        [Test]
        public void RemoveControlGroupRemovesDataItem()
        {
            var input = new Input();
            var controlGroup = new ControlGroup { Inputs = { input }, Outputs = { new Output() } };
            var realTimeControlModel = new RealTimeControlModel { ControlGroups = { controlGroup } };
            var outputParameter = new Parameter<double> { Name = "p1", Value = 1.0 };
            var outputDataItem = new DataItem(outputParameter)
                                 {
                                     ValueType = typeof (double),
                                     ValueConverter = new PropertyValueConverter(outputParameter, "Value")
                                 };

            realTimeControlModel.GetDataItemByValue(input).LinkTo(outputDataItem);

            var rootDataItemCount = realTimeControlModel.DataItems.Count;

            realTimeControlModel.ControlGroups.Remove(controlGroup);

            Assert.AreEqual(rootDataItemCount - 1, realTimeControlModel.DataItems.Count);
        }

        [Test]
        public void RealTimeModelInheritsTimersFromControlledModel()
        {            
            var testCompositeActivity = new TestCompositeActivity();
            var realTimeControlModel = new RealTimeControlModel();
            var controlledTestModel = new ControlledTestModel
                                      {
                                          StartTime = new DateTime(2000, 1, 1, 0, 0, 0),
                                          StopTime = new DateTime(2000, 1, 1, 6, 0, 0),
                                          TimeStep = new TimeSpan(0, 1, 0, 0)
                                      };

            testCompositeActivity.Activities.Add(realTimeControlModel);
            testCompositeActivity.Activities.Add(controlledTestModel);

            Assert.AreEqual(controlledTestModel.StartTime, realTimeControlModel.StartTime);
            Assert.AreEqual(controlledTestModel.StopTime, realTimeControlModel.StopTime);
            Assert.AreEqual(controlledTestModel.TimeStep, realTimeControlModel.TimeStep);
        }

        [Test]
        public void FeatureIsPropagatedToInputAfterLinking()
        {
            // Create domain objects
            var weir = new Weir();

            var input = new Input();
            var realTimeControlModel = new RealTimeControlModel { ControlGroups = { new ControlGroup { Inputs = { input } } } };

            // Create/query data items
            var weirCrestLevelDataItem = new DataItem { ValueType = typeof(double), ValueConverter = new PropertyValueConverter(weir, "CrestLevel") };
            var inputRtcDataItem = realTimeControlModel.GetDataItemByValue(input);

            // Link
            inputRtcDataItem.LinkTo(weirCrestLevelDataItem);

            input.Feature.Should("feature from source data item value converter is set in input").Be.EqualTo(weir);
        }

        [Test]
        public void FeatureIsPropagatedToOutputAfterLinking()
        {
            // Create domain objects
            var weir = new Weir();

            var output = new Output();
            var realTimeControlModel = new RealTimeControlModel { ControlGroups = { new ControlGroup { Outputs = { output } } } };

            // Create/query data items
            var weirCrestLevelDataItem = new DataItem { ValueType = typeof(double), ValueConverter = new PropertyValueConverter(weir, "CrestLevel") };
            var outputRtcDataItem = realTimeControlModel.GetDataItemByValue(output);

            // Link
            outputRtcDataItem.LinkTo(weirCrestLevelDataItem);

            output.Feature.Should("feature from source data item value converter is set in output").Be.EqualTo(weir);
        }

        [Test]
        public void ClearFeatureInInputOnUnlink()
        {
            // Create domain objects
            var weir = new Weir();

            var input = new Input();
            var realTimeControlModel = new RealTimeControlModel { ControlGroups = { new ControlGroup { Inputs = { input } } } };

            // Create/query data items
            var weirCrestLevelDataItem = new DataItem { ValueType = typeof(double), ValueConverter = new PropertyValueConverter(weir, "CrestLevel") };
            var inputRtcDataItem = realTimeControlModel.GetDataItemByValue(input);

            // Link
            inputRtcDataItem.LinkTo(weirCrestLevelDataItem);

            inputRtcDataItem.Unlink();

            input.Feature.Should("feature is cleared in rtc Input after unlink").Be.Null();
        }

        [Test]
        public void ClearFeatureInOutputOnUnlink()
        {
            // Create domain objects
            var weir = new Weir();

            var output = new Output();
            var realTimeControlModel = new RealTimeControlModel { ControlGroups = { new ControlGroup { Outputs = { output } } } };

            // Create/query data items
            var weirCrestLevelDataItem = new DataItem { ValueType = typeof(double), ValueConverter = new PropertyValueConverter(weir, "CrestLevel") };
            var outputRtcDataItem = realTimeControlModel.GetDataItemByValue(output);

            // link
            outputRtcDataItem.LinkTo(weirCrestLevelDataItem);

            outputRtcDataItem.Unlink();

            output.Feature.Should("feature is cleared in rtc Output after unlink").Be.Null();
        }

        [Test]
        public void LinkingOutputShouldResultInIsConnected()
        {
            var realTimeControlModel = new RealTimeControlModel();
            var controlGroup = new ControlGroup { Outputs = { new Output() } };

            realTimeControlModel.ControlGroups.Add(controlGroup);

            var intputDataItem = new DataItem
                                 {
                                     Value = 12.0,
                                     ValueConverter = new FeaturePropertyValueConverter(new RtcTestFeature(), "Value")
                                 };

            intputDataItem.LinkTo(realTimeControlModel.GetDataItemByValue(controlGroup.Outputs[0]));

            Assert.IsTrue(realTimeControlModel.ControlGroups.First().Outputs.First().IsConnected);
        }

        [Test]
        public void LinkingInputShouldResultInIsConnected()
        {
            var realTimeControlModel = new RealTimeControlModel();
            var controlGroup = new ControlGroup { Inputs = { new Input() } };

            realTimeControlModel.ControlGroups.Add(controlGroup);

            var outputDataItem = new DataItem
                                 {
                                     Value = 12.0,
                                     ValueConverter = new FeaturePropertyValueConverter(new RtcTestFeature(), "Value")
                                 };

            realTimeControlModel.GetDataItemByValue(controlGroup.Inputs[0]).LinkTo(outputDataItem);

            Assert.IsTrue(controlGroup.Inputs.First().IsConnected);
        }

        [Test]
        public void ChildDataItemsAreCreatedForInputsAndOutputs()
        {
            var controlGroup = new ControlGroup { Outputs = { new Output() }, Inputs = { new Input() } };
            var realTimeControlModel = new RealTimeControlModel();

            realTimeControlModel.ControlGroups.Add(controlGroup);

            var controlGroupDataItem = realTimeControlModel.GetDataItemByValue(controlGroup);

            Assert.AreEqual(2, controlGroupDataItem.Children.Count);

            var inputDataItem = controlGroupDataItem.Children[0];
            Assert.AreEqual(controlGroup.Inputs[0], inputDataItem.ValueConverter.OriginalValue);
            Assert.AreEqual(controlGroupDataItem, inputDataItem.Parent);

            var outputDataItem = controlGroupDataItem.Children[1];
            Assert.AreEqual(controlGroup.Outputs[0], outputDataItem.ValueConverter.OriginalValue);
            Assert.AreEqual(controlGroupDataItem, outputDataItem.Parent);
        }

        # endregion

        # region Events handling (and refreshing)

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void TestPropertyChangedBubbling()
        {
            var controlGroup = RealTimeControlTestHelper.CreateGroup2Rules();
            var propertyChangedCount = 0;
            var propertyName = "";
            object sender = null;

            ((INotifyPropertyChanged) controlGroup).PropertyChanged += (s, e) =>
            {
                propertyChangedCount++;
                propertyName = e.PropertyName;
                sender = s;
            };

            controlGroup.Rules[0].Name = "Rumpelstiltskin";

            Assert.AreEqual(1, propertyChangedCount);
            Assert.AreEqual("Name", propertyName);
            Assert.AreEqual(controlGroup.Rules[0], sender);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Slow)]
        [NUnit.Framework.Category(TestCategory.WindowsForms)]
        public void RulePropertyChangedShouldRefreshTreeView()
        {
            using (var gui = new DeltaShellGui())
            {
                InitGui(gui);

                Action onShown = delegate
                {
                    var projectExplorer = gui.MainWindow.ProjectExplorer;
                    var realTimeControlModel = new RealTimeControlModel("Test RTC Model");
                    var controlGroup = RealTimeControlTestHelper.CreateGroup2Rules();

                    realTimeControlModel.ControlGroups.Clear();
                    realTimeControlModel.ControlGroups.Add(controlGroup);

                    gui.Application.Project.RootFolder.Add(realTimeControlModel);

                    var treeView = projectExplorer.TreeView;

                    treeView.Refresh();

                    projectExplorer.TreeView.WaitUntilAllEventsAreProcessed();

                    var nodeModel = treeView.GetNodeByTag(realTimeControlModel);

                    nodeModel.Expand(); // model

                    var nodeInput = nodeModel.Nodes[0];
                    nodeInput.Expand();

                    var nodeControlGroups = nodeInput.Nodes.First(n => n.Text == "Control Groups");
                    nodeControlGroups.Expand(); // controlGroups
                    nodeControlGroups.Nodes[0].Expand(); // controlGroup

                    var nodeCondition = nodeControlGroups.Nodes[0].GetNodeByTag(controlGroup.Conditions[0]);

                    Assert.AreNotEqual("condition1", nodeCondition.Text);
                    controlGroup.Conditions[0].Name = "condition1";

                    treeView.Refresh();
                    treeView.WaitUntilAllEventsAreProcessed();

                    Assert.AreEqual("condition1", nodeCondition.Text);
                };
                
                WpfTestHelper.ShowModal((Control)gui.MainWindow, onShown);    
            }
        }

        # endregion

        # region Other

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void TestResetOrphanedControlGroupInputsAndOutputs()
        {
            // SOBEK3-562: Existing projects can have ControlGroups with locations at inputs/outputs but no underlying dataitem links

            ControlledTestModel controlledModel;
            RealTimeControlModel realTimeControlModel;
            RealTimeControlTestHelper.SetupControlledTestModel(out controlledModel, out realTimeControlModel);
            RealTimeControlTestHelper.SetupHydraulicRuleControlGroup(controlledModel, realTimeControlModel, true);

            var controlGroup = realTimeControlModel.ControlGroups.FirstOrDefault();
            Assert.NotNull(controlGroup);

            foreach (var input in controlGroup.Inputs)
            {
                // Manually recreate control group inputs with features but no underlying dataitem links
                var feature = input.Feature;
                var parameter = input.ParameterName;
                var unit = input.UnitName;
                
                var inputDataItem = realTimeControlModel.GetDataItemByValue(input);
                inputDataItem.Unlink();
                
                input.Feature = feature;
                input.ParameterName = parameter;
                input.UnitName = unit;
            }

            foreach (var output in controlGroup.Outputs)
            {
                // Manually recreate control group outputs with features but no underlying dataitem links
                var feature = output.Feature;
                var parameter = output.ParameterName;
                var unit = output.UnitName;
                
                var outputDataItem = realTimeControlModel.GetDataItemByValue(output);
                var toUnlink = outputDataItem.LinkedBy.ToList();
                foreach (var dataItem in toUnlink)
                {
                    dataItem.Unlink();
                }

                output.Feature = feature;
                output.ParameterName = parameter;
                output.UnitName = unit;
            }

            // Call Method
            realTimeControlModel.ResetOrphanedControlGroupInputsAndOutputs(controlGroup);

            // Assert that inputs and outputs have been reset
            foreach (var input in controlGroup.Inputs)
            {
                Assert.AreEqual("[Not Set]", input.Name);
                Assert.IsNull(input.Feature);
            }
            foreach (var output in controlGroup.Outputs)
            {
                Assert.AreEqual("[Not Set]", output.Name);
                Assert.IsNull(output.Feature);
            }
        }
        
        [Test]
        public void TestGetMetaDataRequirementsIsImplementedForAllSupportedVersions()
        {
            var model = new RealTimeControlModel();
            var allSupportedVersions = TypeUtils.GetStaticField<int[]>(typeof(RealTimeControlModel), "SupportedMetaDataVersions");

            foreach (var version in allSupportedVersions)
            {
                Assert.DoesNotThrow(() => TypeUtils.CallPrivateMethod(model, "GetMetaDataRequirements", version));
            }
        }

        # endregion

        # region ControlledTestModel

        [Test]
        public void TestControlledTestModel()
        {
            var controlledModel = new ControlledTestModel
                                  {
                                      StartTime = new DateTime(2000, 1, 1, 0, 0, 0),
                                      StopTime = new DateTime(2000, 1, 1, 6, 0, 0),
                                      TimeStep = new TimeSpan(0, 1, 0, 0)
                                  };

            controlledModel.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            // Run model
            var timeStepsCount = 0;
            while (controlledModel.Status != ActivityStatus.Done)
            {
                Assert.AreEqual(new DateTime(2000, 1, 1, 0 + timeStepsCount, 0, 0), controlledModel.CurrentTime);

                controlledModel.Execute();
                timeStepsCount++;
            }

            Assert.AreEqual(ActivityStatus.Done, controlledModel.Status);
            Assert.AreEqual(6, timeStepsCount);
        }

        # endregion

        # region Helper functions

        private static void InitGui(IGui gui)
        {
            var app = gui.Application;

            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new RealTimeControlApplicationPlugin());
            gui.Plugins.Add(new CommonToolsGuiPlugin());
            gui.Plugins.Add(new ProjectExplorerGuiPlugin());
            gui.Plugins.Add(new RealTimeControlGuiPlugin());

            gui.Run();
        }

        # endregion
    }
}