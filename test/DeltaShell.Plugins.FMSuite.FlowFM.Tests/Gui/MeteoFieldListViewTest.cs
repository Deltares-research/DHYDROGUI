using System;
using System.Threading;
using System.Windows.Forms;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture, Apartment(ApartmentState.STA)]
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
        public void ShowWithOneGlobalMeteoItem()
        {
            var itemList = new EventedList<IFmMeteoField>() { FmMeteoField.CreateMeteoPrecipitationSeries(FmMeteoLocationType.Feature)};
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
            //Create model and view
            var model = new WaterFlowFMModel();
            var view = new FmMeteoFieldListView
            {
                TimeSeriesGeneratorTool = TableViewTimeSeriesGeneratorTool,
                Data = model.ModelDefinition.FmMeteoFields
            };

            //Simulate the clicking of the add button 
            Form testForm = new Form();
            testForm.Controls.Add(view);
            Assert.That(model.ModelDefinition.FmMeteoFields.Count, Is.EqualTo(0));
            WindowsFormsTestHelper.ShowModal(testForm, f =>
            {
                var fmMeteoItems = (IEventedList<IFmMeteoField>)TypeUtils.GetPropertyValue(((FmMeteoFieldListView) f.Controls[0]), "FmMeteoItems");
                fmMeteoItems.Insert(0, FmMeteoField.CreateMeteoPrecipitationSeries(FmMeteoLocationType.Global));
                Console.WriteLine();
            });
            Assert.That(model.ModelDefinition.FmMeteoFields.Count, Is.Not.EqualTo(0));
        }
    }
}