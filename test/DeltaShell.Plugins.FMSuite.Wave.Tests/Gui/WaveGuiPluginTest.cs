using System;
using DelftTools.Shell.Core.Workflow;
using NUnit.Framework;
using Rhino.Mocks;
using System.Reflection;
using DelftTools.Controls;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Forms;
using DeltaShell.Plugins.FMSuite.Wave.Gui;
using DeltaShell.Plugins.FMSuite.Wave.IO.Importers;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using SharpMap;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui
{

    [TestFixture]
    public class WaveGuiPluginTest
    {
        [Test]
        public void AfterImportingModelOrGridFileMapShouldBeZoomedToExtents()
        {
            var mocks = new MockRepository();
            var app = mocks.Stub<IApplication>();
            var gui = mocks.Stub<IGui>();
            var mainWindow = mocks.Stub<IMainWindow>();
            var projectExplorer = mocks.Stub<IProjectExplorer>();
            var treeView = mocks.Stub<ITreeView>();
            var runner = mocks.Stub<IActivityRunner>();
            var mapView = mocks.Stub<MapView>();
            var map = mocks.Stub<Map>();

            gui.Application = app;
            mapView.Map = map;
            gui.Expect(g => g.MainWindow).Return(mainWindow).Repeat.Any();
            mainWindow.Expect(mw => mw.ProjectExplorer).Return(projectExplorer).Repeat.Any();
            projectExplorer.Expect(pe => pe.TreeView).Return(treeView).Repeat.Any();

            using (var gisPlugin = new SharpMapGisGuiPlugin())
            {
                using (var waveGuiPlugin = new WaveGuiPlugin())
                {
                    Func<MapView> myGetActiveMapViewFunc = () => mapView;
                    SetStaticField<WaveGuiPlugin>(waveGuiPlugin, "getActiveMapViewFunc", myGetActiveMapViewFunc);

                    var waveModel = new WaveModel();
                    
                    app.Expect(a => a.ActivityRunner).Return(runner).Repeat.Any();

                    var waveBoundaryFileImporter = new WaveBoundaryFileImporter();
                    var boundaryFileImportActivity = mocks.Stub<FileImportActivity>(waveBoundaryFileImporter, waveModel.OuterDomain);

                    var waveModelFileImporter = new WaveModelFileImporter();
                    var modelFileImportActivity = mocks.Stub<FileImportActivity>(waveModelFileImporter, waveModel.OuterDomain);

                    var waveGridFileImporter = new WaveGridFileImporter(waveGuiPlugin.Name, () => new[] { waveModel });
                    var gridFileImportActivity = mocks.Stub<FileImportActivity>(waveGridFileImporter, waveModel.OuterDomain);

                    mocks.ReplayAll();

                    waveGuiPlugin.Gui = gui;

                    // zoom-to-extents should not be called when importer is not gridfile importer or modelimporter
                    runner.Raise(r => r.ActivityStatusChanged += null, boundaryFileImportActivity, new ActivityStatusChangedEventArgs(ActivityStatus.None, ActivityStatus.Finished));
                    treeView.AssertWasNotCalled(tv => tv.Refresh());
                    map.AssertWasNotCalled(m => m.ZoomToExtents());

                    // gridfile importer
                    runner.Raise(r => r.ActivityStatusChanged += null, gridFileImportActivity, new ActivityStatusChangedEventArgs(ActivityStatus.None, ActivityStatus.Finished));
                    treeView.AssertWasNotCalled(tv => tv.Refresh());
                    map.AssertWasCalled(m => m.ZoomToExtents(), opt => opt.Repeat.Once());

                    // modelfile importer
                    runner.Raise(r => r.ActivityStatusChanged += null, modelFileImportActivity, new ActivityStatusChangedEventArgs(ActivityStatus.None, ActivityStatus.Finished));
                    treeView.AssertWasCalled(tv => tv.Refresh(), opt => opt.Repeat.Once());
                    map.AssertWasCalled(m => m.ZoomToExtents(), opt => opt.Repeat.Twice());

                    map.VerifyAllExpectations();
                }
            }
        }

        private void SetStaticField<T>(object obj, string fieldName, object value)
        {
            var fieldInfo = typeof(T).GetField(fieldName, BindingFlags.Instance
                                                          | BindingFlags.NonPublic | BindingFlags.Static
                                                          | BindingFlags.Public | BindingFlags.FlattenHierarchy);

            if (fieldInfo == null)
            {
                throw new ArgumentOutOfRangeException("fieldName");
            }

            fieldInfo.SetValue(obj, value);
        }
    }
}
