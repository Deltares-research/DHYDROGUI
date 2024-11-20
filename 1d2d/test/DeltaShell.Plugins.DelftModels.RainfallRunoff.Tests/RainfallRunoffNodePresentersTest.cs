using System.Collections.Generic;
using DelftTools.Controls;
using DelftTools.Controls.Swf.TreeViewControls;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.NodePresenters;
using DeltaShell.Plugins.ProjectExplorer.NodePresenters;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests
{
    [TestFixture]
    public class RainfallRunoffNodePresentersTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void Show()
        {
            var mocks = new MockRepository();
            var gui = mocks.DynamicMock<IGui>();
            var application = mocks.DynamicMock<IApplication>();
            var activityRunner = mocks.DynamicMock<IActivityRunner>();
            Expect.Call(application.ActivityRunner).Return(activityRunner).Repeat.Once();
            Expect.Call(application.GetPluginForType(null)).IgnoreArguments().Return(null).Repeat.Any();
            Expect.Call(gui.Application).Return(application).Repeat.AtLeastOnce();
                
                
            mocks.ReplayAll();

            var guiPlugin = new RainfallRunoffGuiPlugin {Gui = gui};

            var model = new RainfallRunoffModel { Name = "model1" };
            model.Basin.Catchments.Add(new Catchment { Name = "Catchment001"});

            var list = new List<ITreeNodePresenter>
                {
                    new RainfallRunoffModelProjectNodePresenter(guiPlugin),
                    new TreeNodeModelDataWrapperNodePresenter(guiPlugin),
                    new DataItemNodePresenter(guiPlugin),
                    new DataItemSetNodePresenter(guiPlugin),
                    new TreeFolderNodePresenter(guiPlugin),
                    new CatchmentModelDataProjectNodePresenter(guiPlugin),
                    new MeteoDataProjectNodePresenter(guiPlugin)
                };

            var treeView = new TreeView();

            foreach (var treeNodePresenter in list)
            {
                treeView.NodePresenters.Add(treeNodePresenter);
            }

            treeView.Data = model;

            WindowsFormsTestHelper.ShowModal(treeView);

            mocks.VerifyAll();
        }
    }
}
