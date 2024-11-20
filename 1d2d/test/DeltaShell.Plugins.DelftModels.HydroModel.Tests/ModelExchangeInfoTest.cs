using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Core.Workflow.DataItems.ValueConverters;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.HydroModel.ModelExchanges;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    public class ModelExchangeInfoTest
    {
        [Test]
        public void GetExchangeIdentifierWithParameter()
        {
            Feature f = new Feature();
            IDataItem item = new DataItem(f, "test");
            item.ValueConverter = new FeaturePropertyValueConverter(f, "Geometry");

            string id = ModelExchange.GetExchangeIdentifier(item);

            Assert.AreEqual("test.Geometry", id);
        }

        [Test]
        public void GetExchangeIdentifierNoParameter()
        {
            Feature f = new Feature();
            IDataItem item = new DataItem(f, "test");
            string id = ModelExchange.GetExchangeIdentifier(item);
            Assert.AreEqual("test", id);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void SaveLoadRegionExchangeInfo()
        {
            var fromName = "From";
            var toName = "To";
            var oneWayDirection = true;
            var wkt = "LINESTRING(1.23 4.56 0,7.89 10.1112 0)";

            var fromRegion = Substitute.For<IRegion>();
            fromRegion.Name = fromName;
            var fromHydroObject = Substitute.For<IHydroObject>();
            fromHydroObject.Name = fromName;
            var toRegion = Substitute.For<IRegion>();
            toRegion.Name = toName;
            var toHydroObject = Substitute.For<IHydroObject>();
            toHydroObject.Name = toName;

            var link = new HydroLink(fromHydroObject, toHydroObject)
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(1.23, 4.56, 0),
                    new Coordinate(7.89, 10.1112, 0)
                })
            };


            var path = "region_exchange_info.json";

            //save
            var regionExchangeInfo = new RegionExchangeInfo(fromRegion, toRegion);
            regionExchangeInfo.Exchanges.Add(new RegionExchange(link));
            var regionExchangeInfos = new List<RegionExchangeInfo>();
            regionExchangeInfos.Add(regionExchangeInfo);
            regionExchangeInfos.WriteToJson(path);

            //load
            var returnRegionExchangeInfos = new List<RegionExchangeInfo>();
            returnRegionExchangeInfos.ReadFromJson(path);
            var returnRegionExchangeInfo = returnRegionExchangeInfos.FirstOrDefault();
            Assert.IsNotNull(returnRegionExchangeInfo);
            var returnExchange = returnRegionExchangeInfo.Exchanges.FirstOrDefault();
            Assert.IsNotNull(returnExchange);

            //check
            Assert.AreEqual(fromName, returnExchange.SourceName);
            Assert.AreEqual(toName, returnExchange.TargetName);
            Assert.AreEqual(wkt, returnExchange.LinkGeometryWkt);
        }
    }
}
