using System.Collections.Specialized;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews;
using GeoAPI.Extensions.Networks;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.Forms.SewerFeatureViews
{
    [TestFixture]
    public class SewerConnectionViewModelTest
    {
        [Test]
        public void Constructor_SewerConnectionNull_ThrowsArgumentNullException()
        {
            // Setup
            RoughnessSection roughnessSection = CreateRoughnessSection();

            // Call
            void Call() => _ = new SewerConnectionViewModel(null, roughnessSection);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_WhenSewerConnectionIsPipe_AndRoughnessSectionNull_ThrowsArgumentNullException()
        {
            // Setup
            var sewerConnection = Substitute.For<IPipe>();

            // Call
            void Call() => _ = new SewerConnectionViewModel(sewerConnection, null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void SourceCompartmentName_WhenSourceIsHydroNode_AlwaysReturnsOpenWaterChannel()
        {
            // Setup
            var sewerConnection = Substitute.For<ISewerConnection>();
            sewerConnection.Source = Substitute.For<IHydroNode>();

            RoughnessSection roughnessSection = CreateRoughnessSection();

            var viewModel = new SewerConnectionViewModel(sewerConnection, roughnessSection);

            // Call
            string result = viewModel.SourceCompartmentName;

            // Assert
            Assert.That(result, Is.EqualTo("Open water channel"));
        }

        [Test]
        public void TargetCompartmentName_WhenTargetIsHydroNode_AlwaysReturnsOpenWaterChannel()
        {
            // Setup
            var sewerConnection = Substitute.For<ISewerConnection>();
            sewerConnection.Target = Substitute.For<IHydroNode>();

            RoughnessSection roughnessSection = CreateRoughnessSection();

            var viewModel = new SewerConnectionViewModel(sewerConnection, roughnessSection);

            // Call
            string result = viewModel.TargetCompartmentName;

            // Assert
            Assert.That(result, Is.EqualTo("Open water channel"));
        }

        [Test]
        public void SourceCompartmentName_WhenSourceIsNotHydroNode_ReturnsNodeName()
        {
            // Setup
            ICompartment compartment = CreateCompartment("some_compartment");
            var sewerConnection = Substitute.For<ISewerConnection>();
            sewerConnection.SourceCompartment = compartment;

            RoughnessSection roughnessSection = CreateRoughnessSection();

            var viewModel = new SewerConnectionViewModel(sewerConnection, roughnessSection);

            // Call
            string result = viewModel.SourceCompartmentName;

            // Assert
            Assert.That(result, Is.EqualTo("some_compartment"));
        }

        [Test]
        public void TargetCompartmentName_WhenTargetIsNotHydroNode_ReturnsNodeName()
        {
            // Setup
            ICompartment compartment = CreateCompartment("some_compartment");
            var sewerConnection = Substitute.For<ISewerConnection>();
            sewerConnection.TargetCompartment = compartment;

            RoughnessSection roughnessSection = CreateRoughnessSection();

            var viewModel = new SewerConnectionViewModel(sewerConnection, roughnessSection);

            // Call
            string result = viewModel.TargetCompartmentName;

            // Assert
            Assert.That(result, Is.EqualTo("some_compartment"));
        }

        private static ICompartment CreateCompartment(string name)
        {
            var compartment = Substitute.For<ICompartment>();
            compartment.Name = name;

            return compartment;
        }

        private static RoughnessSection CreateRoughnessSection()
        {
            INetwork network = Substitute.For<INetwork, INotifyCollectionChanged>();
            var crossSectionSectionType = new CrossSectionSectionType();
            var roughnessSection = new RoughnessSection(crossSectionSectionType, network);
            return roughnessSection;
        }
    }
}