using System;
using System.Collections.Generic;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Core.Workflow.DataItems.ValueConverters;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Forms
{
    [TestFixture]
    public class InputSelectionDialogTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithData()
        {
            var mocks = new MockRepository();
            var dataItem1 = mocks.Stub<IDataItem>();
            var dataItem2 = mocks.Stub<IDataItem>();
            var dataItem3 = mocks.Stub<IDataItem>();
            var dataItem4 = mocks.Stub<IDataItem>();

            var converter1 = mocks.StrictMock<ParameterValueConverter>();
            var converter2 = mocks.StrictMock<ParameterValueConverter>();
            var converter3 = mocks.StrictMock<ParameterValueConverter>();
            var converter4 = mocks.StrictMock<ParameterValueConverter>();

            dataItem1.ValueConverter = converter1;
            dataItem2.ValueConverter = converter2;
            dataItem3.ValueConverter = converter3;
            dataItem4.ValueConverter = converter4;

            converter1.Expect(c => c.ParameterName).Return("DataItem1").Repeat.Any();
            converter2.Expect(c => c.ParameterName).Return("DataItem2").Repeat.Any();
            converter3.Expect(c => c.ParameterName).Return("DataItem3").Repeat.Any();
            converter4.Expect(c => c.ParameterName).Return("DataItem4").Repeat.Any();

            mocks.ReplayAll();

            var feature1 = new MockFeature {Name = "Feature1"};
            var feature2 = new MockFeature {Name = "Feature2"};

            var dataItemsFeature1 = new List<IDataItem>
            {
                dataItem1,
                dataItem2
            };
            var dataItemsFeature2 = new List<IDataItem>
            {
                dataItem3,
                dataItem4
            };
            var dialog = new InputSelectionDialog
            {
                Features = new List<IFeature>
                {
                    feature1,
                    feature2
                },
                GetDataItemsForFeature = (f) =>
                {
                    if (f == feature1)
                    {
                        return dataItemsFeature1;
                    }

                    if (f == feature2)
                    {
                        return dataItemsFeature2;
                    }

                    return null;
                }
            };

            WindowsFormsTestHelper.ShowModal(dialog);
        }

        private class MockFeature : IFeature, INameable
        {
            public long Id { get; set; }

            public IGeometry Geometry { get; set; }

            public IFeatureAttributeCollection Attributes { get; set; }

            public string Name { get; set; }

            public Type GetEntityType()
            {
                return typeof(MockFeature);
            }

            public object Clone()
            {
                return null;
            }
        }
    }
}