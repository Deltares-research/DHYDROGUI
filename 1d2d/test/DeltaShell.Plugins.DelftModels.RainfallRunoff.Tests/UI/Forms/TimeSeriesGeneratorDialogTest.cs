using System;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI.Forms
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    public class TimeSeriesGeneratorDialogTest
    {
        [Test]
        public void ShowEmpty()
        {
            var dialog = new TimeSeriesGeneratorDialog();
            WindowsFormsTestHelper.ShowModal(dialog);
        }

        [Test]
        public void ShowWithEmptySeries()
        {
            var timeSeries = new TimeSeries();
            var dialog = new TimeSeriesGeneratorDialog();
            dialog.SetData(timeSeries.Time);
            WindowsFormsTestHelper.ShowModal(dialog);
        }

        [Test]
        public void ShowWithExistingSeries()
        {
            var timeSeries = new TimeSeries();
            timeSeries.Components.Add(new Variable<double>("value"));

            timeSeries[new DateTime(2010, 1, 1)] = 15.0;
            timeSeries[new DateTime(2010, 1, 2)] = 10.0;
            timeSeries[new DateTime(2010, 1, 3)] = 5.0;

            var dialog = new TimeSeriesGeneratorDialog();
            dialog.SetData(timeSeries.Time);
            WindowsFormsTestHelper.ShowModal(dialog);
        }
    }
}