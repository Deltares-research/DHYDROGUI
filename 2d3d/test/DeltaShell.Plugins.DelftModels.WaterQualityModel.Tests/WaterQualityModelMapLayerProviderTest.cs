using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.FeatureEditing;
using DeltaShell.Plugins.SharpMapGis.Gui;
using NUnit.Framework;
using SharpMap.Api;
using SharpMap.Api.Layers;
using SharpMap.Layers;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests
{
    [TestFixture]
    public class WaterQualityModelMapLayerProviderTest
    {
        [Test]
        public void CanCreateLayerFor_VariousWaterQualityModelData_ReturnTrue()
        {
            // setup
            var model = new WaterQualityModel();
            var layerProvider = new WaterQualityModelMapLayerProvider();

            var dataArray = new object[]
            {
                model.InitialConditions,
                model.ProcessCoefficients,
                model.Dispersion,
                model.Boundaries,
                model.Loads,
                model.ObservationPoints,
                model.OutputSubstancesDataItemSet,
                model.OutputParametersDataItemSet
                // not for observation areas! It is created by SharpMapGisGuiPlugin.
            };

            // call & assert
            foreach (object data in dataArray)
            {
                Assert.IsTrue(layerProvider.CanCreateLayerFor(data, model),
                              "Should be able to create layer for data: {0}", data);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void CreateMapLayerForWaterQualityModel()
        {
            var model = new WaterQualityModel();
            model.SubstanceProcessLibrary.Substances.AddRange(new[]
            {
                new WaterQualitySubstance
                {
                    Name = "B",
                    Active = false
                },
                new WaterQualitySubstance
                {
                    Name = "A",
                    Active = true
                }
            });
            model.Loads.AddRange(new[]
            {
                new WaterQualityLoad {Name = "Load 1"},
                new WaterQualityLoad {Name = "Load 2"}
            });

            var mapLayerProviders = new IMapLayerProvider[]
            {
                new WaterQualityModelMapLayerProvider(),
                new SharpMapLayerProvider()
            };

            var layerObjectLookup = new Dictionary<ILayer, object>();
            ILayer modelLayer = MapLayerProviderHelper.CreateLayersRecursive(model, null, mapLayerProviders, layerObjectLookup);

            Assert.NotNull(modelLayer);
            Assert.NotNull(layerObjectLookup);

            Assert.AreEqual(model.Name, modelLayer.Name);
            Assert.IsTrue(modelLayer.NameIsReadOnly);

            var groupLayer = modelLayer as IGroupLayer;
            Assert.NotNull(groupLayer);
            Assert.IsTrue(groupLayer.LayersReadOnly);

            ILayer[] allLayers = groupLayer.Layers.GetLayersRecursive(true, true).ToArray();

            Assert.AreEqual(17, allLayers.Length);

            #region Loads layer

            ILayer loadsLayer = allLayers.First(l => l.Name == "Loads");
            Assert.IsTrue(loadsLayer.NameIsReadOnly);
            Assert.IsFalse(loadsLayer.ReadOnly);
            Assert.IsInstanceOf<WaterQualityFeatureEditor>(loadsLayer.FeatureEditor);
            Assert.IsInstanceOf<VectorLayer>(loadsLayer);
            var vectorLayer = (VectorLayer) loadsLayer;
            Assert.IsTrue(vectorLayer.Style.HasCustomSymbol);

            IFeatureProvider featureProvider = loadsLayer.DataSource;
            Assert.AreEqual(typeof(WaterQualityLoad), featureProvider.FeatureType);
            Assert.AreEqual(2, featureProvider.GetFeatureCount());
            Assert.AreEqual(model.Grid.CoordinateSystem, featureProvider.CoordinateSystem);

            #endregion

            #region Observation Areas layer

            ILayer observationAreaLayer = allLayers.First(l => l.Name == "Observation Areas");
            Assert.IsTrue(observationAreaLayer.NameIsReadOnly);
            Assert.IsFalse(observationAreaLayer.ReadOnly);
            Assert.IsInstanceOf<UnstructuredGridCellCoverageLayer>(observationAreaLayer);

            #endregion

            #region Substances layer

            ILayer substancesLayer = allLayers.First(l => l.Name == "Substances");
            Assert.IsTrue(substancesLayer.NameIsReadOnly);
            Assert.IsFalse(substancesLayer.ReadOnly);

            #endregion

            #region Output Parameters layer

            ILayer outputParametersLayer = allLayers.First(l => l.Name == "Output Parameters");
            Assert.IsTrue(outputParametersLayer.NameIsReadOnly);
            Assert.IsFalse(outputParametersLayer.ReadOnly);

            #endregion
        }
    }
}