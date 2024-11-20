using System;
using System.Collections.Generic;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.Gui.Forms;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    public class AstroComponentSelectionTest
    {
        [Test]
        public void ShowWithDefaultConstituents()
        {
            var view = new AstroComponentSelection();
            WindowsFormsTestHelper.ShowModal(view);
        }

        [Test]
        public void ShowWithCustomConstituents()
        {
            double[] frequencies = new[]
            {
                0.5 * Math.PI,
                Math.PI,
                1.5 * Math.PI
            };

            var view = new AstroComponentSelection
            {
                AstroComponents =
                    new Dictionary<string, double>
                    {
                        {"aap", frequencies[0]},
                        {"noot", frequencies[1]},
                        {"mies", frequencies[2]}
                    }
            };
            WindowsFormsTestHelper.ShowModal(view);
        }

        [Test]
        public void ShowWithCustomConstituentPeriods()
        {
            double[] frequencies = new[]
            {
                0.5 * Math.PI,
                Math.PI,
                1.5 * Math.PI
            };

            var view = new AstroComponentSelection
            {
                AstroComponents =
                    new Dictionary<string, double>
                    {
                        {"aap", frequencies[0]},
                        {"noot", frequencies[1]},
                        {"mies", frequencies[2]}
                    }
            };
            WindowsFormsTestHelper.ShowModal(view);
        }
    }
}