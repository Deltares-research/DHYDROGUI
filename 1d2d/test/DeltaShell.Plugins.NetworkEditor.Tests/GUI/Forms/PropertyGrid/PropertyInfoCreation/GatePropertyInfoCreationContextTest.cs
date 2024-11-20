using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
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
    public class GatePropertyInfoCreationContextTest
    {
        [Test]
        [TestCaseSource(nameof(CustomizeProperties_AnyArgNullCases))]
        public void CustomizeProperties_AnyArgNull_ThrowsArgumentNullException(GateProperties properties, GuiContainer guiContainer)
        {
            // Setup
            var creationContext = new GatePropertyInfoCreationContext();

            // Call
            void Call()
            {
                creationContext.CustomizeProperties(properties, guiContainer);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void SetName_AfterCustomizeProperties_For1DGate_ValidatesNameForUniqueness()
        {
            var guiContainer = new GuiContainer();
            var creationContext = new GatePropertyInfoCreationContext();

            var hydroNetwork = Substitute.For<IHydroNetwork>();
            IGate[] features = { GetFeature(hydroNetwork, "feat1"), GetFeature(hydroNetwork, "feat2"), GetFeature(hydroNetwork, "feat3") };
            hydroNetwork.Gates.Returns(features);

            IGate propertyData = features[0];
            var properties = new GateProperties { Data = propertyData };

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
        public void SetName_AfterCustomizeProperties_For2DGate_ValidatesNameForUniqueness()
        {
            var guiContainer = new GuiContainer();
            var creationContext = new GatePropertyInfoCreationContext();

            var hydroArea = new HydroArea();
            Gate2D[] features = { GetGate2D("feat1"), GetGate2D("feat2"), GetGate2D("feat3") };
            hydroArea.Gates.AddRange(features);

            guiContainer.Gui = CreateGuiWith(hydroArea);

            IGate propertyData = features[0];
            var properties = new GateProperties { Data = propertyData };

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

        private static IGate GetFeature(IHydroNetwork hydroNetwork, string name)
        {
            var feature = Substitute.For<IGate>();
            feature.Name = name;
            feature.HydroNetwork.Returns(hydroNetwork);

            return feature;
        }

        private static Gate2D GetGate2D(string name)
        {
            return new Gate2D { Name = name };
        }

        private static IGui CreateGuiWith(object newObject)
        {
            var gui = Substitute.For<IGui>();
            var application = Substitute.For<IApplication>();
            var project = new Project();

            gui.Application = application;
            application.ProjectService.Project.Returns(project);
            project.RootFolder.Add(newObject);

            return gui;
        }

        public static IEnumerable<TestCaseData> CustomizeProperties_AnyArgNullCases()
        {
            yield return new TestCaseData(null, new GuiContainer());
            yield return new TestCaseData(new GateProperties(), null);
        }
    }
}