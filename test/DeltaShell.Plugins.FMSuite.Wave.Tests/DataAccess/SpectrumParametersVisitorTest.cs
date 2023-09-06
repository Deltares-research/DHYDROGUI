using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.Common.IO;
using DeltaShell.NGHS.IO.Ini;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess
{
    [TestFixture]
    public class SpectrumParametersVisitorTest
    {
        private static readonly Random random = new Random();
        private static int RandomValue => random.Next();

        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var visitor = new SpectrumParametersVisitor(new IniSection("TestSection"),
                                                        Substitute.For<IFilesManager>());

            // Assert
            Assert.That(visitor, Is.InstanceOf<IForcingTypeDefinedParametersVisitor>());
        }

        [Test]
        public void Visit_FileBasedParameters_FileBasedParametersNull_ThrowsArgumentNullException()
        {
            // Setup
            var visitor = new SpectrumParametersVisitor(new IniSection("TestSection"),
                                                        Substitute.For<IFilesManager>());

            // Call
            void Call() => visitor.Visit((FileBasedParameters) null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("fileBasedParameters"));
        }

        [Test]
        public void Visit_FileBasedParameters_SetsCorrectSpectrumTypeAndFileNameAndProperties()
        {
            // Setup
            var section = new IniSection("TestSection");
            var filesManager = Substitute.For<IFilesManager>();
            var visitor = new SpectrumParametersVisitor(section, filesManager);

            const string fileName = "file.txt";
            var filePath = $"D:\\some_directory\\{fileName}";
            var parameters = new FileBasedParameters(filePath);

            // Call
            visitor.Visit(parameters);

            // Assert
            IniProperty property = section.Properties.Single();
            Assert.That(property.Key, Is.EqualTo(KnownWaveProperties.SpectrumSpec));
            Assert.That(property.Value, Is.EqualTo("from file"));
            Assert.That(visitor.SpectrumType, Is.EqualTo(SpectrumImportExportType.FromFile));
            Assert.That(visitor.SpectrumFile, Is.EqualTo(fileName));
            filesManager.Received(1).Add(filePath, Arg.Is<Action<string>>(a => MatchesAction(parameters, a)));
        }

        [Test]
        public void Visit_FileBasedParameters_WithEmptyFilePath_SetsCorrectSpectrumTypeAndFileName()
        {
            // Setup
            var section = new IniSection("TestSection");
            var filesManager = Substitute.For<IFilesManager>();
            var visitor = new SpectrumParametersVisitor(section, filesManager);

            var parameters = new FileBasedParameters(string.Empty);

            // Call
            visitor.Visit(parameters);

            // Assert
            IniProperty property = section.Properties.Single();
            Assert.That(property.Key, Is.EqualTo(KnownWaveProperties.SpectrumSpec));
            Assert.That(property.Value, Is.EqualTo("from file"));
            Assert.That(visitor.SpectrumType, Is.EqualTo(SpectrumImportExportType.FromFile));
            Assert.That(visitor.SpectrumFile, Is.EqualTo(" "));
            filesManager.DidNotReceiveWithAnyArgs().Add(string.Empty, null);
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
            void Call() => new SpectrumParametersVisitor(section, filesManager);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParamName));
        }

        private static bool MatchesAction(FileBasedParameters parameters, Action<string> s)
        {
            const string setValue = "some_new_file_path";

            s.Invoke(setValue);

            return parameters.FilePath == setValue;
        }

        [TestFixture]
        [TestFixture(typeof(DegreesDefinedSpreading))]
        [TestFixture(typeof(PowerDefinedSpreading))]
        public class WithSpreadingTypes<T> where T : class, IBoundaryConditionSpreading, new()
        {
            [Test]
            public void Visit_ConstantParameters_ConstantParametersNull_ThrowsArgumentNullException()
            {
                // Setup
                var visitor = new SpectrumParametersVisitor(new IniSection("TestSection"),
                                                            Substitute.For<IFilesManager>());

                // Call
                void Call() => visitor.Visit((ConstantParameters<T>) null);

                // Assert
                var exception = Assert.Throws<ArgumentNullException>(Call);
                Assert.That(exception.ParamName, Is.EqualTo("constantParameters"));
            }

            [Test]
            public void Visit_ConstantParameters_SetCorrectSpectrumTypeAndProperty()
            {
                // Setup
                var section = new IniSection("TestSection");
                var visitor = new SpectrumParametersVisitor(section,
                                                            Substitute.For<IFilesManager>());

                var parameters = new ConstantParameters<T>(RandomValue, RandomValue, RandomValue, new T());

                // Call
                visitor.Visit(parameters);

                // Assert
                IniProperty property = section.Properties.Single();
                Assert.That(property.Key, Is.EqualTo(KnownWaveProperties.SpectrumSpec));
                Assert.That(property.Value, Is.EqualTo("parametric"));
                Assert.That(visitor.SpectrumType, Is.EqualTo(SpectrumImportExportType.Parametrized));
                Assert.That(visitor.SpectrumFile, Is.Null);
            }

            [Test]
            public void Visit_TimeDependentParameters_TimeDependentParametersNull_ThrowsArgumentNullException()
            {
                // Setup
                var visitor = new SpectrumParametersVisitor(new IniSection("TestSection"),
                                                            Substitute.For<IFilesManager>());

                // Call
                void Call() => visitor.Visit((TimeDependentParameters<T>) null);

                // Assert
                var exception = Assert.Throws<ArgumentNullException>(Call);
                Assert.That(exception.ParamName, Is.EqualTo("timeDependentParameters"));
            }

            [Test]
            public void Visit_TimeDependentParameters_SetCorrectSpectrumTypeAndProperty()
            {
                // Setup
                var section = new IniSection("TestSection");
                var visitor = new SpectrumParametersVisitor(section,
                                                            Substitute.For<IFilesManager>());

                var parameters = new TimeDependentParameters<T>(Substitute.For<IWaveEnergyFunction<T>>());

                // Call
                visitor.Visit(parameters);

                // Assert
                IniProperty property = section.Properties.Single();
                Assert.That(property.Key, Is.EqualTo(KnownWaveProperties.SpectrumSpec));
                Assert.That(property.Value, Is.EqualTo("parametric"));
                Assert.That(visitor.SpectrumType, Is.EqualTo(SpectrumImportExportType.Parametrized));
                Assert.That(visitor.SpectrumFile, Is.Null);
            }
        }
    }
}