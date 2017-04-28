using System.Linq;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.NodePresenters;
using NUnit.Framework;
using Rhino.Mocks;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    public class RealTimeControlModelNodePresenterTest
    {
        [Test]
        public void GetChildNodes()
        {
            var mockRepo = new MockRepository();
            var gui = mockRepo.StrictMock<IGui>();
            var pluginGuiMock = mockRepo.StrictMock<GuiPlugin>();
            pluginGuiMock.Expect(pg => pg.Gui).Return(gui);

            var realTimeControlModelNodePresenter = new RealTimeControlModelNodePresenter(pluginGuiMock);
            var realTimeControlModel = new RealTimeControlModel();

            var count = realTimeControlModelNodePresenter.GetChildNodeObjects(realTimeControlModel, null).Cast<object>().Count();
            // RealTimeControlModel
            // output; will also allow user to select output
            // Controlgroups
            Assert.AreEqual(2, count);
        }
    }
}