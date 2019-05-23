using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.Wind;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    public class WaveMeteoDataEditorTest
    {
        [Test]
        public void ShowEditor()
        {
            var editor = new WaveMeteoDataEditor { Data = new WaveMeteoData
            {
                FileType = WindDefinitionType.WindXWindY,
                HasSpiderWeb = true,
                SpiderWebFileName = "someSpider.spw",
                XComponentFileName = "theX.wnd",
                YComponentFileName = "theY.wnd"
            } };
            WindowsFormsTestHelper.ShowModal(editor);
        }
    }
}
