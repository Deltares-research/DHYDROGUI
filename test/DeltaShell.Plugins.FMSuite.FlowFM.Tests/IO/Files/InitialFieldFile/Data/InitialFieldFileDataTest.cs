using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Data;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.InitialFieldFile.Data
{
    [TestFixture]
    public class InitialFieldFileDataTest
    {
        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var initialFieldFileData = new InitialFieldFileData();

            // Assert
            Assert.That(initialFieldFileData.General.FileVersion, Is.EqualTo("2.00"));
            Assert.That(initialFieldFileData.General.FileType, Is.EqualTo("iniField"));
            Assert.That(initialFieldFileData.InitialConditions, Is.Empty);
            Assert.That(initialFieldFileData.Parameters, Is.Empty);
        }

        [Test]
        public void AddInitialCondition_InitialFieldNull_ThrowsArgumentNullException()
        {
            // Setup
            var initialFieldFileData = new InitialFieldFileData();

            // Call 
            void Call()
            {
                initialFieldFileData.AddInitialCondition(null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void AddParameter_InitialFieldNull_ThrowsArgumentNullException()
        {
            // Setup
            var initialFieldFileData = new InitialFieldFileData();

            // Call 
            void Call()
            {
                initialFieldFileData.AddParameter(null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void AddInitialCondition_AddsInitialFieldToInitialConditions()
        {
            // Setup
            var initialFieldFileData = new InitialFieldFileData();
            var initialField = new InitialField();

            // Call 
            initialFieldFileData.AddInitialCondition(initialField);

            // Assert
            Assert.That(initialFieldFileData.InitialConditions, Does.Contain(initialField));
        }

        [Test]
        public void AddParameter_AddsInitialFieldToParameters()
        {
            // Setup
            var initialFieldFileData = new InitialFieldFileData();
            var initialField = new InitialField();

            // Call 
            initialFieldFileData.AddParameter(initialField);

            // Assert
            Assert.That(initialFieldFileData.Parameters, Does.Contain(initialField));
        }
    }
}