using DelftTools.Shell.Gui;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.NodePresenters;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.NodePresenters
{
    [TestFixture]
    public class RealTimeControlModelNodePresenterTest
    {
        [Test]
        public void TagTypeShouldBeRealTimeControlModel()
        {
            RealTimeControlModelNodePresenter realTimeControlModelNodePresenter = GetRealTimeControlModelNodePresenter();
            Assert.AreEqual(typeof(RealTimeControlModel), realTimeControlModelNodePresenter.NodeTagType);
        }

        private static RealTimeControlModelNodePresenter GetRealTimeControlModelNodePresenter()
        {
            var mockRepo = new MockRepository();
            var gui = mockRepo.StrictMock<IGui>();
            var pluginGuiMock = mockRepo.StrictMock<GuiPlugin>();
            pluginGuiMock.Expect(pg => pg.Gui).Return(gui);

            return new RealTimeControlModelNodePresenter(pluginGuiMock);
        }
    }
}