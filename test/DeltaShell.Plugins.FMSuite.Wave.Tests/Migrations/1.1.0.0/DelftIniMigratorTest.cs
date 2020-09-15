using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Migrations._1._1._0._0;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Migrations._1._1._0._0
{
    [TestFixture]
    public class DelftIniMigratorTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IMigrationBehaviour>> migrationBehaviourMapping = 
                new Dictionary<string, IReadOnlyDictionary<string, IMigrationBehaviour>>();
            var iniReader = Substitute.For<IDelftIniReader>();
            var iniWriter = Substitute.For<IDelftIniWriter>();

            // Call
            var migrator = new DelftIniMigrator(migrationBehaviourMapping, iniReader, iniWriter, false);

            // Assert
            Assert.That(migrator, Is.InstanceOf<IDelftIniMigrator>());
        }

        private static IEnumerable<TestCaseData> Constructor_ParameterNull_Data()
        {
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IMigrationBehaviour>> migrationBehaviourMapping = 
                new Dictionary<string, IReadOnlyDictionary<string, IMigrationBehaviour>>();
            var iniReader = Substitute.For<IDelftIniReader>();
            var iniWriter = Substitute.For<IDelftIniWriter>();

            yield return new TestCaseData(null, iniReader, iniWriter, "migrationBehaviourMapping");
            yield return new TestCaseData(migrationBehaviourMapping, null, iniWriter, "iniReader");
            yield return new TestCaseData(migrationBehaviourMapping, iniReader, null, "iniWriter");
        }

        [Test]
        [TestCaseSource(nameof(Constructor_ParameterNull_Data))]
        public void Constructor_ParameterNull_ThrowsArgumentNullException(IReadOnlyDictionary<string, IReadOnlyDictionary<string, IMigrationBehaviour>> migrationBehaviour,
                                                                          IDelftIniReader iniReader,
                                                                          IDelftIniWriter iniWriter,
                                                                          string expectedParameterName)
        {
            void Call() => new DelftIniMigrator(migrationBehaviour, iniReader, iniWriter, false);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParameterName));
        }

        private static void VerifyLogHandlerDidNotReceiveAnyReports(ILogHandler logHandler)
        {
            logHandler.DidNotReceiveWithAnyArgs().ReportError(null);
            logHandler.DidNotReceiveWithAnyArgs().ReportErrorFormat(null);
            logHandler.DidNotReceiveWithAnyArgs().ReportWarning(null);
            logHandler.DidNotReceiveWithAnyArgs().ReportWarningFormat(null);
            logHandler.DidNotReceiveWithAnyArgs().ReportInfo(null);
            logHandler.DidNotReceiveWithAnyArgs().ReportInfoFormat(null);
        }

        [Test]
        public void MigrateFile_SourceFileStreamNull_ThrowsArgumentNullException()
        {
            // Setup
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IMigrationBehaviour>> migrationBehaviourMapping = 
                new Dictionary<string, IReadOnlyDictionary<string, IMigrationBehaviour>>();
            var iniReader = Substitute.For<IDelftIniReader>();
            var iniWriter = Substitute.For<IDelftIniWriter>();
            var logHandler = Substitute.For<ILogHandler>();

            var migrator = new DelftIniMigrator(migrationBehaviourMapping, iniReader, iniWriter, false);
            
            // Call | Assert
            void Call() => migrator.MigrateFile(null, 
                                                "./imaginary/toad/to/src.ini", 
                                                "./imaginary/toad/to/tgt.ini", 
                                                logHandler);
            
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("sourceFileStream"));
        }

        [Test]
        public void MigrateFile_SourceFilePath_ThrowsArgumentNullException()
        {
            // Setup
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IMigrationBehaviour>> migrationBehaviourMapping = 
                new Dictionary<string, IReadOnlyDictionary<string, IMigrationBehaviour>>();
            var iniReader = Substitute.For<IDelftIniReader>();
            var iniWriter = Substitute.For<IDelftIniWriter>();
            var logHandler = Substitute.For<ILogHandler>();

            var migrator = new DelftIniMigrator(migrationBehaviourMapping, iniReader, iniWriter, false);
            
            // Call | Assert
            void Call() => migrator.MigrateFile(new MemoryStream(), 
                                                null,
                                                "./imaginary/toad/to/tgt.ini", 
                                                logHandler);
            
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("sourceFilePath"));
        }

        [Test]
        public void MigrateFile_TargetFilePath_ThrowsArgumentNullException()
        {
            // Setup
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IMigrationBehaviour>> migrationBehaviourMapping = 
                new Dictionary<string, IReadOnlyDictionary<string, IMigrationBehaviour>>();
            var iniReader = Substitute.For<IDelftIniReader>();
            var iniWriter = Substitute.For<IDelftIniWriter>();
            var logHandler = Substitute.For<ILogHandler>();

            var migrator = new DelftIniMigrator(migrationBehaviourMapping, iniReader, iniWriter, false);
            
            // Call | Assert
            void Call() => migrator.MigrateFile(new MemoryStream(), 
                                                "./imaginary/toad/to/src.ini", 
                                                null,
                                                logHandler);
            
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("targetFilePath"));
        }


        [Test]
        public void MigrateFile_MigratesPropertiesDefinedInMigrationBehaviourCorrectly()
        {
            // Setup
            // Paths
            var sourceFile = new MemoryStream();

            const string sourcePath = "./fromHere/soooooooooourceyFile.ini";
            const string targetPath = "./goesHere/taaaaaaaaaaargetFile.ini";

            // Ini properties
            const string categoryName = "categoryName";
            var category = new DelftIniCategory(categoryName);

            DelftIniProperty[] properties = 
                Enumerable.Range(0, 5)
                          .Select(i => new DelftIniProperty($"someName_{i}", $"someValue_{i}", $"(someComment_{i}"))
                          .ToArray();
            IMigrationBehaviour[] migrationBehaviours = 
                Enumerable.Range(0, 5)
                          .Select(_ => Substitute.For<IMigrationBehaviour>())
                          .ToArray();

            category.AddProperties(properties);

            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IMigrationBehaviour>> migrationMapping = 
                new Dictionary<string, IReadOnlyDictionary<string, IMigrationBehaviour>>
            {
                {categoryName, new Dictionary<string, IMigrationBehaviour>
                {
                    { properties[0].Name, migrationBehaviours[0] },
                    { properties[1].Name, migrationBehaviours[1] },
                    { properties[2].Name, migrationBehaviours[2] },
                    { properties[3].Name, migrationBehaviours[3] },
                    { properties[4].Name, migrationBehaviours[4] },
                }},
            };

            DelftIniCategory[] categories = {category};

            var iniReader = Substitute.For<IDelftIniReader>();
            iniReader.ReadDelftIniFile(sourceFile, sourcePath).Returns(categories);

            var iniWriter = Substitute.For<IDelftIniWriter>();

            var migrator = new DelftIniMigrator(migrationMapping, iniReader, iniWriter, false);
            var logHandler = Substitute.For<ILogHandler>();

            // Call
            migrator.MigrateFile(sourceFile, sourcePath, targetPath, logHandler);

            // Assert
            iniReader.Received(1).ReadDelftIniFile(sourceFile, sourcePath);
            iniWriter.Received(1).WriteDelftIniFile(categories, targetPath, true);

            VerifyLogHandlerDidNotReceiveAnyReports(logHandler);

            for (var i = 0; i < 5; i++)
            {
                migrationBehaviours[i].Received(1).MigrateProperty(properties[i], logHandler);
                migrationBehaviours[i].DidNotReceive().MigrateProperty(Arg.Is<DelftIniProperty>(x => !x.Name.Equals(properties[i].Name)), 
                                                                       logHandler);
            }
        }

        [Test]
        public void MigrateFile_SkipsPropertiesNotDefinedInMigrationBehaviour()
        {
            // Setup
            var sourceFile = new MemoryStream();

            const string sourcePath = "./fromHere/soooooooooourceyFile.ini";
            const string targetPath = "./goesHere/taaaaaaaaaaargetFile.ini";

            // Ini properties
            const string categoryName = "categoryName";
            var category = new DelftIniCategory(categoryName);

            const string propertyName = "someName";
            const string propertyValue = "someValue";
            const string propertyComment = "someComment";

            var property = new DelftIniProperty(propertyName, propertyValue, propertyComment);
            category.AddProperty(property);

            DelftIniCategory[] categories = {category};

            var migrationBehaviour = Substitute.For<IMigrationBehaviour>();

            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IMigrationBehaviour>> migrationMapping = 
                new Dictionary<string, IReadOnlyDictionary<string, IMigrationBehaviour>>
            {
                {categoryName, new Dictionary<string, IMigrationBehaviour>
                {
                    { "notSomeName", migrationBehaviour },
                }},
            };


            var iniReader = Substitute.For<IDelftIniReader>();
            iniReader.ReadDelftIniFile(sourceFile, sourcePath).Returns(categories);

            var iniWriter = Substitute.For<IDelftIniWriter>();

            var migrator = new DelftIniMigrator(migrationMapping, iniReader, iniWriter, false);
            var logHandler = Substitute.For<ILogHandler>();

            // Call
            migrator.MigrateFile(sourceFile, sourcePath, targetPath, logHandler);

            // Assert
            iniReader.Received(1).ReadDelftIniFile(sourceFile, sourcePath);
            iniWriter.Received(1).WriteDelftIniFile(categories, targetPath, true);

            VerifyLogHandlerDidNotReceiveAnyReports(logHandler);

            migrationBehaviour.DidNotReceiveWithAnyArgs().MigrateProperty(null, null);
            Assert.That(property.Name, Is.EqualTo(propertyName));
            Assert.That(property.Value, Is.EqualTo(propertyValue));
            Assert.That(property.Comment, Is.EqualTo(propertyComment));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCase(true)]
        [TestCase(false)]
        public void MigrateFile_RemoveOriginalIniFile_ExpectedBehaviour(bool removeOriginalIniFile)
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();

            using (var tempDir = new TemporaryDirectory())
            {
                const string sourceSubDirectory = "itDonMean";
                tempDir.CreateDirectory(sourceSubDirectory);

                const string goalSubDirectory = "dooWahDooWah";
                tempDir.CreateDirectory(goalSubDirectory);

                const string fileName = "AThing.ini";
                string sourcePath = Path.Combine(tempDir.Path, sourceSubDirectory, fileName);

                var delftIniWriter = new DelftIniWriter();
                delftIniWriter.WriteDelftIniFile(Enumerable.Empty<DelftIniCategory>(), 
                                                 sourcePath, 
                                                 false);
                
                const string goalDirName = "dooWahDooWah";
                string goalDir = tempDir.CreateDirectory(goalDirName);
                string goalPath = Path.Combine(goalDir, fileName);

                var migrator = new DelftIniMigrator(new Dictionary<string, IReadOnlyDictionary<string, IMigrationBehaviour>>(), 
                                                    new DelftIniReader(), 
                                                    delftIniWriter, 
                                                    removeOriginalIniFile);
                var sourceStream = new FileStream(sourcePath, FileMode.Open);

                // Call
                migrator.MigrateFile(sourceStream, 
                                     sourcePath,  
                                     goalPath, 
                                     logHandler);

                // Assert
                Assert.That(File.Exists(goalPath), Is.True);
                Assert.That(File.Exists(sourcePath), Is.EqualTo(!removeOriginalIniFile));
            }
        }
    }
}