using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain.Restart;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Restart;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Gui.Restart
{
    [TestFixture]
    public class RealTimeControlRestartFileOutputTreeFolderTest
    {
        [Test]
        public void Constructor_ModelNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new RealTimeControlRestartFileOutputTreeFolder(null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("model"));
        }

        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Setup
            var model = new RealTimeControlModel();
            var restartFiles = new EventedList<RealTimeControlRestartFile>(GetRealTimeControlRestartFiles());
            model.RestartOutput = restartFiles;

            // Call
            var folder = new RealTimeControlRestartFileOutputTreeFolder(model);

            // Assert
            Assert.That(folder.Text, Is.EqualTo("Restart"));

            RealTimeControlRestartFile[] items = folder.ChildItems.Cast<RealTimeControlRestartFile>().ToArray();

            for (var i = 0; i < 3; i++)
            {
                RealTimeControlRestartFile item = items[i];
                RealTimeControlRestartFile restartFile = restartFiles[i];

                Assert.That(item, Is.SameAs(restartFile));
            }
        }

        private IEnumerable<RealTimeControlRestartFile> GetRealTimeControlRestartFiles()
        {
            yield return new RealTimeControlRestartFile("first.file", "file one");
            yield return new RealTimeControlRestartFile("second.file", "file two");
            yield return new RealTimeControlRestartFile("third.file", "file three");
        }
    }
}