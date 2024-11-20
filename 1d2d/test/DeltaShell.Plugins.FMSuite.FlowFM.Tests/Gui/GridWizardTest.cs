using System;
using System.Globalization;
using System.Windows.Forms;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    public class GridWizardTest
    {
        private const string dutchText = "10,0";
        private const string englishText = "12.0";
        private const double dutchValue = 10.0d;
        private const double englishValue = 12.0d;
        /// <summary>
        /// zucht.... voor TOOLS-23021
        /// </summary>
        [Test]
        public void SupportPointDistanceTextInvariantTest()
        {
            var tbSupportPointDistance = new TextBox();
            
            tbSupportPointDistance.Text = dutchText;
            Assert.AreEqual(dutchValue, Double.Parse(tbSupportPointDistance.Text.Replace(',','.'), CultureInfo.InvariantCulture));
            
            tbSupportPointDistance.Text = englishText;
            Assert.AreEqual(englishValue, Double.Parse(tbSupportPointDistance.Text.Replace(',', '.'), CultureInfo.InvariantCulture));
        }

        [Test]
        public void MinimumSupportPointDistanceTextInvariantTest()
        {
            var tbMinimumSupportPointDistance = new TextBox();

            tbMinimumSupportPointDistance.Text = dutchText;
            Assert.AreEqual(dutchValue, Double.Parse(tbMinimumSupportPointDistance.Text.Replace(',', '.'), CultureInfo.InvariantCulture));

            tbMinimumSupportPointDistance.Text = englishText;
            Assert.AreEqual(englishValue, Double.Parse(tbMinimumSupportPointDistance.Text.Replace(',', '.'), CultureInfo.InvariantCulture));            
        }
    }
}