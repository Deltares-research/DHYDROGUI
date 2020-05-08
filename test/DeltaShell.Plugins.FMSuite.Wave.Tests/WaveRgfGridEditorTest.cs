using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.Gui.RgfGrid;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests
{
    [TestFixture]
    public class WaveRgfGridEditorTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        [Ignore("Not for build server (requires user input)")]
        public void OpenStructuredGrid()
        {
            string mdwPath = TestHelper.GetTestFilePath(@"wave_timespacevarbnd/tst.mdw");
            Assert.IsTrue(File.Exists(mdwPath));

            var waveModel = new WaveModel(mdwPath);

            string modelDirectoryName = Path.GetDirectoryName(mdwPath);

            string[] grids =
                WaveDomainHelper.GetAllDomains(waveModel.OuterDomain)
                                .Select(d => Path.Combine(modelDirectoryName, d.GridFileName))
                                .ToArray();
            bool[] emptyFlags = grids.Select(p => !File.Exists(p)).ToArray();
            RgfGridEditor.OpenGrids(grids, emptyFlags);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [Ignore("Not for build server (requires user input)")]
        public void OpenMultipleEmptyGrids()
        {
            var grids = new[]
            {
                "grid1.grd",
                "grid2.grd",
                "grid3.grd"
            };
            var empty = new[]
            {
                true,
                true,
                true
            };
            RgfGridEditor.OpenGrids(grids, empty);
        }
    }
}