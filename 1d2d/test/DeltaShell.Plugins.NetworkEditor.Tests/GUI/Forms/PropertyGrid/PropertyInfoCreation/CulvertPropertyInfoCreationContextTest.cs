using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
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
    public class CulvertPropertyInfoCreationContextTest
    {
        [Test]
        [TestCaseSource(nameof(CustomizeProperties_AnyArgNullCases))]
        public void CustomizeProperties_AnyArgNull_ThrowsArgumentNullException(CulvertProperties properties, GuiContainer guiContainer)
        {
            // Setup
            var creationContext = new CulvertPropertyInfoCreationContext();

            // Call
            void Call()
            {
                creationContext.CustomizeProperties(properties, guiContainer);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void SetName_AfterCustomizeProperties_ValidatesNameForUniqueness()
        {
            var guiContainer = new GuiContainer();
            var creationContext = new CulvertPropertyInfoCreationContext();

            var hydroNetwork = Substitute.For<IHydroNetwork>();
            ICulvert[] features = { GetFeature(hydroNetwork, "feat1"), GetFeature(hydroNetwork, "feat2"), GetFeature(hydroNetwork, "feat3") };
            hydroNetwork.Culverts.Returns(features);

            ICulvert propertyData = features[0];
            var properties = new CulvertProperties { Data = propertyData };

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

        private static ICulvert GetFeature(IHydroNetwork hydroNetwork, string name)
        {
            var feature = Substitute.For<ICulvert>();
            feature.Name = name;
            feature.HydroNetwork.Returns(hydroNetwork);

            return feature;
        }

        public static IEnumerable<TestCaseData> CustomizeProperties_AnyArgNullCases()
        {
            yield return new TestCaseData(null, new GuiContainer());
            yield return new TestCaseData(new CulvertProperties(), null);
        }
    }
}