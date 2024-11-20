using DelftTools.Hydro.Area.Objects;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors.ModelFeatureCoordinateDataEditor;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Editors.ModelFeatureCoordinateDataEditor
{
    [TestFixture]
    public class ModelFeatureCoordinateDataViewTest
    {
        [Test]
        [Category(TestCategory.Wpf)]
        public void ShowWithData()
        {
            var feature = new FixedWeir
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(10, 0),
                    new Coordinate(10, 10)
                })
            };
            var data = new ModelFeatureCoordinateData<FixedWeir>
            {
                Feature = feature,
                DataColumns =
                {
                    new DataColumn<double>
                    {
                        Name = "Values1",
                        ValueList =
                        {
                            1,
                            2,
                            3
                        }
                    },
                    new DataColumn<int>
                    {
                        Name = "Values2",
                        ValueList =
                        {
                            4,
                            5,
                            6
                        }
                    },
                    new DataColumn<string>
                    {
                        Name = "Names",
                        ValueList =
                        {
                            "coordinate 1",
                            "coordinate 2",
                            "coordinate 3"
                        }
                    }
                }
            };

            var modelFeatureCoordinateDataView = new ModelFeatureCoordinateDataView {Data = data};

            WpfTestHelper.ShowModal(modelFeatureCoordinateDataView);
        }
    }
}