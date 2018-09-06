using System.Collections.Generic;
using System.Globalization;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
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
            var connectionTypeString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionMapping.ConnectionType.Crest);

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
            
            var weir = new SewerWeirGenerator().Generate(connectionGwswElement) as GwswConnectionWeir;
            Assert.IsNotNull(weir);
            Assert.That(weir.Name, Is.EqualTo(WeirName));
            Assert.That(weir.SourceCompartmentName, Is.EqualTo(SourceCompartmentName));
            Assert.That(weir.TargetCompartmentName, Is.EqualTo(TargetCompartmentName));
            Assert.That(weir.Length, Is.EqualTo(length));
        }

        [Test]
        public void GivenWeirStructureGwswElement_WhenGeneratingWeir_ThenWeirPropertiesArePresentOnWeir()
        {
            #region GwswElement

            var structureTypeString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerStructureMapping.StructureType.Crest);
            var crestWidth = 3.0;
            var crestLevel = 2.7;
            var dischargeCoefficient = 0.9;
            var structureGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Structure.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.UniqueId, WeirName, string.Empty),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StructureType, structureTypeString, string.Empty),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.CrestWidth, crestWidth.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.CrestLevel, crestLevel.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.DischargeCoefficient, dischargeCoefficient.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble)
                }
            };


            #endregion

            var createdWeir = new SewerWeirGenerator().Generate(structureGwswElement) as GwswStructureWeir;
            Assert.IsNotNull(createdWeir);
            Assert.That(createdWeir.CrestWidth, Is.EqualTo(crestWidth));
            Assert.That(createdWeir.CrestLevel, Is.EqualTo(crestLevel));

            var weirFormula = createdWeir.WeirFormula as SimpleWeirFormula;
            Assert.IsNotNull(weirFormula);
            Assert.That(weirFormula.DischargeCoefficient, Is.EqualTo(dischargeCoefficient));
        }

        [TestCase(SewerConnectionMapping.FlowDirection.Open, FlowDirection.Both)]
        [TestCase(SewerConnectionMapping.FlowDirection.Closed, FlowDirection.None)]
        [TestCase(SewerConnectionMapping.FlowDirection.FromStartToEnd, FlowDirection.Positive)]
        [TestCase(SewerConnectionMapping.FlowDirection.FromEndToStart, FlowDirection.Negative)]
        public void GivenConnectionGwswElementWithCrestType_WhenGeneratingCrest_ThenFlowDirectionOnWeirIsCorrect(SewerConnectionMapping.FlowDirection flowDirection, FlowDirection expectedFlowDirection)
        {
            var flowDirectionId = EnumDescriptionAttributeTypeConverter.GetEnumDescription(flowDirection);
            var sewerConnectionGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Connection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.UniqueId, WeirName, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionMapping.ConnectionType.Crest), string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.SourceCompartmentId, SourceCompartmentName, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.TargetCompartmentId, TargetCompartmentName, string.Empty),
                    GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.FlowDirection, flowDirectionId, string.Empty)
                }
            };
            
            var createdWeir = new SewerWeirGenerator().Generate(sewerConnectionGwswElement) as GwswConnectionWeir;
            Assert.IsNotNull(createdWeir);
            Assert.That(createdWeir.FlowDirection, Is.EqualTo(expectedFlowDirection));
        }
    }
}