using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.Wind;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.DomainSpecificDataEditor.ViewModels
{
    [TestFixture]
    public class WindSettingsViewModelTest
    {
        private WaveMeteoData meteoData;
        private WindSettingsViewModel viewModel;

        [SetUp]
        public void Setup()
        {
            meteoData = new WaveMeteoData();
            viewModel = new WindSettingsViewModel(meteoData);
        }

        [Test]
        public void GetXComponentFilePath_ReturnsCorrectValue()
        {
            // Setup
            const string value = "property_value";
            meteoData.XComponentFilePath = value;

            // Call
            string resultedValue = viewModel.XComponentFilePath;

            // Assert
            Assert.That(resultedValue, Is.EqualTo(value));
        }

        [Test]
        public void GetYComponentFilePath_ReturnsCorrectValue()
        {
            // Setup
            const string value = "property_value";
            meteoData.YComponentFilePath = value;

            // Call
            string resultedValue = viewModel.YComponentFilePath;

            // Assert
            Assert.That(resultedValue, Is.EqualTo(value));
        }

        [Test]
        public void GetSpiderWebFilePath_ReturnsCorrectValue()
        {
            // Setup
            const string value = "property_value";
            meteoData.SpiderWebFilePath = value;

            // Call
            string resultedValue = viewModel.SpiderWebFilePath;

            // Assert
            Assert.That(resultedValue, Is.EqualTo(value));
        }

        [Test]
        public void GetWindVelocityFilePath_ReturnsCorrectValue()
        {
            // Setup
            const string value = "property_value";
            meteoData.XYVectorFilePath = value;

            // Call
            string resultedValue = viewModel.WindVelocityFilePath;

            // Assert
            Assert.That(resultedValue, Is.EqualTo(value));
        }

        [TestCase(WindDefinitionType.WindXY, WindInputType.WindVector)]
        [TestCase(WindDefinitionType.SpiderWebGrid, WindInputType.SpiderWebGrid)]
        [TestCase(WindDefinitionType.WindXWindY, WindInputType.XYComponents)]
        [TestCase(WindDefinitionType.WindXYP, WindInputType.WindVector)]
        public void GetInputType_ReturnsCorrectValue(WindDefinitionType fileType,
                                                     WindInputType expectedInputType)
        {
            // Setup
            meteoData.FileType = fileType;

            // Call
            WindInputType resultedInputType = viewModel.InputType;

            // Assert
            Assert.That(resultedInputType, Is.EqualTo(expectedInputType));
        }

        [TestCase(WindInputType.WindVector, WindInputType.WindVector, WindDefinitionType.WindXY, 0)]
        [TestCase(WindInputType.SpiderWebGrid, WindInputType.WindVector, WindDefinitionType.WindXY, 1)]
        [TestCase(WindInputType.XYComponents, WindInputType.WindVector, WindDefinitionType.WindXY, 1)]
        [TestCase(WindInputType.WindVector, WindInputType.SpiderWebGrid, WindDefinitionType.SpiderWebGrid, 1)]
        [TestCase(WindInputType.SpiderWebGrid, WindInputType.SpiderWebGrid, WindDefinitionType.SpiderWebGrid, 0)]
        [TestCase(WindInputType.XYComponents, WindInputType.SpiderWebGrid, WindDefinitionType.SpiderWebGrid, 1)]
        [TestCase(WindInputType.WindVector, WindInputType.XYComponents, WindDefinitionType.WindXWindY, 1)]
        [TestCase(WindInputType.SpiderWebGrid, WindInputType.XYComponents, WindDefinitionType.WindXWindY, 1)]
        [TestCase(WindInputType.XYComponents, WindInputType.XYComponents, WindDefinitionType.WindXWindY, 0)]
        public void SetInputType_SetsCorrectPropertyValueOnModel(WindInputType originalValue,
                                                                 WindInputType setValue,
                                                                 WindDefinitionType expectedFileType,
                                                                 int expectedPropertyChangedCount)
        {
            // Setup
            viewModel.InputType = originalValue;

            // Call
            void Call() => viewModel.InputType = setValue;

            // Assert
            viewModel.AssertPropertyChangedFired(Call, expectedPropertyChangedCount, nameof(viewModel.InputType));
            Assert.That(meteoData.FileType, Is.EqualTo(expectedFileType));
        }

        [TestCase("original", 0)]
        [TestCase("new", 1)]
        public void SetXComponentFilePath_SetsCorrectPropertyValueOnModel(
            string setValue, int expectedPropertyChangedCount)
        {
            // Setup
            viewModel.XComponentFilePath = "original";

            // Call
            void Call() => viewModel.XComponentFilePath = setValue;

            // Assert
            viewModel.AssertPropertyChangedFired(Call, expectedPropertyChangedCount, nameof(viewModel.XComponentFilePath));
            Assert.That(meteoData.XComponentFileName, Is.EqualTo(setValue));
        }

        [TestCase("original", 0)]
        [TestCase("new", 1)]
        public void SetYComponentFilePath_SetsCorrectPropertyValueOnModel(
            string setValue, int expectedPropertyChangedCount)
        {
            // Setup
            viewModel.YComponentFilePath = "original";

            // Call
            void Call() => viewModel.YComponentFilePath = setValue;

            // Assert
            viewModel.AssertPropertyChangedFired(Call, expectedPropertyChangedCount, nameof(viewModel.YComponentFilePath));
            Assert.That(meteoData.YComponentFileName, Is.EqualTo(setValue));
        }

        [TestCase("original", 0)]
        [TestCase("new", 1)]
        public void SetSpiderWebFilePath_SetsCorrectPropertyValueOnModel(
            string setValue, int expectedPropertyChangedCount)
        {
            // Setup
            viewModel.SpiderWebFilePath = "original";

            // Call
            void Call() => viewModel.SpiderWebFilePath = setValue;

            // Assert
            viewModel.AssertPropertyChangedFired(Call, expectedPropertyChangedCount, nameof(viewModel.SpiderWebFilePath));
            Assert.That(meteoData.SpiderWebFileName, Is.EqualTo(setValue));
        }

        [TestCase("original", 0)]
        [TestCase("new", 1)]
        public void SetWindVelocityFilePath_SetsCorrectPropertyValueOnModel(
            string setValue, int expectedPropertyChangedCount)
        {
            // Setup
            viewModel.WindVelocityFilePath = "original";

            // Call
            void Call() => viewModel.WindVelocityFilePath = setValue;

            // Assert
            viewModel.AssertPropertyChangedFired(Call, expectedPropertyChangedCount, nameof(viewModel.WindVelocityFilePath));
            Assert.That(meteoData.XYVectorFileName, Is.EqualTo(setValue));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GetWindVelocityFilePath_ReturnsCorrectValue(bool value)
        {
            // Setup
            meteoData.HasSpiderWeb = value;

            // Call
            bool resultedValue = viewModel.UseSpiderWebGrid;

            // Assert
            Assert.That(resultedValue, Is.EqualTo(value));
        }

        [TestCase(true, true, 0)]
        [TestCase(false, false, 0)]
        [TestCase(true, false, 1)]
        [TestCase(false, true, 1)]
        public void SetUseSpiderWebGrid_SetsCorrectPropertyValueOnModel(bool originalValue, bool setValue,
                                                                        int expectedPropertyChangedCount)
        {
            // Setup
            viewModel.UseSpiderWebGrid = originalValue;

            // Call
            void Call() => viewModel.UseSpiderWebGrid = setValue;

            // Assert
            viewModel.AssertPropertyChangedFired(Call, expectedPropertyChangedCount, nameof(viewModel.UseSpiderWebGrid));
            Assert.That(meteoData.HasSpiderWeb, Is.EqualTo(setValue));
        }
    }
}