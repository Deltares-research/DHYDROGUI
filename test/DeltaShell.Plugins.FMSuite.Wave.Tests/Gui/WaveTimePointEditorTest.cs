using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui
{
    [TestFixture]
    public class WaveTimePointEditorTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowEditor()
        {
            var mdwFilePath = TestHelper.GetTestFilePath(@"bcwTimeseries\bcw.mdw");
            var newFilePath = WaveTestHelper.CreateLocalCopy(mdwFilePath);
            var model = new WaveModel(newFilePath)
            {
                TimePointData =
                {
                    HydroDataType = InputFieldDataType.TimeVarying,
                    WindDataType = InputFieldDataType.TimeVarying
                }
            };

            var view = new WaveTimePointEditor
                {
                    Data = model.TimePointData,
                    ImportFileIntoModelDirectory = model.ImportIntoModelDirectory
                };

            WindowsFormsTestHelper.ShowModal(view);
        }

    }
}
