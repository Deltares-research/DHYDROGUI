using System.Collections.Generic;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.MapTools;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap.Api;
using SharpMap.Api.Layers;
using SharpMap.Layers;
using SharpMap.Rendering;
using SharpMap.UI.Forms;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.GUI.MapTools
{
    [TestFixture]
    public class FindGridCellToolTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup 
            const string toolName = "Just a tool name";

            // Call
            var tool = new FindGridCellTool(toolName);

            // Assert
            Assert.That(tool, Is.InstanceOf<MapTool>(), "Tool is not the correct derived type.");
            Assert.That(tool.Name, Is.EqualTo(toolName), "Name of the tool does not have the correct value.");
            Assert.That(tool.GetWaqModelForGrid, Is.Null, $"Function delegate {nameof(FindGridCellTool.GetWaqModelForGrid)} is not null.");
            Assert.That(tool.LayerFilter, Is.Not.Null, $"Function delegate {nameof(FindGridCellTool.LayerFilter)} is null.");
        }

        [Test]
        public void Enabled_WithoutLayersAndGetWaqModelForGridReturnsValue_ReturnsFalse()
        {
            // Setup
            var mocks = new MockRepository();
            var map = mocks.Stub<IMap>();
            map.Stub(m => m.GetAllVisibleLayers(false))
               .IgnoreArguments()
               .Return(Enumerable.Empty<ILayer>());

            var mapControl = mocks.Stub<IMapControl>();
            mapControl.Stub(mc => mc.Map).Return(map);
            mocks.ReplayAll();

            using (var model = new WaterQualityModel())
            {
                ConfigureWaterQualityModel(model, mocks);
                var tool = new FindGridCellTool(string.Empty)
                {
                    GetWaqModelForGrid = ug => { return model; },
                    MapControl = mapControl
                };

                // Call
                bool isEnabled = tool.Enabled;

                // Assert
                Assert.That(isEnabled, Is.False, "Enabled is true while there are no map layers.");
            }

            mocks.VerifyAll();
        }

        [Test]
        public void Enabled_WithGetWaqModelForGridNull_ReturnsFalse()
        {
            // Setup
            var layer = new UnstructuredGridLayer {Renderer = null};

            var mocks = new MockRepository();
            var map = mocks.Stub<IMap>();
            map.Stub(m => m.GetAllVisibleLayers(false))
               .IgnoreArguments()
               .Return(new[]
               {
                   layer
               });

            var mapControl = mocks.Stub<IMapControl>();
            mapControl.Stub(mc => mc.Map).Return(map);
            mocks.ReplayAll();

            var tool = new FindGridCellTool(string.Empty) {MapControl = mapControl};

            // Call
            bool isEnabled = tool.Enabled;

            // Assert
            Assert.That(isEnabled, Is.False, $"Enabled is true while {nameof(FindGridCellTool.GetWaqModelForGrid)} is null.");
            mocks.VerifyAll();
        }

        [Test]
        [TestCaseSource(nameof(GetGridRendererConfigurations))]
        public void Enabled_WithUnstructuredGridMapLayerWithVariousRendererConfigurations_ReturnsExpectedValue(
            IGridRenderer renderer,
            bool expectedEnabledValue)
        {
            // Setup
            var layer = new UnstructuredGridLayer {Renderer = renderer};

            var mocks = new MockRepository();
            var map = mocks.Stub<IMap>();
            map.Stub(m => m.GetAllVisibleLayers(false))
               .IgnoreArguments()
               .Return(new[]
               {
                   layer
               });

            var mapControl = mocks.Stub<IMapControl>();
            mapControl.Stub(mc => mc.Map).Return(map);
            mocks.ReplayAll();

            using (var model = new WaterQualityModel())
            {
                ConfigureWaterQualityModel(model, mocks);
                var tool = new FindGridCellTool(string.Empty)
                {
                    GetWaqModelForGrid = ug => { return model; },
                    MapControl = mapControl
                };

                // Call
                bool isEnabled = tool.Enabled;

                // Assert
                Assert.That(isEnabled, Is.EqualTo(expectedEnabledValue),
                            $"Enabled is {isEnabled} while there are map layers of type {nameof(UnstructuredGridLayer)}.");
            }

            mocks.VerifyAll();
        }

        [Test]
        public void Enabled_WithUnstructuredGridMapLayerWithNonGridEdgeRenderer_ReturnsTrue()
        {
            // Setup
            var mocks = new MockRepository();

            var layer = new UnstructuredGridLayer();
            var renderer = mocks.Stub<IGridRenderer>();
            layer.Renderer = renderer;

            var map = mocks.Stub<IMap>();
            map.Stub(m => m.GetAllVisibleLayers(false))
               .IgnoreArguments()
               .Return(new[]
               {
                   layer
               });

            var mapControl = mocks.Stub<IMapControl>();
            mapControl.Stub(mc => mc.Map).Return(map);
            mocks.ReplayAll();

            using (var model = new WaterQualityModel())
            {
                ConfigureWaterQualityModel(model, mocks);
                var tool = new FindGridCellTool(string.Empty)
                {
                    GetWaqModelForGrid = ug => { return model; },
                    MapControl = mapControl
                };

                // Call
                bool isEnabled = tool.Enabled;

                // Assert
                Assert.That(isEnabled, Is.True,
                            $"Enabled is false while renderer GridRenderer is not of type {nameof(GridEdgeRenderer)}.");
            }

            mocks.VerifyAll();
        }

        [Test]
        public void Enabled_WithLayersAndPointToGridCellMapper_ReturnsTrue()
        {
            // Setup
            var layer = new UnstructuredGridLayer();

            var mocks = new MockRepository();
            var renderer = mocks.Stub<IGridRenderer>();
            layer.Renderer = renderer;

            var map = mocks.Stub<IMap>();
            map.Stub(m => m.GetAllVisibleLayers(false))
               .IgnoreArguments()
               .Return(new[]
               {
                   layer
               });

            var mapControl = mocks.Stub<IMapControl>();
            mapControl.Stub(mc => mc.Map).Return(map);
            mocks.ReplayAll();

            using (var model = new WaterQualityModel())
            {
                ConfigureWaterQualityModel(model, mocks);
                var tool = new FindGridCellTool(string.Empty)
                {
                    GetWaqModelForGrid = ug => { return model; },
                    MapControl = mapControl
                };

                // Call
                bool isEnabled = tool.Enabled;

                // Assert
                Assert.That(isEnabled, Is.True, $"Enabled is false while layers and a {nameof(PointToGridCellMapper)} are present.");
            }

            mocks.VerifyAll();
        }

        [Test]
        public void Enabled_WithLayersAndWithoutPointToGridCellMapper_ReturnsFalse()
        {
            // Setup
            var layer = new UnstructuredGridLayer();

            var mocks = new MockRepository();
            var renderer = mocks.Stub<IGridRenderer>();
            layer.Renderer = renderer;

            var map = mocks.Stub<IMap>();
            map.Stub(m => m.GetAllVisibleLayers(false))
               .IgnoreArguments()
               .Return(new[]
               {
                   layer
               });

            var mapControl = mocks.Stub<IMapControl>();
            mapControl.Stub(mc => mc.Map).Return(map);
            mocks.ReplayAll();

            using (var model = new WaterQualityModel())
            {
                var tool = new FindGridCellTool(string.Empty)
                {
                    GetWaqModelForGrid = ug => { return model; },
                    MapControl = mapControl
                };

                // Call
                bool isEnabled = tool.Enabled;

                // Assert
                Assert.That(isEnabled, Is.False, $"Enabled is false while no {nameof(PointToGridCellMapper)} is present.");
            }

            mocks.VerifyAll();
        }

        /// <summary>
        /// Configures the <see cref="WaterQualityModel"/> to a valid state.
        /// </summary>
        /// <param name="model">The model to configure.</param>
        /// <param name="mocks">The mock repository to configure the <see cref="WaterQualityModel"/> with.</param>
        private static void ConfigureWaterQualityModel(WaterQualityModel model, MockRepository mocks)
        {
            string testFilePath = TestHelper.GetTestFilePath(@"IO\attribute files\random_3x5.atr");

            var hydroData = MockRepository.GenerateStub<IHydroData>();
            hydroData.Stub(hd => hd.Grid).Return(model.Grid);
            hydroData.Stub(hd => hd.HasSameSchematization(null))
                     .IgnoreArguments()
                     .Return(true);
            hydroData.Stub(hd => hd.DataChanged += null)
                     .IgnoreArguments();
            hydroData.Stub(hd => hd.FilePath)
                     .Return(testFilePath);
            hydroData.Stub(hd => hd.AttributesRelativePath)
                     .Return("random_3x5.atr");
            hydroData.Stub(hd => hd.NumberOfDelwaqSegmentsPerHydrodynamicLayer)
                     .Return(3);
            hydroData.Stub(hd => hd.NumberOfWaqSegmentLayers)
                     .Return(5);
            hydroData.Stub(hd => hd.NumberOfHydrodynamicLayersPerWaqSegmentLayer)
                     .Return(new[]
                     {
                         0,
                         1,
                         2,
                         3,
                         4
                     });
            hydroData.Stub(hd => hd.HydrodynamicLayerThicknesses)
                     .Return(new double[]
                     {
                         0,
                         1,
                         2,
                         3,
                         4,
                         5,
                         6,
                         7,
                         8,
                         9
                     });
            mocks.ReplayAll();

            // The import is needed to set the PointToGridCellMapper.
            model.ImportHydroData(hydroData);
        }

        private static IEnumerable<TestCaseData> GetGridRendererConfigurations()
        {
            yield return new TestCaseData(null, true)
                .SetName("No render");

            yield return new TestCaseData(new GridEdgeRenderer {GridEdgeRenderMode = GridEdgeRenderMode.EdgesOnly},
                                          true)
                .SetName("GridEdgeRender with supported render mode");

            yield return new TestCaseData(
                    new GridEdgeRenderer {GridEdgeRenderMode = GridEdgeRenderMode.EdgesWithBlockedFlowLinks}, false)
                .SetName("GridEdgeRender with unsupported render mode");
        }
    }
}