using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.PropertyGrid.PropertyInfoCreation;
using log4net.Core;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Forms.PropertyInfoCreation
{
    [TestFixture]
    public class FMWeirPropertyInfoCreationContextTest
    {
        [Test]
        [TestCaseSource(nameof(CustomizeProperties_AnyArgNullCases))]
        public void CustomizeProperties_AnyArgNull_ThrowsArgumentNullException(FMWeirProperties properties, GuiContainer guiContainer)
        {
            // Setup
            var creationContext = new FMWeirPropertyInfoCreationContext();

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
            var creationContext = new FMWeirPropertyInfoCreationContext();

            var hydroArea = new HydroArea();
            guiContainer.Gui = CreateGuiWith(hydroArea);
            Weir2D[] features = { GetFeature("feat1"), GetFeature("feat2"), GetFeature("feat3") };
            hydroArea.Weirs.AddRange(features);

            Weir2D propertyData = features[0];
            var properties = new FMWeirProperties { Data = propertyData };

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

        private static IGui CreateGuiWith(object newObject)
        {
            var gui = Substitute.For<IGui>();
            var application = Substitute.For<IApplication>();
            var project = new Project();

            gui.Application = application;
            application.Project.Returns(project);
            project.RootFolder.Add(newObject);

            return gui;
        }

        private static Weir2D GetFeature(string name)
        {
            return new Weir2D { Name = name };
        }

        public static IEnumerable<TestCaseData> CustomizeProperties_AnyArgNullCases()
        {
            yield return new TestCaseData(null, new GuiContainer());
            yield return new TestCaseData(new FMWeirProperties(), null);
        }
    }
}