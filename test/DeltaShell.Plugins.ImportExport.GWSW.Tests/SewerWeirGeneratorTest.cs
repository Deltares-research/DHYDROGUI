using System.Collections.Generic;
using System.Globalization;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Reflection;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.Plugins.ImportExport.GWSW.SewerFeatures;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.GWSW.Tests
{
    [TestFixture]
    public class SewerWeirGeneratorTest : SewerFeatureFactoryTestHelper
    {
        private const string WeirName = "myWeir";
        private const string SourceCompartmentName = "cmp001";
        private const string TargetCompartmentName = "cmp002";

        [Test]
        public void GenerateWeirSewerConnectionFromGwswElement()
        {
            var connectionTypeString = SewerConnectionMapping.ConnectionType.Crest.GetDescription();

            var length = 3.2;
            var connectionGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.UniqueId, WeirName, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, connectionTypeString, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.SourceCompartmentId, SourceCompartmentName, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.TargetCompartmentId, TargetCompartmentName, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.Length, length.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble)
                }
            };
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var weir = new SewerWeirGenerator(logHandler).Generate(connectionGwswElement) as GwswConnectionWeir;
            Assert.IsNotNull(weir);
            Assert.That((object) weir.Name, Is.EqualTo(WeirName));
            Assert.That(weir.SourceCompartmentName, Is.EqualTo(SourceCompartmentName));
            Assert.That(weir.TargetCompartmentName, Is.EqualTo(TargetCompartmentName));
            Assert.That(weir.Length, Is.EqualTo(length));
        }

        [Test]
        public void GivenWeirStructureGwswElement_WhenGeneratingWeir_ThenWeirPropertiesArePresentOnWeir()
        {
            #region GwswElement

            var structureTypeString = SewerStructureMapping.StructureType.Crest.GetDescription();
            var crestWidth = 3.0;
            var crestLevel = 2.7;
            var corrCoefficient = 0.9;
            var structureGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Structure.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.UniqueId, WeirName, string.Empty),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StructureType, structureTypeString, string.Empty),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.CrestWidth, crestWidth.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.CrestLevel, crestLevel.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.DischargeCoefficient, corrCoefficient.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble)
                }
            };


            #endregion
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var createdWeir = new SewerWeirGenerator(logHandler).Generate(structureGwswElement) as GwswStructureWeir;
            Assert.IsNotNull(createdWeir);
            Assert.That(createdWeir.CrestWidth, Is.EqualTo(crestWidth));
            Assert.That(createdWeir.CrestLevel, Is.EqualTo(crestLevel));

            var weirFormula = createdWeir.WeirFormula as SimpleWeirFormula;
            Assert.IsNotNull(weirFormula);
            Assert.That(weirFormula.CorrectionCoefficient, Is.EqualTo(corrCoefficient));
        }

        [TestCase(SewerConnectionMapping.FlowDirection.Open, FlowDirection.Both)]
        [TestCase(SewerConnectionMapping.FlowDirection.Closed, FlowDirection.None)]
        [TestCase(SewerConnectionMapping.FlowDirection.FromStartToEnd, FlowDirection.Positive)]
        [TestCase(SewerConnectionMapping.FlowDirection.FromEndToStart, FlowDirection.Negative)]
        public void GivenConnectionGwswElementWithCrestType_WhenGeneratingCrest_ThenFlowDirectionOnWeirIsCorrect(SewerConnectionMapping.FlowDirection flowDirection, FlowDirection expectedFlowDirection)
        {
            var flowDirectionId = flowDirection.GetDescription();
            var sewerConnectionGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.UniqueId, WeirName, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, SewerConnectionMapping.ConnectionType.Crest.GetDescription(), string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.SourceCompartmentId, SourceCompartmentName, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.TargetCompartmentId, TargetCompartmentName, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.FlowDirection, flowDirectionId, string.Empty)
                }
            };
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var createdWeir = new SewerWeirGenerator(logHandler).Generate(sewerConnectionGwswElement) as GwswConnectionWeir;
            Assert.IsNotNull(createdWeir);
            Assert.That(createdWeir.FlowDirection, Is.EqualTo(expectedFlowDirection));
        }
    }
}