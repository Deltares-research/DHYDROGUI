using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.Gui;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.Gui
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    public class FileBasedWindDefinitionViewTest
    {
        [Test]
        public void ShowView()
        {
            var data = new FileBasedWindDefinition();
            WindowsFormsTestHelper.ShowModal(new FileBasedWindDefinitionView {Data = data});
        }
    }
}
