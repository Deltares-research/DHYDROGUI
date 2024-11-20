using DeltaShell.Plugins.FMSuite.Common.Wind;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.Extensions;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.TimeFrame.Extensions
{
    [TestFixture]
    public class WindInputTypeExtensionsTest
    {
        [Test]
        [TestCase(WindInputType.SpiderWebGrid, WindDefinitionType.SpiderWebGrid)]
        [TestCase(WindInputType.WindVector, WindDefinitionType.WindXY)]
        [TestCase(WindInputType.XYComponents, WindDefinitionType.WindXWindY)]
        public void ConvertToWindDefinitionType_ExpectedResults(WindInputType input, WindDefinitionType expectedOutput)
        {
            // Call
            WindDefinitionType result = input.ConvertToWindDefinitionType();

            // Assert
            Assert.That(result, Is.EqualTo(expectedOutput));
        }

        [Test]
        public void ConvertToWindDefinition_InvalidEnum_ThrowsArgumentOutOfRangeException()
        {
            // Setup
            const WindInputType input = (WindInputType)int.MaxValue;

            // Call | Assert
            void Call() => input.ConvertToWindDefinitionType();
            Assert.Throws<System.ArgumentOutOfRangeException>(Call);
        }

        [Test]
        [TestCase(WindDefinitionType.SpiderWebGrid, WindInputType.SpiderWebGrid)]
        [TestCase(WindDefinitionType.WindXY, WindInputType.WindVector)]
        [TestCase(WindDefinitionType.WindXYP, WindInputType.WindVector)]
        [TestCase(WindDefinitionType.WindXWindY, WindInputType.XYComponents)]
        public void ConvertToWindDefinitionType_ExpectedResults(WindDefinitionType input, WindInputType expectedOutput)
        {
            // Call
            WindInputType result = input.ConvertToWindInputType();

            // Assert
            Assert.That(result, Is.EqualTo(expectedOutput));
        }

        [Test]
        public void ConvertToWindInputType_InvalidEnum_ThrowsArgumentOutOfRangeException()
        {
            // Setup
            const WindDefinitionType input = (WindDefinitionType)int.MaxValue;

            // Call | Assert
            void Call() => input.ConvertToWindInputType();
            Assert.Throws<System.ArgumentOutOfRangeException>(Call);
        }
    }
}