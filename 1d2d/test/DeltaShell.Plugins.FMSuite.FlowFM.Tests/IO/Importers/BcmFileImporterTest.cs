using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture()]
    public class BcmFileImporterTest
    {
        private BcmFileImporter importer;
        private BoundaryCondition boundaryCondition;

        [SetUp]
        public void Setup()
        {
            importer = new BcmFileImporter();
        }

        [TestCase(FlowBoundaryQuantityType.MorphologyBedLevelPrescribed)]
        [TestCase(FlowBoundaryQuantityType.MorphologyBedLevelChangePrescribed)]
        public void GivenBcmFileImporterWhenBoundaryConditionHasCorrectQuantityThenValidateTrue(FlowBoundaryQuantityType flowBoundaryQuantityType)
        {
            FlowBoundaryCondition flowBoundaryCondition = new FlowBoundaryCondition(flowBoundaryQuantityType, BoundaryConditionDataType.TimeSeries);
            var result = importer.CanImportOnBoundaryCondition(flowBoundaryCondition);

            Assert.IsTrue(result);
        }

        [Test]
        public void GivenBcmFileImporterWhenBoundaryConditionHasInCorrectQuantityTypeThenValidateFalse()
        {
            FlowBoundaryCondition flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.Discharge, BoundaryConditionDataType.TimeSeries);
            var result = importer.CanImportOnBoundaryCondition(flowBoundaryCondition);

            Assert.IsFalse(result);
        }

        [Test]
        public void GivenBcmFileImporterWhenBoundaryConditionHasInCorrectDataTypeThenValidateFalse()
        {
            FlowBoundaryCondition flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.MorphologyBedLevelPrescribed, BoundaryConditionDataType.Empty);
            var result = importer.CanImportOnBoundaryCondition(flowBoundaryCondition);

            Assert.IsFalse(result);
        }

        [Test()]
        public void TestBcmFileImporter()
        {
            var importer = new BcmFileImporter();
            Assert.IsTrue((bool)TypeUtils.GetPropertyValue(importer, "OverwriteExistingData"));
            var importerSupportedItemTypes = importer.SupportedItemTypes.ToList();
            Assert.That(importerSupportedItemTypes.Count, Is.EqualTo(3));
            Assert.That(importerSupportedItemTypes[0], Is.EqualTo(typeof(IList<BoundaryConditionSet>)));
            Assert.That(importerSupportedItemTypes[1], Is.EqualTo(typeof(BoundaryConditionSet)));
            Assert.That(importerSupportedItemTypes[2], Is.EqualTo(typeof(BoundaryCondition)));
        }

        [Test()]
        public void TestCanImportOn()
        {
            var importer = new BcmFileImporter();
            Assert.IsTrue(importer.CanImportOn(new object()));
            Assert.IsFalse(importer.CanImportOnRootLevel);
        }

        [Test()]
        public void TestImportItemOnModelBoundaryConditionSets()
        {
            var filePath = TestHelper.GetTestFilePath(@"BcmFiles\MorphologyBedLevelPrescribed.bcm");
            filePath = TestHelper.CreateLocalCopy(filePath);

            //Import
            var importer = new BcmFileImporter
            {
                DeleteDataBeforeImport = true,
            };

            var model = new WaterFlowFMModel();
            model.Name = "newname";
            var boundary = new Feature2D
            {
                Name = "Boundary01",
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(0, 100) })
            };
            model.Boundaries.Add(boundary);

            var fbcFactory = new FlowBoundaryConditionFactory();
            fbcFactory.Model = model;
            var boundCond = fbcFactory.CreateBoundaryCondition(boundary,
                FlowBoundaryQuantityType.WaterLevel.ToString(),
                BoundaryConditionDataType.TimeSeries,
                FlowBoundaryQuantityType.WaterLevel.GetDescription());

            model.BoundaryConditionSets[0].BoundaryConditions.Add(boundCond);
            
            importer.ImportItem(filePath, model.BoundaryConditionSets);
            Assert.That(((FlowBoundaryCondition) model.BoundaryConditionSets[0].BoundaryConditions[1]).FlowQuantity, Is.EqualTo(FlowBoundaryQuantityType.MorphologyBedLevelPrescribed));
        }
        [Test()]
        public void TestImportItemOnAModelBoundaryConditionSet()
        {
            var filePath = TestHelper.GetTestFilePath(@"BcmFiles\MorphologyBedLevelPrescribed.bcm");
            filePath = TestHelper.CreateLocalCopy(filePath);

            //Import
            var importer = new BcmFileImporter
            {
                DeleteDataBeforeImport = true,
            };

            var model = new WaterFlowFMModel();
            model.Name = "newname";
            var boundary = new Feature2D
            {
                Name = "Boundary00",
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(0, 100) })
            };
            model.Boundaries.Add(boundary);

            var boundary1 = new Feature2D
            {
                Name = "Boundary01",
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(0, 100) })
            };
            model.Boundaries.Add(boundary1);

            var fbcFactory = new FlowBoundaryConditionFactory();
            fbcFactory.Model = model;
            var boundCond = fbcFactory.CreateBoundaryCondition(boundary,
                FlowBoundaryQuantityType.WaterLevel.ToString(),
                BoundaryConditionDataType.TimeSeries,
                FlowBoundaryQuantityType.WaterLevel.GetDescription());

            model.BoundaryConditionSets[0].BoundaryConditions.Add(boundCond);
            
            importer.ImportItem(filePath, model.BoundaryConditionSets[1]);
            Assert.That(((FlowBoundaryCondition) model.BoundaryConditionSets[1].BoundaryConditions[0]).FlowQuantity, Is.EqualTo(FlowBoundaryQuantityType.MorphologyBedLevelPrescribed));
        }
        [Test()]
        public void TestImportItemOnBoundaryCondition()
        {
            var filePath = TestHelper.GetTestFilePath(@"BcmFiles\MorphologyBedLevelPrescribed.bcm");
            filePath = TestHelper.CreateLocalCopy(filePath);

            //Import
            var importer = new BcmFileImporter
            {
                FilePaths = new[] {filePath},
                DeleteDataBeforeImport = true,
                ProgressChanged = (name, step, steps) => Console.WriteLine(name)
            };

            var boundary1 = new Feature2D
            {
                Name = "Boundary01",
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(0, 100) })
            };
            
            var fbcFactory = new FlowBoundaryConditionFactory();
            var boundCond = fbcFactory.CreateBoundaryCondition(boundary1,
                FlowBoundaryQuantityType.MorphologyBedLevelPrescribed.ToString(),
                BoundaryConditionDataType.TimeSeries,
                FlowBoundaryQuantityType.MorphologyBedLevelPrescribed.GetDescription());
            
            importer.ImportItem(null, boundCond);
            var data = boundCond.GetDataAtPoint(0);
            Assert.AreEqual(23, data.GetValues<double>().Count);
        }

        [Test]
        public void TestImportItemOnObjectThrowArgumentException()
        {
            var filePath = TestHelper.GetTestFilePath(@"BcmFiles\MorphologyBedLevelPrescribed.bcm");
            filePath = TestHelper.CreateLocalCopy(filePath);

            //Import
            var importer = new BcmFileImporter
            {
                DeleteDataBeforeImport = true,
            };
            
            var myobject = new object();
            Assert.Throws<ArgumentException>(() => importer.ImportItem(filePath, myobject));
        }
    }
}