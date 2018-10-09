using System;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    public class MeteoFieldListViewTest
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
        public void ShowWithZeroMeteoItems()
        {
            var itemList = new EventedList<IFmMeteoField>();
//            var view = new FmMeteoFieldListView
//            {
//                TimeSeriesGeneratorTool = TableViewTimeSeriesGeneratorTool,
//                Data = itemList
//            };
//            WindowsFormsTestHelper.ShowModal(view);
        }
    }
}