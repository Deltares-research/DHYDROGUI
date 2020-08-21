using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.NGHS.Common.Gui.Restart;
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

        [Test]
        public void GetChildNodeObjects_ContainsRestartOutputTreeFolder()
        {
            // Setup
            RealTimeControlModelNodePresenter nodePresenter = GetRealTimeControlModelNodePresenter();
            var model = new RealTimeControlModel();

            // Call
            IEnumerable childObjects = nodePresenter.GetChildNodeObjects(model, null);

            // Assert
            TreeFolder outputTreeFolder = childObjects.OfType<TreeFolder>().Single(f => f.Text == "Output");
            List<RestartFileOutputTreeFolder> restartFileOutputTreeFolders = outputTreeFolder.ChildItems.OfType<RestartFileOutputTreeFolder>().ToList();
            Assert.That(restartFileOutputTreeFolders, Has.Count.EqualTo(1));
        }
    }
}