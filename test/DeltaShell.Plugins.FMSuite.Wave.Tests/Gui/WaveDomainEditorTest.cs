using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui
{
    [TestFixture]
    public class WaveDomainEditorTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowEditorTest()
        {
            var domain = new WaveDomainData("test");
            domain.SpectralDomainData = new SpectralDomainData
            {
                DirectionalSpaceType = WaveDirectionalSpaceType.Sector,
                StartDir = 10.0,
                EndDir = 100.0,
                NDir = 20,
                FreqMin = 0.0,
                FreqMax = 100.0,
                NFreq = 10
            };
            domain.MeteoData = new WaveMeteoData
            {
                FileType = WindDefinitionType.WindXY,
                XYVectorFileName = "test.wnd",
                HasSpiderWeb = true,
                SpiderWebFileName = "spider.spw"
            };
            

            var editor = new WaveDomainEditor {Data = domain};
            WindowsFormsTestHelper.ShowModal(editor);
        }
    }
}
