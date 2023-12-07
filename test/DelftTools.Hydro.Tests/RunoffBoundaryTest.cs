using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.Validation.Common;
using log4net.Core;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class RunoffBoundaryTest
    {
        [Test]
        public void Clone()
        {
            var boundary = new RunoffBoundary { Geometry = new Point(15, 15), Name = "aa", Basin = new DrainageBasin() };
            boundary.Attributes.Add("Milage", 15);

            var clone = boundary.Clone();

            ReflectionTestHelper.AssertPublicPropertiesAreEqual(boundary, clone);
        }
    }
}