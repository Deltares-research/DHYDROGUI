using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Core;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Wave.IO.Importers;
using DeltaShell.Plugins.SharpMapGis;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Importers
{
    [TestFixture]
    public class WaveBoundaryFileImporterTest
    {
        private WaveBoundaryFileImporter importer;

        [Test]
        public void NamePropertyTest()
        {
            var expected = "Wave Boundary Conditions (*.bcw)";
            importer = new WaveBoundaryFileImporter();
            Assert.AreEqual(expected, importer.Name);
        }

        [Test]
        public void SupportedItemTypesTypesPropertyTest()
        {
            var expected = new List<Type> {typeof(IList<WaveBoundaryCondition>)};
            importer = new WaveBoundaryFileImporter();
            Assert.AreEqual(expected, importer.SupportedItemTypes);
        }

        [Test]
        public void CanImportOnPropertyTest()
        {
            importer = new WaveBoundaryFileImporter();
            Assert.IsTrue(importer.CanImportOn(new object()));
            Assert.IsFalse(importer.CanImportOnRootLevel);
        }

        [Test]
        public void FileFilterTest()
        {
            var expected = "Wave Boundary Condition Files (*.bcw;*.sp2)|*.bcw;*.sp2";
            importer = new WaveBoundaryFileImporter();
            Assert.AreEqual(expected, importer.FileFilter);
        }

        [Test]
        public void OpenViewAfterImport()
        {
            importer = new WaveBoundaryFileImporter();
            Assert.IsTrue(importer.OpenViewAfterImport);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportItemTest_WhenTargetIsNotWaveBoundaryConditionCollection_ThenReturnNull()
        {
            importer = new WaveBoundaryFileImporter();
            var target = new List<string>();
            var boundaryConditions = importer.ImportItem(string.Empty, target);
            Assert.IsNull(boundaryConditions);
        }

        [Test]
        public void TargetDataDirectory()
        {
            string targetDataDirectory = "dir";
            importer = new WaveBoundaryFileImporter {TargetDataDirectory = targetDataDirectory};
            Assert.AreEqual(targetDataDirectory, importer.TargetDataDirectory);
        }

        [Test]
        public void ShouldCancelTest()
        {
            importer = new WaveBoundaryFileImporter {ShouldCancel = true};
            Assert.AreEqual(true, importer.ShouldCancel);
            importer.ShouldCancel = false;
            Assert.AreEqual(false, importer.ShouldCancel);
        }

        [Test]
        public void ProgressChangedTest()
        {
            importer = new WaveBoundaryFileImporter();
            bool succes = false;
            importer.ProgressChanged = (name, current, total) => { succes = true; };
            importer.ProgressChanged("Importing boundary file...", 1, 2);
            Assert.IsTrue(succes);
        }

        private static DeltaShellApplication GetRunningApplication(string savePath)
        {
            var app = new DeltaShellApplication {IsProjectCreatedInTemporaryDirectory = true};
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Run();
            app.SaveProjectAs(savePath);
            return app;
        }

        private static void SetData(WaveBoundaryCondition boundaryCondition, DateTime refTime)
        {
            boundaryCondition.PointData[0].Arguments[0].SetValues(new[] {refTime, refTime.AddDays(1)});
            boundaryCondition.PointData[0].Components[0].SetValues(new double[] {1, 2});
            boundaryCondition.PointData[0].Components[1].SetValues(new double[] {3, 4});
        }

        private static Feature2D CreateBoundary(string boundaryName)
        {
            var boundary = new Feature2D
            {
                Geometry = new LineString(new[] {new Coordinate(0, 0), new Coordinate(0, 1)}),
                Name = boundaryName
            };
            return boundary;
        }
    }
}