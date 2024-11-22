﻿using System;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.TestUtils;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class CrossSectionBranchFeatureTest
    {
        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void CreateDefaultCrossSectionTest()
        {
            IChannel channel = new Channel { Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(20, 0) }) };

            var crossSection = CrossSection.CreateDefault(CrossSectionType.YZ, channel, 0);

            // The geometry length is calculated only in x, y and is 100
            Assert.AreEqual(100.0, crossSection.Geometry.Length, 1.0e-6);

            // the total delta x; used as y for the crosssectional view should be 100.
            double length =
                Math.Abs((double) (crossSection.Definition.GetProfile().Last().X - crossSection.Definition.GetProfile().First().X));
            Assert.AreEqual(100.0, length, 1.0e-6);
        }
        
    }
}