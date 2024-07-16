using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Services;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;

namespace DeltaShell.Plugins.NetworkEditor.Tests
{
    [TestFixture]
    public class NetworkEditorPluginTest
    {
        private readonly MockRepository mocks = new MockRepository();

        [Test]
        public void ChildDataItemIsAddedInProjectAfterSubRegionIsAdded()
        {
            var project = new Project();
            var parentRegion = new HydroRegion();
            project.RootFolder.Add(parentRegion);

            // setup mock app
            var dataItemService = mocks.Stub<IDataItemService>();
            dataItemService.Expect(s => s.GetDataItemByValue(project, parentRegion)).Return(project.RootFolder.DataItems.First());

            var app = mocks.Stub<IApplication>();
            app.DataItemService = dataItemService;
            app.Expect(a => a.Project).Repeat.Any().Return(project);

            var projectService = mocks.DynamicMock<IProjectService>();
            app.Stub(a => a.ProjectService).Return(projectService);

            projectService.ProjectClosing += null; LastCall.IgnoreArguments().Repeat.Any();
            projectService.ProjectOpened += null; LastCall.IgnoreArguments().Repeat.Any();
            
            mocks.ReplayAll();

            // create plugin
            var plugin = new NetworkEditorApplicationPlugin { Application = app };
            plugin.Activate();
            projectService.Raise(x => x.ProjectOpened += null, this, new EventArgs<Project>(project)); // makes sure plugin subscribes to project events

            // add sub region
            var subRegion = new HydroRegion();
            parentRegion.SubRegions.Add(subRegion);

            var parentRegionDataItem = (IDataItem)project.RootFolder.Items[0];

            parentRegionDataItem.Children.Count
                .Should("child data item is added after sub-region is added").Be.EqualTo(1);
        }

        [Test]
        public void ChildDataItemIsRemovedFromProjectAfterSubRegionIsRemoved()
        {
            var subRegion = new HydroRegion();
            var parentRegion = new HydroRegion { SubRegions = { subRegion } };

            var subRegionDataItem = new DataItem(subRegion);
            var parentRegionDataItem = new DataItem(parentRegion);
            parentRegionDataItem.Children.Add(subRegionDataItem);

            var project = new Project { RootFolder = { Items = { parentRegionDataItem } } };

            // setup mock app
            var dataItemService = mocks.Stub<IDataItemService>();
            dataItemService.Expect(s => s.GetDataItemByValue(project, parentRegion)).Return(project.RootFolder.DataItems.First());

            var app = mocks.Stub<IApplication>();
            app.DataItemService = dataItemService;
            app.Expect(a => a.Project).Return(project).Repeat.Any();
            var projectService = mocks.Stub<IProjectService>();
            app.Expect(a => a.ProjectService).Return(projectService).Repeat.Any();

            mocks.ReplayAll();

            // create plugin
            var plugin = new NetworkEditorApplicationPlugin { Application = app };
            plugin.Activate();
            projectService.Raise(x => x.ProjectOpened += null, null, new EventArgs<Project>(project)); // makes sure plugin subscribes to project events

            // remove sub-region
            parentRegion.SubRegions.Remove(subRegion);

            // add sub region
            parentRegionDataItem.Children.Count
                .Should("child data item is removed after sub-region is removed").Be.EqualTo(0);
        }

        [Test]
        public void ChildDataItemsIsAddedAfterCompoundRegionIsAddedToProject()
        {
            var subRegion = new HydroRegion();
            var parentRegion = new HydroRegion { SubRegions = { subRegion } };

            var project = new Project();

            // setup mock app
            var app = mocks.Stub<IApplication>();
            app.Expect(a => a.Project).Repeat.Any().Return(project);

            var projectService = mocks.DynamicMock<IProjectService>();
            app.Stub(a => a.ProjectService).Return(projectService);

            projectService.ProjectClosing += null; LastCall.IgnoreArguments().Repeat.Any();
            projectService.ProjectOpened += null; LastCall.IgnoreArguments().Repeat.Any();

            mocks.ReplayAll();

            // create plugin
            var plugin = new NetworkEditorApplicationPlugin { Application = app };
            plugin.Activate();
            projectService.Raise(x => x.ProjectOpened += null, this, new EventArgs<Project>(project)); // makes sure plugin subscribes to project events

            // add region to project
            project.RootFolder.Add(parentRegion);

            var parentRegionDataItem = (IDataItem)project.RootFolder.Items[0];

            parentRegionDataItem.Children.Count
                .Should("child data item is added after compound region is added").Be.EqualTo(1);
        }

        [Test]
        public void ChildDataItemsAreNotAddedTwiceAfterRegionIsAddedToProject()
        {
            var subRegion = new HydroRegion();
            var parentRegion = new HydroRegion { SubRegions = { subRegion } };

            var project = new Project();

            // create data items manually
            var subRegionDataItem = new DataItem(subRegion);
            var parentRegionDataItem = new DataItem(parentRegion);
            parentRegionDataItem.Children.Add(subRegionDataItem);

            // setup mock app
            var app = mocks.Stub<IApplication>();
            app.Expect(a => a.Project).Repeat.Any().Return(project);

            var projectService = mocks.DynamicMock<IProjectService>();
            app.Stub(a => a.ProjectService).Return(projectService);

            projectService.ProjectClosing += null; LastCall.IgnoreArguments().Repeat.Any();
            projectService.ProjectOpened += null; LastCall.IgnoreArguments().Repeat.Any();

            mocks.ReplayAll();

            // create plugin
            var plugin = new NetworkEditorApplicationPlugin { Application = app };
            plugin.Activate();
            projectService.Raise(x => x.ProjectOpened += null, this, new EventArgs<Project>(project)); // makes sure plugin subscribes to project events

            // add region to project
            project.RootFolder.Items.Add(parentRegionDataItem);

            parentRegionDataItem.Children.Count
                .Should("child data item is added after compound region is added").Be.EqualTo(1);
        }
    }
}