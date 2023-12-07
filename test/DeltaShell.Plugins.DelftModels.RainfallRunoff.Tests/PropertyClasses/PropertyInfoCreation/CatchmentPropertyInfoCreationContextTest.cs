using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.PropertyClasses;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.PropertyClasses.PropertyInfoCreation;
using log4net.Core;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.PropertyClasses.PropertyInfoCreation
{
    [TestFixture]
    public class CatchmentPropertyInfoCreationContextTest
    {
        [Test]
        [TestCaseSource(nameof(CustomizeProperties_AnyArgNullCases))]
        public void CustomizeProperties_AnyArgNull_ThrowsArgumentNullException(CatchmentProperties properties, GuiContainer guiContainer)
        {
            // Setup
            var creationContext = new CatchmentPropertyInfoCreationContext();

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
            var properties = new CatchmentProperties();
            var guiContainer = new GuiContainer();
            var creationContext = new CatchmentPropertyInfoCreationContext();

            var drainageBasin = new DrainageBasin();
            Catchment[] features = { GetFeature(drainageBasin, "feat1"), GetFeature(drainageBasin, "feat2"), GetFeature(drainageBasin, "feat3") };
            drainageBasin.Catchments.AddRange(features);

            Catchment propertyData = features[0];
            properties.Data = propertyData;

            RainfallRunoffModel model = GetModelWithCatchmentModelData(propertyData);
            guiContainer.Gui = CreateGuiWith(model);

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

        private static RainfallRunoffModel GetModelWithCatchmentModelData(Catchment propertyData)
        {
            var model = new RainfallRunoffModel();
            CatchmentModelData catchmentModelData = new GreenhouseData(propertyData);
            model.ModelData.Add(catchmentModelData);
            return model;
        }

        private static Catchment GetFeature(IDrainageBasin drainageBasin, string name)
        {
            var feature = Substitute.For<Catchment>();
            feature.Name = name;
            feature.Basin = drainageBasin;

            return feature;
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
            yield return new TestCaseData(new CatchmentProperties(), null);
        }
    }
}