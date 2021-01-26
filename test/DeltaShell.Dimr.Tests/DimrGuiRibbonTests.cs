using System.Linq;
using System.Threading;
using DelftTools.Utils.Reflection;
using Fluent;
using NUnit.Framework;
using Ribbon = DeltaShell.Dimr.Gui.Ribbon;

namespace DeltaShell.Dimr.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class DimrGuiRibbonTests
    {
        [Test()]
        public void TestRibbon()
        {
            var ribbon = new Ribbon();
            var configContextualGroup = (RibbonContextualTabGroup)TypeUtils.GetField(ribbon, "configContextualGroup");
            var tabDimr = (RibbonTabItem)TypeUtils.GetField(ribbon, "tabDimr");
            Assert.AreEqual(configContextualGroup, tabDimr.Group);
            Assert.AreEqual(0, ribbon.Commands.Count());
            ribbon.ValidateItems();//hmm does nothing now...
        }

        [Test()]
        public void TestIsContextualTabVisible()
        {
            var ribbon = new Ribbon();
            var configContextualGroup = (RibbonContextualTabGroup)TypeUtils.GetField(ribbon, "configContextualGroup");
            var tabDimr = (RibbonTabItem)TypeUtils.GetField(ribbon, "tabDimr");
            var dimrGuiPlugin = new TestDimrGuiPlugin();
            dimrGuiPlugin.TestValue = true;
            Assert.True(ribbon.IsContextualTabVisible(configContextualGroup.Name, tabDimr.Name));
        }
        
        [Test()]
        public void TestGetRibbonControl()
        {
            var ribbon = new Ribbon();
            Assert.IsTrue(ribbon.GetRibbonControl().GetType().Namespace.StartsWith("Fluent"));
        }
    }
}