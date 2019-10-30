using DelftTools.Functions.Generic;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.PropertyGrid;
using DeltaShell.Plugins.FMSuite.Common.Gui.Forms;
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

            var properties = new Model1DBoundaryNodeDataProperties
                {
                    Data = new Model1DBoundaryNodeData
                        {
                            Feature = new Node(),
                            DataType = Model1DBoundaryNodeDataType.WaterLevelTimeSeries
                        }
                };

            //change the condition type a few times..
            properties.Type = Model1DBoundaryNodeDataType.WaterLevelConstant;

            //setting the type to none will reset the data function
            properties.Type = Model1DBoundaryNodeDataType.None;
            
            properties.Type = Model1DBoundaryNodeDataType.WaterLevelConstant;

            //get extrapolation type caused an exception
            Assert.AreEqual(ExtrapolationType.None, properties.ExtrapolationTypeT);
        }
    }
}