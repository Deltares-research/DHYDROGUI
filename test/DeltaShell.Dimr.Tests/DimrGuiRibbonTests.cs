using NUnit.Framework;
using DeltaShell.Dimr.Gui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DelftTools.Utils.Reflection;
using Fluent;

namespace DeltaShell.Dimr.Gui.Tests
{
    public class TestDimrGuiPlugin : DimrGuiPlugin
    {
        public bool TestValue { get; set; }

        public override bool IsOnlyDimrModelSelected
        {
            get { return TestValue; }
        }
}
    [TestFixture()]
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
            Assert.That(ribbon.GetRibbonControl().GetType().Namespace, Is.StringStarting("Fluent"));
        }
    }
}