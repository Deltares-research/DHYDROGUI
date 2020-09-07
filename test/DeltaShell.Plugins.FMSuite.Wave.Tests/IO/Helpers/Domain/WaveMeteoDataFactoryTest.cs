using System;
using System.IO;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.Wind;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Domain;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Helpers.Domain
{
    [TestFixture]
    public class WaveMeteoDataFactoryTest
    {
        [TestCase(null)]
        [TestCase("")]
        public void CreateSpiderWebMeteoData_SpwFileNullOrEmpty_ThrowsArgumentException(string spwFile)
        {
            // Call
            void Call() => WaveMeteoDataFactory.CreateSpiderWebMeteoData(spwFile);

            // Assert
            var e = Assert.Throws<ArgumentException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("spwFile"));
        }

        [Test]
        public void CreateSpiderWebMeteoData_CreatesCorrectMeteoData()
        {
            // Call
            WaveMeteoData meteoData = WaveMeteoDataFactory.CreateSpiderWebMeteoData("path/to/file.spw");

            // Assert
            Assert.That(meteoData, Is.Not.Null);
            Assert.That(meteoData.FileType, Is.EqualTo(WindDefinitionType.SpiderWebGrid));
            Assert.That(meteoData.HasSpiderWeb, Is.True);
            Assert.That(meteoData.SpiderWebFilePath, Is.EqualTo("path/to/file.spw"));
        }

        [TestCase(null, "path/to/file", "xFile")]
        [TestCase("", "path/to/file", "xFile")]
        [TestCase("path/to/file", null, "yFile")]
        [TestCase("path/to/file", "", "yFile")]
        public void CreateXYComponentMeteoData_XFileOrYFileNullOrEmpty_ThrowsArgumentException(string xFile, string yFile, string expParamName)
        {
            // Call
            void Call() => WaveMeteoDataFactory.CreateXYComponentMeteoData(xFile, yFile);

            // Assert
            var e = Assert.Throws<ArgumentException>(Call);
            Assert.That(e.ParamName, Is.EqualTo(expParamName));
        }

        [Test]
        public void CreateXYComponentMeteoData_WithoutSpiderweb_CreatesCorrectMeteoData()
        {
            // Call
            WaveMeteoData meteoData = WaveMeteoDataFactory.CreateXYComponentMeteoData("path/to/x_file", "path/to/y_file");

            // Assert
            Assert.That(meteoData, Is.Not.Null);
            Assert.That(meteoData.FileType, Is.EqualTo(WindDefinitionType.WindXWindY));
            Assert.That(meteoData.HasSpiderWeb, Is.False);
            Assert.That(meteoData.SpiderWebFilePath, Is.Null);
            Assert.That(meteoData.XComponentFilePath, Is.EqualTo("path/to/x_file"));
            Assert.That(meteoData.YComponentFilePath, Is.EqualTo("path/to/y_file"));
        }

        [Test]
        public void CreateXYComponentMeteoData_WithSpiderweb_CreatesCorrectMeteoData()
        {
            // Call
            WaveMeteoData meteoData = WaveMeteoDataFactory.CreateXYComponentMeteoData("path/to/x_file", "path/to/y_file", "path/to/spiderweb");

            // Assert
            Assert.That(meteoData, Is.Not.Null);
            Assert.That(meteoData.FileType, Is.EqualTo(WindDefinitionType.WindXWindY));
            Assert.That(meteoData.HasSpiderWeb, Is.True);
            Assert.That(meteoData.SpiderWebFilePath, Is.EqualTo("path/to/spiderweb"));
            Assert.That(meteoData.XComponentFilePath, Is.EqualTo("path/to/x_file"));
            Assert.That(meteoData.YComponentFilePath, Is.EqualTo("path/to/y_file"));
        }

        [TestCase(null, "path/to/file", "file1")]
        [TestCase("", "path/to/file", "file1")]
        [TestCase("path/to/file", null, "file2")]
        [TestCase("path/to/file", "", "file2")]
        public void CreateWndXYComponentMeteoData_File1OrFile2NullOrEmpty_ThrowsArgumentException(string file1, string file2, string expParamName)
        {
            // Call
            void Call() => WaveMeteoDataFactory.CreateWndXYComponentMeteoData(file1, file2);

            // Assert
            var e = Assert.Throws<ArgumentException>(Call);
            Assert.That(e.ParamName, Is.EqualTo(expParamName));
        }

        [Test]
        public void CreateWndXYComponentMeteoData_File1DoesNotExist_ThrowsFileNotFoundException()
        {
            // Setup
            using (var temp = new TemporaryDirectory())
            {
                string file2 = temp.CreateFile("file2.wnd");

                // Call
                void Call() => WaveMeteoDataFactory.CreateWndXYComponentMeteoData("does/not/exist", file2);

                // Assert
                var e = Assert.Throws<FileNotFoundException>(Call);
                Assert.That(e.FileName, Is.EqualTo("does/not/exist"));
                Assert.That(e.Message, Is.EqualTo("Meteo file does not exist."));
            }
        }

        [Test]
        public void CreateWndXYComponentMeteoData_File2DoesNotExist_ThrowsFileNotFoundException()
        {
            // Setup
            using (var temp = new TemporaryDirectory())
            {
                string file1 = temp.CreateFile("file2.wnd");

                // Call
                void Call() => WaveMeteoDataFactory.CreateWndXYComponentMeteoData(file1, "does/not/exist");

                // Assert
                var e = Assert.Throws<FileNotFoundException>(Call);
                Assert.That(e.FileName, Is.EqualTo("does/not/exist"));
                Assert.That(e.Message, Is.EqualTo("Meteo file does not exist."));
            }
        }

        [Test]
        public void CreateWndXYComponentMeteoData_QuantitiesEqual_ReturnsNull()
        {
            // Setup
            using (var temp = new TemporaryDirectory())
            {
                string file1 = temp.CreateFile("file1.wnd", GetExampleContent("x_wind"));
                string file2 = temp.CreateFile("file2.wnd", GetExampleContent("x_wind"));

                // Call
                WaveMeteoData meteoData = WaveMeteoDataFactory.CreateWndXYComponentMeteoData(file1, file2);

                // Assert
                Assert.That(meteoData, Is.Null);
            }
        }

        [Test]
        public void CreateWndXYComponentMeteoData_UnsupportedQuantityFile1_ReturnsNull()
        {
            // Setup
            using (var temp = new TemporaryDirectory())
            {
                string file1 = temp.CreateFile("file1.wnd", GetExampleContent("the_wind"));
                string file2 = temp.CreateFile("file2.wnd", GetExampleContent("y_wind"));

                // Call
                WaveMeteoData meteoData = WaveMeteoDataFactory.CreateWndXYComponentMeteoData(file1, file2);

                // Assert
                Assert.That(meteoData, Is.Null);
            }
        }

        [Test]
        public void CreateWndXYComponentMeteoData_UnsupportedQuantityFile2_ReturnsNull()
        {
            // Setup
            using (var temp = new TemporaryDirectory())
            {
                string file1 = temp.CreateFile("file1.wnd", GetExampleContent("x_wind"));
                string file2 = temp.CreateFile("file2.wnd", GetExampleContent("the_wind"));

                // Call
                WaveMeteoData meteoData = WaveMeteoDataFactory.CreateWndXYComponentMeteoData(file1, file2);

                // Assert
                Assert.That(meteoData, Is.Null);
            }
        }

        [Test]
        public void CreateWndXYComponentMeteoData_QuantityNotSpecifiedInFile1_ReturnsCorrectMeteoData()
        {
            // Setup
            using (var temp = new TemporaryDirectory())
            {
                string file1 = temp.CreateFile("file1.wnd");
                string file2 = temp.CreateFile("file2.wnd", GetExampleContent("y_wind"));

                // Call
                WaveMeteoData meteoData = WaveMeteoDataFactory.CreateWndXYComponentMeteoData(file1, file2);

                // Assert
                Assert.That(meteoData, Is.Null);
            }
        }

        [Test]
        public void CreateWndXYComponentMeteoData_QuantityNotSpecifiedInFile2_ReturnsCorrectMeteoData()
        {
            // Setup
            using (var temp = new TemporaryDirectory())
            {
                string file1 = temp.CreateFile("file1.wnd", GetExampleContent("x_wind"));
                string file2 = temp.CreateFile("file2.wnd");

                // Call
                WaveMeteoData meteoData = WaveMeteoDataFactory.CreateWndXYComponentMeteoData(file1, file2);

                // Assert
                Assert.That(meteoData, Is.Null);
            }
        }

        [Test]
        public void CreateWndXYComponentMeteoData_SupportedQuantities_WithoutSpiderweb_ReturnsCorrectMeteoData()
        {
            // Setup
            using (var temp = new TemporaryDirectory())
            {
                string file1 = temp.CreateFile("file1.wnd", GetExampleContent("x_wind"));
                string file2 = temp.CreateFile("file2.wnd", GetExampleContent("y_wind"));

                // Call
                WaveMeteoData meteoData = WaveMeteoDataFactory.CreateWndXYComponentMeteoData(file1, file2);

                // Assert
                Assert.That(meteoData, Is.Not.Null);
                Assert.That(meteoData.FileType, Is.EqualTo(WindDefinitionType.WindXWindY));
                Assert.That(meteoData.HasSpiderWeb, Is.False);
                Assert.That(meteoData.SpiderWebFilePath, Is.Null);
                Assert.That(meteoData.XComponentFilePath, Is.EqualTo(file1));
                Assert.That(meteoData.YComponentFilePath, Is.EqualTo(file2));
            }
        }

        [Test]
        public void CreateWndXYComponentMeteoData_SupportedQuantities_Unordered_ReturnsCorrectMeteoData()
        {
            // Setup
            using (var temp = new TemporaryDirectory())
            {
                string file1 = temp.CreateFile("file1.wnd", GetExampleContent("y_wind"));
                string file2 = temp.CreateFile("file2.wnd", GetExampleContent("x_wind"));

                // Call
                WaveMeteoData meteoData = WaveMeteoDataFactory.CreateWndXYComponentMeteoData(file1, file2);

                // Assert
                Assert.That(meteoData, Is.Not.Null);
                Assert.That(meteoData.FileType, Is.EqualTo(WindDefinitionType.WindXWindY));
                Assert.That(meteoData.HasSpiderWeb, Is.False);
                Assert.That(meteoData.SpiderWebFilePath, Is.Null);
                Assert.That(meteoData.XComponentFilePath, Is.EqualTo(file2));
                Assert.That(meteoData.YComponentFilePath, Is.EqualTo(file1));
            }
        }

        [Test]
        public void CreateWndXYComponentMeteoData_SupportedQuantities_WithSpiderweb_ReturnsCorrectMeteoData()
        {
            // Setup
            using (var temp = new TemporaryDirectory())
            {
                string file1 = temp.CreateFile("file1.wnd", GetExampleContent("x_wind"));
                string file2 = temp.CreateFile("file2.wnd", GetExampleContent("y_wind"));

                // Call
                WaveMeteoData meteoData = WaveMeteoDataFactory.CreateWndXYComponentMeteoData(file1, file2, "path/to/spiderweb");

                // Assert
                Assert.That(meteoData, Is.Not.Null);
                Assert.That(meteoData.FileType, Is.EqualTo(WindDefinitionType.WindXWindY));
                Assert.That(meteoData.HasSpiderWeb, Is.True);
                Assert.That(meteoData.SpiderWebFilePath, Is.EqualTo("path/to/spiderweb"));
                Assert.That(meteoData.XComponentFilePath, Is.EqualTo(file1));
                Assert.That(meteoData.YComponentFilePath, Is.EqualTo(file2));
            }
        }

        [TestCase(null)]
        [TestCase("")]
        public void CreateVectorMeteoData_XYFileNullOrEmpty_ThrowsArgumentException(string xyFile)
        {
            // Call
            void Call() => WaveMeteoDataFactory.CreateVectorMeteoData(xyFile);

            // Assert
            var e = Assert.Throws<ArgumentException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("xyFile"));
        }

        [Test]
        public void CreateVectorMeteoData_WithoutSpiderweb_CreatesCorrectMeteoData()
        {
            // Call
            WaveMeteoData meteoData = WaveMeteoDataFactory.CreateVectorMeteoData("path/to/file");

            // Assert
            Assert.That(meteoData, Is.Not.Null);
            Assert.That(meteoData.FileType, Is.EqualTo(WindDefinitionType.WindXY));
            Assert.That(meteoData.HasSpiderWeb, Is.False);
            Assert.That(meteoData.SpiderWebFilePath, Is.Null);
            Assert.That(meteoData.XYVectorFilePath, Is.EqualTo("path/to/file"));
        }

        [Test]
        public void CreateVectorMeteoData_WithSpiderweb_CreatesCorrectMeteoData()
        {
            // Call
            WaveMeteoData meteoData = WaveMeteoDataFactory.CreateVectorMeteoData("path/to/file", "path/to/spiderweb");

            // Assert
            Assert.That(meteoData, Is.Not.Null);
            Assert.That(meteoData.FileType, Is.EqualTo(WindDefinitionType.WindXY));
            Assert.That(meteoData.HasSpiderWeb, Is.True);
            Assert.That(meteoData.SpiderWebFilePath, Is.EqualTo("path/to/spiderweb"));
            Assert.That(meteoData.XYVectorFilePath, Is.EqualTo("path/to/file"));
        }

        private static string GetExampleContent(string quantity)
        {
            return "FileVersion      = 1.03\n" +
                   "filetype = meteo_on_equidistant_grid\n" +
                   "NODATA_value = -999\n" +
                   "n_cols = 2\n" +
                   "n_rows = 2\n" +
                   "grid_unit = m\n" +
                   "x_llcorner = 1.00E+03\n" +
                   "y_llcorner = 1.00E+03\n" +
                   "dx = 2.44E+04\n" +
                   "dy = 3.00E+04\n" +
                   "n_quantity = 1\n" +
                   $"quantity1 = {quantity}\n" +
                   "unit1 = m s - 1\n";
        }
    }
}