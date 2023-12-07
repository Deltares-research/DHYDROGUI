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
    public class PumpPropertyInfoCreationContextTest
    {
        [Test]
        [TestCaseSource(nameof(CustomizeProperties_AnyArgNullCases))]
        public void CustomizeProperties_AnyArgNull_ThrowsArgumentNullException(PumpProperties properties, GuiContainer guiContainer)
        {
            // Setup
            var creationContext = new PumpPropertyInfoCreationContext();

            // Call
            void Call()
            {
                creationContext.CustomizeProperties(properties, guiContainer);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void SetName_AfterCustomizeProperties_For1DPump_ValidatesNameForUniqueness()
        {
            var properties = new PumpProperties();
            var guiContainer = new GuiContainer();
            var creationContext = new PumpPropertyInfoCreationContext();

            var hydroNetwork = Substitute.For<IHydroNetwork>();
            IPump[] features = { GetPump(hydroNetwork, "feat1"), GetPump(hydroNetwork, "feat2"), GetPump(hydroNetwork, "feat3") };
            hydroNetwork.Pumps.Returns(features);

            IPump propertyData = features[0];
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
        public void SetName_AfterCustomizeProperties_For2DPump_ValidatesNameForUniqueness()
        {
            var properties = new PumpProperties();
            var guiContainer = new GuiContainer();
            var creationContext = new PumpPropertyInfoCreationContext();

            var hydroArea = new HydroArea();
            Pump2D[] features = { GetPump2D("feat1"), GetPump2D("feat2"), GetPump2D("feat3") };
            hydroArea.Pumps.AddRange(features);

            guiContainer.Gui = CreateGuiWith(hydroArea);

            IPump propertyData = features[0];
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

        private static IPump GetPump(IHydroNetwork hydroNetwork, string name)
        {
            var feature = Substitute.For<IPump>();
            feature.Name = name;
            feature.HydroNetwork.Returns(hydroNetwork);

            return feature;
        }

        private static Pump2D GetPump2D(string name)
        {
            return new Pump2D { Name = name };
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

        public static IEnumerable<TestCaseData> CustomizeProperties_AnyArgNullCases()
        {
            yield return new TestCaseData(null, new GuiContainer());
            yield return new TestCaseData(new PumpProperties(), null);
        }
    }
}