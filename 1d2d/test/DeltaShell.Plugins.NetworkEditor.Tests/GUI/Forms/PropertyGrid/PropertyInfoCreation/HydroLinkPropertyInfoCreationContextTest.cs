using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid.PropertyInfoCreation;
using log4net.Core;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.Forms.PropertyGrid.PropertyInfoCreation
{
    [TestFixture]
    public class HydroLinkPropertyInfoCreationContextTest
    {
        [Test]
        [TestCaseSource(nameof(CustomizeProperties_AnyArgNullCases))]
        public void CustomizeProperties_AnyArgNull_ThrowsArgumentNullException(HydroLinkProperties properties, GuiContainer guiContainer)
        {
            // Setup
            var creationContext = new HydroLinkPropertyInfoCreationContext();

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
            var creationContext = new HydroLinkPropertyInfoCreationContext();

            var hydroRegion = Substitute.For<IHydroRegion>();
            var features = new EventedList<HydroLink>
            {
                GetFeature(hydroRegion, "feat1"),
                GetFeature(hydroRegion, "feat2"),
                GetFeature(hydroRegion, "feat3")
            };
            hydroRegion.Links.Returns(features);

            IGui gui = CreateGuiWith(hydroRegion);
            guiContainer.Gui = gui;

            HydroLink propertyData = features[0];
            var properties = new HydroLinkProperties { Data = propertyData };

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

        private static HydroLink GetFeature(IHydroRegion hydroRegion, string name)
        {
            var feature = new HydroLink { Name = name };

            var source = Substitute.For<IHydroObject>();
            source.Region.Returns(hydroRegion);

            var target = Substitute.For<IHydroObject>();
            target.Region.Returns(hydroRegion);

            feature.Source = source;
            feature.Target = target;

            return feature;
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
            yield return new TestCaseData(new HydroLinkProperties(), null);
        }
    }
}