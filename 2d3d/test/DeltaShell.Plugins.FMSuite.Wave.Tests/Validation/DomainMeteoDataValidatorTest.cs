using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Common.Wind;
using DeltaShell.Plugins.FMSuite.Wave.Validation;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Validation
{
    [TestFixture]
    public class DomainMeteoDataValidatorTest
    {
        [Test]
        public void Validate_MeteoDataNull_ThrowsArgumentNullException()
        {
            // Call
            TestDelegate call = () => DomainMeteoDataValidator.Validate(null);

            // Assert
            Assert.That(call, Throws.TypeOf<ArgumentNullException>()
                                    .With.Property(nameof(ArgumentNullException.ParamName))
                                    .EqualTo("meteoData"));
        }

        [Test]
        [TestCaseSource(nameof(GetNullOrWhitespaceCases))]
        public void Validate_WindXY_XYVectorFilePathNullOrWhiteSpace_ReturnsExpectedMessage(string filepath)
        {
            // Setup
            var meteoData = new WaveMeteoData()
            {
                FileType = WindDefinitionType.WindXY,
                XYVectorFilePath = filepath,
                HasSpiderWeb = false
            };

            // Call
            IEnumerable<string> validationMessages = DomainMeteoDataValidator.Validate(meteoData);

            // Assert
            const string expectedMessage = "Use custom wind file option is switched on but no file has been selected.";
            Assert.That(validationMessages, Has.Count.EqualTo(1));

            string validationMessage = validationMessages.First();
            Assert.That(validationMessage, Is.EqualTo(expectedMessage));
        }

        [Test]
        [TestCaseSource(nameof(GetInvalidFilePathCases))]
        public void Validate_WindXY_InvalidXYVectorFilePath_ReturnsExpectedMessage(string filepath)
        {
            // Setup
            var meteoData = new WaveMeteoData()
            {
                FileType = WindDefinitionType.WindXY,
                XYVectorFilePath = filepath,
                HasSpiderWeb = false
            };

            // Call
            IEnumerable<string> validationMessages = DomainMeteoDataValidator.Validate(meteoData);

            // Assert
            const string expectedMessage = "The provided custom wind file does not exist.";
            Assert.That(validationMessages, Has.Count.EqualTo(1));

            string validationMessage = validationMessages.First();
            Assert.That(validationMessage, Is.EqualTo(expectedMessage));
        }

        [Test]
        [TestCaseSource(nameof(GetNullOrWhitespaceCases))]
        public void Validate_WindXY_HasSpiderWeb_SpiderWebFilePathNullOrWhitespace_ReturnsExpectedMessage(string filepath)
        {
            // Setup
            string randomXYVectorFilePath = Path.GetTempFileName();

            try
            {
                var meteoData = new WaveMeteoData()
                {
                    FileType = WindDefinitionType.WindXY,
                    XYVectorFilePath = randomXYVectorFilePath,
                    HasSpiderWeb = true,
                    SpiderWebFilePath = filepath
                };

                // Call
                IEnumerable<string> validationMessages = DomainMeteoDataValidator.Validate(meteoData);

                // Assert
                const string expectedMessage = "Use spider web file option is switched on but no file has been selected.";
                Assert.That(validationMessages, Has.Count.EqualTo(1));

                string validationMessage = validationMessages.First();
                Assert.That(validationMessage, Is.EqualTo(expectedMessage));
            }
            finally
            {
                File.Delete(randomXYVectorFilePath);
            }
        }

        [Test]
        [TestCaseSource(nameof(GetInvalidFilePathCases))]
        public void Validate_WindXY_HasSpiderWeb_InvalidSpiderWebFile_ReturnsExpectedMessage(string filepath)
        {
            // Setup
            string randomXYVectorFilePath = Path.GetTempFileName();

            try
            {
                var meteoData = new WaveMeteoData()
                {
                    FileType = WindDefinitionType.WindXY,
                    XYVectorFilePath = randomXYVectorFilePath,
                    HasSpiderWeb = true,
                    SpiderWebFilePath = filepath
                };

                // Call
                IEnumerable<string> validationMessages = DomainMeteoDataValidator.Validate(meteoData);

                // Assert
                const string expectedMessage = "The provided spider web file does not exist.";
                Assert.That(validationMessages, Has.Count.EqualTo(1));

                string validationMessage = validationMessages.First();
                Assert.That(validationMessage, Is.EqualTo(expectedMessage));
            }
            finally
            {
                File.Delete(randomXYVectorFilePath);
            }
        }

        [Test]
        public void Validate_WindXY_ValidFilePath_ReturnsNoMessages()
        {
            // Setup
            string randomXYVectorFilePath = Path.GetTempFileName();

            try
            {
                var meteoData = new WaveMeteoData()
                {
                    FileType = WindDefinitionType.WindXY,
                    XYVectorFilePath = randomXYVectorFilePath,
                    HasSpiderWeb = false
                };

                // Call
                IEnumerable<string> validationMessages = DomainMeteoDataValidator.Validate(meteoData);

                // Assert
                Assert.That(validationMessages, Is.Empty);
            }
            finally
            {
                File.Delete(randomXYVectorFilePath);
            }
        }

        [Test]
        public void Validate_WindXY_HasSpiderWeb_ValidFilePaths_ReturnsNoMessages()
        {
            // Setup
            string randomXYVectorFilePath = Path.GetTempFileName();
            string randomSpiderWebFile = Path.GetTempFileName();

            try
            {
                var meteoData = new WaveMeteoData()
                {
                    FileType = WindDefinitionType.WindXY,
                    XYVectorFilePath = randomXYVectorFilePath,
                    HasSpiderWeb = true,
                    SpiderWebFilePath = randomSpiderWebFile
                };

                // Call
                IEnumerable<string> validationMessages = DomainMeteoDataValidator.Validate(meteoData);

                // Assert
                Assert.That(validationMessages, Is.Empty);
            }
            finally
            {
                File.Delete(randomXYVectorFilePath);
                File.Delete(randomSpiderWebFile);
            }
        }

        [Test]
        [TestCaseSource(nameof(GetNullOrWhitespaceCases))]
        public void Validate_WindXWindY_XComponentFilePathNullOrWhitespace_ReturnsExpectedMessage(string filepath)
        {
            // Setup
            string randomYComponentFile = Path.GetTempFileName();

            try
            {
                var meteoData = new WaveMeteoData()
                {
                    FileType = WindDefinitionType.WindXWindY,
                    XComponentFilePath = filepath,
                    YComponentFilePath = randomYComponentFile,
                    HasSpiderWeb = false
                };

                // Call
                IEnumerable<string> validationMessages = DomainMeteoDataValidator.Validate(meteoData);

                // Assert
                const string expectedMessage = "Use custom wind file option is switched on but no x-component file has been selected.";
                Assert.That(validationMessages, Has.Count.EqualTo(1));

                string validationMessage = validationMessages.First();
                Assert.That(validationMessage, Is.EqualTo(expectedMessage));
            }
            finally
            {
                File.Delete(randomYComponentFile);
            }
        }

        [Test]
        [TestCaseSource(nameof(GetInvalidFilePathCases))]
        public void Validate_WindXWindY_InvalidXComponentFilePath_ReturnsExpectedMessage(string filepath)
        {
            // Setup
            string randomYComponentFile = Path.GetTempFileName();

            try
            {
                var meteoData = new WaveMeteoData()
                {
                    FileType = WindDefinitionType.WindXWindY,
                    XComponentFilePath = filepath,
                    YComponentFilePath = randomYComponentFile,
                    HasSpiderWeb = false
                };

                // Call
                IEnumerable<string> validationMessages = DomainMeteoDataValidator.Validate(meteoData);

                // Assert
                const string expectedMessage = "The provided x-component file does not exist.";
                Assert.That(validationMessages, Has.Count.EqualTo(1));

                string validationMessage = validationMessages.First();
                Assert.That(validationMessage, Is.EqualTo(expectedMessage));
            }
            finally
            {
                File.Delete(randomYComponentFile);
            }
        }

        [Test]
        [TestCaseSource(nameof(GetNullOrWhitespaceCases))]
        public void Validate_WindXWindY_YComponentFilePathNullOrWhitespace_ReturnsExpectedMessage(string filepath)
        {
            // Setup
            string randomXComponentFile = Path.GetTempFileName();

            try
            {
                var meteoData = new WaveMeteoData()
                {
                    FileType = WindDefinitionType.WindXWindY,
                    XComponentFilePath = randomXComponentFile,
                    YComponentFilePath = filepath,
                    HasSpiderWeb = false
                };

                // Call
                IEnumerable<string> validationMessages = DomainMeteoDataValidator.Validate(meteoData);

                // Assert
                const string expectedMessage = "Use custom wind file option is switched on but no y-component file has been selected.";
                Assert.That(validationMessages, Has.Count.EqualTo(1));

                string validationMessage = validationMessages.First();
                Assert.That(validationMessage, Is.EqualTo(expectedMessage));
            }
            finally
            {
                File.Delete(randomXComponentFile);
            }
        }

        [Test]
        [TestCaseSource(nameof(GetInvalidFilePathCases))]
        public void Validate_WindXWindY_InvalidYComponentFilePath_ReturnsExpectedMessage(string filepath)
        {
            // Setup
            string randomXComponentFile = Path.GetTempFileName();

            try
            {
                var meteoData = new WaveMeteoData()
                {
                    FileType = WindDefinitionType.WindXWindY,
                    XComponentFilePath = randomXComponentFile,
                    YComponentFilePath = filepath,
                    HasSpiderWeb = false
                };

                // Call
                IEnumerable<string> validationMessages = DomainMeteoDataValidator.Validate(meteoData);

                // Assert
                const string expectedMessage = "The provided y-component file does not exist.";
                Assert.That(validationMessages, Has.Count.EqualTo(1));

                string validationMessage = validationMessages.First();
                Assert.That(validationMessage, Is.EqualTo(expectedMessage));
            }
            finally
            {
                File.Delete(randomXComponentFile);
            }
        }

        [Test]
        [TestCaseSource(nameof(GetNullOrWhitespaceCases))]
        public void Validate_WindXWindY_HasSpiderWeb_SpiderWebFilePathNullOrWhitespace_ReturnsExpectedMessage(string filepath)
        {
            // Setup
            string randomXComponentFile = Path.GetTempFileName();
            string randomYComponentFile = Path.GetTempFileName();

            try
            {
                var meteoData = new WaveMeteoData()
                {
                    FileType = WindDefinitionType.WindXWindY,
                    XComponentFilePath = randomXComponentFile,
                    YComponentFilePath = randomYComponentFile,
                    HasSpiderWeb = true,
                    SpiderWebFilePath = filepath
                };

                // Call
                IEnumerable<string> validationMessages = DomainMeteoDataValidator.Validate(meteoData);

                // Assert
                const string expectedMessage = "Use spider web file option is switched on but no file has been selected.";
                Assert.That(validationMessages, Has.Count.EqualTo(1));

                string validationMessage = validationMessages.First();
                Assert.That(validationMessage, Is.EqualTo(expectedMessage));
            }
            finally
            {
                File.Delete(randomXComponentFile);
                File.Delete(randomYComponentFile);
            }
        }

        [Test]
        [TestCaseSource(nameof(GetInvalidFilePathCases))]
        public void Validate_WindXWindY_HasSpiderWeb_InvalidSpiderWebFilePath_ReturnsExpectedMessage(string filepath)
        {
            // Setup
            string randomXComponentFile = Path.GetTempFileName();
            string randomYComponentFile = Path.GetTempFileName();

            try
            {
                var meteoData = new WaveMeteoData()
                {
                    FileType = WindDefinitionType.WindXWindY,
                    XComponentFilePath = randomXComponentFile,
                    YComponentFilePath = randomYComponentFile,
                    HasSpiderWeb = true,
                    SpiderWebFilePath = filepath
                };

                // Call
                IEnumerable<string> validationMessages = DomainMeteoDataValidator.Validate(meteoData);

                // Assert
                const string expectedMessage = "The provided spider web file does not exist.";
                Assert.That(validationMessages, Has.Count.EqualTo(1));

                string validationMessage = validationMessages.First();
                Assert.That(validationMessage, Is.EqualTo(expectedMessage));
            }
            finally
            {
                File.Delete(randomXComponentFile);
                File.Delete(randomYComponentFile);
            }
        }

        [Test]
        public void Validate_WindXWindY_ValidFilePaths_ReturnsNoMessages()
        {
            // Setup
            string randomXComponentFile = Path.GetTempFileName();
            string randomYComponentFile = Path.GetTempFileName();

            try
            {
                var meteoData = new WaveMeteoData()
                {
                    FileType = WindDefinitionType.WindXWindY,
                    XComponentFilePath = randomXComponentFile,
                    YComponentFilePath = randomYComponentFile,
                    HasSpiderWeb = false
                };

                // Call
                IEnumerable<string> validationMessages = DomainMeteoDataValidator.Validate(meteoData);

                // Assert
                Assert.That(validationMessages, Is.Empty);
            }
            finally
            {
                File.Delete(randomXComponentFile);
                File.Delete(randomYComponentFile);
            }
        }

        [Test]
        public void Validate_WindXWindY_HasSpiderWeb_ValidFilePaths_ReturnsNoMessages()
        {
            // Setup
            string randomXComponentFile = Path.GetTempFileName();
            string randomYComponentFile = Path.GetTempFileName();
            string randomSpiderWebFile = Path.GetTempFileName();

            try
            {
                var meteoData = new WaveMeteoData()
                {
                    FileType = WindDefinitionType.WindXWindY,
                    XComponentFilePath = randomXComponentFile,
                    YComponentFilePath = randomYComponentFile,
                    HasSpiderWeb = true,
                    SpiderWebFilePath = randomSpiderWebFile
                };

                // Call
                IEnumerable<string> validationMessages = DomainMeteoDataValidator.Validate(meteoData);

                // Assert
                Assert.That(validationMessages, Is.Empty);
            }
            finally
            {
                File.Delete(randomXComponentFile);
                File.Delete(randomYComponentFile);
                File.Delete(randomSpiderWebFile);
            }
        }

        [Test]
        [TestCaseSource(nameof(GetNullOrWhitespaceCases))]
        public void Validate_SpiderWebGrid_SpiderWebFilePathEmptyOrWhiteSpace_ReturnsExpectedMessage(string filepath)
        {
            // Setup
            var meteoData = new WaveMeteoData()
            {
                FileType = WindDefinitionType.SpiderWebGrid,
                SpiderWebFilePath = filepath,
            };

            // Call
            IEnumerable<string> validationMessages = DomainMeteoDataValidator.Validate(meteoData);

            // Assert
            const string expectedMessage = "Use spider web file option is switched on but no file has been selected.";
            Assert.That(validationMessages, Has.Count.EqualTo(1));

            string validationMessage = validationMessages.First();
            Assert.That(validationMessage, Is.EqualTo(expectedMessage));
        }

        [Test]
        [TestCaseSource(nameof(GetInvalidFilePathCases))]
        public void Validate_SpiderWebGrid_InvalidSpiderWebGridFilePath_ReturnsExpectedMessage(string filepath)
        {
            // Setup
            var meteoData = new WaveMeteoData()
            {
                FileType = WindDefinitionType.SpiderWebGrid,
                SpiderWebFilePath = filepath
            };

            // Call
            IEnumerable<string> validationMessages = DomainMeteoDataValidator.Validate(meteoData);

            // Assert
            const string expectedMessage = "The provided spider web file does not exist.";
            Assert.That(validationMessages, Has.Count.EqualTo(1));

            string validationMessage = validationMessages.First();
            Assert.That(validationMessage, Is.EqualTo(expectedMessage));
        }

        [Test]
        public void Validate_SpiderWebGrid_ValidFilePath_ReturnsNoMessages()
        {
            // Setup
            string randomSpiderWebFile = Path.GetTempFileName();

            try
            {
                var meteoData = new WaveMeteoData()
                {
                    FileType = WindDefinitionType.SpiderWebGrid,
                    SpiderWebFilePath = randomSpiderWebFile
                };

                // Call
                IEnumerable<string> validationMessages = DomainMeteoDataValidator.Validate(meteoData);

                // Assert
                Assert.That(validationMessages, Is.Empty);
            }
            finally
            {
                File.Delete(randomSpiderWebFile);
            }
        }

        [Test]
        public void Validate_WindXYP_ReturnsNoMessages()
        {
            // Setup
            var meteoData = new WaveMeteoData() {FileType = WindDefinitionType.WindXYP};

            // Call
            IEnumerable<string> validationMessages = DomainMeteoDataValidator.Validate(meteoData);

            // Assert
            Assert.That(validationMessages, Is.Empty);
        }

        [Test]
        public void Validate_UnsupportedWindDefinitionType_ThrowsNotSupportedException()
        {
            // Setup
            var meteoData = new WaveMeteoData() {FileType = (WindDefinitionType) 80085};

            // Call
            TestDelegate call = () => DomainMeteoDataValidator.Validate(meteoData);

            // Assert
            Assert.That(call, Throws.TypeOf<NotSupportedException>());
        }
        
        private static IEnumerable<string> GetNullOrWhitespaceCases()
        {
            yield return null;
            yield return "";
            yield return "    ";
        }
        
        private static IEnumerable<string> GetInvalidFilePathCases()
        {
            yield return "InvalidFilePath";
            yield return "C:\\NonExistingFilePath.txt";
        }
    }
}