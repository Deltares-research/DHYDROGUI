using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialField.Data;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.InitialField.Data
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
            Assert.That(initialFieldFileData.AllFields, Is.Empty);
        }

        [Test]
        public void AddInitialCondition_InitialFieldDataNull_ThrowsArgumentNullException()
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
        public void AddParameter_InitialFieldDataNull_ThrowsArgumentNullException()
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
        public void AddInitialCondition_AddsInitialFieldDataToInitialConditions()
        {
            // Setup
            var initialFieldFileData = new InitialFieldFileData();
            var initialField = new InitialFieldData();

            // Call 
            initialFieldFileData.AddInitialCondition(initialField);

            // Assert
            Assert.That(initialFieldFileData.InitialConditions, Does.Contain(initialField));
        }

        [Test]
        public void AddParameter_AddsInitialFieldDataToParameters()
        {
            // Setup
            var initialFieldFileData = new InitialFieldFileData();
            var initialField = new InitialFieldData();

            // Call 
            initialFieldFileData.AddParameter(initialField);

            // Assert
            Assert.That(initialFieldFileData.Parameters, Does.Contain(initialField));
        }

        [Test]
        public void AllFields_ReturnsInitialConditionsAndParameters()
        {
            // Setup
            var initialFieldFileData = new InitialFieldFileData();
            var initialCondition = new InitialFieldData();
            var parameter = new InitialFieldData();
            
            // Call 
            initialFieldFileData.AddInitialCondition(initialCondition);
            initialFieldFileData.AddParameter(parameter);
            
            // Assert
            Assert.That(initialFieldFileData.AllFields, Does.Contain(initialCondition).And.Contain(parameter));
        }
    }
}