using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.NGHS.Common.Gui.Restart;
using DeltaShell.NGHS.Common.IO.RestartFiles;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Tests.Gui.Restart
{
    [TestFixture]
    public class RestartFileOutputTreeFolderTest
    {
        [Test]
        public void Constructor_ModelNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new RestartFileOutputTreeFolder(null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("model"));
        }

        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Setup
            var model = Substitute.For<IRestartModel>();
            RestartFile[] restartFiles = GetRestartFiles().ToArray();
            model.RestartOutput.Returns(restartFiles);

            // Call
            var folder = new RestartFileOutputTreeFolder(model);

            // Assert
            Assert.That(folder.Text, Is.EqualTo("Restart"));

            IDataItem[] items = folder.ChildItems.Cast<IDataItem>().ToArray();

            for (var i = 0; i < 3; i++)
            {
                IDataItem item = items[i];
                RestartFile restartFile = restartFiles[i];

                Assert.That(item.Value, Is.SameAs(restartFile));
                Assert.That(item.Tag, Is.SameAs(restartFile.Name));
            }
        }

        private IEnumerable<RestartFile> GetRestartFiles()
        {
            yield return new RestartFile("first.file");
            yield return new RestartFile("second.file");
            yield return new RestartFile("third.file");
        }
    }
}