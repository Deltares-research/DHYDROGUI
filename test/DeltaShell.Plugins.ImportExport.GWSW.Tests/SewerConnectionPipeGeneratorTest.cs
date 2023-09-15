using System.Collections.Generic;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Reflection;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.GWSW.Tests
{
    [TestFixture]
    public class SewerConnectionPipeGeneratorTest: SewerFeatureFactoryTestHelper
    {
        #region Pipes

        [Test]
        public void GeneratePipeFromGwswConnectionElementReturnsValidObject()
        {
            var startNode = "node001";
            var endNode = "node002";
            var pipeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, SewerConnectionMapping.ConnectionType.InfiltrationPipe.GetDescription(), string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.SourceCompartmentId, startNode, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.TargetCompartmentId, endNode, string.Empty)
                }
            };
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var element = new SewerConnectionPipeGenerator(logHandler).Generate(pipeGwswElement);

            //A pipe is created.
            var pipe = element as IPipe;
            Assert.IsNotNull(pipe);
        }

        [Test]
        [TestCase("GSL")]
        [TestCase("ITR")]
        [TestCase("OPL")]
        public void CreatePipeWhenGivingPipeIndicatorAttributeFromGenerator(string typeOfConnection)
        {
            var pipeId = "123";
            var sourceCompartmentId = "node001";
            var targetCompartmentId = "node002";
            var pipeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeId, pipeId, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, typeOfConnection, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.SourceCompartmentId, sourceCompartmentId, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.TargetCompartmentId, targetCompartmentId, string.Empty)
                }
            };

            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var element = new SewerConnectionPipeGenerator(logHandler).Generate(pipeGwswElement); 
            Assert.IsTrue(element is Pipe);
            
            var pipe = element as Pipe;
            Assert.IsNotNull(pipe);
            Assert.That(pipe.PipeId, Is.EqualTo(pipeId));
            Assert.That(pipe.SourceCompartmentName, Is.EqualTo(sourceCompartmentId));
            Assert.That(pipe.TargetCompartmentName, Is.EqualTo(targetCompartmentId));
        }

        [Test]
        public void CreatePipeFromFactoryWithKnownAttributes()
        {
            var sourceLevel = 30;
            var targetLevel = 100;
            var length = 200;
            var crossSectionDefinitionName = "crossSectionDef001";
            var sourceCompartmentId = "cmp001";
            var targetCompartmentId = "cmp002";
            var pipeType = SewerConnectionMapping.ConnectionType.ClosedConnection;
            var pipeTypeString = pipeType.GetDescription();

            var defaultString = string.Empty;
            var defaultDouble = 0.0;

            var pipeGwswElement = GetSewerConnectionGwswElement(string.Empty, sourceCompartmentId, targetCompartmentId, pipeTypeString, sourceLevel, targetLevel, defaultString, length, crossSectionDefinitionName, defaultString, defaultString, defaultDouble, defaultDouble, defaultDouble, defaultDouble);

            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var createdPipe = new SewerConnectionPipeGenerator(logHandler).Generate(pipeGwswElement) as IPipe;
            Assert.IsNotNull(createdPipe);

            Assert.That(createdPipe.SourceCompartmentName, Is.EqualTo(sourceCompartmentId));
            Assert.That(createdPipe.TargetCompartmentName, Is.EqualTo(targetCompartmentId));
            Assert.That(createdPipe.CrossSectionDefinitionName, Is.EqualTo(crossSectionDefinitionName));

            Assert.IsNull(createdPipe.SourceCompartment);
            Assert.IsNull(createdPipe.TargetCompartment);
            Assert.IsNull(createdPipe.Source);
            Assert.IsNull(createdPipe.Target);
            Assert.IsNull(createdPipe.CrossSection?.Definition);

            //Defined
            Assert.That(createdPipe.LevelSource, Is.EqualTo(sourceLevel));
            Assert.That(createdPipe.LevelTarget, Is.EqualTo(targetLevel));
            Assert.That(createdPipe.Length, Is.EqualTo(length));
        }

        [Test]
        public void GivenPipeGwswElementWithCrossSectionDefinitionId_WhenCreatingPipe_ThenCrossSectionDefinitionIdIsSet()
        {
            var crossSectionDefinitionName = "crossSectionDef001";
            var startNode = "node001";
            var endNode = "node002";
            var pipeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.CrossSectionDefinitionId, crossSectionDefinitionName, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, SewerConnectionMapping.ConnectionType.ClosedConnection.GetDescription(), string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.SourceCompartmentId, startNode, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.TargetCompartmentId, endNode, string.Empty)
                }
            };
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var createdPipe = new SewerConnectionPipeGenerator(logHandler).Generate(pipeGwswElement) as IPipe;
            Assert.IsNotNull(createdPipe);
            Assert.That(createdPipe.CrossSectionDefinitionName, Is.EqualTo(crossSectionDefinitionName));
            Assert.IsNull(createdPipe.CrossSection?.Definition);
        }

        #endregion
    }
}