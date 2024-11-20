using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.TestUtils;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid.PropertyInfoCreation;
using log4net.Core;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.Forms.PropertyGrid.PropertyInfoCreation
{
    [TestFixture]
    public class ManholePropertyInfoCreationContextTest
    {
        [Test]
        [TestCaseSource(nameof(CustomizeProperties_AnyArgNullCases))]
        public void CustomizeProperties_AnyArgNull_ThrowsArgumentNullException(ManholeProperties properties, GuiContainer guiContainer)
        {
            // Setup
            var creationContext = new ManholePropertyInfoCreationContext();

            // Call
            void Call()
            {
                creationContext.CustomizeProperties(properties, guiContainer);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void SetName_AfterCustomizeProperties_ValidatesManholeNameForUniqueness()
        {
            var properties = new ManholeProperties();
            var guiContainer = new GuiContainer();
            var creationContext = new ManholePropertyInfoCreationContext();

            var hydroNetwork = Substitute.For<IHydroNetwork>();
            Manhole[] features = { GetFeature(hydroNetwork, "feat1"), GetFeature(hydroNetwork, "feat2"), GetFeature(hydroNetwork, "feat3") };
            hydroNetwork.Manholes.Returns(features);

            Manhole propertyData = features[0];
            properties.Data = propertyData;

            creationContext.CustomizeProperties(properties, guiContainer);

            // Call
            void Call()
            {
                properties.Name = "feat2";
            }

            // Assert
            string warning = TestHelper.GetAllRenderedMessages(Call, Level.Warn).Single();
            Assert.That(warning, Is.EqualTo("Item with the name 'feat2' already exists."));
            Assert.That(properties.Name, Is.EqualTo("feat1"));
        }

        [Test]
        public void SetName_AfterCustomizeProperties_ValidatesCompartmentNameForUniqueness()
        {
            var guiContainer = new GuiContainer();
            var creationContext = new ManholePropertyInfoCreationContext();

            var hydroNetwork = Substitute.For<IHydroNetwork>();

            Manhole manhole = GetFeature(hydroNetwork, "manhole_name");
            Compartment[] compartments = { GetCompartment("feat1"), GetCompartment("feat2"), GetCompartment("feat3") };
            manhole.Compartments.AddRange(compartments);

            hydroNetwork.Manholes.Returns(new[] { manhole });
            hydroNetwork.Compartments.Returns(compartments);

            var properties = new ManholeProperties { Data = manhole };

            creationContext.CustomizeProperties(properties, guiContainer);

            // Call
            void Call()
            {
                properties.CompartmentOneName = "feat2";
            }

            // Assert
            string warning = TestHelper.GetAllRenderedMessages(Call, Level.Warn).Single();
            Assert.That(warning, Is.EqualTo("Item with the name 'feat2' already exists."));
            Assert.That(properties.CompartmentOneName, Is.EqualTo("feat1"));
        }

        private static Manhole GetFeature(IHydroNetwork hydroNetwork, string name)
        {
            var feature = Substitute.For<Manhole>();
            feature.Name = name;
            feature.HydroNetwork.Returns(hydroNetwork);

            return feature;
        }

        private static Compartment GetCompartment(string name)
        {
            return new Compartment(name);
        }

        public static IEnumerable<TestCaseData> CustomizeProperties_AnyArgNullCases()
        {
            yield return new TestCaseData(null, new GuiContainer());
            yield return new TestCaseData(new ManholeProperties(), null);
        }
    }
}