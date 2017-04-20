using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Ribbon;
using DeltaShell.Plugins.SharpMapGis.Gui;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.Ribbon
{
    [TestFixture]
    public class WaterQualityRibbonTest
    {
        [Test]
        public void GetCommandsInRibbonTest()
        {
            MockRepository mocks = new MockRepository();
            var guiStub = mocks.Stub<IGui>();
            var applicationStub = mocks.Stub<IApplication>();
            guiStub.Application = applicationStub;

            using (var gisPlugin = new SharpMapGisGuiPlugin())
            {
                gisPlugin.Gui = guiStub;
                gisPlugin.InitializeSpatialOperationSetLayerView();

                // setup
                var ribbon = new WaterQualityRibbon();

                // call
                var commands = ribbon.Commands.ToArray();

                // assert
                Assert.AreEqual(3, commands.Length);
            }
        }

        [Test]
        public void GetRibbonControlTest()
        {
            MockRepository mocks = new MockRepository();
            var guiStub = mocks.Stub<IGui>();
            var applicationStub = mocks.Stub<IApplication>();
            guiStub.Application = applicationStub;

            using (var gisPlugin = new SharpMapGisGuiPlugin())
            {
                gisPlugin.Gui = guiStub;    
                gisPlugin.InitializeSpatialOperationSetLayerView();

                // setup
                var ribbon = new WaterQualityRibbon();

                // call
                var control = ribbon.GetRibbonControl();

                // assert
                Assert.IsNotNull(control);
            }
        }
    }
}