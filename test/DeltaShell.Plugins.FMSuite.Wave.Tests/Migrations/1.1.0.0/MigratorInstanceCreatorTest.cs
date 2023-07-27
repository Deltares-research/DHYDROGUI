using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.DelftIniOperations;
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

                var obstacleFileInformation = new DelftIniCategory("ObstacleFileInformation");
                obstacleFileInformation.AddProperties(new[]
                {
                    new DelftIniProperty("File", "100.0", ""),
                    new DelftIniProperty("PolylineFile", polyLineFileName, ""),
                });

                var oldCategories = new DelftIniCategory[6];

                oldCategories[0] = obstacleFileInformation;

                for (var i = 1; i < 6; i++)
                {
                    oldCategories[i] = new DelftIniCategory("Obstacle");

                    oldCategories[i].AddProperties(new[]
                    {
                        new DelftIniProperty("Name", $"SomeName_{i}", ""),
                        new DelftIniProperty("Type", "Sheet", ""),
                        new DelftIniProperty("TransmCoef", "5.67", ""),
                        new DelftIniProperty("Height", "4.56", ""),
                        new DelftIniProperty("Alpha", "3.45", ""),
                        new DelftIniProperty("Beta", "2.34", ""),
                        new DelftIniProperty("Reflections", "specular", ""),
                        new DelftIniProperty("ReflecCoef", "1.23", ""),
                    });
                }

                const string obstacleFileName = "cornwall.obs";
                string inputPath = Path.Combine(relativePath, obstacleFileName);

                var delftIniWriter = new DelftIniWriter();
                delftIniWriter.WriteDelftIniFile(oldCategories, inputPath, false);

                IDelftIniFileOperator migrator = MigratorInstanceCreator.CreateObsMigrator(relativePath, absoluteGoalDir);

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

                IList<DelftIniCategory> newCategories = new DelftIniReader().ReadDelftIniFile(obsFileInfo.OpenRead(),
                                                                                              obsFileInfo.FullName);

                Assert.That(newCategories.Count, Is.EqualTo(oldCategories.Length));

                for (var i = 0; i < oldCategories.Length; i++)
                {
                    Assert.That(newCategories[i].Name, Is.EqualTo(oldCategories[i].Name));

                    DelftIniProperty[] oldProperties = oldCategories[i].Properties.ToArray();
                    DelftIniProperty[] newProperties = newCategories[i].Properties.ToArray();

                    Assert.That(newProperties.Length, Is.EqualTo(oldProperties.Length));

                    for (var j = 0; j < oldProperties.Length; j++)
                    {
                        Assert.That(newProperties[j].Name, Is.EqualTo(oldProperties[j].Name));
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

                var obstacleFileInformation = new DelftIniCategory("ObstacleFileInformation");
                obstacleFileInformation.AddProperties(new[]
                {
                    new DelftIniProperty("File", "100.0", ""),
                    new DelftIniProperty("PolylineFile", $"./{polSubDir}/{polyLineFileName}", ""),
                });

                var oldCategories = new DelftIniCategory[6];

                oldCategories[0] = obstacleFileInformation;

                for (var i = 1; i < 6; i++)
                {
                    oldCategories[i] = new DelftIniCategory("Obstacle");

                    oldCategories[i].AddProperties(new[]
                    {
                        new DelftIniProperty("Name", $"SomeName_{i}", ""),
                        new DelftIniProperty("Type", "Sheet", ""),
                        new DelftIniProperty("TransmCoef", "5.67", ""),
                        new DelftIniProperty("Height", "4.56", ""),
                        new DelftIniProperty("Alpha", "3.45", ""),
                        new DelftIniProperty("Beta", "2.34", ""),
                        new DelftIniProperty("Reflections", "specular", ""),
                        new DelftIniProperty("ReflecCoef", "1.23", ""),
                    });
                }

                const string obstacleFileName = "cornwall.obs";
                string inputPath = Path.Combine(relativePath, obstacleFileName);

                var delftIniWriter = new DelftIniWriter();
                delftIniWriter.WriteDelftIniFile(oldCategories, inputPath, false);

                IDelftIniFileOperator migrator = MigratorInstanceCreator.CreateObsMigrator(relativePath, absoluteGoalDir);

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

                IList<DelftIniCategory> newCategories = new DelftIniReader().ReadDelftIniFile(obsFileInfo.OpenRead(),
                                                                                              obsFileInfo.FullName);

                Assert.That(newCategories.Count, Is.EqualTo(oldCategories.Length));

                Assert.That(newCategories[0].Name, Is.EqualTo(oldCategories[0].Name));
                DelftIniProperty[] newPropertiesObstacleFileInformation =
                    newCategories[0].Properties.ToArray();

                DelftIniProperty[] oldPropertiesObstacleFileInformation =
                    oldCategories[0].Properties.ToArray();

                Assert.That(newPropertiesObstacleFileInformation.Length, Is.EqualTo(2));
                Assert.That(newPropertiesObstacleFileInformation[0].Name, Is.EqualTo(oldPropertiesObstacleFileInformation[0].Name));
                Assert.That(newPropertiesObstacleFileInformation[0].Value, Is.EqualTo(oldPropertiesObstacleFileInformation[0].Value));
                Assert.That(newPropertiesObstacleFileInformation[0].Comment, Is.EqualTo(oldPropertiesObstacleFileInformation[0].Comment));

                Assert.That(newPropertiesObstacleFileInformation[1].Name, Is.EqualTo(oldPropertiesObstacleFileInformation[1].Name));
                Assert.That(newPropertiesObstacleFileInformation[1].Value, Is.EqualTo(polyLineFileName));
                Assert.That(newPropertiesObstacleFileInformation[1].Comment, Is.EqualTo(oldPropertiesObstacleFileInformation[1].Comment));

                for (var i = 1; i < oldCategories.Length; i++)
                {
                    Assert.That(newCategories[i].Name, Is.EqualTo(oldCategories[i].Name));

                    DelftIniProperty[] oldProperties = oldCategories[i].Properties.ToArray();
                    DelftIniProperty[] newProperties = newCategories[i].Properties.ToArray();

                    Assert.That(newProperties.Length, Is.EqualTo(oldProperties.Length));

                    for (var j = 0; j < oldProperties.Length; j++)
                    {
                        Assert.That(newProperties[j].Name, Is.EqualTo(oldProperties[j].Name));
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

                var obstacleFileInformation = new DelftIniCategory("ObstacleFileInformation");
                obstacleFileInformation.AddProperties(new[]
                {
                    new DelftIniProperty("File", "100.0", ""),
                    new DelftIniProperty("PolylineFile", oldPolPath, ""),
                });

                var oldCategories = new DelftIniCategory[6];

                oldCategories[0] = obstacleFileInformation;

                for (var i = 1; i < 6; i++)
                {
                    oldCategories[i] = new DelftIniCategory("Obstacle");

                    oldCategories[i].AddProperties(new[]
                    {
                        new DelftIniProperty("Name", $"SomeName_{i}", ""),
                        new DelftIniProperty("Type", "Sheet", ""),
                        new DelftIniProperty("TransmCoef", "5.67", ""),
                        new DelftIniProperty("Height", "4.56", ""),
                        new DelftIniProperty("Alpha", "3.45", ""),
                        new DelftIniProperty("Beta", "2.34", ""),
                        new DelftIniProperty("Reflections", "specular", ""),
                        new DelftIniProperty("ReflecCoef", "1.23", ""),
                    });
                }

                const string obstacleFileName = "cornwall.obs";
                string inputPath = Path.Combine(relativePath, obstacleFileName);

                var delftIniWriter = new DelftIniWriter();
                delftIniWriter.WriteDelftIniFile(oldCategories, inputPath, false);

                IDelftIniFileOperator migrator = MigratorInstanceCreator.CreateObsMigrator(relativePath, absoluteGoalDir);

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

                IList<DelftIniCategory> newCategories = new DelftIniReader().ReadDelftIniFile(obsFileInfo.OpenRead(),
                                                                                              obsFileInfo.FullName);

                Assert.That(newCategories.Count, Is.EqualTo(oldCategories.Length));

                Assert.That(newCategories[0].Name, Is.EqualTo(oldCategories[0].Name));
                DelftIniProperty[] newPropertiesObstacleFileInformation =
                    newCategories[0].Properties.ToArray();

                DelftIniProperty[] oldPropertiesObstacleFileInformation =
                    oldCategories[0].Properties.ToArray();

                Assert.That(newPropertiesObstacleFileInformation.Length, Is.EqualTo(2));
                Assert.That(newPropertiesObstacleFileInformation[0].Name, Is.EqualTo(oldPropertiesObstacleFileInformation[0].Name));
                Assert.That(newPropertiesObstacleFileInformation[0].Value, Is.EqualTo(oldPropertiesObstacleFileInformation[0].Value));
                Assert.That(newPropertiesObstacleFileInformation[0].Comment, Is.EqualTo(oldPropertiesObstacleFileInformation[0].Comment));

                Assert.That(newPropertiesObstacleFileInformation[1].Name, Is.EqualTo(oldPropertiesObstacleFileInformation[1].Name));
                Assert.That(newPropertiesObstacleFileInformation[1].Value, Is.EqualTo(polyLineFileName));
                Assert.That(newPropertiesObstacleFileInformation[1].Comment, Is.EqualTo(oldPropertiesObstacleFileInformation[1].Comment));

                for (var i = 1; i < oldCategories.Length; i++)
                {
                    Assert.That(newCategories[i].Name, Is.EqualTo(oldCategories[i].Name));

                    DelftIniProperty[] oldProperties = oldCategories[i].Properties.ToArray();
                    DelftIniProperty[] newProperties = newCategories[i].Properties.ToArray();

                    Assert.That(newProperties.Length, Is.EqualTo(oldProperties.Length));

                    for (var j = 0; j < oldProperties.Length; j++)
                    {
                        Assert.That(newProperties[j].Name, Is.EqualTo(oldProperties[j].Name));
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

                IDelftIniFileOperator migrator = MigratorInstanceCreator.CreateMdwMigrator(sourceModelFolder, resultPath);

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

                var reader = new DelftIniReader();
                IList<DelftIniCategory> newCategories =
                    reader.ReadDelftIniFile(new FileStream(resultMdwPath, FileMode.Open), resultMdwPath);

                IList<DelftIniCategory> referenceCategories =
                    reader.ReadDelftIniFile(new FileStream(referenceMdwPath, FileMode.Open), referenceMdwPath);

                for (var i = 0; i < newCategories.Count; i++)
                {
                    Assert.That(newCategories[i].Name, Is.EqualTo(referenceCategories[i].Name));

                    DelftIniProperty[] referenceProperties = referenceCategories[i].Properties.ToArray();
                    DelftIniProperty[] newProperties = newCategories[i].Properties.ToArray();

                    Assert.That(newProperties.Length, Is.EqualTo(referenceProperties.Length));

                    for (var j = 0; j < referenceProperties.Length; j++)
                    {
                        Assert.That(newProperties[j].Name, Is.EqualTo(referenceProperties[j].Name));
                        Assert.That(newProperties[j].Value, Is.EqualTo(referenceProperties[j].Value));
                        Assert.That(newProperties[j].Comment, Is.EqualTo(referenceProperties[j].Comment));
                    }
                }

                foreach (string path in Directory.GetFiles(referenceModelFolder, "*.obs", SearchOption.TopDirectoryOnly))
                {
                    string newObsPath = Path.Combine(sourceMdwPath, Path.GetFileName(path));
                    IList<DelftIniCategory> obsNewCategories =
                        reader.ReadDelftIniFile(new FileStream(newObsPath, FileMode.Open), newObsPath);

                    IList<DelftIniCategory> obsReferenceCategories =
                        reader.ReadDelftIniFile(new FileStream(path, FileMode.Open), path);

                    for (var i = 0; i < obsNewCategories.Count; i++)
                    {
                        Assert.That(obsNewCategories[i].Name, Is.EqualTo(obsReferenceCategories[i].Name));

                        DelftIniProperty[] obsReferenceProperties = obsReferenceCategories[i].Properties.ToArray();
                        DelftIniProperty[] obsNewProperties = obsNewCategories[i].Properties.ToArray();

                        Assert.That(obsNewProperties.Length, Is.EqualTo(obsReferenceProperties.Length));

                        for (var j = 0; j < obsReferenceProperties.Length; j++)
                        {
                            Assert.That(obsNewProperties[j].Name, Is.EqualTo(obsReferenceProperties[j].Name));
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