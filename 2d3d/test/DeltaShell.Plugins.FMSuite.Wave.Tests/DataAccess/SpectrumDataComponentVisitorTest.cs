using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.Common.IO;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess
{
    [TestFixture]
    public class SpectrumDataComponentVisitorTest
    {
        private static readonly Random random = new Random();
        private static int RandomValue => random.Next();

        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var visitor = new SpectrumDataComponentVisitor(new IniSection("TestSection"),
                                                           Substitute.For<IFilesManager>());

            // Assert
            Assert.That(visitor, Is.InstanceOf<ISpatiallyDefinedDataComponentVisitor>());
        }

        [Test]
        public void Visit_UniformDataComponent_UniformDataComponentNull_ThrowsArgumentNullException()
        {
            // Setup
            var visitor = new SpectrumDataComponentVisitor(new IniSection("TestSection"),
                                                           Substitute.For<IFilesManager>());

            // Call
            void Call() => visitor.Visit((UniformDataComponent<IForcingTypeDefinedParameters>) null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("uniformDataComponent"));
        }

        [Test]
        public void Visit_UniformDataComponent_WithFileBasedParameters_SetsCorrectSpectrumTypeAndProperties()
        {
            // Setup
            var section = new IniSection("TestSection");
            var filesManager = Substitute.For<IFilesManager>();
            var visitor = new SpectrumDataComponentVisitor(section, filesManager);

            const string fileName = "file.txt";
            var filePath = $"D:\\some_directory\\{fileName}";
            var parameters = new FileBasedParameters(filePath);
            var dataComponent = new UniformDataComponent<FileBasedParameters>(parameters);

            // Call
            visitor.Visit(dataComponent);

            // Assert
            Assert.That(visitor.SpectrumType, Is.EqualTo(SpectrumImportExportType.FromFile));

            IniProperty[] properties = section.Properties.ToArray();
            Assert.That(properties, Has.Length.EqualTo(2));
            AssertProperty(properties[0], KnownWaveProperties.SpectrumSpec, "from file");
            AssertProperty(properties[1], KnownWaveProperties.Spectrum, fileName);

            filesManager.Received(1).Add(filePath, Arg.Is<Action<string>>(a => MatchesAction(parameters, a)));
        }

        [Test]
        public void Visit_SpatiallyVaryingDataComponent_SpatiallyVaryingDataComponentNull_ThrowsArgumentNullException()
        {
            // Setup
            var visitor = new SpectrumDataComponentVisitor(new IniSection("TestSection"),
                                                           Substitute.For<IFilesManager>());

            // Call
            void Call() => visitor.Visit((SpatiallyVaryingDataComponent<IForcingTypeDefinedParameters>) null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("spatiallyVaryingDataComponent"));
        }

        [Test]
        public void Visit_SpatiallyVaryingDataComponent_WithFileBasedParameters_SetsCorrectSpectrumTypeAndProperties()
        {
            // Setup
            var section = new IniSection("TestSection");
            var filesManager = Substitute.For<IFilesManager>();
            var visitor = new SpectrumDataComponentVisitor(section, filesManager);

            const int distance1 = 0;
            const string fileName1 = "file_1.txt";
            var filePath1 = $"D:\\some_directory\\{fileName1}";

            const int distance2 = 5;
            const string fileName2 = "file_2.txt";
            var filePath2 = $"D:\\some_directory\\{fileName2}";

            const int distance3 = 10;
            const string fileName3 = "file_3.txt";
            var filePath3 = $"D:\\some_directory\\{fileName3}";

            var dataComponent = new SpatiallyVaryingDataComponent<FileBasedParameters>();

            var parameters2 = new FileBasedParameters(filePath2);
            dataComponent.AddParameters(GetSupportPoint(distance2), parameters2);
            var parameters3 = new FileBasedParameters(filePath3);
            dataComponent.AddParameters(GetSupportPoint(distance3), parameters3);
            var parameters1 = new FileBasedParameters(filePath1);
            dataComponent.AddParameters(GetSupportPoint(distance1), parameters1);

            // Call
            visitor.Visit(dataComponent);

            // Assert
            Assert.That(visitor.SpectrumType, Is.EqualTo(SpectrumImportExportType.FromFile));

            IniProperty[] properties = section.Properties.ToArray();
            Assert.That(properties, Has.Length.EqualTo(7));
            AssertProperty(properties[0], KnownWaveProperties.SpectrumSpec, "from file");
            AssertProperty(properties[1], KnownWaveProperties.CondSpecAtDist, distance1);
            AssertProperty(properties[2], KnownWaveProperties.Spectrum, fileName1);
            AssertProperty(properties[3], KnownWaveProperties.CondSpecAtDist, distance2);
            AssertProperty(properties[4], KnownWaveProperties.Spectrum, fileName2);
            AssertProperty(properties[5], KnownWaveProperties.CondSpecAtDist, distance3);
            AssertProperty(properties[6], KnownWaveProperties.Spectrum, fileName3);

            filesManager.Received(1).Add(filePath1, Arg.Is<Action<string>>(a => MatchesAction(parameters1, a)));
            filesManager.Received(1).Add(filePath2, Arg.Is<Action<string>>(a => MatchesAction(parameters2, a)));
            filesManager.Received(1).Add(filePath3, Arg.Is<Action<string>>(a => MatchesAction(parameters3, a)));
        }

        private static IEnumerable<TestCaseData> ConstructorArgumentNullCases()
        {
            yield return new TestCaseData(null, Substitute.For<IFilesManager>(), "section");
            yield return new TestCaseData(new IniSection("TestSection"), null, "filesManager");
        }

        [TestCaseSource(nameof(ConstructorArgumentNullCases))]
        public void Constructor_ArgumentNull_ThrowsArgumentNullException(IniSection section, IFilesManager filesManager, string expectedParamName)
        {
            // Call
            void Call() => new SpectrumDataComponentVisitor(section, filesManager);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParamName));
        }

        [TestFixture]
        [TestFixture(typeof(DegreesDefinedSpreading))]
        [TestFixture(typeof(PowerDefinedSpreading))]
        public class WithSpreadingTypes<T> where T : class, IBoundaryConditionSpreading, new()
        {
            private static ConstantParameters<T> ConstantParameters =>
                new ConstantParameters<T>(RandomValue, RandomValue, RandomValue, new T());

            private static TimeDependentParameters<T> TimeDependentParameters =>
                new TimeDependentParameters<T>(Substitute.For<IWaveEnergyFunction<T>>());

            private static SupportPoint SupportPoint => GetSupportPoint(RandomValue);

            [Test]
            public void Visit_UniformDataComponent_WithConstantParameters_SetsCorrectSpectrumTypeAndProperty()
            {
                // Setup
                var section = new IniSection("TestSection");
                var visitor = new SpectrumDataComponentVisitor(section,
                                                               Substitute.For<IFilesManager>());

                var dataComponent = new UniformDataComponent<ConstantParameters<T>>(ConstantParameters);

                // Call
                visitor.Visit(dataComponent);

                // Assert
                AssertProperty(section.Properties.Single(),
                               KnownWaveProperties.SpectrumSpec, "parametric");
                Assert.That(visitor.SpectrumType, Is.EqualTo(SpectrumImportExportType.Parametrized));
            }

            [Test]
            public void Visit_UniformDataComponent_WithTimeDependentParameters_SetsCorrectSpectrumTypeAndProperty()
            {
                // Setup
                var section = new IniSection("TestSection");
                var visitor = new SpectrumDataComponentVisitor(section,
                                                               Substitute.For<IFilesManager>());

                var dataComponent = new UniformDataComponent<TimeDependentParameters<T>>(TimeDependentParameters);

                // Call
                visitor.Visit(dataComponent);

                // Assert
                AssertProperty(section.Properties.Single(),
                               KnownWaveProperties.SpectrumSpec, "parametric");
                Assert.That(visitor.SpectrumType, Is.EqualTo(SpectrumImportExportType.Parametrized));
            }

            [Test]
            public void Visit_SpatiallyVaryingDataComponent_WithConstantParameters_SetsCorrectSpectrumTypeAndProperty()
            {
                // Setup
                var section = new IniSection("TestSection");
                var visitor = new SpectrumDataComponentVisitor(section,
                                                               Substitute.For<IFilesManager>());

                var dataComponent = new SpatiallyVaryingDataComponent<ConstantParameters<T>>();
                dataComponent.AddParameters(SupportPoint, ConstantParameters);
                dataComponent.AddParameters(SupportPoint, ConstantParameters);
                dataComponent.AddParameters(SupportPoint, ConstantParameters);

                // Call
                visitor.Visit(dataComponent);

                // Assert
                AssertProperty(section.Properties.Single(),
                               KnownWaveProperties.SpectrumSpec, "parametric");
                Assert.That(visitor.SpectrumType, Is.EqualTo(SpectrumImportExportType.Parametrized));
            }

            [Test]
            public void Visit_SpatiallyVaryingDataComponent_WithTimeDependentParameters_SetsCorrectSpectrumTypeAndProperty()
            {
                // Setup
                var section = new IniSection("TestSection");
                var visitor = new SpectrumDataComponentVisitor(section,
                                                               Substitute.For<IFilesManager>());

                var dataComponent = new SpatiallyVaryingDataComponent<TimeDependentParameters<T>>();
                dataComponent.AddParameters(SupportPoint, TimeDependentParameters);
                dataComponent.AddParameters(SupportPoint, TimeDependentParameters);
                dataComponent.AddParameters(SupportPoint, TimeDependentParameters);

                // Call
                visitor.Visit(dataComponent);

                // Assert
                AssertProperty(section.Properties.Single(),
                               KnownWaveProperties.SpectrumSpec, "parametric");
                Assert.That(visitor.SpectrumType, Is.EqualTo(SpectrumImportExportType.Parametrized));
            }
        }

        private static SupportPoint GetSupportPoint(double distance) =>
            new SupportPoint(distance, Substitute.For<IWaveBoundaryGeometricDefinition>());

        private static void AssertProperty(IniProperty property, string key, double value)
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
    }
}