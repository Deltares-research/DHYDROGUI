using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.Common.IO.ImportExport;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO.ImportExport
{
    [TestFixture]
    public class Feature2DImportExportBaseTest
    {
        [Test]
        public void GivenTestImportExporter_WhenModeIsImport_ThenNameIsCorrectlySet()
        {
            var importExporter = new TestFeature2DImportExporter {Mode = Feature2DImportExportMode.Import};
            Assert.That(importExporter.Name, Is.EqualTo("Importer name"));
        }
        
        [Test]
        public void GivenTestImportExporter_WhenModeIsExport_ThenNameIsCorrectlySet()
        {
            var importExporter = new TestFeature2DImportExporter {Mode = Feature2DImportExportMode.Export};
            Assert.That(importExporter.Name, Is.EqualTo("Exporter name"));
        }

        private class TestFeature2DImportExporter : Feature2DImportExportBase<TestFeature>
        {
            public override string Category { get; }
            public override string Description { get; }
            public override Bitmap Image { get; }
            public override string FileFilter { get; }
            protected override string ExporterName => "Exporter name";
            protected override string ImporterName => "Importer name";

            protected override IEnumerable<TestFeature> Import(string path)
            {
                throw new NotImplementedException();
            }

            protected override void Export(IEnumerable<TestFeature> features, string path)
            {
                throw new NotImplementedException();
            }
        }

        private class TestFeature : IFeature, INameable
        {
            public long Id { get; set; }

            public IGeometry Geometry { get; set; }
            public IFeatureAttributeCollection Attributes { get; set; }
            public string Name { get; set; }

            public Type GetEntityType()
            {
                throw new NotImplementedException();
            }

            public object Clone()
            {
                throw new NotImplementedException();
            }
        }
    }
}