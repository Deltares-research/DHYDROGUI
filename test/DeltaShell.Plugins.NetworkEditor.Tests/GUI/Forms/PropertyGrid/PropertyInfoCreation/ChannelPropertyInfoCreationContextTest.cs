using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
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
    public class ChannelPropertyInfoCreationContextTest
    {
        [Test]
        [TestCaseSource(nameof(CustomizeProperties_AnyArgNullCases))]
        public void CustomizeProperties_AnyArgNull_ThrowsArgumentNullException(ChannelProperties properties, GuiContainer guiContainer)
        {
            // Setup
            var creationContext = new ChannelPropertyInfoCreationContext();

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
            var creationContext = new ChannelPropertyInfoCreationContext();

            var hydroNetwork = Substitute.For<IHydroNetwork>();
            IChannel[] features = { GetFeature(hydroNetwork, "feat1"), GetFeature(hydroNetwork, "feat2"), GetFeature(hydroNetwork, "feat3") };
            hydroNetwork.Channels.Returns(features);

            IChannel propertyData = features[0];
            var properties = new ChannelProperties { Data = propertyData };

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

        private static IChannel GetFeature(IHydroNetwork hydroNetwork, string name)
        {
            var feature = Substitute.For<IChannel>();
            feature.Name = name;
            feature.HydroNetwork.Returns(hydroNetwork);

            return feature;
        }

        public static IEnumerable<TestCaseData> CustomizeProperties_AnyArgNullCases()
        {
            yield return new TestCaseData(null, new GuiContainer());
            yield return new TestCaseData(new ChannelProperties(), null);
        }
    }
}