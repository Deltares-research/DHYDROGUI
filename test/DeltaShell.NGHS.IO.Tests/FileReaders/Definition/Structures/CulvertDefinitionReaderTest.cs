using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileReaders.Definition.Structures;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders.Definition.Structures
{
    [TestFixture]
    public class CulvertDefinitionReaderTest
    {
        [Test]
        public void ReadDefinition_SubTypeInvertedSiphon_ReadsCorrectCulvert()
        {
            // Setup
            var category = new DelftIniCategory("Structure");

            AddProperty(category, "id", "some_culvert_name");
            AddProperty(category, "branchId", "T2_B1_EW");
            AddProperty(category, "chainage", "1.23");
            AddProperty(category, "type", "culvert");
            AddProperty(category, "allowedFlowDir", "both");
            AddProperty(category, "leftLevel", "2.34");
            AddProperty(category, "rightLevel", "3.45");
            AddProperty(category, "csDefId", "Culvert2");
            AddProperty(category, "length", "4.56");
            AddProperty(category, "inletLossCoeff", "5.67");
            AddProperty(category, "outletLossCoeff", "6.78");
            AddProperty(category, "valveOnOff", "0");
            AddProperty(category, "valveOpeningHeight", "7.89");
            AddProperty(category, "numLossCoeff", "9.87");
            AddProperty(category, "relOpening", "8.76");
            AddProperty(category, "lossCoeff", "7.65");
            AddProperty(category, "subType", "invertedSiphon");
            AddProperty(category, "bendLossCoeff", "6.54");
            AddProperty(category, "bedFrictionType", "chezy");
            AddProperty(category, "bedFriction", "5.43");

            var branch = Substitute.For<IBranch>();
            branch.Length = 123;

            // Call
            var culvert = (ICulvert) category.ReadStructure(new List<ICrossSectionDefinition>(), branch, "Culvert");

            // Assert
            Assert.That(culvert.Name, Is.EqualTo("some_culvert_name"));
            Assert.That(culvert.Branch, Is.SameAs(branch));
            Assert.That(culvert.Chainage, Is.EqualTo(1.23));
            Assert.That(culvert.GeometryType, Is.EqualTo(CulvertGeometryType.Tabulated));
            Assert.That(culvert.TabulatedCrossSectionDefinition, Is.Not.Null);
            Assert.That(culvert.FlowDirection, Is.EqualTo(FlowDirection.Both));
            Assert.That(culvert.InletLevel, Is.EqualTo(2.34));
            Assert.That(culvert.OutletLevel, Is.EqualTo(3.45));
            Assert.That(culvert.Length, Is.EqualTo(4.56));
            Assert.That(culvert.InletLossCoefficient, Is.EqualTo(5.67));
            Assert.That(culvert.OutletLossCoefficient, Is.EqualTo(6.78));
            Assert.That(culvert.IsGated, Is.EqualTo(false));
            Assert.That(culvert.BendLossCoefficient, Is.EqualTo(6.54));
            Assert.That(culvert.FrictionDataType, Is.EqualTo(Friction.Chezy));
            Assert.That(culvert.Friction, Is.EqualTo(5.43));
            Assert.That(culvert.GateInitialOpening, Is.EqualTo(7.89));
            Assert.That(culvert.CulvertType, Is.EqualTo(CulvertType.InvertedSiphon));
        }

        private static void AddProperty(IDelftIniCategory category, string key, string value)
        {
            category.Properties.Add(new DelftIniProperty(key, value, ""));
        }
    }
}