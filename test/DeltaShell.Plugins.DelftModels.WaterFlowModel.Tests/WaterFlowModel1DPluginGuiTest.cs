using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls.Swf;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Actions;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;
using Rhino.Mocks;
using Is = Rhino.Mocks.Constraints.Is;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests
{
    [TestFixture]
    public class WaterFlowModel1DPluginGuiTest
    {
        private WaterFlowModel1DGuiPlugin guiPlugin;

        [SetUp]
        public void SetUp()
        {
            guiPlugin = new WaterFlowModel1DGuiPlugin();
        }

        [Test]
        public void ContextMenuGenerateSerieForBoundaryData()
        {
            var dataItemInput = new DataItem() { Role = DataItemRole.Input};
            var dataItemOutput = new DataItem() { Role = DataItemRole.Output};
            var dataItemLinked = new DataItem() { Role = DataItemRole.Input };
            dataItemLinked.LinkTo(dataItemInput);

            var boundaryFlowSerie = new WaterFlowModel1DBoundaryNodeData(){DataType = WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries};
            var boundaryFlowConstant = new WaterFlowModel1DBoundaryNodeData() { DataType = WaterFlowModel1DBoundaryNodeDataType.FlowConstant };
            var boundaryLevelSerie = new WaterFlowModel1DBoundaryNodeData(){DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries};
            var boundaryLevelConstant = new WaterFlowModel1DBoundaryNodeData() { DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant };
            var function = new Function();

            //menu
            Assert.AreNotEqual(null ,guiPlugin.GetContextMenu(new TreeNode(){ Tag = dataItemInput}, boundaryFlowSerie));
            
            //no menu
            Assert.AreEqual(null, guiPlugin.GetContextMenu(new TreeNode(){ Tag = dataItemInput}, boundaryFlowConstant));
            Assert.AreEqual(null, guiPlugin.GetContextMenu(new TreeNode(){ Tag = dataItemOutput}, boundaryFlowSerie));
            Assert.AreEqual(null, guiPlugin.GetContextMenu(new TreeNode() { Tag = dataItemLinked }, boundaryFlowSerie));
            
            //menu
            Assert.AreNotEqual(null, guiPlugin.GetContextMenu(new TreeNode(){ Tag = dataItemInput}, boundaryLevelSerie));
            
            //no menu
            Assert.AreEqual(null, guiPlugin.GetContextMenu(new TreeNode(){ Tag = dataItemInput}, boundaryLevelConstant));
            Assert.AreEqual(null, guiPlugin.GetContextMenu(new TreeNode(){ Tag = dataItemOutput}, boundaryLevelSerie));
            Assert.AreEqual(null, guiPlugin.GetContextMenu(new TreeNode() { Tag = dataItemLinked }, boundaryLevelSerie));

            Assert.AreEqual(null, guiPlugin.GetContextMenu(new TreeNode(){ Tag = dataItemInput}, function));

        }

        [Test]
        public void ContextMenuGenerateSerieForLateralData()
        {
            var dataItemInput = new DataItem() { Role = DataItemRole.Input };
            var dataItemOutput = new DataItem() { Role = DataItemRole.Output };
            var dataItemLinked = new DataItem() { Role = DataItemRole.Input };
            dataItemLinked.LinkTo(dataItemInput);

            var lateralFlowSerie = new WaterFlowModel1DLateralSourceData() { DataType = WaterFlowModel1DLateralDataType.FlowTimeSeries };
            var lateralFlowConstant = new WaterFlowModel1DLateralSourceData() { DataType = WaterFlowModel1DLateralDataType.FlowConstant };
            var function = new Function();

            //menu
            Assert.AreNotEqual(null, guiPlugin.GetContextMenu(new TreeNode(){ Tag = dataItemInput}, lateralFlowSerie));

            //no menu
            Assert.AreEqual(null, guiPlugin.GetContextMenu(new TreeNode(){ Tag = dataItemInput}, lateralFlowConstant));
            Assert.AreEqual(null, guiPlugin.GetContextMenu(new TreeNode(){ Tag = dataItemOutput}, lateralFlowSerie));
            Assert.AreEqual(null, guiPlugin.GetContextMenu(new TreeNode() { Tag = dataItemLinked }, lateralFlowSerie));

            //no menu
            Assert.AreEqual(null, guiPlugin.GetContextMenu(new TreeNode() { Tag = dataItemInput }, function));
        }


        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        [NUnit.Framework.Category(TestCategory.WorkInProgress)]//for some reason doesn't work on buildserver
        public void WaterFlowModel1DPluginGuiShowADialogWhenMergeActionIsStartedForBranchesOnWhichDataIsDefined()
        {
            //global idea is to let the plugin Gui show a dialog when branches are being merged en there is data on these branches.
            //the user should get a dialog saying the data will be lost
            //sheer size of this test is an indication that too many 'players' are involved. Should discuss how we should implement this 
            //kind of functionality. don't know where to put this functionality...not in NetworkHelper (does not know about coverages and GUI) not in NetworkCoverage (does not know about GUI)
            guiPlugin = new WaterFlowModel1DGuiPlugin();
            var mocks = new MockRepository();
            var application = mocks.Stub<IApplication>();
            //create a project 
            var project = new Project();
            //network ..
            var fakeNetwork = mocks.StrictMultiMock<INotifyPropertyChange>(typeof (IHydroNetwork));
            var fakeCoverage = mocks.Stub<INetworkCoverage>();
            var noSayingDialog = mocks.Stub<IMessageBox>();

            application.Expect(a => a.Project).Return(project).Repeat.Any();
            //subscribtion occurs project opened / closed
            application.ProjectOpened += null;
            LastCall.Constraints(Is.NotNull()).Repeat.Any();
            application.ProjectClosing += null;
            LastCall.Constraints(Is.NotNull()).Repeat.Any();

            //dataitem will unsubcscribe & subscribe!?
            fakeNetwork.PropertyChanged += null;
            LastCall.Constraints(Is.NotNull()).Repeat.Any();
            fakeNetwork.PropertyChanged -= null;
            LastCall.Constraints(Is.NotNull()).Repeat.Any();
            fakeNetwork.PropertyChanging += null;
            LastCall.Constraints(Is.NotNull()).Repeat.Any();
            fakeNetwork.PropertyChanging -= null;
            LastCall.Constraints(Is.NotNull()).Repeat.Any();

            ((IHydroNetwork) fakeNetwork).Expect(c => c.Name).PropertyBehavior();
            ((IHydroNetwork) fakeNetwork).Expect(c => c.IsEditing).Return(true).Repeat.Any();
            ((IHydroNetwork) fakeNetwork).Expect(c => c.CurrentEditAction).Return(new BranchMergeAction()).Repeat.Any();
            ((IHydroNetwork) fakeNetwork).Expect(c => c.GetDirectChildren()).Return(Enumerable.Empty<object>()).
                Repeat.Any();

            //IMPORTANT check..the canceledit method should be called!
            ((IHydroNetwork) fakeNetwork).Expect(c => c.CancelEdit());


            //the fake coverage has data on any branch :)
            fakeCoverage.Name = "Some initial data";
            fakeCoverage.CollectionChanged += null;
            LastCall.Constraints(Is.NotNull()).Repeat.Any();
            fakeCoverage.CollectionChanged -= null;
            LastCall.Constraints(Is.NotNull()).Repeat.Any();
            fakeCoverage.CollectionChanging += null;
            LastCall.Constraints(Is.NotNull()).Repeat.Any();
            fakeCoverage.CollectionChanging -= null;
            LastCall.Constraints(Is.NotNull()).Repeat.Any();
            fakeCoverage.Expect(c => c.GetLocationsForBranch(null)).IgnoreArguments().Return(new[]
                                                                                                 {
                                                                                                     new NetworkLocation
                                                                                                         ()
                                                                                                 });
            //the dialog/user 
            noSayingDialog.Expect(d => d.Show(
                "If you merge these branches the following data will be removed for these branches:\n\nSome initial data\n\nDo you want to continue merging the branches?",
                "Merge conflict", MessageBoxButtons.YesNo)).Return(DialogResult.No);

            mocks.ReplayAll();
            guiPlugin.Activate();
            //fake a project opened so the wfm1d can register
            application.Raise(a => a.ProjectOpened += null, application, null);

            //time to add something...to project
            project.RootFolder.Add(fakeNetwork);
            project.RootFolder.Add(fakeCoverage);

            //setup the dialog to return a no answer
            DelftTools.Controls.Swf.MessageBox.CustomMessageBox = noSayingDialog;
            //action! fake a network starting the branch merge action
            fakeNetwork.Raise(a => a.PropertyChanged += null, fakeNetwork, new PropertyChangedEventArgs("IsEditing"));

            mocks.VerifyAll();

            //reset the dialog
            DelftTools.Controls.Swf.MessageBox.CustomMessageBox = null;

        }
    }
}
