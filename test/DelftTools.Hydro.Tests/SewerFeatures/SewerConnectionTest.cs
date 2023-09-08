using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.SewerFeatures
{
    [TestFixture]
    public class SewerConnectionTest
    {
        [Test]
        public void CreateSewerConnection()
        {
            var sewerConnection = new SewerConnection();
            Assert.IsNotNull(sewerConnection);
        }

        [Test]
        public void CreateSewerConnectionGivingName()
        {
            var nameSewer = "TestSewer";
            var sewerConnection = new SewerConnection(nameSewer);
            Assert.IsNotNull(sewerConnection);
            Assert.AreEqual(nameSewer, sewerConnection.Name);
        }
        
        [Test]
        public void CreateSimpleSewerConnectionCreatesEmptyEnumerableBranchFeaturesList()
        {
            var sewerConnection = new SewerConnection();
            Assert.IsNotNull(sewerConnection.BranchFeatures);
            Assert.IsFalse(sewerConnection.BranchFeatures.Any());
        }

        [Test]
        public void SimpleSewerConnectionIsNotPipe()
        {
            var sewerConnection = new SewerConnection();
            Assert.IsNotNull(sewerConnection.BranchFeatures);
            Assert.IsFalse(sewerConnection.IsPipe());
        }

        [Test]
        public void SewerConnectionWaterTypeDefaultValueIsNotNull()
        {
            var sewerConnection = new SewerConnection();
            Assert.IsNotNull(sewerConnection.WaterType);
            //This default value might change, but at least we have here a trigger.
            Assert.AreEqual(SewerConnectionWaterType.None, sewerConnection.WaterType);
        }

        [Test]
        public void SewerConnectionBranchFeaturesShouldBePresentInNetworkBranchFeatures()
        {
            var sewerConnection = GetSewerConnectionWithSourceAndTarget();
            Assert.IsNotNull(sewerConnection);

            var network = new HydroNetwork();
            NetworkHelper.AddChannelToHydroNetwork(network, sewerConnection);
            
            var featureOne = new Pump();
            Assert.IsNotNull(featureOne);

            Assert.AreEqual(0, sewerConnection.BranchFeatures.Count);

            //Add one feature.
            var compositeStructure = sewerConnection.AddStructureToBranch(featureOne);

            Assert.IsTrue(network.BranchFeatures.Contains(compositeStructure));
            Assert.IsTrue(network.BranchFeatures.Contains(featureOne));
        }

        [Test]
        public void SewerConnectionBranchFeaturesDoesNotAcceptMoreThanOneFeature()
        {
            var sewerConnection = GetSewerConnectionWithSourceAndTarget();
            sewerConnection.Network = new HydroNetwork();
            Assert.That(sewerConnection.BranchFeatures.Count, Is.EqualTo(0));

            var pump1 = new Pump();
            var pump2 = new Pump();

            //Add one feature.
            sewerConnection.BranchFeatures.Add(pump1);
            Assert.That(sewerConnection.BranchFeatures.Count, Is.EqualTo(1));
            Assert.That(sewerConnection.BranchFeatures.First(), Is.EqualTo(pump1));

            //Try to add a second one, but the feature should still be the first one.
            var expectedLogMessage = $"Sewer connection {sewerConnection.Name} does not accept more than one branch feature";
            TestHelper.AssertAtLeastOneLogMessagesContains(() => sewerConnection.BranchFeatures.Add(pump2), expectedLogMessage);
            Assert.That(sewerConnection.BranchFeatures.Count, Is.EqualTo(1));
            Assert.That(sewerConnection.BranchFeatures.First(), Is.EqualTo(pump1));
        }

        [Test]
        public void SewerConnectionBranchFeaturesDoesNotAcceptToAddMoreThanFeatureToFeatureBranches()
        {
            var sewerConnection = GetSewerConnectionWithSourceAndTarget();
            Assert.IsNotNull(sewerConnection);

            var network = new HydroNetwork();
            sewerConnection.Network = network;

            #region Features
            var featureOne = new Pump();
            Assert.IsNotNull(featureOne);

            var featureTwo = new Pump();
            Assert.IsNotNull(featureTwo);
            #endregion

            Assert.AreEqual(0, sewerConnection.BranchFeatures.Count);
            var expectedLogMessage = string.Format("Sewer connection {0} does not accept more than one branch feature", sewerConnection.Name);

            //Add one composite feature.
            var compositeStructure = sewerConnection.AddStructureToBranch(featureOne);

            //Try to add an extra feature to the branch feature itself.
            TestHelper.AssertAtLeastOneLogMessagesContains(() => sewerConnection.BranchFeatures.Add(featureTwo), expectedLogMessage);
            Assert.AreEqual(2/*CompositeStructure, and Structure on it*/, sewerConnection.BranchFeatures.Count);
            Assert.AreEqual(compositeStructure, sewerConnection.BranchFeatures.First());
            Assert.AreEqual(featureOne, sewerConnection.BranchFeatures.Last());

            var foundComposite = sewerConnection.BranchFeatures.First() as CompositeBranchStructure;
            Assert.IsNotNull(foundComposite);
            Assert.IsTrue(foundComposite.Structures.Count.Equals(1));
            Assert.IsTrue(foundComposite.Structures.Contains(featureOne));

            Assert.AreEqual(featureOne, sewerConnection.GetStructuresFromBranchFeatures<Pump>().First());
        }

        [Test]
        public void SewerConnectionBranchFeaturesAcceptsReplacementOfFeature()
        {
            var sewerConnection = GetSewerConnectionWithSourceAndTarget();
            Assert.IsNotNull(sewerConnection);

            var network = new HydroNetwork();
            sewerConnection.Network = network;

            #region Features
            var featureOne = new Pump();
            Assert.IsNotNull(featureOne);

            var featureTwo = new Pump();
            Assert.IsNotNull(featureTwo);

            var featureThree = new Pump();
            Assert.IsNotNull(featureThree);
            #endregion

            Assert.AreEqual(0, sewerConnection.BranchFeatures.Count);

            //Add one feature.
            sewerConnection.BranchFeatures.Add(featureOne);
            Assert.AreEqual(1, sewerConnection.BranchFeatures.Count);
            Assert.AreEqual(featureOne, sewerConnection.BranchFeatures.First());

            //Try to replace the existent feature, it should be possible
            sewerConnection.BranchFeatures[0] = featureTwo;
            Assert.AreEqual(1, sewerConnection.BranchFeatures.Count);
            Assert.AreEqual(featureTwo, sewerConnection.BranchFeatures.First());
        }

        [Test]
        public void SewerConnectionBranchFeaturesAcceptsNewFeatureIfPreviousIsRemoved()
        {
            var sewerConnection = GetSewerConnectionWithSourceAndTarget();
            Assert.IsNotNull(sewerConnection);

            var network = new HydroNetwork();
            sewerConnection.Network = network;

            #region Features
            var featureOne = new Pump();
            Assert.IsNotNull(featureOne);

            var featureTwo = new Pump();
            Assert.IsNotNull(featureTwo);

            var featureThree = new Pump();
            Assert.IsNotNull(featureThree);
            #endregion

            Assert.AreEqual(0, sewerConnection.BranchFeatures.Count);

            //Add one feature.
            sewerConnection.BranchFeatures.Add(featureOne);
            Assert.AreEqual(1, sewerConnection.BranchFeatures.Count);
            Assert.AreEqual(featureOne, sewerConnection.BranchFeatures.First());

            //Try removing said feature and adding a new one.
            sewerConnection.BranchFeatures.Clear();
            Assert.IsFalse(sewerConnection.BranchFeatures.Any());

            sewerConnection.BranchFeatures.Add(featureThree);
            Assert.AreEqual(1, sewerConnection.BranchFeatures.Count);
            Assert.AreEqual(featureThree, sewerConnection.BranchFeatures.First());
        }

        [Test]
        public void SewerConnectionBranchFeaturesAcceptsCompositeStructureWithOneStructureAsFeatureBranch()
        {
            var sewerConnection = GetSewerConnectionWithSourceAndTarget();
            Assert.IsNotNull(sewerConnection);

            var network = new HydroNetwork();
            sewerConnection.Network = network;

            #region Features
            var featureOne = new Pump();
            Assert.IsNotNull(featureOne);

            #endregion

            Assert.AreEqual(0, sewerConnection.BranchFeatures.Count);

            //Add one composite feature.
            var compositeStructure = sewerConnection.AddStructureToBranch(featureOne);
            Assert.AreEqual(2 /*CompositeStructure, and Structure on it*/, sewerConnection.BranchFeatures.Count);
            Assert.AreEqual(compositeStructure, sewerConnection.BranchFeatures.First());
            Assert.AreEqual(featureOne, sewerConnection.BranchFeatures.Last());

            var foundComposite = sewerConnection.BranchFeatures.First() as CompositeBranchStructure;
            Assert.IsNotNull(foundComposite);
            Assert.IsTrue(foundComposite.Structures.Any());
            Assert.IsTrue(foundComposite.Structures.Contains(featureOne));

            Assert.AreEqual(featureOne, sewerConnection.GetStructuresFromBranchFeatures<Pump>().First());
        }

        [Test]
        public void SewerConnectionBranchFeaturesDoesNotAcceptAFeatureBranchCompositeStructureWithMoreThanOneStructure()
        {
            var sewerConnection = GetSewerConnectionWithSourceAndTarget();
            sewerConnection.Network = new HydroNetwork();

            var pump1 = new Pump();
            var pump2 = new Pump();

            Assert.That(sewerConnection.BranchFeatures.Count, Is.EqualTo(0));
            var expectedLogMessage = $"Sewer connection {sewerConnection.Name} does not accept more than one branch feature";

            //Add one composite feature.
            var compositeStructure = sewerConnection.AddStructureToBranch(pump1);

            //Try to add a feature to the composite instead, it should still fail.
            TestHelper.AssertAtLeastOneLogMessagesContains(() => sewerConnection.AddStructureToBranch(pump2), expectedLogMessage);
            Assert.That(sewerConnection.BranchFeatures.Count, Is.EqualTo(2)); /*CompositeStructure, and Structure on it*/
            Assert.That(sewerConnection.BranchFeatures.First(), Is.EqualTo(compositeStructure));
            Assert.That(sewerConnection.BranchFeatures.Last(), Is.EqualTo(pump1));

            var foundComposite = sewerConnection.BranchFeatures.First() as CompositeBranchStructure;
            Assert.IsNotNull(foundComposite);
            Assert.That(foundComposite.Structures.Count, Is.EqualTo(1));
            Assert.That(foundComposite.Structures.Contains(pump1));

            Assert.That(sewerConnection.GetStructuresFromBranchFeatures<Pump>().First(), Is.EqualTo(pump1));
        }

        [Test]
        public void ReplacingSewerConnectionFeatureBranchesWithOneStructureReturnsLogMessage()
        {
            var sewerConnection = new SewerConnection();
            Assert.IsNotNull(sewerConnection);

            #region Features
            var featureOne = new Pump();
            Assert.IsNotNull(featureOne);

            var featureTwo = new Pump();
            Assert.IsNotNull(featureTwo);

            var featureThree = new Pump();
            Assert.IsNotNull(featureThree);

            var featureList = new EventedList<IBranchFeature>() {featureOne, featureTwo, featureThree};
            Assert.IsNotNull(featureList);
            Assert.IsTrue(featureList.Any());
            #endregion

            var expectedLogMessage = string.Format("Sewer connection {0} does not accept more than one branch feature", sewerConnection.Name);
            TestHelper.AssertAtLeastOneLogMessagesContains(() => sewerConnection.BranchFeatures = featureList, expectedLogMessage);
        }

        [Test]
        public void ReplacingSewerConnectionFeatureBranchesWithOneCompositeBranchFeatureWithMultipleStructuresReturnsLogMessage()
        {
            var sewerConnection = GetSewerConnectionWithSourceAndTarget();
            Assert.IsNotNull(sewerConnection);

            var network = new HydroNetwork();
            sewerConnection.Network = network;

            var featureOne = new Pump();
            Assert.IsNotNull(featureOne);

            var featureTwo = new Pump();
            Assert.IsNotNull(featureTwo);
            var featureThree = new Pump();
            Assert.IsNotNull(featureThree);
            var compositeStructureTwo = new CompositeBranchStructure();
            compositeStructureTwo.Structures.AddRange(new []{featureTwo, featureThree});
            var featureReplacementList = new EventedList<IBranchFeature>() {compositeStructureTwo, featureTwo, featureThree};

            //No problem adding the first structure.
            var compositeStructureOne = sewerConnection.AddStructureToBranch(featureOne);
            Assert.IsTrue(sewerConnection.BranchFeatures.Any());
            Assert.IsTrue(sewerConnection.BranchFeatures.Count.Equals(2));
            Assert.IsTrue(sewerConnection.BranchFeatures.Contains(compositeStructureOne));
            Assert.IsTrue(sewerConnection.BranchFeatures.Contains(featureOne));

            //Try to replace the branch features directly with compositeStructureTwo, it should not be possible because there are more than one structures.
            var expectedLogMessage = string.Format("Sewer connection {0} does not accept more than one branch feature", sewerConnection.Name);
            TestHelper.AssertAtLeastOneLogMessagesContains(() => sewerConnection.BranchFeatures = featureReplacementList, expectedLogMessage);

            //Check the branch features remain the same
            Assert.IsTrue(sewerConnection.BranchFeatures.Any());
            Assert.IsTrue(sewerConnection.BranchFeatures.Count.Equals(2));
            Assert.IsTrue(sewerConnection.BranchFeatures.Contains(compositeStructureOne));
            Assert.IsTrue(sewerConnection.BranchFeatures.Contains(featureOne));
        }

        [Test]
        [TestCaseSource(nameof(ChangeBranchFeaturesCases))]
        public void AddPumpOrWeirToBranchFeatures_SetsSpecialConnectionTypeToCorrectType(
            IBranchFeature feature, SewerConnectionSpecialConnectionType expType)
        {
            // Setup
            var sewerConnection = new SewerConnection();

            // Precondition
            Assert.That(sewerConnection.SpecialConnectionType, Is.EqualTo(SewerConnectionSpecialConnectionType.None));

            // Call
            sewerConnection.BranchFeatures.Add(feature);

            // Assert
            Assert.That(sewerConnection.SpecialConnectionType, Is.EqualTo(expType));
        }

        [TestCaseSource(nameof(ChangeBranchFeaturesCases))]
        public void RemovePumpOrWeirFromBranchFeatures_SetsSpecialConnectionTypeToNone(
            IBranchFeature feature, SewerConnectionSpecialConnectionType precondition)
        {
            // Setup
            var sewerConnection = new SewerConnection();
            sewerConnection.BranchFeatures.Add(feature);

            // Precondition
            Assert.That(sewerConnection.SpecialConnectionType, Is.EqualTo(precondition));

            // Call
            sewerConnection.BranchFeatures.Remove(feature);

            // Assert
            Assert.That(sewerConnection.SpecialConnectionType, Is.EqualTo(SewerConnectionSpecialConnectionType.None));
        }

        [Test]
        [TestCaseSource(nameof(UpdateSpecialConnectionTypeUpdatesCrossSectionDefinitionCases))]
        public void UpdateSpecialConnectionType_DefaultPreviousCrossSectionDefinition_UpdatesCrossSectionDefinition(IBranchFeature feature, ICrossSectionDefinition previousDefinition, string expCrossSectionDefinitionName)
        {
            // Setup
            var hydroNetwork = Substitute.For<IHydroNetwork>();
            var sharedCrossSectionDefinitions = new EventedList<ICrossSectionDefinition> { previousDefinition };
            hydroNetwork.SharedCrossSectionDefinitions.Returns(sharedCrossSectionDefinitions);

            var sewerConnection = new SewerConnection
            {
                CrossSection = new CrossSection(previousDefinition),
                Network = hydroNetwork
            };

            // Call
            sewerConnection.BranchFeatures.Add(feature);

            // Assert
            Assert.That(sewerConnection.CrossSection.Definition.Name, Is.EqualTo(expCrossSectionDefinitionName));
        }

        [Test]
        public void UpdateSpecialConnectionType_CustomPreviousCrossSectionDefinition_UpdatesCrossSectionDefinition()
        {
            // Setup
            var hydroNetwork = Substitute.For<IHydroNetwork>();
            ICrossSectionDefinition previousDefinition = Substitute.For<ICrossSectionDefinition, INotifyPropertyChanged>();
            previousDefinition.Name = "custom_definition";
            var sharedCrossSectionDefinitions = new EventedList<ICrossSectionDefinition> { previousDefinition };
            hydroNetwork.SharedCrossSectionDefinitions.Returns(sharedCrossSectionDefinitions);

            var sewerConnection = new SewerConnection
            {
                CrossSection = new CrossSection(previousDefinition),
                Network = hydroNetwork
            };

            // Call
            sewerConnection.BranchFeatures.Add(Substitute.For<IWeir>());

            // Assert
            Assert.That(sewerConnection.CrossSection.Definition, Is.SameAs(previousDefinition));
        }

        private static IEnumerable<TestCaseData> UpdateSpecialConnectionTypeUpdatesCrossSectionDefinitionCases()
        {
            ICrossSectionDefinition pressurizedPipeCrossSectionDefinition = Substitute.For<ICrossSectionDefinition, INotifyPropertyChanged>();
            pressurizedPipeCrossSectionDefinition.Name = SewerCrossSectionDefinitionFactory.DefaultPumpSewerStructureProfileName;

            ICrossSectionDefinition weirCrossSectionDefinition = Substitute.For<ICrossSectionDefinition, INotifyPropertyChanged>();
            weirCrossSectionDefinition.Name = SewerCrossSectionDefinitionFactory.DefaultWeirSewerStructureProfileName;

            yield return new TestCaseData(Substitute.For<IWeir>(), pressurizedPipeCrossSectionDefinition, SewerCrossSectionDefinitionFactory.DefaultWeirSewerStructureProfileName);
            yield return new TestCaseData(Substitute.For<IPump>(), weirCrossSectionDefinition, SewerCrossSectionDefinitionFactory.DefaultPumpSewerStructureProfileName);
        }

        private static IEnumerable<TestCaseData> ChangeBranchFeaturesCases()
        {
            yield return new TestCaseData(Substitute.For<IPump>(), SewerConnectionSpecialConnectionType.Pump);
            yield return new TestCaseData(Substitute.For<IWeir>(), SewerConnectionSpecialConnectionType.Weir);
        }

        #region Adding source and target

        [Test]
        public void ChangingSourceCompartmentChangesSourceTest()
        {
            var manhole = new Manhole("manholeTest");
            var compartment = new Compartment("compartmentTest");
            manhole.Compartments.Add(compartment);

            var sewerConnection = new SewerConnection();
            
            Assert.IsNull(sewerConnection.Source);
            Assert.IsNull(sewerConnection.SourceCompartment);

            sewerConnection.SourceCompartment = compartment;

            Assert.That(sewerConnection.SourceCompartment, Is.EqualTo(compartment));
            Assert.That(sewerConnection.Source, Is.EqualTo(manhole));
        }

        [Test]
        public void ChangingTargetCompartmentChangesTargetTest()
        {
            var manhole = new Manhole("manholeTest");
            var compartment = new Compartment("compartmentTest");
            manhole.Compartments.Add(compartment);

            var sewerConnection = new SewerConnection();

            Assert.IsNull(sewerConnection.Target);
            Assert.IsNull(sewerConnection.TargetCompartment);

            sewerConnection.TargetCompartment = compartment;

            Assert.That(sewerConnection.TargetCompartment, Is.EqualTo(compartment));
            Assert.That(sewerConnection.Target, Is.EqualTo(manhole));
        }

        [Test]
        public void GivenSewerConnection_WhenAddingSourceCompartmentWithoutParentManhole_ThenSourceCompartmentIsNotAdded()
        {
            var compartment = new Compartment("compartmentTest") { ParentManhole = null };
            var sewerConnection = new SewerConnection {SourceCompartment = compartment};

            Assert.IsNull(sewerConnection.SourceCompartment);
            Assert.IsNull(sewerConnection.SourceCompartmentName);
        }

        [Test]
        public void GivenSewerConnection_WhenAddingTargetCompartmentWithoutParentManhole_ThenTargetCompartmentIsNotAdded()
        {
            var compartment = new Compartment("compartmentTest") { ParentManhole = null };
            var sewerConnection = new SewerConnection { TargetCompartment = compartment };

            Assert.IsNull(sewerConnection.TargetCompartment);
            Assert.IsNull(sewerConnection.TargetCompartmentName);
        }

        [Test]
        public void GivenSewerConnection_WhenAddingSourceCompartmentWithoutParentManhole_ThenLogMessageIsGiven()
        {
            var compartment = new Compartment("SourceCompartment") { ParentManhole = null };
            var sewerConnection = new SewerConnection("mySewerConnection");

            var expectedLogMessage = 
                $"We cannot add compartment {compartment.Name} as source of sewer connection {sewerConnection.Name}, because it has no parent manhole.";
            TestHelper.AssertAtLeastOneLogMessagesContains(() => sewerConnection.SourceCompartment = compartment, expectedLogMessage);
        }

        [Test]
        public void GivenSewerConnection_WhenAddingTargetCompartmentWithoutParentManhole_ThenLogMessageIsGiven()
        {
            var compartment = new Compartment("TargetCompartment") { ParentManhole = null };
            var sewerConnection = new SewerConnection("mySewerConnection");

            var expectedLogMessage =
                $"We cannot add compartment {compartment.Name} as target of sewer connection {sewerConnection.Name}, because it has no parent manhole.";
            TestHelper.AssertAtLeastOneLogMessagesContains(() => sewerConnection.TargetCompartment = compartment, expectedLogMessage);
        }

        [Test]
        public void GivenSewerConnection_WhenChangingSourceCompartment_ThenSourceCompartmentIdIsEqualToSourceCompartmentName()
        {
            var compartmentName = "myCompartment";
            var sewerConnection = new SewerConnection();
            var compartment = new Compartment(compartmentName) { ParentManhole = new Manhole("myManhole") };
            
            sewerConnection.SourceCompartment = compartment;
            Assert.That(sewerConnection.SourceCompartmentName, Is.EqualTo(compartmentName));
        }

        [Test]
        public void GivenSewerConnection_WhenChangingTargetCompartment_ThenTargetCompartmentIdIsEqualTargetCompartmentName()
        {
            var compartmentName = "myCompartment";
            var sewerConnection = new SewerConnection();
            var compartment = new Compartment(compartmentName) { ParentManhole = new Manhole("myManhole") };
            
            sewerConnection.TargetCompartment = compartment;
            Assert.That(sewerConnection.TargetCompartmentName, Is.EqualTo(compartmentName));
        }

        [Test]
        public void GivenSewerConnection_WhenAddingManholeAsSourceAndNoSourceCompartmentIsDefined_ThenTheFirstCompartmentIsTheSourceCompartment()
        {
            var sourceGeometry = new Point(0, 10);

            var sourceCompartment1 = new Compartment { Geometry = sourceGeometry };
            var sourceCompartment2 = new Compartment { Geometry = sourceGeometry };
            var sourceManhole = new Manhole("SourceManhole")
            {
                Compartments = new EventedList<ICompartment> { sourceCompartment1, sourceCompartment2 }
            };
            var sewerConnection = new SewerConnection {SourceCompartment = null};

            sewerConnection.Source = sourceManhole;
            Assert.That(sewerConnection.SourceCompartment, Is.EqualTo(sourceCompartment1));
        }

        [Test]
        public void GivenSewerConnectionWithSourceCompartment_WhenChangingSource_ThenTheFirstCompartmentOfTheNewSourceIsTheSourceCompartment()
        {
            var initialSource = new Manhole("initialManhole")
            {
                Compartments = new EventedList<ICompartment> { new Compartment { Name = "initialSourceCompartment", Geometry = new Point(0, 33) } }
            };
            var sewerConnection = new SewerConnection { Source = initialSource };

            var newSourceGeometry = new Point(0, 10);
            var newSourceCompartment1 = new Compartment { Name = "cmp1", Geometry = newSourceGeometry };
            var newSourceCompartment2 = new Compartment { Name = "cmp2", Geometry = newSourceGeometry };
            var newSourceManhole = new Manhole("NewSourceManhole")
            {
                Compartments = new EventedList<ICompartment> { newSourceCompartment1, newSourceCompartment2 }
            };

            sewerConnection.Source = newSourceManhole;
            Assert.That(sewerConnection.SourceCompartment, Is.EqualTo(newSourceCompartment1));
        }

        [Test]
        public void GivenSewerConnection_WhenAddingManholeAsSourceWithSourceCompartmentDefined_ThenTheSourceCompartmentIsUnchanged()
        {
            var sourceGeometry = new Point(0, 10);

            var sourceCompartment1 = new Compartment { Name = "compartment1", Geometry = sourceGeometry };
            var sourceCompartment2 = new Compartment { Name = "compartment2", Geometry = sourceGeometry };
            var sourceManhole = new Manhole("SourceManhole")
            {
                Compartments = new EventedList<ICompartment> { sourceCompartment1, sourceCompartment2 }
            };
            var sewerConnection = new SewerConnection { SourceCompartment = sourceCompartment2 };

            sewerConnection.Source = sourceManhole;
            Assert.That(sewerConnection.SourceCompartment, Is.EqualTo(sourceCompartment2));
        }

        [Test]
        public void GivenSewerConnection_WhenAddingManholeAsTargetAndNoTargetCompartmentIsDefined_ThenTheSecondCompartmentIsTheTargetCompartment()
        {
            var targetGeometry = new Point(0, 10);

            var targetCompartment1 = new Compartment { Name = "compartment1", Geometry = targetGeometry };
            var targetCompartment2 = new Compartment { Name = "compartment2", Geometry = targetGeometry };
            var targetManhole = new Manhole("TargetManhole")
            {
                Compartments = new EventedList<ICompartment> { targetCompartment1, targetCompartment2 }
            };

            var sewerConnection = new SewerConnection {TargetCompartment = null};
            sewerConnection.Target = targetManhole;
            Assert.That(sewerConnection.TargetCompartment.Name, Is.EqualTo(targetCompartment2.Name));
        }

        [Test]
        public void GivenSewerConnectionWithTargetCompartment_WhenChangingTarget_ThenTheFirstCompartmentOfTheNewTargetIsTheTargetCompartment()
        {
            var initialTarget = new Manhole("initialManhole")
            {
                Compartments = new EventedList<ICompartment> { new Compartment { Name = "initialTargetCompartment", Geometry = new Point(0, 33) } }
            };
            var sewerConnection = new SewerConnection { Target = initialTarget };

            var newTargetGeometry = new Point(0, 10);
            var newTargetCompartment1 = new Compartment { Name = "cmp1", Geometry = newTargetGeometry };
            var newTargetCompartment2 = new Compartment { Name = "cmp2", Geometry = newTargetGeometry };
            var newTargetManhole = new Manhole("NewTargetManhole")
            {
                Compartments = new EventedList<ICompartment> { newTargetCompartment1, newTargetCompartment2 }
            };

            sewerConnection.Source = newTargetManhole;
            sewerConnection.Target = newTargetManhole;
            Assert.That(sewerConnection.SourceCompartment, Is.EqualTo(newTargetCompartment1));
            Assert.That(sewerConnection.TargetCompartment, Is.EqualTo(newTargetCompartment2));
        }

        [Test]
        public void GivenSewerConnection_WhenAddingManholeAsTargetWithTargetCompartmentDefined_ThenTheTargetCompartmentIsUnchanged()
        {
            var targetGeometry = new Point(0, 10);

            var otherCompartment = new Compartment { Name = "compartment1", Geometry = targetGeometry };
            var targetCompartment = new Compartment { Name = "compartment2", Geometry = targetGeometry };
            var targetManhole = new Manhole("TargetManhole")
            {
                Compartments = new EventedList<ICompartment> { otherCompartment, targetCompartment }
            };
            var sewerConnection = new SewerConnection { TargetCompartment = targetCompartment };

            sewerConnection.Target = targetManhole;
            Assert.That(sewerConnection.TargetCompartment, Is.EqualTo(targetCompartment));
        }

        #endregion

        #region SewerConnection geometry

        [Test]
        public void GivenSewerConnection_WhenAddingSourceManholeAndTargetManhole_ThenSewerConnectionGeometryDependsOnSourceAndTarget()
        {
            var sourceGeometry = new Point(0, 10);
            var targetGeometry = new Point(20, 30);

            var sourceCompartment = new Compartment { Geometry = sourceGeometry};
            var sourceManhole = new Manhole("SourceManhole")
            {
                Compartments = new EventedList<ICompartment> { sourceCompartment }
            };
            var targetCompartment = new Compartment { Geometry = targetGeometry };
            var targetManhole = new Manhole("TargetManhole")
            {
                Compartments = new EventedList<ICompartment> { targetCompartment }
            };
            var sewerConnection = new SewerConnection("mySewerConnection")
            {
                Source = sourceManhole,
                Target = targetManhole
            };

            Assert.That(sewerConnection.SourceCompartment, Is.EqualTo(sourceCompartment));
            Assert.That(sewerConnection.TargetCompartment, Is.EqualTo(targetCompartment));

            var expectedGeometry = new LineString(new[] {sourceGeometry.Coordinate, targetGeometry.Coordinate});
            Assert.That(sewerConnection.Geometry, Is.EqualTo(expectedGeometry));
        }

        [Test]
        public void GivenSewerConnection_WhenAddingSourceManholeWithoutCompartment_ThenSourceHasNotBeenAddedToSewerConnection()
        {
            var sewerConnection = new SewerConnection {Source = new Manhole("myManhole") { Compartments = new EventedList<ICompartment>()}};
            Assert.IsNull(sewerConnection.Source);
        }

        [Test]
        public void GivenSewerConnection_WhenAddingTargetManholeWithoutCompartment_ThenTargetHasNotBeenAddedToSewerConnection()
        {
            var sewerConnection = new SewerConnection { Target = new Manhole("myManhole") { Compartments = new EventedList<ICompartment>() } };
            Assert.IsNull(sewerConnection.Target);
        }

        [Test]
        public void GivenSewerConnection_WhenAddingSourceCompartmentAndTargetCompartment_ThenPipeGeometryIsSetToCompartmentGeometries()
        {
            var sourceGeometry = new Point(0, 10);
            var targetGeometry = new Point(20, 30);

            var sourceCompartment = new Compartment("SourceCompartment")
            {
                Geometry = sourceGeometry
            };
            var targetCompartment = new Compartment("TargetCompartment")
            {
                Geometry = targetGeometry
            };
            var sourceManhole = new Manhole("SourceManhole")
            {
                Compartments = new EventedList<ICompartment> { sourceCompartment }
            };
            var targetManhole = new Manhole("TargetManhole")
            {
                Compartments = new EventedList<ICompartment> { targetCompartment }
            };

            var sewerConnection = new SewerConnection("mySewerConnection")
            {
                SourceCompartment = sourceCompartment
            };
            Assert.That(sewerConnection.Source, Is.EqualTo(sourceManhole));
            Assert.IsNull(sewerConnection.Geometry);

            var expectedGeometry = new LineString(new[] {sourceManhole.Geometry.Coordinate, targetManhole.Geometry.Coordinate});
            sewerConnection.TargetCompartment = targetCompartment;
            Assert.That(sewerConnection.Target, Is.EqualTo(targetManhole));
            Assert.That(sewerConnection.Geometry, Is.EqualTo(expectedGeometry));
        }

        [Test]
        public void SewerConnectionGeometryGetsRefreshedWhenManholesGeometryChanges()
        {
            //This test relies on maholes getting a default geometry when being created.
            var sourceGeometry = new Point(1, 3);
            var targetGeometry = new Point(10, 7);

            var sourceCompartment = new Compartment { Geometry = sourceGeometry};
            var targetCompartment = new Compartment { Geometry = targetGeometry};
            var sourceManhole = new Manhole("sourceManhole");
            sourceManhole.Compartments.Add(sourceCompartment);
            var targetManhole = new Manhole("targetManhole");
            targetManhole.Compartments.Add(targetCompartment);

            var sewerConnection = new SewerConnection
            {
                SourceCompartment = sourceCompartment,
                TargetCompartment = targetCompartment
            };

            var connectionGeometry = sewerConnection.Geometry;
            var connectionLength = connectionGeometry.Length;
            Assert.IsNotNull(connectionGeometry);
            Assert.IsTrue(connectionGeometry.IsValid);
            Assert.IsTrue(connectionGeometry.Coordinates.Any());
            Assert.IsTrue(connectionGeometry.Coordinates.Contains(sourceGeometry.Coordinate));
            Assert.IsTrue(connectionGeometry.Coordinates.Contains(targetGeometry.Coordinate));

            //Change geometry now.
            var newSourceGeometry = new Coordinate(30, 30);
            var deltaX = newSourceGeometry.X - sourceGeometry.X;
            var deltaY = newSourceGeometry.Y - sourceGeometry.Y;
            GeometryHelper.MoveCoordinate(sourceManhole.Geometry, 0, deltaX, deltaY);

            Assert.IsNotNull(connectionGeometry);
            Assert.IsTrue(connectionGeometry.IsValid);
            Assert.IsTrue(connectionGeometry.Coordinates.Any());
            Assert.IsTrue(connectionGeometry.Coordinates.Contains(targetGeometry.Coordinate));
            Assert.IsTrue(connectionGeometry.Coordinates.Contains(newSourceGeometry));

            Assert.That(sourceCompartment.Geometry, Is.EqualTo(sourceManhole.Geometry));

            Assert.AreNotEqual(connectionLength, connectionGeometry.Length);
        }

        [Test]
        public void SetSourceCompartment_WithNull_SetsSourceCompartmentNameToNull()
        {
            // Setup
            var sewerConnection = new SewerConnection();
            var compartment = Substitute.For<ICompartment>();
            compartment.Name = "some_compartment_name";

            sewerConnection.SourceCompartment = compartment;

            Assert.That(sewerConnection.SourceCompartmentName, Is.Not.Null);

            // Call
            sewerConnection.SourceCompartment = null;

            // Assert
            Assert.That(sewerConnection.SourceCompartmentName, Is.Null);
        }

        [Test]
        public void SetTargetCompartment_WithNull_SetsTargetCompartmentNameToNull()
        {
            // Setup
            var sewerConnection = new SewerConnection();
            var compartment = Substitute.For<ICompartment>();
            compartment.Name = "some_compartment_name";

            sewerConnection.TargetCompartment = compartment;

            Assert.That(sewerConnection.TargetCompartmentName, Is.Not.Null);

            // Call
            sewerConnection.TargetCompartment = null;

            // Assert
            Assert.That(sewerConnection.TargetCompartmentName, Is.Null);
        }

        [Test]
        public void SetSource_WithHydroNode_SetsSourceCompartmentToNull()
        {
            // Setup
            var sewerConnection = new SewerConnection();
            var compartment = Substitute.For<ICompartment>();
            compartment.Name = "some_compartment_name";

            sewerConnection.SourceCompartment = compartment;

            Assert.That(sewerConnection.SourceCompartment, Is.Not.Null);

            // Call
            sewerConnection.Source = new HydroNode();

            // Assert
            Assert.That(sewerConnection.SourceCompartment, Is.Null);
        }

        [Test]
        public void SetTarget_WithHydroNode_SetsTargetCompartmentToNull()
        {
            // Setup
            var sewerConnection = new SewerConnection();
            var compartment = Substitute.For<ICompartment>();
            compartment.Name = "some_compartment_name";

            sewerConnection.TargetCompartment = compartment;

            Assert.That(sewerConnection.TargetCompartment, Is.Not.Null);

            // Call
            sewerConnection.Target = new HydroNode();

            // Assert
            Assert.That(sewerConnection.TargetCompartment, Is.Null);
        }

        #endregion

        #region Helpers

        private SewerConnection GetSewerConnectionWithSourceAndTarget()
        {
            var compartmentOne = new Compartment("compartmentOne") { Geometry = new Point(1, 3) };
            var sourceManhole = new Manhole("sourceManhole")
            {
                Compartments = new EventedList<ICompartment> {compartmentOne}
            };

            var compartmentTwo = new Compartment("compartmentTwo") {Geometry = new Point(10, 7)};
            var targetManhole = new Manhole("targetManhole")
            {
                Compartments = new EventedList<ICompartment> {compartmentTwo}
            };

            var sewerConnection = new SewerConnection
            {
                SourceCompartment = compartmentOne,
                TargetCompartment = compartmentTwo
            };

            Assert.AreEqual(compartmentOne, sewerConnection.SourceCompartment);
            Assert.AreEqual(compartmentTwo, sewerConnection.TargetCompartment);
            Assert.AreEqual(sourceManhole, sewerConnection.Source);
            Assert.AreEqual(targetManhole, sewerConnection.Target);

            sewerConnection.Geometry = new LineString(new []{sourceManhole.Geometry.Coordinate, targetManhole.Geometry.Coordinate});
            Assert.IsNotNull(sewerConnection.Geometry);

            return sewerConnection;
        }

        #endregion
    }
}