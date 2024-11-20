using System;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.FileWriter
{
    [TestFixture]
    public class RRModelHybridFileWriterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void AddGreenhouse_And_WriteFiles_WritesCorrectFiles()
        {
            using (var temp = new TemporaryDirectory())
            {
                var writer = new RRModelHybridFileWriter();

                // Calls
                double[] areas = Enumerable.Repeat(3.33, 10).ToArray();
                writer.AddGreenhouse("some_id", areas, 1.23, 2.34, 3.45, 4.56, 5.67, false, 6.78, "some_meteo_station", 7.89, 1.11, 2.22);
                DoInDirectory(temp.Path, () => writer.WriteFiles());

                // Assert
                AssertSingleLine(Path.Combine(temp.Path, "greenhse.3b"),
                                 "GRHS id 'some_id' na 10  ar 3.33 3.33 3.33 3.33 3.33 3.33 3.33 3.33 3.33 3.33 sl 1.23 as 0 si 'some_id' sd 'some_id' ms 'some_meteo_station'  aaf 7.89 is 0.0 grhs");
                AssertSingleLine(Path.Combine(temp.Path, "greenhse.sil"),
                                 "SILO id 'some_id' nm 'some_id' sc 4.56 pc 5.67 silo");
                AssertSingleLine(Path.Combine(temp.Path, "greenhse.rf"),
                                 "STDF id 'some_id' nm 'some_id' mk 3.45 ik 2.34 stdf");
            }
        }

        private static void AssertSingleLine(string path, string expLine)
        {
            Assert.That(path, Does.Exist);
            string line = File.ReadAllLines(path).Single();
            Assert.That(line, Is.EqualTo(expLine));
        }

        private static void DoInDirectory(string path, Action action)
        {
            // D-Rainfall Runoff writes the files in the Environment.CurrentDirectory.
            // see: RainfallRunoffModelController.SwitchToWorkingDirectory()
            string currentDirectory = Environment.CurrentDirectory;
            try
            {
                Environment.CurrentDirectory = path;
                action();
            }
            finally
            {
                Environment.CurrentDirectory = currentDirectory;
            }
        }
    }
}