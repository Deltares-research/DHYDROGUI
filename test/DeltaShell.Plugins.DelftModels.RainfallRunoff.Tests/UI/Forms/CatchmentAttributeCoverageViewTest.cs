using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.FeatureCoverageProviders;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI.Forms
{
    [TestFixture, Apartment(ApartmentState.STA)]
    [Category(TestCategory.WindowsForms)]
    public class CatchmentAttributeCoverageViewTest
    {
        [Test]
        public void ShowEmpty()
        {
            var featureCoverageView = new CatchmentAttributeCoverageView();
            WindowsFormsTestHelper.ShowModal(featureCoverageView);
        }

        [Test]
        public void ShowWithUnpavedData()
        {
            var geometries = CreateFourPolygons().ToList();
            var model = new RainfallRunoffModel();

            var unpaved1 = CreateDataInModel(model, geometries[0]);
            unpaved1.GroundWaterLayerThickness = 1;
            unpaved1.InfiltrationCapacity = 0.3;

            var unpaved2 = CreateDataInModel(model, geometries[1]);
            unpaved2.GroundWaterLayerThickness = 2;
            unpaved2.InfiltrationCapacity = 0.6;

            var unpaved3 = CreateDataInModel(model, geometries[2]);
            unpaved3.GroundWaterLayerThickness = 3;
            unpaved3.InfiltrationCapacity = 0.1;

            var unpaved4 = CreateDataInModel(model, geometries[3]);
            unpaved4.GroundWaterLayerThickness = 2;
            unpaved4.InfiltrationCapacity = 2;

            var unpavedFeatureCoverageProvider = new UnpavedFeatureCoverageProvider(model);
            var featureCoverageView = new CatchmentAttributeCoverageView {Data = unpavedFeatureCoverageProvider};

            WindowsFormsTestHelper.ShowModal(featureCoverageView);
        }

        #region Helpers

        private UnpavedData CreateDataInModel(RainfallRunoffModel model, IGeometry geometry)
        {
            var catchment1 = new Catchment { Geometry = geometry, CatchmentType = CatchmentType.Unpaved};
            model.Basin.Catchments.Add(catchment1);
            return model.GetAllModelData().First() as UnpavedData;
        }

        private IEnumerable<IGeometry> CreateFourPolygons()
        {
            //use catchment to adjust geometry internally to polyon

            var catchment1 = new Catchment { Geometry = new Point(-300, 150), IsGeometryDerivedFromAreaSize = true };
            var catchment2 = new Catchment { Geometry = new Point(200, 120), IsGeometryDerivedFromAreaSize = true};
            var catchment3 = new Catchment { Geometry = new Point(100, 200), IsGeometryDerivedFromAreaSize = true};
            var catchment4 = new Catchment
            {
                Geometry =
                    new Polygon(
                    new LinearRing(new Coordinate[]
                        {
                            new Coordinate(0, 0), new Coordinate(100, 100),
                            new Coordinate(200, -100), new Coordinate(100, -300),
                            new Coordinate(-100, -300), new Coordinate(-400, -100),
                            new Coordinate(-100, 100), new Coordinate(0, 0)
                        }))
            };

            catchment1.SetAreaSize(40000);
            catchment2.SetAreaSize(10000);
            catchment3.SetAreaSize(10000);

            yield return catchment1.Geometry;
            yield return catchment2.Geometry;
            yield return catchment3.Geometry;
            yield return catchment4.Geometry;
        }

        #endregion
    }
}
