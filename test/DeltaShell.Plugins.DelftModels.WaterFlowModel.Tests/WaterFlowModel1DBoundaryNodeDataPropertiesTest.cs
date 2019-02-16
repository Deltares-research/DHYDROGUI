using DelftTools.Functions.Generic;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.PropertyGrid;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests
{
    [TestFixture]
    public class WaterFlowModel1DBoundaryNodeDataPropertiesTest
    {
        [Test]
        public void ResettingNodeTypeToWaterLevelNotGiveException()
        {
            //reproduces issue 4160

            var properties = new WaterFlowModel1DBoundaryNodeDataProperties
                {
                    Data = new WaterFlowModel1DBoundaryNodeData
                        {
                            Feature = new Node(),
                            DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries
                        }
                };

            //change the condition type a few times..
            properties.Type = WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant;

            //setting the type to none will reset the data function
            properties.Type = WaterFlowModel1DBoundaryNodeDataType.None;
            
            properties.Type = WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant;

            //get extrapolation type caused an exception
            Assert.AreEqual(Flow1DExtrapolationType.Constant, properties.ExtrapolationType);
        }
    }
}