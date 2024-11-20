using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.Common.IO;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess
{
    [TestFixture]
    public class MdwBoundarySectionsCreatorTest
    {
        private const double factor = 3.3;

        private const BoundaryConditionPeriodType periodType = BoundaryConditionPeriodType.Peak;
        private static readonly Random random = new Random();
        private readonly JonswapShape jonswapShape = new JonswapShape {PeakEnhancementFactor = factor};

        [Test]
        public void CreateSections_ShouldCreateACompleteSectionForOneBoundary()
        {
            var boundaryContainer = Substitute.For<IBoundaryContainer>();

            // Boundary
            IWaveBoundary boundary1 = CreateWaveBoundary(out SupportPoint supportPoint1, out SupportPoint supportPoint2);
            const string boundaryName = "boundary1";
            boundary1.Name = boundaryName;

            // Add boundary to container
            var boundaries = new EventedList<IWaveBoundary> {boundary1};
            boundaryContainer.Boundaries.Returns(boundaries);

            // Setup boundary container For boundary
            var boundarySnappingCalculator = Substitute.For<IBoundarySnappingCalculator>();
            boundaryContainer.GetBoundarySnappingCalculator().Returns(boundarySnappingCalculator);
            SetupBoundaryContainerForBoundary(boundarySnappingCalculator, supportPoint1, supportPoint2, out Coordinate coordinate1, out Coordinate coordinate2);

            // Act
            IEnumerable<IniSection> sections = MdwBoundarySectionsCreator.CreateSections(boundaryContainer, Substitute.For<IFilesManager>());

            // Assert
            IniSection createdSection = sections.Single();
            List<IniProperty> properties = createdSection.Properties.ToList();

            CheckCreatedSection(properties, boundaryName, coordinate1, coordinate2);
        }

        [Test]
        public void CreateSections_ShouldCreateSectionsForAllBoundaries()
        {
            var boundaryContainer = Substitute.For<IBoundaryContainer>();

            // Create boundaries
            IWaveBoundary boundary1 = CreateWaveBoundary(out SupportPoint boundary1SupportPoint1, out SupportPoint boundary1SupportPoint2);
            const string boundary1Name = "boundary1";
            boundary1.Name = boundary1Name;
            IWaveBoundary boundary2 = CreateWaveBoundary(out SupportPoint boundary2SupportPoint1, out SupportPoint boundary2SupportPoint2);
            const string boundary2Name = "boundary2";
            boundary2.Name = boundary2Name;

            // Add Boundaries to container
            var boundaries = new EventedList<IWaveBoundary>
            {
                boundary1,
                boundary2
            };
            boundaryContainer.Boundaries.Returns(boundaries);

            // Setup boundary container for boundaries
            var boundarySnappingCalculator = Substitute.For<IBoundarySnappingCalculator>();
            boundaryContainer.GetBoundarySnappingCalculator().Returns(boundarySnappingCalculator);
            SetupBoundaryContainerForBoundary(boundarySnappingCalculator, boundary1SupportPoint1, boundary1SupportPoint2,
                                              out Coordinate boundary1Coordinate1, out Coordinate boundary1Coordinate2);
            SetupBoundaryContainerForBoundary(boundarySnappingCalculator, boundary2SupportPoint1, boundary2SupportPoint2,
                                              out Coordinate boundary2Coordinate1, out Coordinate boundary2Coordinate2);

            // Act
            List<IniSection> sections = MdwBoundarySectionsCreator.CreateSections(boundaryContainer, Substitute.For<IFilesManager>()).ToList();

            // Assert
            Assert.AreEqual(2, sections.Count);
            IniSection section1 = sections.First();
            IniSection section2 = sections.Last();

            CheckCreatedSection(section1.Properties.ToList(), boundary1Name, boundary1Coordinate1, boundary1Coordinate2);
            CheckCreatedSection(section2.Properties.ToList(), boundary2Name, boundary2Coordinate1, boundary2Coordinate2);
        }

        [Test]
        public void CreateSections_WithUniformFileBasedData_CreatesCorrectSection()
        {
            // Setup
            const string name = "boundary_name";
            double startX = random.NextDouble();
            double startY = random.NextDouble();
            double endX = random.NextDouble();
            double endY = random.NextDouble();
            const string fileName = "some_file.txt";
            var filePath = $"D:\\some_directory\\{fileName}";

            var boundaryContainer = Substitute.For<IBoundaryContainer>();
            IWaveBoundary boundary = WaveBoundaryBuilder.Create(name, boundaryContainer, 0, 10)
                                                        .WithCoordinates(startX, startY, endX, endY)
                                                        .WithUniformFileBasedData(filePath)
                                                        .Finish();
            boundaryContainer.Boundaries.Returns(new EventedList<IWaveBoundary> {boundary});

            var filesManager = Substitute.For<IFilesManager>();

            // Call
            List<IniSection> sections = MdwBoundarySectionsCreator.CreateSections(boundaryContainer, filesManager).ToList();

            // Assert
            IniSection section = sections.Single();
            IniProperty[] properties = section.Properties.ToArray();
            Assert.That(properties, Has.Length.EqualTo(8));
            AssertProperty(properties[0], KnownWaveProperties.Name, name);
            AssertProperty(properties[1], KnownWaveProperties.Definition, "xy-coordinates");
            AssertSpatialProperty(properties[2], KnownWaveProperties.StartCoordinateX, startX);
            AssertSpatialProperty(properties[3], KnownWaveProperties.EndCoordinateX, endX);
            AssertSpatialProperty(properties[4], KnownWaveProperties.StartCoordinateY, startY);
            AssertSpatialProperty(properties[5], KnownWaveProperties.EndCoordinateY, endY);
            AssertProperty(properties[6], KnownWaveProperties.SpectrumSpec, "from file");
            AssertProperty(properties[7], KnownWaveProperties.Spectrum, fileName);

            FileBasedParameters parameters = ((UniformDataComponent<FileBasedParameters>)
                                                 boundary.ConditionDefinition.DataComponent).Data;
            filesManager.Received(1).Add(filePath, Arg.Is<Action<string>>(a => MatchesAction(parameters, a)));
        }

        [Test]
        public void CreateSections_WithSpatiallyVaryingFileBasedData_CreatesCorrectSection()
        {
            // Setup
            const string name = "boundary_name";
            double startX = random.NextDouble();
            double startY = random.NextDouble();
            double endX = random.NextDouble();
            double endY = random.NextDouble();

            const int distance1 = 0;
            const string fileName1 = "file_1.txt";
            var filePath1 = $"D:\\some_directory\\{fileName1}";

            const int distance2 = 5;
            const string fileName2 = "file_2.txt";
            var filePath2 = $"D:\\some_directory\\{fileName2}";

            const int distance3 = 10;
            const string fileName3 = "file_3.txt";
            var filePath3 = $"D:\\some_directory\\{fileName3}";

            var boundaryContainer = Substitute.For<IBoundaryContainer>();
            IWaveBoundary boundary = WaveBoundaryBuilder.Create(name, boundaryContainer, distance1, distance2, distance3)
                                                        .WithCoordinates(startX, startY, endX, endY)
                                                        .WithSpatiallyVaryingFileBasedData(filePath1, filePath2, filePath3)
                                                        .Finish();
            boundaryContainer.Boundaries.Returns(new EventedList<IWaveBoundary> {boundary});

            var filesManager = Substitute.For<IFilesManager>();

            // Call
            List<IniSection> sections = MdwBoundarySectionsCreator.CreateSections(boundaryContainer, filesManager).ToList();

            // Assert
            IniSection section = sections.Single();
            IniProperty[] properties = section.Properties.ToArray();
            Assert.That(properties, Has.Length.EqualTo(13));
            AssertProperty(properties[0], KnownWaveProperties.Name, name);
            AssertProperty(properties[1], KnownWaveProperties.Definition, "xy-coordinates");
            AssertSpatialProperty(properties[2], KnownWaveProperties.StartCoordinateX, startX);
            AssertSpatialProperty(properties[3], KnownWaveProperties.EndCoordinateX, endX);
            AssertSpatialProperty(properties[4], KnownWaveProperties.StartCoordinateY, startY);
            AssertSpatialProperty(properties[5], KnownWaveProperties.EndCoordinateY, endY);
            AssertProperty(properties[6], KnownWaveProperties.SpectrumSpec, "from file");
            AssertSpatialProperty(properties[7], KnownWaveProperties.CondSpecAtDist, distance1);
            AssertProperty(properties[8], KnownWaveProperties.Spectrum, fileName1);
            AssertSpatialProperty(properties[9], KnownWaveProperties.CondSpecAtDist, distance2);
            AssertProperty(properties[10], KnownWaveProperties.Spectrum, fileName2);
            AssertSpatialProperty(properties[11], KnownWaveProperties.CondSpecAtDist, distance3);
            AssertProperty(properties[12], KnownWaveProperties.Spectrum, fileName3);

            List<FileBasedParameters> parameters = GetFileBasedParameters(boundary);
            filesManager.Received(1).Add(filePath1, Arg.Is<Action<string>>(a => MatchesAction(parameters[0], a)));
            filesManager.Received(1).Add(filePath2, Arg.Is<Action<string>>(a => MatchesAction(parameters[1], a)));
            filesManager.Received(1).Add(filePath3, Arg.Is<Action<string>>(a => MatchesAction(parameters[2], a)));
        }

        [Test]
        public void CreateSections_WithFromSpectrumFileDefinedBoundaries_CreatesCorrectSection()
        {
            // Setup
            const string fileName = "file.txt";
            var filePath = $"D:\\some_directory\\{fileName}";

            var boundaryContainer = Substitute.For<IBoundaryContainer>();
            boundaryContainer.DefinitionPerFileUsed = true;
            boundaryContainer.FilePathForBoundariesPerFile = filePath;

            var filesManager = Substitute.For<IFilesManager>();

            // Call
            IEnumerable<IniSection> sections = MdwBoundarySectionsCreator.CreateSections(boundaryContainer, filesManager);

            // Assert
            IniSection section = sections.Single();
            IniProperty[] properties = section.Properties.ToArray();
            Assert.That(properties, Has.Length.EqualTo(2));
            AssertProperty(properties[0], KnownWaveProperties.Definition, "fromsp2file");
            AssertProperty(properties[1], KnownWaveProperties.OverallSpecFile, fileName);

            filesManager.Received(1).Add(filePath, Arg.Is<Action<string>>(a => MatchesAction(boundaryContainer, a)));
        }

        [Test]
        public void CreateSections_WithFromSpectrumFileDefinedBoundaries_WithEmptyFilePath_CreatesCorrectSection()
        {
            // Setup
            var boundaryContainer = Substitute.For<IBoundaryContainer>();
            boundaryContainer.DefinitionPerFileUsed = true;
            boundaryContainer.FilePathForBoundariesPerFile = string.Empty;

            var filesManager = Substitute.For<IFilesManager>();

            // Call
            IEnumerable<IniSection> sections = MdwBoundarySectionsCreator.CreateSections(boundaryContainer, filesManager);

            // Assert
            IniSection section = sections.Single();
            IniProperty[] properties = section.Properties.ToArray();
            Assert.That(properties, Has.Length.EqualTo(2));
            AssertProperty(properties[0], KnownWaveProperties.Definition, "fromsp2file");
            AssertProperty(properties[1], KnownWaveProperties.OverallSpecFile, " ");

            filesManager.DidNotReceiveWithAnyArgs().Add(null, null);
        }

        private static IEnumerable<TestCaseData> ConstructorArgumentNullCases()
        {
            yield return new TestCaseData(null, Substitute.For<IFilesManager>(), "boundaryContainer");
            yield return new TestCaseData(Substitute.For<IBoundaryContainer>(), null, "filesManager");
        }

        [TestCaseSource(nameof(ConstructorArgumentNullCases))]
        public void CreateSections_ArgumentNull_ThrowsArgumentNullException(IBoundaryContainer boundaryContainer,
                                                                            IFilesManager filesManager,
                                                                            string expectedParamName)
        {
            // Act
            void Call() => MdwBoundarySectionsCreator.CreateSections(boundaryContainer, filesManager).ToList();

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParamName));
        }

        private static List<FileBasedParameters> GetFileBasedParameters(IWaveBoundary boundary)
        {
            return ((SpatiallyVaryingDataComponent<FileBasedParameters>)
                       boundary.ConditionDefinition.DataComponent)
                   .Data.Select(d => d.Value).ToList();
        }

        private IWaveBoundary CreateWaveBoundary(out SupportPoint supportPoint1, out SupportPoint supportPoint2)
        {
            var boundary = Substitute.For<IWaveBoundary>();

            // Geometry definition for boundary
            var geometryDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            supportPoint1 = new SupportPoint(0, geometryDefinition);
            supportPoint2 = new SupportPoint(33, geometryDefinition);
            var supportPoints = new EventedList<SupportPoint>
            {
                supportPoint2,
                supportPoint1
            };
            boundary.GeometricDefinition.SupportPoints.Returns(supportPoints);

            // Condition definition for boundary
            var dataComponent = new UniformDataComponent<TimeDependentParameters<PowerDefinedSpreading>>(
                new TimeDependentParameters<PowerDefinedSpreading>(
                    Substitute.For<IWaveEnergyFunction<PowerDefinedSpreading>>()));
            var conditionDefinition = new WaveBoundaryConditionDefinition(jonswapShape, periodType, dataComponent);
            boundary.ConditionDefinition.Returns(conditionDefinition);
            return boundary;
        }

        private static void SetupBoundaryContainerForBoundary(IBoundarySnappingCalculator boundarySnappingCalculator, SupportPoint supportPoint1,
                                                              SupportPoint supportPoint2, out Coordinate coordinate1,
                                                              out Coordinate coordinate2)
        {
            coordinate1 = Substitute.For<Coordinate>();
            coordinate1.X = random.NextDouble();
            coordinate1.Y = random.NextDouble();
            coordinate2 = Substitute.For<Coordinate>();
            coordinate2.X = random.NextDouble();
            coordinate2.Y = random.NextDouble();
            boundarySnappingCalculator.CalculateCoordinateFromSupportPoint(supportPoint1).Returns(coordinate1);
            boundarySnappingCalculator.CalculateCoordinateFromSupportPoint(supportPoint2).Returns(coordinate2);
        }

        private static void CheckCreatedSection(List<IniProperty> properties, string boundary1Name, Coordinate coordinate1,
                                                Coordinate coordinate2)
        {
            Assert.AreEqual(11, properties.Count);
            AssertProperty(properties[0], KnownWaveProperties.Name, boundary1Name);
            AssertProperty(properties[1], KnownWaveProperties.Definition, "xy-coordinates");
            AssertSpatialProperty(properties[2], KnownWaveProperties.StartCoordinateX, coordinate1.X);
            AssertSpatialProperty(properties[3], KnownWaveProperties.EndCoordinateX, coordinate2.X);
            AssertSpatialProperty(properties[4], KnownWaveProperties.StartCoordinateY, coordinate1.Y);
            AssertSpatialProperty(properties[5], KnownWaveProperties.EndCoordinateY, coordinate2.Y);
            AssertProperty(properties[6], KnownWaveProperties.SpectrumSpec, "parametric");
            AssertProperty(properties[7], KnownWaveProperties.ShapeType, "Jonswap");
            AssertProperty(properties[8], KnownWaveProperties.PeriodType, KnownWaveBoundariesFileConstants.PeakPeriodType);
            AssertProperty(properties[9], KnownWaveProperties.DirectionalSpreadingType, "Power");
            AssertProperty(properties[10], KnownWaveProperties.PeakEnhancementFactor, factor);
        }

        private static void AssertProperty(IniProperty property, string key, double value)
        {
            AssertProperty(property, key, value.ToString("e7", CultureInfo.InvariantCulture));
        }

        private static void AssertSpatialProperty(IniProperty property, string key, double value)
        {
            AssertProperty(property, key, value.ToString("F7", CultureInfo.InvariantCulture));
        }

        private static void AssertProperty(IniProperty property, string key, string value)
        {
            Assert.That(property.Key, Is.EqualTo(key));
            Assert.That(property.Value, Is.EqualTo(value));
        }

        private static bool MatchesAction(FileBasedParameters parameters, Action<string> s)
        {
            const string setValue = "some_new_file_path";

            s.Invoke(setValue);

            return parameters.FilePath == setValue;
        }

        private static bool MatchesAction(IBoundaryContainer boundaryContainer, Action<string> s)
        {
            const string setValue = "some_new_file_path";

            s.Invoke(setValue);

            return boundaryContainer.FilePathForBoundariesPerFile == setValue;
        }

        private class WaveBoundaryBuilder
        {
            private readonly IBoundaryContainer boundaryContainer;
            private readonly IWaveBoundary boundary;
            private readonly IWaveBoundaryConditionDefinition conditionDefinition;
            private readonly SupportPoint[] supportPoints;

            private WaveBoundaryBuilder(string name, IBoundaryContainer boundaryContainer, params double[] distances)
            {
                this.boundaryContainer = boundaryContainer;

                boundary = Substitute.For<IWaveBoundary>();
                boundary.Name.Returns(name);

                var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
                boundary.GeometricDefinition.Returns(geometricDefinition);
                supportPoints = distances.OrderBy(d => d)
                                         .Select(d => new SupportPoint(d, geometricDefinition)).ToArray();
                geometricDefinition.SupportPoints.Returns(new EventedList<SupportPoint>(supportPoints));

                conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
                boundary.ConditionDefinition.Returns(conditionDefinition);
            }

            public static WaveBoundaryBuilder Create(string name, IBoundaryContainer boundaryContainer, params double[] distances) => new WaveBoundaryBuilder(name, boundaryContainer, distances);

            public WaveBoundaryBuilder WithCoordinates(double startX, double startY, double endX, double endY)
            {
                var boundarySnappingCalculator = Substitute.For<IBoundarySnappingCalculator>();
                boundaryContainer.GetBoundarySnappingCalculator().Returns(boundarySnappingCalculator);

                boundarySnappingCalculator.CalculateCoordinateFromSupportPoint(supportPoints.First())
                                          .Returns(new Coordinate(startX, startY));
                boundarySnappingCalculator.CalculateCoordinateFromSupportPoint(supportPoints.Last())
                                          .Returns(new Coordinate(endX, endY));

                return this;
            }

            public WaveBoundaryBuilder WithUniformFileBasedData(string filePath)
            {
                var dataComponent = new UniformDataComponent<FileBasedParameters>(new FileBasedParameters(filePath));
                conditionDefinition.DataComponent.Returns(dataComponent);

                return this;
            }

            public WaveBoundaryBuilder WithSpatiallyVaryingFileBasedData(params string[] filePaths)
            {
                var dataComponent = new SpatiallyVaryingDataComponent<FileBasedParameters>();
                foreach (DelftTools.Utils.Tuple<SupportPoint, string> pair in supportPoints.Zip(filePaths))
                {
                    dataComponent.AddParameters(pair.First, new FileBasedParameters(pair.Second));
                }

                conditionDefinition.DataComponent.Returns(dataComponent);

                return this;
            }

            public IWaveBoundary Finish() => boundary;
        }
    }
}