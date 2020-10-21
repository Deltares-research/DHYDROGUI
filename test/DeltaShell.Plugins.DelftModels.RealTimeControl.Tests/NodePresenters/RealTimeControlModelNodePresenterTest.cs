using System.Collections;
using System.Linq;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common.IO.RestartFiles;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain.Restart;
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

        [Test]
        public void GetChildNodeObjects_ContainsRestartOutputTreeFolder()
        {
            // Setup
            RealTimeControlModelNodePresenter nodePresenter = GetRealTimeControlModelNodePresenter();
            var model = new RealTimeControlModel()
            {
                RestartOutput = new EventedList<RestartFile>(new[]
                {
                    new RestartFile()
                })
            };

            // Call
            IEnumerable childObjects = nodePresenter.GetChildNodeObjects(model, null);

            // Assert
            TreeFolder outputTreeFolder = childObjects.OfType<TreeFolder>().Single(f => f.Text == "Output");
            TreeFolder restartFileOutputTreeFolder = outputTreeFolder.ChildItems.OfType<TreeFolder>().Single(f => f.Text == "Restart");
            Assert.That(restartFileOutputTreeFolder.Text, Is.EqualTo("Restart"));
            Assert.That(restartFileOutputTreeFolder.ChildItems.OfType<RestartFile>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void GetChildNodeObjects_ContainsRestartInputTreeFolder()
        {
            // Setup
            RealTimeControlModelNodePresenter nodePresenter = GetRealTimeControlModelNodePresenter();
            var model = new RealTimeControlModel();

            // Call
            IEnumerable childObjects = nodePresenter.GetChildNodeObjects(model, null);

            // Assert
            TreeFolder inputTreeFolder =
                childObjects.OfType<TreeFolder>().Single(f => f.Text == "Input");
            TreeFolder initialConditionsFolder =
                inputTreeFolder.ChildItems.OfType<TreeFolder>().Single(f => f.Text == "Initial Conditions");
            RealTimeControlRestartFile inputRestartFile =
                initialConditionsFolder.ChildItems.OfType<RealTimeControlRestartFile>().Single();

            Assert.That(inputRestartFile, Is.Not.Null);
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