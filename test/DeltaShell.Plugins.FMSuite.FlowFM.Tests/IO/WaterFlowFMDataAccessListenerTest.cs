using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.Core;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class WaterFlowFMDataAccessListenerTest
    {
        private WaterFlowFMDataAccessListener dataAccessListener;
        private WaterFlowFMModel waterFlowFmModel;

        [TestFixtureSetUp]
        public void SetUp()
        {
            dataAccessListener = new WaterFlowFMDataAccessListener
            {
                ProjectRepository = new InMemoryProjectRepository()
            };
            waterFlowFmModel = new WaterFlowFMModel();
        }

        [Test]
        public void GivenAWaterFlowFMDataAccessListener_WhenAWaterFlowFMModelIsLoaded_ThenSpatialOperationsOfTheBedLevelDataItemAreRemoved()
        {
            // Given
            IDataItem bedLevelDataItem = GetBedLevelDataItem(waterFlowFmModel.DataItems);
            bedLevelDataItem.ValueConverter = new CoverageSpatialOperationValueConverter();

            // When
            dataAccessListener.OnPostLoad(waterFlowFmModel, null, null);

            // Then
            bedLevelDataItem = GetBedLevelDataItem(waterFlowFmModel.DataItems);
            Assert.IsNull(bedLevelDataItem.ValueConverter,
                          "Spatial operations should have been removed from the bed level data item after loading.");
        }

        private static IDataItem GetBedLevelDataItem(IEnumerable<IDataItem> dataItems)
        {
            return dataItems.FirstOrDefault(di => di.Name == WaterFlowFMModelDefinition.BathymetryDataItemName);
        }
    }
}