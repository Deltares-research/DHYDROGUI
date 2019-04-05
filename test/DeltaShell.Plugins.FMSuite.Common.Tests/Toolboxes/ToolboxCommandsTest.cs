using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.Toolboxes;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.Toolboxes
{
    [TestFixture]
    public class ToolboxCommandsTest
    {
        [Test]
        public void LoadScriptsFromDisk()
        {
            var toolboxDirectory = Path.Combine(TestHelper.GetTestDataDirectory(), "toolboxes");
            var commands = ToolboxCommands.LoadFrom(toolboxDirectory).OrderBy(c => c.Title).ToList();

            Assert.AreEqual(2, commands.Count);

            Assert.AreEqual("Bathymetry from Gebco", commands[0].Title);
            Assert.IsNotNull(commands[0].Image);
            Assert.AreEqual("Observationpoints from DB", commands[1].Title);
            Assert.IsNotNull(commands[1].Image);
        }
    }
}