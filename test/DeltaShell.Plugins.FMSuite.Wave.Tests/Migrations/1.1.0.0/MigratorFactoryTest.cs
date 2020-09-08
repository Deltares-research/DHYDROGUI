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
    public class MigratorFactoryTest
    {
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
                obstacleFileInformation.AddProperties( new []
                {
                    new DelftIniProperty("File", "100.0", ""),
                    new DelftIniProperty("PolyLineFile", polyLineFileName, ""), 
                });

                var oldCategories = new DelftIniCategory[6];

                oldCategories[0] = obstacleFileInformation;

                for (var i = 1; i < 6; i++)
                {
                    oldCategories[i] = new DelftIniCategory("Obstacle");

                    oldCategories[i].AddProperties(new []
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

                IDelftIniMigrator migrator = MigratorFactory.CreateObsMigrator(relativePath, absoluteGoalDir);

                var fileStream = new FileStream(inputPath, FileMode.Open);

                var logHandler = Substitute.For<ILogHandler>();

                // Call 
                migrator.MigrateFile(fileStream, inputPath, Path.Combine(absoluteGoalDir, obstacleFileName), logHandler);

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
                        Assert.That(newProperties[j].Name,    Is.EqualTo(oldProperties[j].Name));
                        Assert.That(newProperties[j].Value,   Is.EqualTo(oldProperties[j].Value));
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
                obstacleFileInformation.AddProperties( new []
                {
                    new DelftIniProperty("File", "100.0", ""),
                    new DelftIniProperty("PolyLineFile", $"./{polSubDir}/{polyLineFileName}", ""), 
                });

                var oldCategories = new DelftIniCategory[6];

                oldCategories[0] = obstacleFileInformation;

                for (var i = 1; i < 6; i++)
                {
                    oldCategories[i] = new DelftIniCategory("Obstacle");

                    oldCategories[i].AddProperties(new []
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

                IDelftIniMigrator migrator = MigratorFactory.CreateObsMigrator(relativePath, absoluteGoalDir);

                var fileStream = new FileStream(inputPath, FileMode.Open);

                var logHandler = Substitute.For<ILogHandler>();

                // Call 
                migrator.MigrateFile(fileStream, inputPath, Path.Combine(absoluteGoalDir, obstacleFileName), logHandler);

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
                        Assert.That(newProperties[j].Name,    Is.EqualTo(oldProperties[j].Name));
                        Assert.That(newProperties[j].Value,   Is.EqualTo(oldProperties[j].Value));
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
                obstacleFileInformation.AddProperties( new []
                {
                    new DelftIniProperty("File", "100.0", ""),
                    new DelftIniProperty("PolyLineFile", oldPolPath, ""), 
                });

                var oldCategories = new DelftIniCategory[6];

                oldCategories[0] = obstacleFileInformation;

                for (var i = 1; i < 6; i++)
                {
                    oldCategories[i] = new DelftIniCategory("Obstacle");

                    oldCategories[i].AddProperties(new []
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

                IDelftIniMigrator migrator = MigratorFactory.CreateObsMigrator(relativePath, absoluteGoalDir);

                var fileStream = new FileStream(inputPath, FileMode.Open);

                var logHandler = Substitute.For<ILogHandler>();

                // Call 
                migrator.MigrateFile(fileStream, inputPath, Path.Combine(absoluteGoalDir, obstacleFileName), logHandler);

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
                        Assert.That(newProperties[j].Name,    Is.EqualTo(oldProperties[j].Name));
                        Assert.That(newProperties[j].Value,   Is.EqualTo(oldProperties[j].Value));
                        Assert.That(newProperties[j].Comment, Is.EqualTo(oldProperties[j].Comment));
                    }
                }
            }
        }
    }
}