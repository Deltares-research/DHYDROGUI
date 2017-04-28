using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls.Swf.DataEditorGenerator;
using DelftTools.Controls.Swf.DataEditorGenerator.FromType;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Unpaved;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI
{
    [TestFixture]
    public class RRDataEditorGeneratorSwfTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void GenerateEditorForUnpavedTestData()
        {
            var testUnpavedData = new TestUnpavedData {DrainageFormula = new DeZeeuwHellingaDrainageFormula()};

            var control = DataEditorGeneratorSwf.GenerateView(
                ObjectDescriptionFromTypeExtractor.ExtractObjectDescription(testUnpavedData.GetType()));

            control.Dock = DockStyle.Fill;
            control.Data = testUnpavedData;

            Assert.AreEqual(21, control.Bindings.Count);
            Assert.AreEqual(1, control.GetCustomControls().Count());

            WindowsFormsTestHelper.ShowModal(control);
        }
    }
}