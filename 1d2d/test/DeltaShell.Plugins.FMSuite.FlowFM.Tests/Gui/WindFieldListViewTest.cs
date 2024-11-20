using System;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    public class WindFieldListViewTest
    {
        private static TableViewTimeSeriesGeneratorTool TableViewTimeSeriesGeneratorTool
        {
            get
            {
                return new TableViewTimeSeriesGeneratorTool
                {
                    GetStartTime = () => DateTime.Now,
                    GetStopTime = () => DateTime.Now.AddHours(1),
                    GetTimeStep = () => new TimeSpan(0, 10, 0)
                };
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowEmptyShouldNotThrowException()
        {
            var view = new WindFieldListView();
            WindowsFormsTestHelper.ShowModal(view);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void SettingNullDataNotThrowException()
        {
            var view = new WindFieldListView() {Data = null};
            WindowsFormsTestHelper.ShowModal(view);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithZeroWindItems()
        {
            var itemList = new EventedList<IWindField>();
            var view = new WindFieldListView
            {
                TimeSeriesGeneratorTool = TableViewTimeSeriesGeneratorTool,
                Data = itemList
            };
            WindowsFormsTestHelper.ShowModal(view);
        }

    }
}
