using System.Collections.Generic;
using System.Windows.Media;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Services;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.GraphicsProviders;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Gui
{
    [TestFixture]
    public class HydroModelGuiGraphicsProviderTests
    {
        [TestCaseSource(nameof(DrawingGroupCreations))]
        public void CanProvideDrawingGroupFor_1D2DModel(object entry)
        {
            // Arrange
            var hydroModelGuiGraphicsProvider = new HydroModelGuiGraphicsProvider();

            // Act
            bool result = hydroModelGuiGraphicsProvider.CanProvideDrawingGroupFor(entry);
            
            // Assert
            Assert.IsTrue(result);
        }

        [TestCaseSource(nameof(DrawingGroupCreations))]
        public void CreateDrawingGroupFor_ProjectTemplate(object entry)
        {
            // Arrange
            var hydroModelGuiGraphicsProvider = new HydroModelGuiGraphicsProvider();

            // Act
            DrawingGroup result = hydroModelGuiGraphicsProvider.CreateDrawingGroupFor(entry);
            
            // Assert
            Assert.IsNotNull(result);
        }

        private static IEnumerable<object> DrawingGroupCreations()
        {
            var one= new ModelInfo
            {
                Name = "1D-2D Integrated Model (RHU)"
            };
            var two = new ProjectTemplate
            {
                Id = "RHUIntegratedModel"
            };
            var three = new ProjectTemplate
            {
                Id = "DimrProjectTemplateId"
            };
            var four = new DHydroConfigXmlImporter(
                Substitute.For<IFileImportService>(), 
                Substitute.For<IHydroModelReader>(),
                () => string.Empty);

            yield return new TestCaseData(one);
            yield return new TestCaseData(two);
            yield return new TestCaseData(three);
            yield return new TestCaseData(four);
        }
    }
}