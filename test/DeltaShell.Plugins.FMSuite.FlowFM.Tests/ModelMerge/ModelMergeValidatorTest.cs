using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelMerge;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.ModelMerge
{
    [TestFixture]
    public class ModelMergeValidatorTest
    {
        [Test]
        public void Validate_ExistingModelNull_ThrowsArgumentNullException()
        {
            // Setup
            var validator = new ModelMergeValidator();

            // Call
            TestDelegate action = () => validator.Validate(null, new WaterFlowFMModel());

            // Assert
            Assert.That(action, Throws.TypeOf<ArgumentNullException>());
        }
        
        [Test]
        public void Validate_NewModelNull_ThrowsArgumentNullException()
        {
            // Setup
            var validator = new ModelMergeValidator();

            // Call
            TestDelegate action = () => validator.Validate(new WaterFlowFMModel(), null);

            // Assert
            Assert.That(action, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        [TestCaseSource(nameof(GetValidateTestCases), new object[]{"NameToDuplicate"})]
        public void Validate_DuplicateItemFoundInNewModel_ReturnsFalseAndAddsNameToDuplicateNamesCollection(Action<IHydroNetwork, string> addItemWithDuplicateName, string expectedDuplicateName)
        {
            // Setup
            const string nameToDuplicate = "NameToDuplicate";
            var validator = new ModelMergeValidator();

            var existingModel = new WaterFlowFMModel();
            IHydroNetwork existingNetwork = existingModel.Network;
            existingNetwork.Branches.Add(new Channel(){Name = nameToDuplicate});
            
            var newModel = new WaterFlowFMModel();
            IHydroNetwork newNetwork = newModel.Network;
            addItemWithDuplicateName(newNetwork, nameToDuplicate);
            
            // Call
            bool isValid = validator.Validate(existingModel, newModel);

            // Assert
            Assert.That(isValid, Is.False);
            Assert.That(validator.DuplicateNames.Count, Is.EqualTo(1));
            Assert.That(validator.DuplicateNames.First(), Is.EqualTo(expectedDuplicateName));
        }

        private static IEnumerable<TestCaseData> GetValidateTestCases(string nameToDuplicate)
        {
            yield return new TestCaseData(new Action<IHydroNetwork, string>((network, name) =>
            {
                network.Branches.Add(new Channel(){Name = name});
            }), $"{nameToDuplicate} (branch)").SetName("Duplicate Channel");
            
            yield return new TestCaseData(new Action<IHydroNetwork, string>((network, name) =>
            {
                network.Branches.Add(new Pipe(){Name = name});
            }), $"{nameToDuplicate} (branch)").SetName("Duplicate Pipe");
            
            yield return new TestCaseData(new Action<IHydroNetwork, string>((network, name) =>
            {
                network.Branches.Add(new SewerConnection(){Name = name});
            }), $"{nameToDuplicate} (branch)").SetName("Duplicate SewerConnection");
            
            yield return new TestCaseData(new Action<IHydroNetwork, string>((network, name) =>
            {
                network.Nodes.Add(new HydroNode(){Name = name});
            }), $"{nameToDuplicate} (node)").SetName("Duplicate HydroNode");
            
            yield return new TestCaseData(new Action<IHydroNetwork, string>((network, name) =>
            {
                network.Nodes.Add(new Manhole(){Name = name});
            }), $"{nameToDuplicate} (node)").SetName("Duplicate Manhole");
            
            yield return new TestCaseData(new Action<IHydroNetwork, string>((network, name) =>
            {
                var manhole = new Manhole();
                manhole.Compartments.Add(new Compartment(nameToDuplicate));
                network.Nodes.Add(manhole);
            }), $"{nameToDuplicate} (compartment)").SetName("Duplicate Compartment");
            
            yield return new TestCaseData(new Action<IHydroNetwork, string>((network, name) =>
            {
                var manhole = new Manhole();
                manhole.Compartments.Add(new OutletCompartment(nameToDuplicate));
                network.Nodes.Add(manhole);
            }), $"{nameToDuplicate} (compartment)").SetName("Duplicate OutletComparment");
            
            yield return new TestCaseData(new Action<IHydroNetwork, string>((network, name) =>
            {
                var channel = new Channel();
                channel.BranchFeatures.Add(new Bridge() {Name = nameToDuplicate});
                network.Branches.Add(channel);
            }), $"{nameToDuplicate} (bridge)").SetName("Duplicate Bridge");
            
            yield return new TestCaseData(new Action<IHydroNetwork, string>((network, name) =>
            {
                var channel = new Channel();
                channel.BranchFeatures.Add(new CompositeBranchStructure() {Name = nameToDuplicate});
                network.Branches.Add(channel);
            }), $"{nameToDuplicate} (composite structure)").SetName("Duplicate CompositeStructure");
            
            yield return new TestCaseData(new Action<IHydroNetwork, string>((network, name) =>
            {
                var channel = new Channel();
                ICrossSection crossSection = CrossSection.CreateDefault(CrossSectionType.Standard, channel);
                crossSection.Name = nameToDuplicate;
                channel.BranchFeatures.Add(crossSection);
                network.Branches.Add(channel);
            }), $"{nameToDuplicate} (cross-section)").SetName("Duplicate CrossSection");
            
            yield return new TestCaseData(new Action<IHydroNetwork, string>((network, name) =>
            {
                var channel = new Channel();
                channel.BranchFeatures.Add(new Culvert() {Name = nameToDuplicate});
                network.Branches.Add(channel);
            }), $"{nameToDuplicate} (culvert)").SetName("Duplicate Culvert");

            yield return new TestCaseData(new Action<IHydroNetwork, string>((network, name) =>
            {
                var channel = new Channel();
                channel.BranchFeatures.Add(new Gate() {Name = nameToDuplicate});
                network.Branches.Add(channel);
            }), $"{nameToDuplicate} (gate)").SetName("Duplicate Gate");
            
            yield return new TestCaseData(new Action<IHydroNetwork, string>((network, name) =>
            {
                var channel = new Channel();
                channel.BranchFeatures.Add(new LateralSource() {Name = nameToDuplicate});
                network.Branches.Add(channel);
            }), $"{nameToDuplicate} (lateral source)").SetName("Duplicate LateralSource");
            
            yield return new TestCaseData(new Action<IHydroNetwork, string>((network, name) =>
            {
                var channel = new Channel();
                channel.BranchFeatures.Add(new ObservationPoint() {Name = nameToDuplicate});
                network.Branches.Add(channel);
            }), $"{nameToDuplicate} (observation point)").SetName("Duplicate ObservationPoint");
            
            yield return new TestCaseData(new Action<IHydroNetwork, string>((network, name) =>
            {
                var channel = new Channel();
                channel.BranchFeatures.Add(new Orifice() {Name = nameToDuplicate});
                network.Branches.Add(channel);
            }), $"{nameToDuplicate} (weir)").SetName("Duplicate Orifice");
            
            yield return new TestCaseData(new Action<IHydroNetwork, string>((network, name) =>
            {
                var channel = new Channel();
                channel.BranchFeatures.Add(new Pump() {Name = nameToDuplicate});
                network.Branches.Add(channel);
            }), $"{nameToDuplicate} (pump)").SetName("Duplicate Pump");
            
            yield return new TestCaseData(new Action<IHydroNetwork, string>((network, name) =>
            {
                var channel = new Channel();
                channel.BranchFeatures.Add(new Retention() {Name = nameToDuplicate});
                network.Branches.Add(channel);
            }), $"{nameToDuplicate} (retention)").SetName("Duplicate Retention");
            
            yield return new TestCaseData(new Action<IHydroNetwork, string>((network, name) =>
            {
                var crossSectionDefinition = new CrossSectionDefinitionYZ(nameToDuplicate);
                network.SharedCrossSectionDefinitions.Add(crossSectionDefinition);
            }), $"{nameToDuplicate} (shared cross-section definition)").SetName("Duplicate SharedCrossSection");
            
            yield return new TestCaseData(new Action<IHydroNetwork, string>((network, name) =>
            {
                var channel = new Channel();
                channel.BranchFeatures.Add(new Weir() {Name = nameToDuplicate});
                network.Branches.Add(channel);
            }), $"{nameToDuplicate} (weir)").SetName("Duplicate Weir");
            
            yield return new TestCaseData(new Action<IHydroNetwork, string>((network, name) =>
            {
                network.Links.Add(new HydroLink(Substitute.For<IHydroObject>(), Substitute.For<IHydroObject>()){Name = nameToDuplicate});
            }), $"{nameToDuplicate} (link)").SetName("Duplicate Link");
        }
    }
}