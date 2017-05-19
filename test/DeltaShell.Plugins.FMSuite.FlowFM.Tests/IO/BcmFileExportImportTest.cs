using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using DelftTools.TestUtils;
using DelftTools.Units;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    public class BcmFileExportImportTest
    {
        [Test]
        [TestCase(2, 3, @"sedmor\files\2section3parameter3records.bcm")]
        public void BcmFileIsReadForMorphology(int numberOfBoundaries, int numberOfParameters, string filePath)
        {
            var fileReader = new BcmFile();
            var dataBlocks = fileReader.Read(TestHelper.GetTestFilePath(filePath)).ToList();
            
            Assert.True(dataBlocks.Count == numberOfBoundaries);

            Assert.True(dataBlocks[0].Quantities.Count == numberOfParameters);
            var expectedB0 = new List<List<string>>() /* Each row represents the values per column in the file.*/
            {
                new List<string>(){"0.00000000", "2.50000000", "1.4400000e+003" },
                new List<string>(){ "0.0", "0.0", "0.0"},
                new List<string>(){ "1", "1", "1"}
            };
            var actualB0 = dataBlocks[0].Quantities.Select(q => q.Values).ToList();
            CollectionAssert.AreEqual(expectedB0, actualB0);

            Assert.True(dataBlocks[1].Quantities.Count == numberOfParameters);
            var expectedB1 = new List<List<string>>() /* Each row represents the values per column in the file.*/
            {
                new List<string>(){"0.00000000", "2.50000000", "1.4400000e+003" },
                new List<string>(){ "0.0", "0.0", "0.0"},
                new List<string>(){ "1", "1", "1"}
            };
            var actualB1 = dataBlocks[0].Quantities.Select(q => q.Values).ToList();
            CollectionAssert.AreEqual(expectedB1, actualB1);
        }

        [Test]
        public void BcmFileIsWrittenFromMorphology()
        {
            var feature = new Feature2D
            {
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(0, 1) }),
                Name = "pli1"
            };

            var boundaryConditionSet = new BoundaryConditionSet { Feature = feature };
            var flowBc = new FlowBoundaryCondition(FlowBoundaryQuantityType.MorphologyBedLoadTransport,
                BoundaryConditionDataType.TimeSeries)
            {
                Feature = feature,
                SedimentFractionNames = new List<string>() { "Frac1", "Frac2" }
            };
            
            Assert.NotNull(flowBc);

            //3 entries, 3 parameters (2 fractions + time).
            flowBc.AddPoint(0);
            
            var bcData0 = flowBc.GetDataAtPoint(0);
            bcData0.Arguments[0].Unit = new Unit("-", "-"); 
            bcData0[new DateTime(2014, 1, 1, 0, 0, 0)] = new[] { 0.2, -0.3 };
            bcData0[new DateTime(2014, 1, 1, 0, 10, 0)] = new[] { 0.22, -0.32 };
            bcData0[new DateTime(2014, 1, 1, 0, 20, 0)] = new[] { 0.26, -0.36 };

            //1 entries, 5 parameters (4 comps + time).
            flowBc.AddPoint(2);
            var velocityData2 = flowBc.GetDataAtPoint(2);
            velocityData2.Arguments[0].Unit = new Unit("-", "-");
            velocityData2[new DateTime(2014, 1, 1, 0, 0, 0)] = new[] { -0.28, 0.38 };

            boundaryConditionSet.BoundaryConditions.Add(flowBc);

            const string filePath = "BcmFileIsWrittenFromMorphology.bcm";

            var writer = new BcmFile() { MultiFileMode = BcmFile.WriteMode.SingleFile };
            writer.Write(new[] { boundaryConditionSet }, filePath, new BcmFileFlowBoundaryDataBuilder());

            //Check whether if there are two blocks as well.
            var fileReader = new BcmFile();
            var dataBlocks = fileReader.Read(filePath).ToList();

            Assert.IsNotNull(dataBlocks);
            Assert.AreEqual(2, dataBlocks.Count);
        }
    }
}
