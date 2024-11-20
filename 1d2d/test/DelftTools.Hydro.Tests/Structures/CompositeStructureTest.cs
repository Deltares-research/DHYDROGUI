using System.Linq;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;
using ValidationAspects;

namespace DelftTools.Hydro.Tests.Structures
{
    [TestFixture]
    public class CompositeStructureTest
    {
        [Test]
        public void EmptyCompositeStructure()
        {
            var compositeBranchStructure = new CompositeBranchStructure();
            var validationResult = compositeBranchStructure.Validate();
            Assert.AreEqual(false, validationResult.IsValid);

            Assert.AreEqual(1, validationResult.Messages.Count());
            Assert.AreEqual(compositeBranchStructure, validationResult.ValidationException.Context.Instance);
        }
        [Test]
        public void CompositeStructure1SimpleWeir()
        {
            var compositeBranchStructure = new CompositeBranchStructure();
            IWeir weir = new Weir("test");
            compositeBranchStructure.Structures.Add(weir);

            var validationResult = compositeBranchStructure.Validate();
            Assert.AreEqual(true, validationResult.IsValid);
        }

        [Test]
        public void CompositeStructure1Gate()
        {
            var compositeBranchStructure = new CompositeBranchStructure();
            IGate gate = new Gate("test");
            compositeBranchStructure.Structures.Add(gate);

            var validationResult = compositeBranchStructure.Validate();
            Assert.AreEqual(true, validationResult.IsValid);
        }

        [Test]
        public void Composite2IdenticalWeirs()
        {
            // 2 identical and thus overlapping weirs
            var compositeBranchStructure = new CompositeBranchStructure();
            IWeir weir1 = new Weir("test");
            compositeBranchStructure.Structures.Add(weir1);
            IWeir weir2 = new Weir("test");
            compositeBranchStructure.Structures.Add(weir2);

            var validationResult = compositeBranchStructure.Validate();
            // offsetY is representing property only and should not generate validation error
            Assert.IsTrue(validationResult.IsValid);
        }

        [Test]
        public void Composite2IdenticalGates()
        {
            // 2 identical and thus overlapping weirs
            var compositeBranchStructure = new CompositeBranchStructure();
            IGate gate1 = new Gate("test");
            compositeBranchStructure.Structures.Add(gate1);
            IGate gate2 = new Gate("test");
            compositeBranchStructure.Structures.Add(gate2);

            var validationResult = compositeBranchStructure.Validate();
            // offsetY is representing property only and should not generate validation error
            Assert.IsTrue(validationResult.IsValid);
        }

        [Test]
        public void Composite2NeightbourWeirs()
        {
            // 2 identical and thus overlapping weirs
            var compositeBranchStructure = new CompositeBranchStructure();
            IWeir weir1 = new Weir("test") {OffsetY = 0, CrestWidth = 100};
            compositeBranchStructure.Structures.Add(weir1);
            IWeir weir2 = new Weir("test") {OffsetY = 100, CrestWidth = 100};
            compositeBranchStructure.Structures.Add(weir2);

            var validationResult = compositeBranchStructure.Validate();
            Assert.AreEqual(true, validationResult.IsValid);
        }

        [Test]
        public void Composite2NeightbourGates()
        {
            // 2 identical and thus overlapping weirs
            var compositeBranchStructure = new CompositeBranchStructure();
            IGate gate1 = new Gate("test") { OffsetY = 0, OpeningWidth = 100 };
            compositeBranchStructure.Structures.Add(gate1);
            IGate gate2 = new Gate("test") { OffsetY = 100, OpeningWidth = 100 };
            compositeBranchStructure.Structures.Add(gate2);

            var validationResult = compositeBranchStructure.Validate();
            Assert.AreEqual(true, validationResult.IsValid);
        }

        [Test]
        public void ChangingChildBranchStructureChainageWillUpdateParentAndSubsequentChildren() // Issue#: SOBEK3-638
        {
            // Setup
            const double compositeStructureOriginalChainage = 300.0d;
            var compositeBranchStructure = new CompositeBranchStructure()
            {
                Chainage = compositeStructureOriginalChainage, 
                Branch = new Channel()
            };

            IWeir weir1 = new Weir("test");
            IPump pump1 = new Pump("test");
            
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, weir1);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, pump1);

            Assert.AreEqual(compositeStructureOriginalChainage, compositeBranchStructure.Chainage,
                "Adding child BranchStructures to a CompositeBranchStructure should not change Chainage");
            foreach (var structure in compositeBranchStructure.Structures)
            {
                Assert.AreEqual(compositeBranchStructure.Chainage, structure.Chainage,
                    "CompositeBranchStructure should always overwrite Chainage in child BranchStructure");
            }

            // Update Child BranchStructure
            const double updatedWeirChainage = 200.0d;
            weir1.Chainage = updatedWeirChainage;

            Assert.AreEqual(updatedWeirChainage, compositeBranchStructure.Chainage,
                "Changing Chainage in a child BranchStructures should update CompositeBranchStructure Chainage");
            foreach (var structure in compositeBranchStructure.Structures)
            {
                Assert.AreEqual(compositeBranchStructure.Chainage, structure.Chainage,
                    "Changing Chainage in CompositeBranchStructure should always update all child BranchStructure");
            }

            // Update Child BranchStructure
            const double updatedPumpChainage = 100.0d;
            pump1.Chainage = updatedPumpChainage;

            Assert.AreEqual(updatedPumpChainage, compositeBranchStructure.Chainage,
                "Changing Chainage in a child BranchStructures should update CompositeBranchStructure Chainage");
            foreach (var structure in compositeBranchStructure.Structures)
            {
                Assert.AreEqual(compositeBranchStructure.Chainage, structure.Chainage,
                    "Changing Chainage in CompositeBranchStructure should always update all child BranchStructure");
            }
        }

        [Test]
        public void CompositeBranchStructureOnlyFiresOnePropertyChangedEventEachTimeChainageUpdated()
        {
            var branch = new Branch();
            var compositeBranchStructure = new CompositeBranchStructure();
            branch.BranchFeatures.Add(compositeBranchStructure);

            IWeir weir = new Weir("test");
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, weir);

            var numEvents = 0;
            compositeBranchStructure.PropertyChanged += (s, e) => { numEvents++; };

            compositeBranchStructure.Chainage = 0.6;
            Assert.AreEqual(1, numEvents, 
                "Composite branch structure should only fire one property changed event when chainage is updated");

            weir.Chainage = 0.4;
            Assert.AreEqual(2, numEvents, 
                "Child of composite branch structure should only fire one property changed event when chainage is updated");
        }
        
    }
}
