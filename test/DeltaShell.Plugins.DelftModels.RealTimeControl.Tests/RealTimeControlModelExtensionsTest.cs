using System;
using System.Linq;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests
{
    [TestFixture]
    public class RealTimeControlModelExtensionsTest
    {
        [Test]
        public void TestMakeControlGroupNamesUnique_UpdatesNamesAndGivesWarningWhenNamesAreNotAlreadyUnique()
        {
            //setup
            var rtcModel = new RealTimeControlModel("RTC-Model")
            {
                ControlGroups = new EventedList<ControlGroup>
                {
                    new ControlGroup() { Name = "ControlGroup1" },
                    new ControlGroup() { Name = "ControlGroup1" },
                    new ControlGroup() { Name = "ControlGroup1" },
                    new ControlGroup() { Name = "ControlGroup4" },
                    new ControlGroup() { Name = "ControlGroup4" },
                    new ControlGroup() { Name = "ControlGroup4" }
                }
            };

            // assert start state
            Assert.IsFalse(rtcModel.ControlGroups.Select(cg => cg.Name).HasUniqueValues());

            // assert results
            TestHelper.AssertAtLeastOneLogMessagesContains(() => { rtcModel.MakeControlGroupNamesUnique(); },
                string.Format(Resources.RealTimeControlModelExtensions_MakeControlGroupNamesUnique_ControlGroup_names_for_Model__0__were_not_unique__1_Control_Groups_have_been_renamed_such_that_they_are_now_unique_,
                    rtcModel.Name, Environment.NewLine));

            Assert.IsTrue(rtcModel.ControlGroups.Select(cg => cg.Name).HasUniqueValues());
        }

        [Test]
        public void TestMakeControlGroupNamesUnique_DoesNotUpdateNamesAndGivesNoWarningWhenNamesAreAlreadyUnique()
        {
            //setup
            var rtcModel = new RealTimeControlModel("RTC-Model");
            var controlGroup1 = new ControlGroup() { Name = "ControlGroup1" };
            var controlGroup2 = new ControlGroup() { Name = "ControlGroup2" };
            var controlGroup3 = new ControlGroup() { Name = "ControlGroup3" };
            var controlGroup4 = new ControlGroup() { Name = "ControlGroup4" };
            var controlGroup5 = new ControlGroup() { Name = "ControlGroup5" };
            var controlGroup6 = new ControlGroup() { Name = "ControlGroup6" };

            rtcModel.ControlGroups.Add(controlGroup1);
            rtcModel.ControlGroups.Add(controlGroup2);
            rtcModel.ControlGroups.Add(controlGroup3);
            rtcModel.ControlGroups.Add(controlGroup4);
            rtcModel.ControlGroups.Add(controlGroup5);
            rtcModel.ControlGroups.Add(controlGroup6);

            // assert start state
            Assert.IsTrue(rtcModel.ControlGroups.Select(cg => cg.Name).HasUniqueValues());

            // assert results
            Assert.Throws<AssertionException>(() =>
                TestHelper.AssertAtLeastOneLogMessagesContains(() => { rtcModel.MakeControlGroupNamesUnique(); },
                    string.Format(Resources.RealTimeControlModelExtensions_MakeControlGroupNamesUnique_ControlGroup_names_for_Model__0__were_not_unique__1_Control_Groups_have_been_renamed_such_that_they_are_now_unique_,
                    rtcModel.Name, Environment.NewLine)),
                "Warning message was logged where we did not expect it to be");

            Assert.AreEqual("ControlGroup1", controlGroup1.Name);
            Assert.AreEqual("ControlGroup2", controlGroup2.Name);
            Assert.AreEqual("ControlGroup3", controlGroup3.Name);
            Assert.AreEqual("ControlGroup4", controlGroup4.Name);
            Assert.AreEqual("ControlGroup5", controlGroup5.Name);
            Assert.AreEqual("ControlGroup6", controlGroup6.Name);
        }

        [Test]
        public void TestSyncControlGroupDataItemNames_ReturnsForNoControlGroups()
        {
            var rtcModel = new RealTimeControlModel();
            Assert.DoesNotThrow(() => rtcModel.SyncControlGroupDataItemNames());
        }

        [Test]
        public void TestSyncControlGroupDataItemNames_UpdatesChildDataItemNamesOfMultipleControlGroups()
        {
            // setup
            var rtcModel = new RealTimeControlModel();
            var controlGroup1 = new ControlGroup() { Name = "ControlGroup1" };
            controlGroup1.Inputs.Add(new Input());
            controlGroup1.Outputs.Add(new Output());

            var controlGroup2 = new ControlGroup() { Name = "ControlGroup2" };
            controlGroup2.Inputs.Add(new Input());
            controlGroup2.Outputs.Add(new Output());

            var controlGroup3 = new ControlGroup() { Name = "ControlGroup3" };
            controlGroup3.Inputs.Add(new Input());
            controlGroup3.Outputs.Add(new Output());

            rtcModel.ControlGroups.Add(controlGroup1);
            rtcModel.ControlGroups.Add(controlGroup2);
            rtcModel.ControlGroups.Add(controlGroup3);

            var controlGroup1DataItem = rtcModel.DataItems.FirstOrDefault(di => ReferenceEquals(di.Value, controlGroup1));
            Assert.NotNull(controlGroup1DataItem);
            Assert.AreEqual(2, controlGroup1DataItem.Children.Count);

            var controlGroup2DataItem = rtcModel.DataItems.FirstOrDefault(di => ReferenceEquals(di.Value, controlGroup2));
            Assert.NotNull(controlGroup2DataItem);
            Assert.AreEqual(2, controlGroup2DataItem.Children.Count);

            var controlGroup3DataItem = rtcModel.DataItems.FirstOrDefault(di => ReferenceEquals(di.Value, controlGroup3));
            Assert.NotNull(controlGroup3DataItem);
            Assert.AreEqual(2, controlGroup3DataItem.Children.Count);

            var controlGroup1InputDataItem = controlGroup1DataItem.Children.FirstOrDefault(di => (di.Role & DataItemRole.Input) == DataItemRole.Input);
            Assert.NotNull(controlGroup1InputDataItem);

            var controlGroup1OutputDataItem = controlGroup1DataItem.Children.FirstOrDefault(di => (di.Role & DataItemRole.Output) == DataItemRole.Output);
            Assert.NotNull(controlGroup1OutputDataItem);

            var controlGroup2InputDataItem = controlGroup2DataItem.Children.FirstOrDefault(di => (di.Role & DataItemRole.Input) == DataItemRole.Input);
            Assert.NotNull(controlGroup2InputDataItem);

            var controlGroup2OutputDataItem = controlGroup2DataItem.Children.FirstOrDefault(di => (di.Role & DataItemRole.Output) == DataItemRole.Output);
            Assert.NotNull(controlGroup2OutputDataItem);

            var controlGroup3InputDataItem = controlGroup3DataItem.Children.FirstOrDefault(di => (di.Role & DataItemRole.Input) == DataItemRole.Input);
            Assert.NotNull(controlGroup3InputDataItem);

            var controlGroup3OutputDataItem = controlGroup3DataItem.Children.FirstOrDefault(di => (di.Role & DataItemRole.Output) == DataItemRole.Output);
            Assert.NotNull(controlGroup3OutputDataItem);

            // simulate ChildDataItem names being 'out of sync' on ControlGroups 1 and 3
            controlGroup1InputDataItem.Name = controlGroup1InputDataItem.Name.Replace("ControlGroup1", "ControlGroup1_Renamed");
            controlGroup1OutputDataItem.Name = controlGroup1OutputDataItem.Name.Replace("ControlGroup1", "ControlGroup1_Renamed");
            controlGroup3InputDataItem.Name = controlGroup3InputDataItem.Name.Replace("ControlGroup3", "ControlGroup3_Renamed");
            controlGroup3OutputDataItem.Name = controlGroup3OutputDataItem.Name.Replace("ControlGroup3", "ControlGroup3_Renamed");

            // assert start state
            Assert.AreEqual("ControlGroup1", controlGroup1.Name);
            Assert.IsTrue(controlGroup1InputDataItem.Name.StartsWith(controlGroup1.Name + "_Renamed" + RealTimeControlModel.InputPostFix));
            Assert.IsTrue(controlGroup1OutputDataItem.Name.StartsWith(controlGroup1.Name + "_Renamed" + RealTimeControlModel.OutputPostFix));

            Assert.AreEqual("ControlGroup2", controlGroup2.Name);
            Assert.IsTrue(controlGroup2InputDataItem.Name.StartsWith(controlGroup2.Name + RealTimeControlModel.InputPostFix));
            Assert.IsTrue(controlGroup2OutputDataItem.Name.StartsWith(controlGroup2.Name + RealTimeControlModel.OutputPostFix));

            Assert.AreEqual("ControlGroup3", controlGroup3.Name);
            Assert.IsTrue(controlGroup3InputDataItem.Name.StartsWith(controlGroup3.Name + "_Renamed" + RealTimeControlModel.InputPostFix));
            Assert.IsTrue(controlGroup3OutputDataItem.Name.StartsWith(controlGroup3.Name + "_Renamed" + RealTimeControlModel.OutputPostFix));

            // synchronise the DataItemNames
            rtcModel.SyncControlGroupDataItemNames();

            // assert end state
            Assert.AreEqual("ControlGroup1", controlGroup1.Name);
            Assert.IsTrue(controlGroup1InputDataItem.Name.StartsWith(controlGroup1.Name + RealTimeControlModel.InputPostFix));
            Assert.IsTrue(controlGroup1OutputDataItem.Name.StartsWith(controlGroup1.Name + RealTimeControlModel.OutputPostFix));

            Assert.AreEqual("ControlGroup2", controlGroup2.Name);
            Assert.IsTrue(controlGroup2InputDataItem.Name.StartsWith(controlGroup2.Name + RealTimeControlModel.InputPostFix));
            Assert.IsTrue(controlGroup2OutputDataItem.Name.StartsWith(controlGroup2.Name + RealTimeControlModel.OutputPostFix));

            Assert.AreEqual("ControlGroup3", controlGroup3.Name);
            Assert.IsTrue(controlGroup3InputDataItem.Name.StartsWith(controlGroup3.Name + RealTimeControlModel.InputPostFix));
            Assert.IsTrue(controlGroup3OutputDataItem.Name.StartsWith(controlGroup3.Name + RealTimeControlModel.OutputPostFix));
        }

        [Test]
        public void TestSyncControlGroupChildDataItemNames_ReturnsForNoDataItem()
        {
            var rtcModel = new RealTimeControlModel();
            Assert.DoesNotThrow(() => rtcModel.SyncControlGroupChildDataItemNames(new ControlGroup()));
        }

        [Test]
        public void TestSyncControlGroupChildDataItemNames_ReturnsForNoChildDataItems()
        {
            var rtcModel = new RealTimeControlModel();
            var controlGroup = new ControlGroup() { Name = "ControlGroup" };
            rtcModel.ControlGroups.Add(controlGroup);
            Assert.DoesNotThrow(() => rtcModel.SyncControlGroupChildDataItemNames(controlGroup));
        }

        [Test]
        public void TestSyncControlGroupChildDataItemNames_SkipsInputsWithIncorrectPostFix()
        {
            // setup
            var rtcModel = new RealTimeControlModel();
            var controlGroup = new ControlGroup() { Name = "ControlGroup" };
            controlGroup.Inputs.Add(new Input());
            controlGroup.Inputs.Add(new Input());

            rtcModel.ControlGroups.Add(controlGroup);

            var controlGroupDataItem = rtcModel.DataItems.FirstOrDefault(di => ReferenceEquals(di.Value, controlGroup));
            Assert.NotNull(controlGroupDataItem);
            Assert.AreEqual(2, controlGroupDataItem.Children.Count);

            var controlGroupInput1DataItem = controlGroupDataItem.Children[0];
            Assert.NotNull(controlGroupInput1DataItem);

            var controlGroupInput2DataItem = controlGroupDataItem.Children[1];
            Assert.NotNull(controlGroupInput2DataItem);

            // simulate ChildDataItem names being 'out of sync'
            controlGroupInput1DataItem.Name = controlGroupInput1DataItem.Name.Replace("ControlGroup", "ControlGroup_Renamed");
            controlGroupInput1DataItem.Name = controlGroupInput1DataItem.Name.Replace(RealTimeControlModel.InputPostFix, ".somethingElse");
            controlGroupInput2DataItem.Name = controlGroupInput2DataItem.Name.Replace("ControlGroup", "ControlGroup_Renamed");
            
            // synchronise the ChildDataItemNames
            rtcModel.SyncControlGroupChildDataItemNames(controlGroup);

            // assert end state
            Assert.AreEqual("ControlGroup", controlGroup.Name);
            Assert.IsFalse(controlGroupInput1DataItem.Name.StartsWith(controlGroup.Name + RealTimeControlModel.InputPostFix));
            Assert.IsTrue(controlGroupInput2DataItem.Name.StartsWith(controlGroup.Name + RealTimeControlModel.InputPostFix));
        }

        [Test]
        public void TestSyncControlGroupChildDataItemNames_SkipsOutputsWithIncorrectPostFix()
        {
            // setup
            var rtcModel = new RealTimeControlModel();
            var controlGroup = new ControlGroup() { Name = "ControlGroup" };
            controlGroup.Outputs.Add(new Output());
            controlGroup.Outputs.Add(new Output());

            rtcModel.ControlGroups.Add(controlGroup);

            var controlGroupDataItem = rtcModel.DataItems.FirstOrDefault(di => ReferenceEquals(di.Value, controlGroup));
            Assert.NotNull(controlGroupDataItem);
            Assert.AreEqual(2, controlGroupDataItem.Children.Count);

            var controlGroupOutput1DataItem = controlGroupDataItem.Children[0];
            Assert.NotNull(controlGroupOutput1DataItem);

            var controlGroupOutput2DataItem = controlGroupDataItem.Children[1];
            Assert.NotNull(controlGroupOutput2DataItem);

            // simulate ChildDataItem names being 'out of sync'
            controlGroupOutput1DataItem.Name = controlGroupOutput1DataItem.Name.Replace("ControlGroup", "ControlGroup_Renamed");
            controlGroupOutput1DataItem.Name = controlGroupOutput1DataItem.Name.Replace(RealTimeControlModel.OutputPostFix, ".somethingElse");
            controlGroupOutput2DataItem.Name = controlGroupOutput2DataItem.Name.Replace("ControlGroup", "ControlGroup_Renamed");

            // synchronise the ChildDataItemNames
            rtcModel.SyncControlGroupChildDataItemNames(controlGroup);

            // assert end state
            Assert.AreEqual("ControlGroup", controlGroup.Name);
            Assert.IsFalse(controlGroupOutput1DataItem.Name.StartsWith(controlGroup.Name + RealTimeControlModel.OutputPostFix));
            Assert.IsTrue(controlGroupOutput2DataItem.Name.StartsWith(controlGroup.Name + RealTimeControlModel.OutputPostFix));
        }

        [Test]
        public void TestSyncControlGroupChildDataItemNames_UpdatesChildDataItemNamesOfSingleControlGroup()
        {
            // setup
            var rtcModel = new RealTimeControlModel();
            var controlGroup = new ControlGroup() { Name = "ControlGroup" };
            controlGroup.Inputs.Add(new Input());
            controlGroup.Outputs.Add(new Output());

            rtcModel.ControlGroups.Add(controlGroup);
            
            var controlGroupDataItem = rtcModel.DataItems.FirstOrDefault(di => ReferenceEquals(di.Value, controlGroup));
            Assert.NotNull(controlGroupDataItem);
            Assert.AreEqual(2, controlGroupDataItem.Children.Count);

            var controlGroupInputDataItem = controlGroupDataItem.Children.FirstOrDefault(di => (di.Role & DataItemRole.Input) == DataItemRole.Input);
            Assert.NotNull(controlGroupInputDataItem);

            var controlGroupOutputDataItem = controlGroupDataItem.Children.FirstOrDefault(di => (di.Role & DataItemRole.Output) == DataItemRole.Output);
            Assert.NotNull(controlGroupOutputDataItem);

            // simulate ChildDataItem names being 'out of sync'
            controlGroupInputDataItem.Name = controlGroupInputDataItem.Name.Replace("ControlGroup", "ControlGroup_Renamed");
            controlGroupOutputDataItem.Name = controlGroupOutputDataItem.Name.Replace("ControlGroup", "ControlGroup_Renamed");

            // assert start state
            Assert.AreEqual("ControlGroup", controlGroup.Name);
            Assert.IsTrue(controlGroupInputDataItem.Name.StartsWith(controlGroup.Name + "_Renamed" + RealTimeControlModel.InputPostFix));
            Assert.IsTrue(controlGroupOutputDataItem.Name.StartsWith(controlGroup.Name + "_Renamed" + RealTimeControlModel.OutputPostFix));

            // synchronise the ChildDataItemNames
            rtcModel.SyncControlGroupChildDataItemNames(controlGroup);

            // assert end state
            Assert.AreEqual("ControlGroup", controlGroup.Name);
            Assert.IsTrue(controlGroupInputDataItem.Name.StartsWith(controlGroup.Name + RealTimeControlModel.InputPostFix));
            Assert.IsTrue(controlGroupOutputDataItem.Name.StartsWith(controlGroup.Name + RealTimeControlModel.OutputPostFix));
        }

    }
}
