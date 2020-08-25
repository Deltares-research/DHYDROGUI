using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain.Restart;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Restart;
using NSubstitute;
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
            RealTimeControlRestartFile[] restartFiles = GetRealTimeControlRestartFiles().ToArray();
            model.RestartOutput.Returns(restartFiles);

            // Call
            var folder = new RealTimeControlRestartFileOutputTreeFolder(model);

            // Assert
            Assert.That(folder.Text, Is.EqualTo("Restart"));

            IDataItem[] items = folder.ChildItems.Cast<IDataItem>().ToArray();

            for (var i = 0; i < 3; i++)
            {
                IDataItem item = items[i];
                RealTimeControlRestartFile restartFile = restartFiles[i];

                Assert.That(item.Value, Is.SameAs(restartFile));
                Assert.That(item.Tag, Is.SameAs(restartFile.Name));
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