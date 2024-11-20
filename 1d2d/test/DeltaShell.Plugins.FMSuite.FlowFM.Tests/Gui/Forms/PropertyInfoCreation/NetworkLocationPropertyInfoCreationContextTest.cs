using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.PropertyGrid.PropertyInfoCreation;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid;
using GeoAPI.Extensions.Coverages;
using log4net.Core;
using NetTopologySuite.Extensions.Coverages;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Forms.PropertyInfoCreation
{
    [TestFixture]
    public class NetworkLocationPropertyInfoCreationContextTest
    {
        [Test]
        [TestCaseSource(nameof(CustomizeProperties_AnyArgNullCases))]
        public void CustomizeProperties_AnyArgNull_ThrowsArgumentNullException(NetworkLocationProperties properties, GuiContainer guiContainer)
        {
            // Setup
            var creationContext = new NetworkLocationPropertyInfoCreationContext();

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
            var creationContext = new NetworkLocationPropertyInfoCreationContext();

            var model = new WaterFlowFMModel();
            model.NetworkDiscretization = new Discretization();
            guiContainer.Gui = CreateGuiWithModel(model);
            INetworkLocation[] features = { GetFeature("feat1"), GetFeature("feat2"), GetFeature("feat3") };
            model.NetworkDiscretization.Locations.AddValues(features);

            INetworkLocation propertyData = features[0];
            var properties = new NetworkLocationProperties { Data = propertyData };

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

        private static IGui CreateGuiWithModel(IModel model)
        {
            var gui = Substitute.For<IGui>();

            var project = new Project();
            project.RootFolder.Add(model);
            gui.Application.ProjectService.Project.Returns(project);
            gui.Application.ProjectService.IsProjectOpen.Returns(true);

            return gui;
        }

        private static INetworkLocation GetFeature(string name)
        {
            var feature = Substitute.For<INetworkLocation>();
            feature.Name = name;

            return feature;
        }

        public static IEnumerable<TestCaseData> CustomizeProperties_AnyArgNullCases()
        {
            yield return new TestCaseData(null, new GuiContainer());
            yield return new TestCaseData(new NetworkLocationProperties(), null);
        }
    }
}