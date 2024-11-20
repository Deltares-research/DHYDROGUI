using System.Collections.Generic;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using NetTopologySuite.Extensions.Features;
using NUnit.Framework;
using SharpMap.Api.Layers;
using SharpMap.Editors.Interactors;
using SharpMap.Layers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Layers
{
    [TestFixture]
    public class LateralMapLayerProviderTest
    {
        [Test]
        [TestCaseSource(nameof(Create_ArgNullCases))]
        public void Create_ArgNull_ThrowsArgumentNullException(IEventedList<Feature2D> lateralFeatures, WaterFlowFMModel model)
        {
            // Setup
            void Call() => LateralMapLayerProvider.Create(lateralFeatures, model);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Create_CreateCorrectVectorLayer()
        {
            // Setup
            IEventedList<Feature2D> lateralFeatures = new EventedList<Feature2D>();
            var model = new WaterFlowFMModel();

            // Setup
            ILayer layer = LateralMapLayerProvider.Create(lateralFeatures, model);

            // Assert
            Assert.That(layer, Is.TypeOf<VectorLayer>());
            var vectorLayer = (VectorLayer)layer;
            Assert.That(vectorLayer.Name, Is.EqualTo("Laterals"));
            Assert.That(vectorLayer.NameIsReadOnly, Is.True);
            Assert.That(vectorLayer.ShowInLegend, Is.False);
            Assert.That(vectorLayer.DataSource.Features, Is.SameAs(lateralFeatures));
            Assert.That(vectorLayer.DataSource.FeatureType, Is.EqualTo(typeof(Feature2D)));
            Assert.That(vectorLayer.FeatureEditor, Is.TypeOf<Feature2DEditor>());
        }

        private static IEnumerable<TestCaseData> Create_ArgNullCases()
        {
            IEventedList<Feature2D> lateralFeatures = new EventedList<Feature2D>();
            var model = new WaterFlowFMModel();

            yield return new TestCaseData(null, model);
            yield return new TestCaseData(lateralFeatures, null);
        }
    }
}