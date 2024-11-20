using DelftTools.Hydro.GroupableFeatures;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.GroupableFeatures
{
    [TestFixture]
    public class GroupablePointFeatureTest
    {
        [Test]
        public void GivenAGroupablePointFeature_WhenCloningIt_ThenTheCloneShouldBeEqualToTheOriginalGroupablePointFeature()
        {
            var dryPoint = new GroupablePointFeature()
            {
                GroupName = "DryPoints",
                Geometry = new Point(new Coordinate(0, 100))
            };

            var clonedDryPoint = (GroupablePointFeature) dryPoint.Clone();

            Assert.AreEqual(dryPoint.Geometry, clonedDryPoint.Geometry);
            Assert.AreEqual(dryPoint.Attributes, clonedDryPoint.Attributes);
            Assert.AreEqual(dryPoint.GroupName, clonedDryPoint.GroupName);
            Assert.AreEqual(dryPoint.IsDefaultGroup, clonedDryPoint.IsDefaultGroup);
        }
    }
}