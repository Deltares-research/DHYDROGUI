using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Feature;
using NUnit.Framework;
using SharpTestsEx;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class HydroRegionTest
    {
        [Test]
        public void GetAllRegions()
        {
            var subRegion2 = new HydroRegion();

            var subRegion1 = new HydroRegion {SubRegions = {subRegion2}};
            var subRegion3 = new HydroRegion();
            var region = new HydroRegion
            {
                SubRegions =
                {
                    subRegion1,
                    subRegion3
                }
            };

            region.AllRegions
                  .Should().Have.SameSequenceAs(new IHydroRegion[]
                  {
                      region,
                      subRegion1,
                      subRegion2,
                      subRegion3
                  });
        }

        [Test]
        public void UnsubscribeFromSubRegions()
        {
            var headRegion = new HydroRegion();
            var newRegionList = new EventedList<IRegion>();
            var newSubRegion = new HydroRegion();

            IEventedList<IRegion> oldSubRegions = headRegion.SubRegions;

            headRegion.SubRegions = newRegionList;

            // asserts
            ((INotifyCollectionChange) headRegion).CollectionChanged += (sender, args) => Assert.Fail("unsubscription failed");

            oldSubRegions.Add(newSubRegion);
        }
    }
}