using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DeltaShell.NGHS.Common.IO;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.IO;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO
{
    [TestFixture]
    public class SpectrumDataComponentVisitorTest
    {
        private static readonly Random random = new Random();
        private static int RandomValue => random.Next();

        private IEnumerable<TestCaseData> ConstructorArgumentNullCases()
        {
            yield return new TestCaseData(null, Substitute.For<IFilesManager>(), "category");
            yield return new TestCaseData(new DelftIniCategory(""), null, "filesManager");
        }

        [TestCaseSource(nameof(ConstructorArgumentNullCases))]
        public void Constructor_ArgumentNull_ThrowsArgumentNullException(DelftIniCategory category, IFilesManager filesManager, string expectedParamName)
        {
            // Call
            void Call() => new SpectrumDataComponentVisitor(category, filesManager);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParamName));
        }

        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var visitor = new SpectrumDataComponentVisitor(new DelftIniCategory(""),
                                                           Substitute.For<IFilesManager>());

            // Assert
            Assert.That(visitor, Is.InstanceOf<ISpatiallyDefinedDataComponentVisitor>());
        }

        [Test]
        public void Visit_UniformDataComponent_UniformDataComponentNull_ThrowsArgumentNullException()
        {
            // Setup
            var visitor = new SpectrumDataComponentVisitor(new DelftIniCategory(""),
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
            var category = new DelftIniCategory("");
            var filesManager = Substitute.For<IFilesManager>();
            var visitor = new SpectrumDataComponentVisitor(category, filesManager);

            const string fileName = "file.txt";
            string filePath = $"D:\\some_directory\\{fileName}";
            var parameters = new FileBasedParameters(filePath);
            var dataComponent = new UniformDataComponent<FileBasedParameters>(parameters);

            // Call
            visitor.Visit(dataComponent);

            // Assert
            Assert.That(visitor.SpectrumType, Is.EqualTo(SpectrumImportExportType.FromFile));

            DelftIniProperty[] properties = category.Properties.ToArray();
            Assert.That(properties, Has.Length.EqualTo(2));
            AssertProperty(properties[0], KnownWaveProperties.SpectrumSpec, "from file");
            AssertProperty(properties[1], KnownWaveProperties.Spectrum, fileName);

            filesManager.Received(1).Add(filePath);
        }

        [Test]
        public void Visit_SpatiallyVaryingDataComponent_SpatiallyVaryingDataComponentNull_ThrowsArgumentNullException()
        {
            // Setup
            var visitor = new SpectrumDataComponentVisitor(new DelftIniCategory(""),
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
            var category = new DelftIniCategory("");
            var filesManager = Substitute.For<IFilesManager>();
            var visitor = new SpectrumDataComponentVisitor(category, filesManager);

            const int distance1 = 0;
            const string fileName1 = "file_1.txt";
            string filePath1 = $"D:\\some_directory\\{fileName1}";

            const int distance2 = 5;
            const string fileName2 = "file_2.txt";
            string filePath2 = $"D:\\some_directory\\{fileName2}";

            const int distance3 = 10;
            const string fileName3 = "file_3.txt";
            string filePath3 = $"D:\\some_directory\\{fileName3}";

            var dataComponent = new SpatiallyVaryingDataComponent<FileBasedParameters>();
            dataComponent.AddParameters(GetSupportPoint(distance2), new FileBasedParameters(filePath2));
            dataComponent.AddParameters(GetSupportPoint(distance3), new FileBasedParameters(filePath3));
            dataComponent.AddParameters(GetSupportPoint(distance1), new FileBasedParameters(filePath1));

            // Call
            visitor.Visit(dataComponent);

            // Assert
            Assert.That(visitor.SpectrumType, Is.EqualTo(SpectrumImportExportType.FromFile));

            DelftIniProperty[] properties = category.Properties.ToArray();
            Assert.That(properties, Has.Length.EqualTo(7));
            AssertProperty(properties[0], KnownWaveProperties.SpectrumSpec, "from file");
            AssertProperty(properties[1], KnownWaveProperties.CondSpecAtDist, distance1);
            AssertProperty(properties[2], KnownWaveProperties.Spectrum, fileName1);
            AssertProperty(properties[3], KnownWaveProperties.CondSpecAtDist, distance2);
            AssertProperty(properties[4], KnownWaveProperties.Spectrum, fileName2);
            AssertProperty(properties[5], KnownWaveProperties.CondSpecAtDist, distance3);
            AssertProperty(properties[6], KnownWaveProperties.Spectrum, fileName3);

            filesManager.Received(1).Add(filePath1);
            filesManager.Received(1).Add(filePath2);
            filesManager.Received(1).Add(filePath3);
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
                var category = new DelftIniCategory("");
                var visitor = new SpectrumDataComponentVisitor(category,
                                                               Substitute.For<IFilesManager>());

                var dataComponent = new UniformDataComponent<ConstantParameters<T>>(ConstantParameters);

                // Call
                visitor.Visit(dataComponent);

                // Assert
                AssertProperty(category.Properties.Single(),
                               KnownWaveProperties.SpectrumSpec, "parametric");
                Assert.That(visitor.SpectrumType, Is.EqualTo(SpectrumImportExportType.Parametrized));
            }

            [Test]
            public void Visit_UniformDataComponent_WithTimeDependentParameters_SetsCorrectSpectrumTypeAndProperty()
            {
                // Setup
                var category = new DelftIniCategory("");
                var visitor = new SpectrumDataComponentVisitor(category,
                                                               Substitute.For<IFilesManager>());

                var dataComponent = new UniformDataComponent<TimeDependentParameters<T>>(TimeDependentParameters);

                // Call
                visitor.Visit(dataComponent);

                // Assert
                AssertProperty(category.Properties.Single(),
                               KnownWaveProperties.SpectrumSpec, "parametric");
                Assert.That(visitor.SpectrumType, Is.EqualTo(SpectrumImportExportType.Parametrized));
            }

            [Test]
            public void Visit_SpatiallyVaryingDataComponent_WithConstantParameters_SetsCorrectSpectrumTypeAndProperty()
            {
                // Setup
                var category = new DelftIniCategory("");
                var visitor = new SpectrumDataComponentVisitor(category,
                                                               Substitute.For<IFilesManager>());

                var dataComponent = new SpatiallyVaryingDataComponent<ConstantParameters<T>>();
                dataComponent.AddParameters(SupportPoint, ConstantParameters);
                dataComponent.AddParameters(SupportPoint, ConstantParameters);
                dataComponent.AddParameters(SupportPoint, ConstantParameters);

                // Call
                visitor.Visit(dataComponent);

                // Assert
                AssertProperty(category.Properties.Single(),
                               KnownWaveProperties.SpectrumSpec, "parametric");
                Assert.That(visitor.SpectrumType, Is.EqualTo(SpectrumImportExportType.Parametrized));
            }

            [Test]
            public void Visit_SpatiallyVaryingDataComponent_WithTimeDependentParameters_SetsCorrectSpectrumTypeAndProperty()
            {
                // Setup
                var category = new DelftIniCategory("");
                var visitor = new SpectrumDataComponentVisitor(category,
                                                               Substitute.For<IFilesManager>());

                var dataComponent = new SpatiallyVaryingDataComponent<TimeDependentParameters<T>>();
                dataComponent.AddParameters(SupportPoint, TimeDependentParameters);
                dataComponent.AddParameters(SupportPoint, TimeDependentParameters);
                dataComponent.AddParameters(SupportPoint, TimeDependentParameters);

                // Call
                visitor.Visit(dataComponent);

                // Assert
                AssertProperty(category.Properties.Single(),
                               KnownWaveProperties.SpectrumSpec, "parametric");
                Assert.That(visitor.SpectrumType, Is.EqualTo(SpectrumImportExportType.Parametrized));
            }
        }

        private static SupportPoint GetSupportPoint(double distance) =>
            new SupportPoint(distance, Substitute.For<IWaveBoundaryGeometricDefinition>());

        private static void AssertProperty(DelftIniProperty property, string name, double value)
        {
            AssertProperty(property, name, value.ToString("e7", CultureInfo.InvariantCulture));
        }

        private static void AssertProperty(DelftIniProperty property, string name, string value)
        {
            Assert.That(property.Name, Is.EqualTo(name));
            Assert.That(property.Value, Is.EqualTo(value));
        }
    }
}