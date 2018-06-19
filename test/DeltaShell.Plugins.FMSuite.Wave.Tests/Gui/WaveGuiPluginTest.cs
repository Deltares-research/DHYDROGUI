using System;
using DelftTools.Shell.Core.Workflow;
using NUnit.Framework;
using Rhino.Mocks;
using System.Reflection;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.FMSuite.Wave.Gui;
using DeltaShell.Plugins.FMSuite.Wave.IO.Importers;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using SharpMap;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui
{
    class WaveGuiPluginTest
    {
        [Test]
        public void AfterImportingGridFileMapShouldBeZoomedToExtent()
        {
            var mocks = new MockRepository();
            var app = mocks.Stub<IApplication>();
            var gui = mocks.Stub<IGui>();
            var runner = mocks.Stub<IActivityRunner>();
            var mapView = mocks.Stub<MapView>();
            var map = mocks.Stub<Map>();
            
            gui.Application = app;
            mapView.Map = map;

            using (var gisPlugin = new SharpMapGisGuiPlugin())
            {
                using (var waveGuiPlugin = new WaveGuiPlugin())
                {
                    Func<MapView> myGetActiveMapViewFunc = () => mapView;
                    SetStaticField<WaveGuiPlugin>(waveGuiPlugin, "getActiveMapViewFunc", myGetActiveMapViewFunc);
                    
                    var waveModel = new WaveModel();
                    var waveGridFileImporter = new WaveGridFileImporter(waveGuiPlugin.Name, () => new[] {waveModel});
                    
                    app.Expect(a => a.ActivityRunner).Return(runner).Repeat.Any();
                    
                    var fileImportActivity = mocks.Stub<FileImportActivity>(waveGridFileImporter, waveModel.OuterDomain);

                    mocks.ReplayAll();

                    waveGuiPlugin.Gui = gui;
                    runner.Raise(r => r.ActivityStatusChanged += null, fileImportActivity, new ActivityStatusChangedEventArgs(ActivityStatus.None, ActivityStatus.Finished));
                    map.AssertWasCalled(m => m.ZoomToExtents(), opt => opt.Repeat.Once());
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
