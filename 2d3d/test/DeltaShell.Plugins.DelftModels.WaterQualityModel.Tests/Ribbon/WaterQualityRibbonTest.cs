using System.Linq;
using DelftTools.Controls;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Ribbon;
using DeltaShell.Plugins.SharpMapGis.Gui;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.Ribbon
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    [Category(TestCategory.Slow)]
    public class WaterQualityRibbonTest
    {
        [Test]
        public void GetCommandsInRibbonTest()
        {
            var mocks = new MockRepository();
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
                ICommand[] commands = ribbon.Commands.ToArray();

                // assert
                Assert.AreEqual(3, commands.Length);
            }
        }

        [Test]
        public void GetRibbonControlTest()
        {
            var mocks = new MockRepository();
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
                object control = ribbon.GetRibbonControl();

                // assert
                Assert.IsNotNull(control);
            }
        }
    }
}