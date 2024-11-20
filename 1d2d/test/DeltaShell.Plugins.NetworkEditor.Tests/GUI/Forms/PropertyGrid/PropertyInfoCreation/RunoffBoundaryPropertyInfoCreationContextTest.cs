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
    public class RunoffBoundaryPropertyInfoCreationContextTest
    {
        [Test]
        [TestCaseSource(nameof(CustomizeProperties_AnyArgNullCases))]
        public void CustomizeProperties_AnyArgNull_ThrowsArgumentNullException(RunoffBoundaryProperties properties, GuiContainer guiContainer)
        {
            // Setup
            var creationContext = new RunoffBoundaryPropertyInfoCreationContext();

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
            var properties = new RunoffBoundaryProperties();
            var guiContainer = new GuiContainer();
            var creationContext = new RunoffBoundaryPropertyInfoCreationContext();

            var drainageBasin = new DrainageBasin();
            RunoffBoundary[] features = { GetFeature(drainageBasin, "feat1"), GetFeature(drainageBasin, "feat2"), GetFeature(drainageBasin, "feat3") };
            drainageBasin.Boundaries.AddRange(features);

            RunoffBoundary propertyData = features[0];
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

        private static RunoffBoundary GetFeature(IDrainageBasin drainageBasin, string name)
        {
            var feature = Substitute.For<RunoffBoundary>();
            feature.Name = name;
            feature.Basin = drainageBasin;

            return feature;
        }

        public static IEnumerable<TestCaseData> CustomizeProperties_AnyArgNullCases()
        {
            yield return new TestCaseData(null, new GuiContainer());
            yield return new TestCaseData(new RunoffBoundaryProperties(), null);
        }
    }
}