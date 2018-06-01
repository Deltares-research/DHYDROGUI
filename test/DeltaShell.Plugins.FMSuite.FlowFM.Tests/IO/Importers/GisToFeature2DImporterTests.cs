using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class GisToFeature2DImporterTests
    {
        [Test]
        public void TestPointShapeImport()
        {
            var importer = new GisToFeature2DImporter<IPoint,Feature2D>();
        }

        [Test]
        public void TestLineStringShapeImport()
        {
            var importer = new GisToFeature2DImporter<ILineString, Feature2D>();
        }

        [Test]
        public void TestPolygonShapeImport()
        {
            var importer = new GisToFeature2DImporter<IPolygon, Feature2D>();
        }
    }
}
