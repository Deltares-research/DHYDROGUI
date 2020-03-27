using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Core;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.Properties;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.SpectralData;
using DeltaShell.Plugins.FMSuite.Wave.IO;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using GeoAPI.Geometries;
using log4net.Core;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks.Constraints;
using Is = NUnit.Framework.Is;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO
{
    [TestFixture]
    public class MdwFileTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadAndWriteMdwFile()
        {            
            var mdwPath = TestHelper.GetTestFilePath(@"coordinateBasedBoundary\obw.mdw");
            const string mdwTargetPath = "obw_compare.mdw";

            var mdwFile = new MdwFile();
            var modelDef = mdwFile.Load(mdwPath);

           
            mdwFile.SaveTo(mdwTargetPath, modelDef, true);
 
            var target = new MdwFile();
            var modelDefOut = target.Load(mdwTargetPath);

            foreach (var propDef in modelDef.ModelSchema.PropertyDefinitions.Values)
            {
                var valueBefore = modelDef.GetModelProperty(propDef.FileCategoryName, propDef.FilePropertyName).Value;
                var valueAfter = modelDefOut.GetModelProperty(propDef.FileCategoryName, propDef.FilePropertyName).Value;
                Assert.AreEqual(valueBefore, valueAfter);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Load_GridsAreImportedOnDomains()
        {
            // Setup
            var mdwPath = TestHelper.GetTestFilePath(@"coordinateBasedBoundary\obw.mdw");

            // Call
            var modelDef = new MdwFile().Load(mdwPath);

            // Assert
            Assert.That(modelDef.OuterDomain.Grid.IsEmpty, Is.False);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadObstacles()
        {
            var mdwPath = TestHelper.GetTestFilePath(@"wave_spacevarbnd\tst.mdw");
            var mdwFile = new MdwFile();
            var modelDef = mdwFile.Load(mdwPath);

            var obs1 = modelDef.Obstacles[0];
            var obs2 = modelDef.Obstacles[1];
            
            Assert.AreEqual("Obstacle 1", obs1.Name);
            Assert.AreEqual(ObstacleType.Dam, obs1.Type);
            Assert.AreEqual(0, obs1.Height, 1e-05);
            Assert.AreEqual(2.5999, obs1.Alpha, 1e-03);
            Assert.AreEqual(0.15, obs1.Beta, 1e-05);
            Assert.AreEqual(0, obs1.TransmissionCoefficient, 1e-05);
            Assert.AreEqual(ReflectionType.No, obs1.ReflectionType);
            
            Assert.AreEqual("Obstacle 2", obs2.Name);
            Assert.AreEqual(ObstacleType.Sheet, obs2.Type);
            Assert.AreEqual(0, obs2.Height, 1e-05);
            Assert.AreEqual(0, obs2.Alpha, 1e-05);
            Assert.AreEqual(0, obs2.Beta, 1e-05);
            Assert.AreEqual(0.5, obs2.TransmissionCoefficient, 1e-05);
            Assert.AreEqual(ReflectionType.No, obs2.ReflectionType);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadObstaclesInvalidFile()
        {
            var mdwPath = TestHelper.GetTestFilePath(@"wave_spacevarbnd\tstInvalidObtFile.mdw");
            var mdwFile = new MdwFile();

            const string warningMessage = "Parsing error in file 'tstInvalid.obt'. Can't convert 'ThisInvalidString' to a double. The property 'Beta' has been given the default value '0'.";
            
            WaveModelDefinition modelDef = null;
            LogHelper.ConfigureLogging();
            LogHelper.SetLoggingLevel(Level.Warn);
            TestHelper.AssertLogMessageIsGenerated(
                () => modelDef = mdwFile.Load(mdwPath), warningMessage);
            LogHelper.SetLoggingLevel(Level.Error);
            LogHelper.ResetLogging();
            
            var obs1 = modelDef.Obstacles[0];
            var obs2 = modelDef.Obstacles[1];

            Assert.AreEqual("Obstacle 1", obs1.Name);
            Assert.AreEqual(ObstacleType.Dam, obs1.Type);
            Assert.AreEqual(0, obs1.Height, 1e-05);
            Assert.AreEqual(2.5999, obs1.Alpha, 1e-03);
            Assert.AreEqual(0, obs1.Beta, 1e-05); // The Beta value in the file is "ThisInvalidString" -> reading this should return the default value (0)
            Assert.AreEqual(0, obs1.TransmissionCoefficient, 1e-05);
            Assert.AreEqual(ReflectionType.No, obs1.ReflectionType);

            Assert.AreEqual("Obstacle 2", obs2.Name);
            Assert.AreEqual(ObstacleType.Sheet, obs2.Type);
            Assert.AreEqual(0, obs2.Height, 1e-05);
            Assert.AreEqual(0, obs2.Alpha, 1e-05);
            Assert.AreEqual(0, obs2.Beta, 1e-05);
            Assert.AreEqual(0.5, obs2.TransmissionCoefficient, 1e-05);
            Assert.AreEqual(ReflectionType.No, obs2.ReflectionType);
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadTimePoints()
        {
            var mdwPath = TestHelper.GetTestFilePath(@"wave_timespacevarbnd\tst.mdw");
            var mdwFile = new MdwFile();
            var modelDef = mdwFile.Load(mdwPath);

            var function = modelDef.TimePointData.InputFields;

            Assert.AreEqual(3, function.Arguments[0].Values.Count);
            Assert.AreEqual(new DateTime(2006, 1, 5), function.Arguments[0].Values[0]);
            Assert.AreEqual(0.0, function.Components[0].Values[0]);
            Assert.AreEqual(0.0, function.Components[1].Values[0]);
            Assert.AreEqual(new DateTime(2006, 1, 5).AddMinutes(60.0), function.Arguments[0].Values[1]);
            Assert.AreEqual(new DateTime(2006, 1, 5).AddMinutes(120.0), function.Arguments[0].Values[2]);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadConstantXyBoundaries()
        {
            var mdwPath = TestHelper.GetTestFilePath(@"coordinateBasedBoundary\obw.mdw");
            var mdwFile = new MdwFile();
            var modelDef = mdwFile.Load(mdwPath);

            var bc = modelDef.BoundaryConditions[0];
            Assert.AreEqual("Boundary 1", bc.Feature.Name);
            Assert.AreEqual("Boundary 1", bc.Name);

            Assert.AreEqual(BoundaryConditionDataType.ParameterizedSpectrumConstant, bc.DataType);
            Assert.AreEqual(WaveSpectrumShapeType.Jonswap, bc.ShapeType);
            Assert.AreEqual(WavePeriodType.Peak, bc.PeriodType);
            Assert.AreEqual(WaveDirectionalSpreadingType.Power, bc.DirectionalSpreadingType);
            Assert.AreEqual(3.3, bc.PeakEnhancementFactor);
            Assert.AreEqual(0.01, bc.GaussianSpreadingValue, 1e-06);

            var waveheight = bc.SpectrumParameters[0].Height;
            var period = bc.SpectrumParameters[0].Period;
            var dir = bc.SpectrumParameters[0].Direction;
            var spread = bc.SpectrumParameters[0].Spreading;
            Assert.AreEqual(2.82, waveheight, 1e-06);
            Assert.AreEqual(6.67, period, 1e-06);
            Assert.AreEqual(250.0, dir, 1e-06);
            Assert.AreEqual(4.0, spread, 1e-06);

            bc = modelDef.BoundaryConditions[1];
            Assert.AreEqual("Boundary 2", bc.Feature.Name);
            Assert.AreEqual("Boundary 2", bc.Name);

            Assert.AreEqual(BoundaryConditionDataType.ParameterizedSpectrumConstant, bc.DataType);
            Assert.AreEqual(WaveSpectrumShapeType.PiersonMoskowitz, bc.ShapeType);
            Assert.AreEqual(WavePeriodType.Mean, bc.PeriodType);
            Assert.AreEqual(WaveDirectionalSpreadingType.Degrees, bc.DirectionalSpreadingType);
            Assert.AreEqual(3.3, bc.PeakEnhancementFactor);
            Assert.AreEqual(0.01, bc.GaussianSpreadingValue, 1e-06);

            waveheight = bc.SpectrumParameters[1].Height;
            period = bc.SpectrumParameters[1].Period;
            dir = bc.SpectrumParameters[1].Direction;
            spread = bc.SpectrumParameters[1].Spreading;
            Assert.AreEqual(4.0, waveheight, 1e-06);
            Assert.AreEqual(10.0, period, 1e-06);
            Assert.AreEqual(30.0, dir, 1e-06);
            Assert.AreEqual(4.0, spread, 1e-06);

            waveheight = bc.SpectrumParameters[2].Height;
            period = bc.SpectrumParameters[2].Period;
            dir = bc.SpectrumParameters[2].Direction;
            spread = bc.SpectrumParameters[2].Spreading;
            Assert.AreEqual(10.0, waveheight, 1e-06);
            Assert.AreEqual(20.0, period, 1e-06);
            Assert.AreEqual(30.0, dir, 1e-06);
            Assert.AreEqual(4.0, spread, 1e-06);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadModelWithBcwFile()
        {
            var mdwPath = TestHelper.GetTestFilePath(@"bcwTimeseries\bcw.mdw");
            var mdwFile = new MdwFile();
            var modelDef = mdwFile.Load(mdwPath);

            var hs = modelDef.BoundaryConditions[1].GetDataAtPoint(1).Components[0].GetValues<double>();
            Assert.AreEqual(new []{2.0,2.1,1.7},hs);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadOrientedBoundaries()
        {
            var mdwPath = TestHelper.GetTestFilePath(@"wave_timespacevarbnd\tst.mdw");
            var mdwFile = new MdwFile();
            var modelDef = mdwFile.Load(mdwPath);

            Assert.AreEqual(2, modelDef.OrientedBoundaryConditions.Count);
            Assert.AreEqual("south", modelDef.OrientedBoundaryConditions[0].Feature.Attributes["orientation"]);
            Assert.AreEqual("west", modelDef.OrientedBoundaryConditions[1].Feature.Attributes["orientation"]);
            Assert.AreEqual(2, modelDef.OrientedBoundaryConditions.Count);
            Assert.AreEqual(0, modelDef.BoundaryConditions.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void SaveLoadBoundaries()
        {
            var mdwPath = TestHelper.GetTestFilePath(@"coordinateBasedBoundary\obw.mdw");
            var mdwFile = new MdwFile();
            var modelDef = mdwFile.Load(mdwPath);

            var targetPath = TestHelper.GetCurrentMethodName() + "output.mdw";
            mdwFile.SaveTo(targetPath, modelDef, true);

            var savedModelDef = mdwFile.Load(targetPath);

            Assert.AreEqual(modelDef.BoundaryConditions.Count, savedModelDef.BoundaryConditions.Count);
            Assert.AreEqual(modelDef.BoundaryConditions[0].Feature.Geometry, savedModelDef.BoundaryConditions[0].Feature.Geometry);
            Assert.AreEqual(modelDef.BoundaryConditions[0].PointData.Count, savedModelDef.BoundaryConditions[0].PointData.Count);
            for (int i = 0; i < modelDef.BoundaryConditions[0].PointData[0].Components.Count; ++i)
                Assert.AreEqual(modelDef.BoundaryConditions[0].PointData[0].Components[i].Values,
                                savedModelDef.BoundaryConditions[0].PointData[0].Components[i].Values);

            Assert.AreEqual(modelDef.BoundaryConditions[1].Feature.Geometry, savedModelDef.BoundaryConditions[1].Feature.Geometry);
            Assert.AreEqual(modelDef.BoundaryConditions[1].PointData.Count, savedModelDef.BoundaryConditions[1].PointData.Count);
            for (int i = 0; i < modelDef.BoundaryConditions[1].PointData[0].Components.Count; ++i)
                Assert.AreEqual(modelDef.BoundaryConditions[1].PointData[0].Components[i].Values,
                                savedModelDef.BoundaryConditions[1].PointData[0].Components[i].Values);

        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void SaveLoadSpectralSpacePerDomain()
        {
            var mdwPath = TestHelper.GetTestFilePath(@"domainWithSpectralData\te0.mdw");
            var mdwFile = new MdwFile();
            var modelDef = mdwFile.Load(mdwPath);

            var targetPath = TestHelper.GetCurrentMethodName() + "output.mdw";
            mdwFile.SaveTo(targetPath, modelDef, true);

            var savedModelDef = mdwFile.Load(targetPath);

            Assert.AreEqual(modelDef.OuterDomain.SpectralDomainData.NFreq, savedModelDef.OuterDomain.SpectralDomainData.NFreq);
            Assert.AreEqual(modelDef.OuterDomain.SpectralDomainData.FreqMax, savedModelDef.OuterDomain.SpectralDomainData.FreqMax, 1e-07);
            Assert.AreEqual(modelDef.OuterDomain.SpectralDomainData.FreqMin, savedModelDef.OuterDomain.SpectralDomainData.FreqMin, 1e-07);
            Assert.AreEqual(modelDef.OuterDomain.SpectralDomainData.NDir, savedModelDef.OuterDomain.SpectralDomainData.NDir);
            Assert.AreEqual(modelDef.OuterDomain.SpectralDomainData.StartDir, savedModelDef.OuterDomain.SpectralDomainData.StartDir, 1e-07);
            Assert.AreEqual(modelDef.OuterDomain.SpectralDomainData.EndDir, savedModelDef.OuterDomain.SpectralDomainData.EndDir, 1e-07);
        }

        [TestCase(WaveDirectionalSpaceType.Sector)]
        [TestCase(WaveDirectionalSpaceType.Circle)]
        [Category(TestCategory.DataAccess)]
        public void Load_ThenCorrectSpectralDomainDataIsSet(WaveDirectionalSpaceType directionalSpaceType)
        {
            var random = new Random();
            var expectedDomainData = new SpectralDomainData()
            {
                DirectionalSpaceType = directionalSpaceType,
                NDir = random.Next(),
                StartDir = GetRandomRoundedValue(random),
                EndDir = GetRandomRoundedValue(random),
                NFreq = random.Next(),
                FreqMin = GetRandomRoundedValue(random),
                FreqMax = GetRandomRoundedValue(random),
            };

            // Setup
            using (var tempDirectory = new TemporaryDirectory())
            {
                string filePath = CreateMdwFileWithSpectralDomainData(tempDirectory.Path, expectedDomainData);

                // Call
                WaveModelDefinition modelDefinition = new MdwFile().Load(filePath);

                // Assert
                SpectralDomainData spectralData = modelDefinition.OuterDomain.SpectralDomainData;
                Assert.That(spectralData.DirectionalSpaceType, Is.EqualTo(directionalSpaceType), "Directional space type");
                Assert.That(spectralData.NDir, Is.EqualTo(expectedDomainData.NDir), "NDir");
                Assert.That(spectralData.StartDir, Is.EqualTo(expectedDomainData.StartDir), "StartDir");
                Assert.That(spectralData.EndDir, Is.EqualTo(expectedDomainData.EndDir), "EndDir");
                Assert.That(spectralData.FreqMin, Is.EqualTo(expectedDomainData.FreqMin), "FreqMin");
                Assert.That(spectralData.FreqMax, Is.EqualTo(expectedDomainData.FreqMax), "FreqMax");
                Assert.That(spectralData.NFreq, Is.EqualTo(expectedDomainData.NFreq), "NFreq");
            }
        }

        private static string CreateMdwFileWithSpectralDomainData(string tempDirPath, SpectralDomainData domainData)
        {
            string filePath = Path.Combine(tempDirPath, "file.mdw");
            string directionalSpaceTypeValue = domainData.DirectionalSpaceType == WaveDirectionalSpaceType.Circle
                                                   ? "circle"
                                                   : "sector";

            string[] content =
            {
                "[Domain]",
                $"DirSpace  = {directionalSpaceTypeValue}",
                $"NDir      = {domainData.NDir}",
                $"StartDir  = {ToDoubleString(domainData.StartDir)}",
                $"EndDir    = {ToDoubleString(domainData.EndDir)}",
                $"FreqMin   = {ToDoubleString(domainData.FreqMin)}",
                $"FreqMax   = {ToDoubleString(domainData.FreqMax)}",
                $"NFreq     = {domainData.NFreq}",
                "[Output]",
                "[General]"
            };

            File.WriteAllLines(filePath, content);

            return filePath;
        }

        private static string ToDoubleString(double value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static double GetRandomRoundedValue(Random random)
        {
            const int factor = 10000000;
            return Math.Floor(random.NextDouble() * factor) / factor;
        }

        /// <summary>
        /// Load a boundary condition that has a uniform boundary with a timeseries.
        /// The support points don't contain data, so the data will be set in the first data point.
        /// </summary>
        [Test]// TOOLS-20998
        public void LoadUniformBoundaryWithTimeseries()
        {
            string mdwfilepath = TestHelper.GetTestFilePath(@"uniformBoundaryWithTimeseries\bcw.mdw");
            var mdwFile = new MdwFile();
            var modelDef = LoadUniformBoundaryWithTimeseriesFileMdwFile(mdwFile, mdwfilepath);

            // get the data of the first point and check that there is data
            var uniformFunc = modelDef.BoundaryConditions[1].GetDataAtPoint(0);
            Assert.IsNotNull(uniformFunc);
        }

        /// <summary>
        /// Save a boundary condition that has a uniform boundary with a timeseries.
        /// </summary>
        [Test]
        public void SaveUniformBoundaryWithTimeseries()
        {
            string mdwfilepath = TestHelper.GetTestFilePath(@"uniformBoundaryWithTimeseries\bcw.mdw");
            var mdwFile = new MdwFile();
            var modelDef = LoadUniformBoundaryWithTimeseriesFileMdwFile(mdwFile, mdwfilepath);

            // get the data of the first point and check that there is data
            var uniformFunc = modelDef.BoundaryConditions[1].GetDataAtPoint(0);
            Assert.IsNotNull(uniformFunc);

            // save the model definition back to a file
            var targetPath = TestHelper.GetCurrentMethodName() + "output.mdw";
            mdwFile.SaveTo(targetPath, modelDef, true);

            // load it back in
            var savedModelDef = LoadUniformBoundaryWithTimeseriesFileMdwFile(mdwFile, targetPath);

            // test that 
            Assert.AreEqual(uniformFunc.GetValues(), savedModelDef.BoundaryConditions[1].GetDataAtPoint(0).GetValues());
        }

        /// <summary>
        /// Helper function for test <see cref="SaveUniformBoundaryWithTimeseries"/> and 
        /// <see cref="LoadUniformBoundaryWithTimeseries"/>.
        /// </summary>
        /// <param name="mdwFile">The mdwfile to use for loading the WaveModelDefinition.</param>
        /// <param name="mdwfilepath">The file path of the mdw file to load.</param>
        /// <returns>The WaveModelDefinition coming from the mdw file.</returns>
        private static WaveModelDefinition LoadUniformBoundaryWithTimeseriesFileMdwFile(MdwFile mdwFile, string mdwfilepath)
        {
            // load a file with uniform boundary conditions with a timeseries.
            WaveModelDefinition modelDef = mdwFile.Load(mdwfilepath);

            // test that the geometry of the boundary just contains 2 points
            // and is uniform and a timeseries
            Assert.AreEqual(modelDef.BoundaryConditions[1].Feature.Geometry.Coordinates.Length, 2);
            Assert.AreEqual(modelDef.BoundaryConditions[1].SpatialDefinitionType,
                            WaveBoundaryConditionSpatialDefinitionType.Uniform);
            Assert.AreEqual(modelDef.BoundaryConditions[1].DataType, BoundaryConditionDataType.ParameterizedSpectrumTimeseries);
            return modelDef;
        }

        /// <summary>
        /// Load a boundary condition that doesn't contain information at the first point.
        /// Add a function to that point.
        /// Save it and check that the values are written in sorted order.
        /// </summary>
        [Test]
        public void CreateBoundaryInWrongOrderAndSave()
        {
            var mdwfilepath = TestHelper.GetTestFilePath(@"bcwTimeseriesNotOnFirstAndLast\bcw.mdw");
            var mdwFile = new MdwFile();
            var modelDef = mdwFile.Load(mdwfilepath);

            // check that the data doesn't contain information at datapoint 0
            Assert.IsNull(modelDef.BoundaryConditions[0].GetDataAtPoint(0));

            // check that there is other data in the boundarycondition
            Assert.IsNotNull(modelDef.BoundaryConditions[0].GetDataAtPoint(1));


            modelDef.BoundaryConditions[0].AddPoint(0);
            var f = modelDef.BoundaryConditions[0].GetDataAtPoint(0);
            f[new DateTime()] = new[] {1, 1, 1, 1};

            // save the model
            string targetPath = TestHelper.GetCurrentMethodName() + "output.mdw";
            mdwFile.SaveTo(targetPath, modelDef, true);

            // load the model back from disk
            WaveModelDefinition savedModelDef = mdwFile.Load(targetPath);

            // TODO: Check if I can do equality check in func
            Assert.AreEqual(f.GetValues<double>(), savedModelDef.BoundaryConditions[0].GetDataAtPoint(0).GetValues<double>());

            // TODO: somehow assert that the lines are ok. You cannot check that here in code, but we really have to check the file.
        }

        /// <summary>
        /// Test added for jira issue: DELFT3DFM-33
        /// </summary>
        [Test]
        public void SaveMdwFile_SpatiallyVaryingBoundaryConditionWithNoDataDefaultsToUniform()
        {
            var outputPath = WaveTestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(@"wave_spacevarbnd\DELFT3DFM-33.mdw"));
            var mdwPath = TestHelper.GetTestFilePath(@"wave_spacevarbnd\tst.mdw");
            var mdwFile = new MdwFile();
            var modelDefOut = mdwFile.Load(mdwPath);

            modelDefOut.BoundaryConditions.Clear();
            modelDefOut.BoundaryConditions.Add(new WaveBoundaryCondition(BoundaryConditionDataType.Constant)
            {
                Name = "BoundaryCondition01",
                SpatialDefinitionType = WaveBoundaryConditionSpatialDefinitionType.SpatiallyVarying,
                Feature = new Feature2D()
                {
                    Name = "BoundaryCondition01",
                    Id = 0,
                    Geometry = new LineString(new []
                    {
                        new Coordinate(100.0, 100.0), 
                        new Coordinate(500.0, 100.0)
                    })
                }
            });

            if (File.Exists(outputPath)) File.Delete(outputPath);
            mdwFile.SaveTo(outputPath, modelDefOut, false);
            var modelDefIn = mdwFile.Load(outputPath);

            Assert.AreEqual(1, modelDefIn.BoundaryConditions.Count);
            Assert.AreEqual(WaveBoundaryConditionSpatialDefinitionType.Uniform,
                modelDefIn.BoundaryConditions[0].SpatialDefinitionType, 
                "WaveBoundaryCondition of Spatially Varying DefinitionType with no data should default to Uniform on MdwFile save");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAModelWithABoundaryCondition_WhenSaved_ThenBcwFileIsReferencedInMdwFile()
        {
            var tempDirPath = FileUtils.CreateTempDirectory();
            var tempProjectFilePath = Path.Combine(tempDirPath, "Project.dsproj");

            try
            {
                using (var app = GetConfiguredApplication(tempProjectFilePath))
                {
                    using (var model = new WaveModel())
                    {
                        var project = app.Project;
                        project.RootFolder.Add(model);

                        var boundary = new Feature2D
                        {
                            Geometry =
                                new LineString(new[]
                                    {new Coordinate(0, 0), new Coordinate(1, 0)}),
                            Name = "boundary"
                        };

                        var boundaryConditionFactory = new WaveBoundaryConditionFactory();
                        var boundaryCondition = boundaryConditionFactory.CreateBoundaryCondition(boundary, "",
                            BoundaryConditionDataType.ParameterizedSpectrumTimeseries);

                        var refTime = model.ModelDefinition.ModelReferenceDateTime;
                        boundaryCondition.DataPointIndices.Add(1);
                        boundaryCondition.PointData[0].Arguments[0]
                            .SetValues(new[] { refTime, refTime.AddDays(1) });

                        model.Boundaries.Add(boundary);
                        model.BoundaryConditions.Add((WaveBoundaryCondition)boundaryCondition);

                        Assert.AreEqual(string.Empty, model.ModelDefinition.GetModelProperty(KnownWaveCategories.GeneralCategory,KnownWaveProperties.TimeSeriesFile).GetValueAsString());

                        //During a save modeldefinition properties are also updated
                        app.SaveProject();

                        Assert.AreEqual("Waves.bcw", model.ModelDefinition.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.TimeSeriesFile).GetValueAsString());

                        var mdwFilePath = model.MdwFilePath;
                        IList<DelftIniCategory> categories;
                        using (var fileStream = new FileStream(mdwFilePath, FileMode.Open, FileAccess.Read))
                        {
                            categories = new DelftIniReader().ReadDelftIniFile(fileStream, mdwFilePath);
                        }

                        var tSeriesFilePropValue = categories
                            .FirstOrDefault(c => c.Name == KnownWaveCategories.GeneralCategory)
                            .GetPropertyValue(KnownWaveProperties.TimeSeriesFile);

                        Assert.AreEqual(model.Name + ".bcw", tSeriesFilePropValue);

                        app.CloseProject();

                    }
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(tempDirPath);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAModelWithABoundaryCondition_WhenSavedAndLoaded_BoundaryConditionIsCorrectlyLoaded()
        {
            var tempDirPath = FileUtils.CreateTempDirectory();
            var tempProjectFilePath = Path.Combine(tempDirPath, "Project.dsproj");

            var mdwFilePath = string.Empty;

            try
            {
                using (var app = GetConfiguredApplication(tempProjectFilePath))
                {
                    using (var model = new WaveModel())
                    {
                        var project = app.Project;
                        project.RootFolder.Add(model);

                        var boundary = new Feature2D
                        {
                            Geometry =
                                new LineString(new[]
                                    {new Coordinate(0, 0), new Coordinate(1, 0)}),
                            Name = "boundary"
                        };

                        var boundaryConditionFactory = new WaveBoundaryConditionFactory();
                        var boundaryCondition = boundaryConditionFactory.CreateBoundaryCondition(boundary, "",
                            BoundaryConditionDataType.ParameterizedSpectrumTimeseries);

                        var refTime = model.ModelDefinition.ModelReferenceDateTime;
                        boundaryCondition.DataPointIndices.Add(1);
                        boundaryCondition.PointData[0].Arguments[0]
                            .SetValues(new[] { refTime, refTime.AddDays(1) });

                        model.Boundaries.Add(boundary);
                        model.BoundaryConditions.Add((WaveBoundaryCondition)boundaryCondition);
                     
                        app.SaveProject();

                        mdwFilePath = model.MdwFilePath;

                        app.CloseProject();

                    }

                    using (var model = new WaveModel(mdwFilePath)) { 
                                   
                        var boundaries = model.Boundaries;
                        var boundaryConditions = model.BoundaryConditions;

                        Assert.IsNotNull(boundaryConditions);
                        Assert.IsNotNull(boundaries);
                    }
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(tempDirPath);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAMdwFileWithObstacleFile_WhenImportedAndObstaclesRemoved_ThenObstacleFileShouldBeRemovedFromTheModeldefinitionProperties()
        {

            var mdwFile = new MdwFile();
            var importedMdwFilePath = TestHelper.GetTestFilePath(@"wad\wad.mdw");
            var modelDef = mdwFile.Load(importedMdwFilePath);
            var targetPath = TestHelper.GetCurrentMethodName() + "output.mdw";

            Assert.AreEqual("wad.obt",
                modelDef.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.ObstacleFile)
                    .GetValueAsString());

            modelDef.Obstacles.Clear();

            //Modeldefinition properties updated during save
            mdwFile.SaveTo(targetPath, modelDef, true);

            Assert.AreEqual(string.Empty,
                modelDef.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.ObstacleFile)
                    .GetValueAsString());

            //Verify what was really written in the file
            var modelDef2 = mdwFile.Load(targetPath);

            Assert.AreEqual(string.Empty,
                modelDef2.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.ObstacleFile)
                    .GetValueAsString());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAMdwFileWithConstantWind_WhenImportedAndChangedToTimeseries_ThenZerosShouldBeWrittenForWindSpeedAndDirectionInTheModelDefinitionProperties()
        {

            var mdwFile = new MdwFile();
            var importedMdwFilePath = TestHelper.GetTestFilePath(@"wad\wad.mdw");
            var modelDef = mdwFile.Load(importedMdwFilePath);
            var targetPath = TestHelper.GetCurrentMethodName() + "output.mdw";

            Assert.AreEqual("10",
                modelDef.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WindSpeed)
                    .GetValueAsString());

            Assert.AreEqual("315",
                modelDef.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WindDirection)
                    .GetValueAsString());

            modelDef.TimePointData.WindDataType = InputFieldDataType.TimeVarying;
            
            //Modeldefinition properties updated during save
            mdwFile.SaveTo(targetPath, modelDef, true);

            Assert.AreEqual("0",
                modelDef.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WindSpeed)
                    .GetValueAsString());

            Assert.AreEqual("0",
                modelDef.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WindDirection)
                    .GetValueAsString());

            //Verify what was really written in the file
            var modelDef2 = mdwFile.Load(targetPath);

            Assert.AreEqual("0",
                modelDef2.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WindSpeed)
                    .GetValueAsString());

            Assert.AreEqual("0",
                modelDef2.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WindDirection)
                    .GetValueAsString());
        }
        
        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAWaveModelWithConstantHydronamics_WhenChangedToTimeseries_ThenZerosShouldBeWrittenForConstantWaterLevelVelocityXAndVelocityYInTheModelDefinitionProperties()
        {

            var mdwFile = new MdwFile();
            var modelDef = new WaveModelDefinition()
            {
                OuterDomain = new WaveDomainData("Outer")
            };
            var targetPath = TestHelper.GetCurrentMethodName() + "output.mdw";

            modelDef.TimePointData.HydroDataType = InputFieldDataType.Constant;
            modelDef.TimePointData.WaterLevelConstant = 6;
            modelDef.TimePointData.VelocityXConstant = 6;
            modelDef.TimePointData.VelocityYConstant = 6;

            // update modeldefintion properties
            mdwFile.SaveTo(targetPath, modelDef, true);

            Assert.AreEqual("6",
                modelDef.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WaterLevel)
                    .GetValueAsString());

            Assert.AreEqual("6",
                modelDef.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WaterVelocityX)
                    .GetValueAsString());

            Assert.AreEqual("6",
                modelDef.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WaterVelocityX)
                    .GetValueAsString());

            modelDef.TimePointData.HydroDataType = InputFieldDataType.TimeVarying;

            mdwFile.SaveTo(targetPath, modelDef, true);

            Assert.AreEqual("0",
                modelDef.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WaterLevel)
                    .GetValueAsString());

            Assert.AreEqual("0",
                modelDef.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WaterVelocityX)
                    .GetValueAsString());

            Assert.AreEqual("0",
                modelDef.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WaterVelocityX)
                    .GetValueAsString());

            //Verify what was really written in the file
            var modelDef2 = mdwFile.Load(targetPath);

            Assert.AreEqual("0",
                modelDef2.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WaterLevel)
                    .GetValueAsString());

            Assert.AreEqual("0",
                modelDef2.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WaterVelocityX)
                    .GetValueAsString());

            Assert.AreEqual("0",
                modelDef2.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WaterVelocityX)
                    .GetValueAsString());
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAWaveModelWithObservationPoints_WhenImportedAndObservationsPointsRemoved_ThenLocationFileWithObservationPointsShouldBeRemovedFromTheModelDefinitionProperties()
        {
            var mdwFile = new MdwFile();
            var importedMdwFilePath = TestHelper.GetTestFilePath(@"wad\wad.mdw");
            var modelDef = mdwFile.Load(importedMdwFilePath);
            var targetPath = TestHelper.GetCurrentMethodName() + "output.mdw";

            Assert.AreEqual("wad.loc",
                modelDef.GetModelProperty(KnownWaveCategories.OutputCategory, KnownWaveProperties.LocationFile)
                    .GetValueAsString());

            modelDef.ObservationPoints.Clear();

            //Modeldefinition properties updated during save
            mdwFile.SaveTo(targetPath, modelDef, true);

            Assert.AreEqual(string.Empty,
                modelDef.GetModelProperty(KnownWaveCategories.OutputCategory, KnownWaveProperties.LocationFile)
                    .GetValueAsString());

            //Verify what was really written in the file
            var modelDef2 = mdwFile.Load(targetPath);

            Assert.AreEqual(string.Empty, 
                modelDef2.GetModelProperty(KnownWaveCategories.OutputCategory, KnownWaveProperties.LocationFile)
                    .GetValueAsString());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAMdwFileWithMissingBedFrictionCoefAndMaxIter_WhenImportingThisModel_ThenTheCorrectDefaultValuesShouldBeSetBasedOnBedFrictionAndSimMode()
        {
            var mdwFile = new MdwFile();
            var importedMdwFilePath = TestHelper.GetTestFilePath(@"ModelWithMissingMultipleDefaultValues\Waves.mdw");

            List<string> expectedMessages= new List<string>();
            expectedMessages.Add(
                "In the MDW file the property BedFricCoef is missing. Based on property BedFriction the default value is set");
            expectedMessages.Add(
                "In the MDW file the property MaxIter is missing. Based on property SimMode the default value is set");

            IEnumerable<string> messages = expectedMessages;

            TestHelper.AssertLogMessagesAreGenerated(() =>
                {
                    var modelDefinition = mdwFile.Load(importedMdwFilePath);
                    var propertyBedFrictionCoef = modelDefinition.GetModelProperty(KnownWaveCategories.ProcessesCategory,
                        KnownWaveProperties.BedFrictionCoef);
                    Assert.IsNotNull(propertyBedFrictionCoef);
                    Assert.AreEqual("0.05", propertyBedFrictionCoef.GetValueAsString());

                    var propertyMaxIter = modelDefinition.GetModelProperty(KnownWaveCategories.NumericsCategory,
                        KnownWaveProperties.MaxIter);
                    Assert.IsNotNull(propertyMaxIter);
                    Assert.AreEqual("15", propertyMaxIter.GetValueAsString());
                }
                ,messages);
        }
        private DeltaShellApplication GetConfiguredApplication(string savePath)
        {
            var app = new DeltaShellApplication();
            app.IsProjectCreatedInTemporaryDirectory = true;
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            app.Run();
            app.SaveProjectAs(Path.Combine(savePath));
            return app;
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void SaveTo_WithCommunicationFilePathWithBackSlashFileSeparators_ThenFilePathIsExportedWithForwardSlashFileSeparators()
        {
            // Arrange
            using (var temporaryDirectory = new TemporaryDirectory())
            {
                const string comFilePath = @"myDir1\myDir2\myComFile_com.nc";
                var modelDefinition = new WaveModelDefinition
                {
                    CommunicationsFilePath = comFilePath
                };

                // Act
                var mdwFile = new MdwFile();
                string mdwFilePath = Path.Combine(temporaryDirectory.Path, "myModel.mdw");
                mdwFile.SaveTo(mdwFilePath, modelDefinition, false);

                // Assert
                IEnumerable<string> mdwFileLines = File.ReadLines(mdwFilePath);
                string comFileLine = mdwFileLines.Single(line => line.Trim().StartsWith(KnownWaveProperties.COMFile));

                string exportedComFilePath = comFileLine.Split('=')[1].Trim();
                Assert.That(exportedComFilePath, Is.EqualTo(comFilePath.Replace('\\','/')));
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void Load_LegacyPropertiesAreReplaced()
        {
            // Setup
            using (var temporaryDirectory = new TemporaryDirectory())
            {
                string legacyFile = temporaryDirectory.CopyTestDataFileToTempDirectory("MdwFile\\TScaleLegacy.mdw");

                var mdwFile = new MdwFile();

                // Call
                WaveModelDefinition result = null; 

                void Call() => result = mdwFile.Load(legacyFile);

                List<string> logMessages =
                    TestHelper.GetAllRenderedMessages(Call, Level.Warn).ToList();


                // Assert
                WaveModelProperty timeFrameProperty =
                    result.Properties.FirstOrDefault(x => x.PropertyDefinition.FilePropertyName == "TimeInterval");

                Assert.That(timeFrameProperty, Is.Not.Null, "Expected the TimeFrame property to be found.");
                Assert.That(timeFrameProperty.PropertyDefinition.Category, Is.EqualTo("General"));

                var value = (double) timeFrameProperty.Value;
                Assert.That(value, Is.EqualTo(255.0));

                Assert.That(result.Properties.Any(x => x.PropertyDefinition.FilePropertyName == "TScale"), 
                            Is.False,
                            "Expected no property with the file name TScale");

                string expectedMsg = string.Format(
                    Resources.DelftIniBackwardsCompatibilityHelper_GetUpdatedName_Backwards_Compatibility____0___has_been_updated_to___1__,
                    "TScale", "TimeInterval");
                Assert.That(logMessages.Any(x => x.Contains(expectedMsg)), Is.True, "Expected a warning messages logged.");
            }
        }
    }
}
