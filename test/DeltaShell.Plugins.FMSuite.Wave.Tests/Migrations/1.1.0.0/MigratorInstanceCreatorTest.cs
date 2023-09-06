using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.Ini;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.IniOperations;
using DeltaShell.Plugins.FMSuite.Wave.Migrations._1._1._0._0;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Migrations._1._1._0._0
{
    [TestFixture]
    public class MigratorInstanceCreatorTest
    {
        [Test]
        [TestCase(null, "somePath", "relativeDirectory")]
        [TestCase("somePath", null, "goalDirectory")]
        public void CreateObsMigration_ParameterNull_ThrowsArgumentNullException(string relativeDirectory,
                                                                                 string goalDirectory,
                                                                                 string expectedParameterName)
        {
            void Call() => MigratorInstanceCreator.CreateObsMigrator(relativeDirectory, goalDirectory);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParameterName));
        }

        [Test]
        [TestCase(null, "somePath", "relativeDirectory")]
        [TestCase("somePath", null, "goalDirectory")]
        public void CreateMdwMigration_ParameterNull_ThrowsArgumentNullException(string relativeDirectory,
                                                                                 string goalDirectory,
                                                                                 string expectedParameterName)
        {
            void Call() => MigratorInstanceCreator.CreateMdwMigrator(relativeDirectory, goalDirectory);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParameterName));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void CreateObsMigration_RelativePath_ProducesCorrectFunctioningMigrator()
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                const string inputDir = "temp_mdw_dir";
                string relativePath = tempDir.CreateDirectory(inputDir);

                const string goalDir = "new_dir/input";
                string absoluteGoalDir = tempDir.CreateDirectory(goalDir);

                const string polyLineFileContent = "Poly joke beach, Cornwall, England";
                const string polyLineFileName = "cornwall.pol";

                string oldPolFilePath = tempDir.CreateFile(Path.Combine(inputDir, polyLineFileName), polyLineFileContent);
                var oldPolFileInfo = new FileInfo(oldPolFilePath);

                var obstacleFileInformation = new IniSection("ObstacleFileInformation");
                obstacleFileInformation.AddMultipleProperties(new[]
                {
                    new IniProperty("File", "100.0", ""),
                    new IniProperty("PolylineFile", polyLineFileName, ""),
                });

                var oldSections = new IniSection[6];

                oldSections[0] = obstacleFileInformation;

                for (var i = 1; i < 6; i++)
                {
                    oldSections[i] = new IniSection("Obstacle");

                    oldSections[i].AddMultipleProperties(new[]
                    {
                        new IniProperty("Name", $"SomeName_{i}", ""),
                        new IniProperty("Type", "Sheet", ""),
                        new IniProperty("TransmCoef", "5.67", ""),
                        new IniProperty("Height", "4.56", ""),
                        new IniProperty("Alpha", "3.45", ""),
                        new IniProperty("Beta", "2.34", ""),
                        new IniProperty("Reflections", "specular", ""),
                        new IniProperty("ReflecCoef", "1.23", ""),
                    });
                }

                const string obstacleFileName = "cornwall.obs";
                string inputPath = Path.Combine(relativePath, obstacleFileName);

                var oldIniData = new IniData();
                oldIniData.AddMultipleSections(oldSections);
                
                var iniWriter = new IniWriter();
                iniWriter.WriteIniFile(oldIniData, inputPath, false);

                IIniFileOperator migrator = MigratorInstanceCreator.CreateObsMigrator(relativePath, absoluteGoalDir);

                var fileStream = new FileStream(inputPath, FileMode.Open);

                var logHandler = Substitute.For<ILogHandler>();

                // Call 
                migrator.Invoke(fileStream, inputPath, logHandler);

                // Assert
                Assert.That(oldPolFileInfo.Exists, Is.False);

                var polFileInfo = new FileInfo(Path.Combine(absoluteGoalDir, polyLineFileName));
                Assert.That(polFileInfo.Exists, Is.True);
                Assert.That(File.ReadAllText(polFileInfo.FullName), Is.EqualTo(polyLineFileContent));

                var obsFileInfo = new FileInfo(Path.Combine(absoluteGoalDir, obstacleFileName));
                Assert.That(obsFileInfo.Exists, Is.True);

                IniData newIniData = new IniReader().ReadIniFile(obsFileInfo.OpenRead(), obsFileInfo.FullName);
                List<IniSection> newSections = newIniData.Sections.ToList();

                Assert.That(newSections.Count, Is.EqualTo(oldSections.Length));

                for (var i = 0; i < oldSections.Length; i++)
                {
                    Assert.That(newSections[i].Name, Is.EqualTo(oldSections[i].Name));

                    IniProperty[] oldProperties = oldSections[i].Properties.ToArray();
                    IniProperty[] newProperties = newSections[i].Properties.ToArray();

                    Assert.That(newProperties.Length, Is.EqualTo(oldProperties.Length));

                    for (var j = 0; j < oldProperties.Length; j++)
                    {
                        Assert.That(newProperties[j].Key, Is.EqualTo(oldProperties[j].Key));
                        Assert.That(newProperties[j].Value, Is.EqualTo(oldProperties[j].Value));
                        Assert.That(newProperties[j].Comment, Is.EqualTo(oldProperties[j].Comment));
                    }
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void CreateObsMigration_RelativePathWithSubdir_ProducesCorrectFunctioningMigrator()
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                const string inputDir = "temp_mdw_dir";
                string relativePath = tempDir.CreateDirectory(inputDir);

                const string polSubDir = "pol";
                tempDir.CreateDirectory(Path.Combine(inputDir, polSubDir));

                const string goalDir = "new_dir/input";
                string absoluteGoalDir = tempDir.CreateDirectory(goalDir);

                const string polyLineFileContent = "Poly joke beach, Cornwall, England";
                const string polyLineFileName = "cornwall.pol";

                string oldPolPath = tempDir.CreateFile(Path.Combine(inputDir, polSubDir, polyLineFileName), polyLineFileContent);
                var oldPolInfo = new FileInfo(oldPolPath);

                var obstacleFileInformation = new IniSection("ObstacleFileInformation");
                obstacleFileInformation.AddMultipleProperties(new[]
                {
                    new IniProperty("File", "100.0", ""),
                    new IniProperty("PolylineFile", $"./{polSubDir}/{polyLineFileName}", ""),
                });

                var oldSections = new IniSection[6];

                oldSections[0] = obstacleFileInformation;

                for (var i = 1; i < 6; i++)
                {
                    oldSections[i] = new IniSection("Obstacle");

                    oldSections[i].AddMultipleProperties(new[]
                    {
                        new IniProperty("Name", $"SomeName_{i}", ""),
                        new IniProperty("Type", "Sheet", ""),
                        new IniProperty("TransmCoef", "5.67", ""),
                        new IniProperty("Height", "4.56", ""),
                        new IniProperty("Alpha", "3.45", ""),
                        new IniProperty("Beta", "2.34", ""),
                        new IniProperty("Reflections", "specular", ""),
                        new IniProperty("ReflecCoef", "1.23", ""),
                    });
                }

                const string obstacleFileName = "cornwall.obs";
                string inputPath = Path.Combine(relativePath, obstacleFileName);

                var oldIniData = new IniData();
                oldIniData.AddMultipleSections(oldSections);
                
                var iniWriter = new IniWriter();
                iniWriter.WriteIniFile(oldIniData, inputPath, false);

                IIniFileOperator migrator = MigratorInstanceCreator.CreateObsMigrator(relativePath, absoluteGoalDir);

                var fileStream = new FileStream(inputPath, FileMode.Open);

                var logHandler = Substitute.For<ILogHandler>();

                // Call 
                migrator.Invoke(fileStream, inputPath, logHandler);

                // Assert
                Assert.That(oldPolInfo.Exists, Is.False);

                var polFileInfo = new FileInfo(Path.Combine(absoluteGoalDir, polyLineFileName));
                Assert.That(polFileInfo.Exists, Is.True);
                Assert.That(File.ReadAllText(polFileInfo.FullName), Is.EqualTo(polyLineFileContent));

                var obsFileInfo = new FileInfo(Path.Combine(absoluteGoalDir, obstacleFileName));
                Assert.That(obsFileInfo.Exists, Is.True);

                IniData newIniData = new IniReader().ReadIniFile(obsFileInfo.OpenRead(), obsFileInfo.FullName);
                List<IniSection> newSections = newIniData.Sections.ToList();
                
                Assert.That(newSections.Count, Is.EqualTo(oldSections.Length));

                Assert.That(newSections[0].Name, Is.EqualTo(oldSections[0].Name));
                IniProperty[] newPropertiesObstacleFileInformation = newSections[0].Properties.ToArray();
                IniProperty[] oldPropertiesObstacleFileInformation = oldSections[0].Properties.ToArray();

                Assert.That(newPropertiesObstacleFileInformation.Length, Is.EqualTo(2));
                Assert.That(newPropertiesObstacleFileInformation[0].Key, Is.EqualTo(oldPropertiesObstacleFileInformation[0].Key));
                Assert.That(newPropertiesObstacleFileInformation[0].Value, Is.EqualTo(oldPropertiesObstacleFileInformation[0].Value));
                Assert.That(newPropertiesObstacleFileInformation[0].Comment, Is.EqualTo(oldPropertiesObstacleFileInformation[0].Comment));

                Assert.That(newPropertiesObstacleFileInformation[1].Key, Is.EqualTo(oldPropertiesObstacleFileInformation[1].Key));
                Assert.That(newPropertiesObstacleFileInformation[1].Value, Is.EqualTo(polyLineFileName));
                Assert.That(newPropertiesObstacleFileInformation[1].Comment, Is.EqualTo(oldPropertiesObstacleFileInformation[1].Comment));

                for (var i = 1; i < oldSections.Length; i++)
                {
                    Assert.That(newSections[i].Name, Is.EqualTo(oldSections[i].Name));

                    IniProperty[] oldProperties = oldSections[i].Properties.ToArray();
                    IniProperty[] newProperties = newSections[i].Properties.ToArray();

                    Assert.That(newProperties.Length, Is.EqualTo(oldProperties.Length));

                    for (var j = 0; j < oldProperties.Length; j++)
                    {
                        Assert.That(newProperties[j].Key, Is.EqualTo(oldProperties[j].Key));
                        Assert.That(newProperties[j].Value, Is.EqualTo(oldProperties[j].Value));
                        Assert.That(newProperties[j].Comment, Is.EqualTo(oldProperties[j].Comment));
                    }
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void CreateObsMigration_AbsolutePathWithSubdir_ProducesCorrectFunctioningMigrator()
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                const string inputDir = "temp_mdw_dir";
                string relativePath = tempDir.CreateDirectory(inputDir);

                const string polSubDir = "pol";
                tempDir.CreateDirectory(Path.Combine(inputDir, polSubDir));

                const string goalDir = "new_dir/input";
                string absoluteGoalDir = tempDir.CreateDirectory(goalDir);

                const string polyLineFileContent = "Poly joke beach, Cornwall, England";
                const string polyLineFileName = "cornwall.pol";

                string oldPolPath = tempDir.CreateFile(Path.Combine(inputDir, polSubDir, polyLineFileName), polyLineFileContent);
                var oldPolInfo = new FileInfo(oldPolPath);

                var obstacleFileInformation = new IniSection("ObstacleFileInformation");
                obstacleFileInformation.AddMultipleProperties(new[]
                {
                    new IniProperty("File", "100.0", ""),
                    new IniProperty("PolylineFile", oldPolPath, ""),
                });

                var oldSections = new IniSection[6];

                oldSections[0] = obstacleFileInformation;

                for (var i = 1; i < 6; i++)
                {
                    oldSections[i] = new IniSection("Obstacle");

                    oldSections[i].AddMultipleProperties(new[]
                    {
                        new IniProperty("Name", $"SomeName_{i}", ""),
                        new IniProperty("Type", "Sheet", ""),
                        new IniProperty("TransmCoef", "5.67", ""),
                        new IniProperty("Height", "4.56", ""),
                        new IniProperty("Alpha", "3.45", ""),
                        new IniProperty("Beta", "2.34", ""),
                        new IniProperty("Reflections", "specular", ""),
                        new IniProperty("ReflecCoef", "1.23", ""),
                    });
                }

                const string obstacleFileName = "cornwall.obs";
                string inputPath = Path.Combine(relativePath, obstacleFileName);

                var oldIniData = new IniData();
                oldIniData.AddMultipleSections(oldSections);

                var iniWriter = new IniWriter();
                iniWriter.WriteIniFile(oldIniData, inputPath, false);

                IIniFileOperator migrator = MigratorInstanceCreator.CreateObsMigrator(relativePath, absoluteGoalDir);

                var fileStream = new FileStream(inputPath, FileMode.Open);

                var logHandler = Substitute.For<ILogHandler>();

                // Call 
                migrator.Invoke(fileStream, inputPath, logHandler);

                // Assert
                Assert.That(oldPolInfo.Exists, Is.False);

                var polFileInfo = new FileInfo(Path.Combine(absoluteGoalDir, polyLineFileName));
                Assert.That(polFileInfo.Exists, Is.True);
                Assert.That(File.ReadAllText(polFileInfo.FullName), Is.EqualTo(polyLineFileContent));

                var obsFileInfo = new FileInfo(Path.Combine(absoluteGoalDir, obstacleFileName));
                Assert.That(obsFileInfo.Exists, Is.True);

                IniData newIniData = new IniReader().ReadIniFile(obsFileInfo.OpenRead(), obsFileInfo.FullName);
                List<IniSection> newSections = newIniData.Sections.ToList();
                
                Assert.That(newSections.Count, Is.EqualTo(oldSections.Length));

                Assert.That(newSections[0].Name, Is.EqualTo(oldSections[0].Name));
                IniProperty[] newPropertiesObstacleFileInformation = newSections[0].Properties.ToArray();
                IniProperty[] oldPropertiesObstacleFileInformation = oldSections[0].Properties.ToArray();

                Assert.That(newPropertiesObstacleFileInformation.Length, Is.EqualTo(2));
                Assert.That(newPropertiesObstacleFileInformation[0].Key, Is.EqualTo(oldPropertiesObstacleFileInformation[0].Key));
                Assert.That(newPropertiesObstacleFileInformation[0].Value, Is.EqualTo(oldPropertiesObstacleFileInformation[0].Value));
                Assert.That(newPropertiesObstacleFileInformation[0].Comment, Is.EqualTo(oldPropertiesObstacleFileInformation[0].Comment));

                Assert.That(newPropertiesObstacleFileInformation[1].Key, Is.EqualTo(oldPropertiesObstacleFileInformation[1].Key));
                Assert.That(newPropertiesObstacleFileInformation[1].Value, Is.EqualTo(polyLineFileName));
                Assert.That(newPropertiesObstacleFileInformation[1].Comment, Is.EqualTo(oldPropertiesObstacleFileInformation[1].Comment));

                for (var i = 1; i < oldSections.Length; i++)
                {
                    Assert.That(newSections[i].Name, Is.EqualTo(oldSections[i].Name));

                    IniProperty[] oldProperties = oldSections[i].Properties.ToArray();
                    IniProperty[] newProperties = newSections[i].Properties.ToArray();

                    Assert.That(newProperties.Length, Is.EqualTo(oldProperties.Length));

                    for (var j = 0; j < oldProperties.Length; j++)
                    {
                        Assert.That(newProperties[j].Key, Is.EqualTo(oldProperties[j].Key));
                        Assert.That(newProperties[j].Value, Is.EqualTo(oldProperties[j].Value));
                        Assert.That(newProperties[j].Comment, Is.EqualTo(oldProperties[j].Comment));
                    }
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [TestCase("grw.zip")]
        [TestCase("waddenzee.zip")]
        [TestCase("westerscheldt.zip")]
        public void CreateMdwMigration_ExpectedResults(string testFileName)
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                string inputDataPath = TestHelper.GetTestFilePath(Path.Combine("Migrations", "1.1.0.0", "MigratorFactoryTest", testFileName));

                string sourceModelFolder = Path.Combine(tempDir.Path, "source_model_folder");
                string sourceMdwPath = Path.Combine(sourceModelFolder, "waves.mdw");

                ZipFileUtils.Extract(inputDataPath, tempDir.Path);
                string resultPath = tempDir.CreateDirectory("result_model_folder/input");
                string resultMdwPath = Path.Combine(resultPath, "waves.mdw");

                string referenceModelFolder = Path.Combine(tempDir.Path, "reference_model_folder", "input");
                string referenceMdwPath = Path.Combine(referenceModelFolder, "waves.mdw");

                IIniFileOperator migrator = MigratorInstanceCreator.CreateMdwMigrator(sourceModelFolder, resultPath);

                var fileStream = new FileStream(sourceMdwPath, FileMode.Open);
                var logHandler = Substitute.For<ILogHandler>();

                // call 
                migrator.Invoke(fileStream, sourceMdwPath, logHandler);

                // Assert
                Assert.That(Directory.EnumerateFiles(sourceModelFolder), Is.Empty);

                string[] referenceFiles = Directory.GetFiles(referenceModelFolder);
                string[] resultFiles = Directory.GetFiles(resultPath);

                Assert.That(resultFiles.Length, Is.EqualTo(referenceFiles.Length));

                for (var i = 0; i < resultFiles.Length; i++)
                {
                    Assert.That(Path.GetFileName(resultFiles[i]),
                                Is.EqualTo(Path.GetFileName(referenceFiles[i])));
                }

                Assert.That(File.Exists(resultMdwPath), Is.True);

                var reader = new IniReader();
                IniData newIniData = reader.ReadIniFile(new FileStream(resultMdwPath, FileMode.Open), resultMdwPath);
                IniData referenceIniData = reader.ReadIniFile(new FileStream(referenceMdwPath, FileMode.Open), referenceMdwPath);

                List<IniSection> newSections = newIniData.Sections.ToList();
                List<IniSection> referenceSections = referenceIniData.Sections.ToList();

                for (var i = 0; i < newSections.Count; i++)
                {
                    Assert.That(newSections[i].Name, Is.EqualTo(referenceSections[i].Name));

                    IniProperty[] referenceProperties = referenceSections[i].Properties.ToArray();
                    IniProperty[] newProperties = newSections[i].Properties.ToArray();

                    Assert.That(newProperties.Length, Is.EqualTo(referenceProperties.Length));

                    for (var j = 0; j < referenceProperties.Length; j++)
                    {
                        Assert.That(newProperties[j].Key, Is.EqualTo(referenceProperties[j].Key));
                        Assert.That(newProperties[j].Value, Is.EqualTo(referenceProperties[j].Value));
                        Assert.That(newProperties[j].Comment, Is.EqualTo(referenceProperties[j].Comment));
                    }
                }

                foreach (string path in Directory.GetFiles(referenceModelFolder, "*.obs", SearchOption.TopDirectoryOnly))
                {
                    string newObsPath = Path.Combine(sourceMdwPath, Path.GetFileName(path));
                    IniData obsNewIniData = reader.ReadIniFile(new FileStream(newObsPath, FileMode.Open), newObsPath);
                    IniData obsReferenceIniData = reader.ReadIniFile(new FileStream(path, FileMode.Open), path);
                    
                    List<IniSection> obsNewSections = obsNewIniData.Sections.ToList();
                    List<IniSection> obsReferenceSections = obsReferenceIniData.Sections.ToList();

                    for (var i = 0; i < obsNewSections.Count; i++)
                    {
                        Assert.That(obsNewSections[i].Name, Is.EqualTo(obsReferenceSections[i].Name));

                        IniProperty[] obsReferenceProperties = obsReferenceSections[i].Properties.ToArray();
                        IniProperty[] obsNewProperties = obsNewSections[i].Properties.ToArray();

                        Assert.That(obsNewProperties.Length, Is.EqualTo(obsReferenceProperties.Length));

                        for (var j = 0; j < obsReferenceProperties.Length; j++)
                        {
                            Assert.That(obsNewProperties[j].Key, Is.EqualTo(obsReferenceProperties[j].Key));
                            Assert.That(obsNewProperties[j].Value, Is.EqualTo(obsReferenceProperties[j].Value));
                            Assert.That(obsNewProperties[j].Comment, Is.EqualTo(obsReferenceProperties[j].Comment));
                        }
                    }
                }
                
                Thread.Sleep(3000);
            }
        }
    }
}