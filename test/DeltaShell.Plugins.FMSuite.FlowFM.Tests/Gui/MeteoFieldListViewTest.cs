using System;
using System.Windows.Forms;
using DelftTools.Controls.Swf.Table;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
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
            var view = new FmMeteoFieldListView
            {
                TimeSeriesGeneratorTool = TableViewTimeSeriesGeneratorTool,
                Data = itemList
            };
            WindowsFormsTestHelper.ShowModal(view);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithMeteoItems()
        {

            var model = new WaterFlowFMModel();

            
            var view = new FmMeteoFieldListView
            {
                TimeSeriesGeneratorTool = TableViewTimeSeriesGeneratorTool,
                Data = model.ModelDefinition.FmMeteoFields
            };
            
            Action<Form> shown = delegate
            {
                //view.Controls[0]
                Console.WriteLine();
            };

            Form testForm = new Form();
            testForm.Controls.Add(view);
            WindowsFormsTestHelper.ShowModal(testForm, f =>
            {
                //((Button)f.Controls[0].Controls[1].Controls[1]).PerformClick();
                //TypeUtils.GetPropertyInfo(typeof(IEventedList<IFmMeteoField>),"");
                var fmMeteoItems =(IEventedList<IFmMeteoField>)TypeUtils.GetPropertyValue(((FmMeteoFieldListView) f.Controls[0]), "FmMeteoItems");
                fmMeteoItems.Insert(0, FmMeteoField.CreateMeteoPrecipitationSeries());
                var table = (TableView) f.Controls[0].Controls[0].Controls[0].Controls[0].Controls[0].Controls[0];
                table.r
                Console.WriteLine();

            });
        }
    }
}