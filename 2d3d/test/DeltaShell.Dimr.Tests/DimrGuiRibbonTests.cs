using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using Fluent;
using NUnit.Framework;

namespace DeltaShell.Dimr.Gui.Tests
{
    public class TestDimrGuiPlugin : DimrGuiPlugin
    {
        public override bool IsOnlyDimrModelSelected
        {
            get
            {
                return TestValue;
            }
        }

        public bool TestValue { get; set; }
    }

    [TestFixture()]
    public class DimrGuiRibbonTests
    {
        [Test()]
        public void TestRibbon()
        {
            var ribbon = new Ribbon();
            var configContextualGroup = (RibbonContextualTabGroup) TypeUtils.GetField(ribbon, "configContextualGroup");
            var tabDimr = (RibbonTabItem) TypeUtils.GetField(ribbon, "tabDimr");
            Assert.AreEqual(configContextualGroup, tabDimr.Group);
            Assert.AreEqual(0, ribbon.Commands.Count());
            ribbon.ValidateItems(); //hmm does nothing now...
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void TestIsContextualTabVisible()
        {
            var ribbon = new Ribbon();
            var configContextualGroup = (RibbonContextualTabGroup) TypeUtils.GetField(ribbon, "configContextualGroup");
            var tabDimr = (RibbonTabItem) TypeUtils.GetField(ribbon, "tabDimr");
            var dimrGuiPlugin = new TestDimrGuiPlugin();
            dimrGuiPlugin.TestValue = true;
            Assert.True(ribbon.IsContextualTabVisible(configContextualGroup.Name, tabDimr.Name));
        }

        [Test]
        public void TestGetRibbonControl()
        {
            var ribbon = new Ribbon();
            Assert.That(ribbon.GetRibbonControl().GetType().Namespace, Does.StartWith("Fluent"));
        }
    }
}