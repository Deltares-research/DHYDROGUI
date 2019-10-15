using System;
using System.Windows.Forms.VisualStyles;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.DataObjects
{
    [TestFixture]
    public class WaterFlowModel1DBoundaryNodeDataTest
    {
        [Test]
        public void ChangeTypeClearsValuesOfBoundaryCondition()
        {
            var node = new HydroNode();
            var boundaryCondition = new WaterFlowModel1DBoundaryNodeData
                                        {
                                            Feature = node,
                                            DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries
                                        };
            
            boundaryCondition.Data[DateTime.Now] = 0.1;
            
            // Change the type
            boundaryCondition.DataType = WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries;
            
            // Assert the condition is cleared
            Assert.AreEqual(0, boundaryCondition.Data.Arguments[0].Values.Count);
        }

        [Test]
        public void ChangeTypeToConstantAgainClearsValuesOfBoundaryCondition()
        {
            var node = new HydroNode();
            var boundaryCondition = new WaterFlowModel1DBoundaryNodeData
                                        {
                                            Feature = node,
                                            DataType = WaterFlowModel1DBoundaryNodeDataType.FlowConstant,
                                            Flow = 42
                                        };

            boundaryCondition.DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries;
            boundaryCondition.Data[DateTime.Now] = 0.1;

            // Change the type
            boundaryCondition.DataType = WaterFlowModel1DBoundaryNodeDataType.FlowConstant;

            // Assert the condition is set to 0
            Assert.AreEqual(0, boundaryCondition.Flow);
        }

        [Test]
        public void SetFunctionDataInNoneBoundaryNodeChangesType()
        {
            var node = new HydroNode();
            var boundaryCondition = new WaterFlowModel1DBoundaryNodeData
                                        {
                                            Feature = node,
                                            DataType = WaterFlowModel1DBoundaryNodeDataType.None,
                                            Data = HydroTimeSeriesFactory.CreateFlowTimeSeries()
                                        };

            // Assert the condition type changed
            Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries, boundaryCondition.DataType);
        }

        [Test]
        public void WaterLevelOnlyIsUpdatedAfterSettingFeature()
        {
            var nodeConnectedToSingleBranch = new HydroNode();
            var nodeConnectedToMultipleBranches = new HydroNode();

            nodeConnectedToSingleBranch.OutgoingBranches.Add(new Branch());
            nodeConnectedToMultipleBranches.OutgoingBranches.Add(new Branch());
            nodeConnectedToMultipleBranches.IncomingBranches.Add(new Branch());

            var boundaryCondition1 = new WaterFlowModel1DBoundaryNodeData { Feature = null };
            Assert.IsFalse(boundaryCondition1.WaterLevelOnly);

            var boundaryCondition2 = new WaterFlowModel1DBoundaryNodeData { Feature = nodeConnectedToSingleBranch };
            Assert.IsFalse(boundaryCondition2.WaterLevelOnly);

            var boundaryCondition3 = new WaterFlowModel1DBoundaryNodeData { Feature = nodeConnectedToMultipleBranches };
            Assert.IsTrue(boundaryCondition3.WaterLevelOnly);
        }

        [Test]
        public void WaterLevelOnlyIsUpdatedAfterChangingBranches()
        {
            var node = new HydroNode();
            var boundaryCondition = new WaterFlowModel1DBoundaryNodeData { Feature = node };

            Assert.IsFalse(boundaryCondition.WaterLevelOnly);

            node.OutgoingBranches.Add(new Branch());
            Assert.IsFalse(boundaryCondition.WaterLevelOnly);

            node.IncomingBranches.Add(new Branch());
            Assert.IsTrue(boundaryCondition.WaterLevelOnly);

            node.IncomingBranches.RemoveAt(0);
            Assert.IsFalse(boundaryCondition.WaterLevelOnly);
        }

        [Test]
        public void SettingWaterLevelOnlyResultsInChangesForFlowDataTypes()
        {
            var node = new HydroNode();
            var boundaryCondition = new WaterFlowModel1DBoundaryNodeData
                                        {
                                            Feature = node,
                                            DataType = WaterFlowModel1DBoundaryNodeDataType.FlowConstant
                                        };

            Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.FlowConstant, boundaryCondition.DataType);

            node.OutgoingBranches.Add(new Branch());
            Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.FlowConstant, boundaryCondition.DataType);

            node.OutgoingBranches.Add(new Branch()); // Water level only
            Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant, boundaryCondition.DataType);

            node.OutgoingBranches.RemoveAt(0);
            Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant, boundaryCondition.DataType);

            boundaryCondition.DataType = WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries;
            Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries, boundaryCondition.DataType);

            node.OutgoingBranches.Add(new Branch()); // Water level only
            Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant, boundaryCondition.DataType);

            node.OutgoingBranches.RemoveAt(0);
            Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant, boundaryCondition.DataType);

            boundaryCondition.DataType = WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable;
            Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable, boundaryCondition.DataType);

            node.OutgoingBranches.Add(new Branch()); // Water level only
            Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant, boundaryCondition.DataType);

            node.OutgoingBranches.RemoveAt(0);
            Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant, boundaryCondition.DataType);
        }

        [Test]
        public void SettingWaterLevelOnlyDoesNotResultInChangesForNonFlowDataTypes()
        {
            var node = new HydroNode();
            var boundaryCondition = new WaterFlowModel1DBoundaryNodeData { Feature = node };

            Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.None, boundaryCondition.DataType);

            node.OutgoingBranches.Add(new Branch());
            Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.None, boundaryCondition.DataType);

            node.OutgoingBranches.Add(new Branch()); // Water level only
            Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.None, boundaryCondition.DataType);

            node.OutgoingBranches.RemoveAt(0);
            Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.None, boundaryCondition.DataType);

            boundaryCondition.DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries;
            Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries, boundaryCondition.DataType);

            node.OutgoingBranches.Add(new Branch()); // Water level only
            Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries, boundaryCondition.DataType);

            node.OutgoingBranches.RemoveAt(0);
            Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries, boundaryCondition.DataType);

            boundaryCondition.DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant;
            Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant, boundaryCondition.DataType);

            node.OutgoingBranches.Add(new Branch()); // Water level only
            Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant, boundaryCondition.DataType);

            node.OutgoingBranches.RemoveAt(0);
            Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant, boundaryCondition.DataType);
        }

        [Test]
        public void LinkBoundaryConditionToFlowWaterLevelSeriesDataItem()
        {
            var node = new HydroNode { Name = "Node1" };
            var boundaryCondition = new WaterFlowModel1DBoundaryNodeData
                                        {
                                            Feature = node,
                                            DataType = WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable
                                        };

            // Get a flow timeseries 
            var flowWaterLevelSeries = new FlowWaterLevelTable();
            flowWaterLevelSeries[2.0] = 5.0;
            var dataItem = new DataItem(flowWaterLevelSeries) { Name = "Amsterdam" };

            // Link the boundary condition to the data item
            boundaryCondition.SeriesDataItem.LinkTo(dataItem);

            // Assert the condition was linked correctly
            Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable, boundaryCondition.DataType);
            Assert.AreEqual("Node1 - Q(h) (Amsterdam)", boundaryCondition.Name);
            Assert.AreEqual(5.0, boundaryCondition.Data[2.0]);
        }

        [Test]
        public void Clone()
        {
            var node = new HydroNode { Name = "Node1" };
            var boundaryCondition = new WaterFlowModel1DBoundaryNodeData
            {
                Feature = node,
                DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant,
                WaterLevel = 5,
                UseSalt = true,
                ThatcherHarlemannCoefficient = 3600,
                SaltConcentrationConstant = 18.3333,
                SaltConditionType = SaltBoundaryConditionType.TimeDependent,

                UseTemperature = true,
                TemperatureConstant = 6.7778,
                TemperatureConditionType = TemperatureBoundaryConditionType.TimeDependent,
            };

            boundaryCondition.SaltConcentrationTimeSeries[new DateTime(2000, 1, 1)] = 4.0;
            boundaryCondition.TemperatureTimeSeries[new DateTime(2000, 1, 1)] = 5.6667;
            
            var clonedBoundaryCondition = (WaterFlowModel1DBoundaryNodeData) boundaryCondition.Clone();
            
            Assert.AreEqual(boundaryCondition.DataType,clonedBoundaryCondition.DataType);
            Assert.AreEqual(boundaryCondition.WaterLevel, clonedBoundaryCondition.WaterLevel);

            Assert.AreEqual(boundaryCondition.UseSalt, clonedBoundaryCondition.UseSalt);
            Assert.AreEqual(boundaryCondition.ThatcherHarlemannCoefficient, clonedBoundaryCondition.ThatcherHarlemannCoefficient);
            Assert.AreEqual(boundaryCondition.SaltConcentrationConstant, clonedBoundaryCondition.SaltConcentrationConstant);
            Assert.AreEqual(boundaryCondition.SaltConditionType, clonedBoundaryCondition.SaltConditionType);
            Assert.AreEqual((double) boundaryCondition.SaltConcentrationTimeSeries[new DateTime(2000, 1, 1)], (double) clonedBoundaryCondition.SaltConcentrationTimeSeries[new DateTime(2000, 1, 1)]);

            Assert.AreEqual(boundaryCondition.UseTemperature, clonedBoundaryCondition.UseTemperature);
            Assert.AreEqual(boundaryCondition.TemperatureConstant, clonedBoundaryCondition.TemperatureConstant);
            Assert.AreEqual(boundaryCondition.TemperatureConditionType, clonedBoundaryCondition.TemperatureConditionType);
            Assert.AreEqual((double)boundaryCondition.TemperatureTimeSeries[new DateTime(2000, 1, 1)], (double)clonedBoundaryCondition.TemperatureTimeSeries[new DateTime(2000, 1, 1)]);
        }

        [Test]
        public void AddLinkToNoneBoundaryAddsQIs0Boundary()
        {
            // TOOLS-20866
            var boundary = new WaterFlowModel1DBoundaryNodeData();
            boundary.DataType = WaterFlowModel1DBoundaryNodeDataType.None;

            HydroNode node1 = new HydroNode("node1");
            boundary.Feature = node1; 
            node1.Links.Add(new HydroLink());

            // Now the type of the boundary should be set to Q=0
            Assert.That(boundary.DataType == WaterFlowModel1DBoundaryNodeDataType.FlowConstant);
            Assert.That((boundary.FlowConstantDataItem.Value as FlowParameter).Value == 0.0);
        }

        [Test]
        public void EnableSaltAddsProperties()
        {
            var node = new HydroNode();
            var boundaryCondition = new WaterFlowModel1DBoundaryNodeData
                                        {
                                            Feature = node,
                                            DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries
                                        };

            // Defaults to false
            Assert.IsFalse(boundaryCondition.UseSalt);

            // No data available
            Assert.AreEqual(0, boundaryCondition.SaltConcentrationConstant); 
            Assert.IsNull(boundaryCondition.SaltConcentrationTimeSeries);

            // Enum defaults to none
            Assert.AreEqual(SaltBoundaryConditionType.None, boundaryCondition.SaltConditionType);

            // Spice it up with some salt
            boundaryCondition.UseSalt = true;
            
            Assert.IsNotNull(boundaryCondition.SaltConcentrationTimeSeries);
            Assert.AreEqual(SaltBoundaryConditionType.Constant, boundaryCondition.SaltConditionType);

            // Turn if off again
            boundaryCondition.UseSalt = false;

            // Should revert to condition before the salt was added
            Assert.IsNull(boundaryCondition.SaltConcentrationTimeSeries);
            Assert.AreEqual(SaltBoundaryConditionType.None, boundaryCondition.SaltConditionType);
        }

        [Test]
        public void EnableTemperatureAddsProperties()
        {
            var node = new HydroNode();
            var boundaryCondition = new WaterFlowModel1DBoundaryNodeData
            {
                Feature = node,
                DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries
            };

            // Check default values
            Assert.IsFalse(boundaryCondition.UseTemperature);
            Assert.AreEqual(0, boundaryCondition.TemperatureConstant);
            Assert.IsNull(boundaryCondition.TemperatureTimeSeries);
            Assert.AreEqual(TemperatureBoundaryConditionType.None, boundaryCondition.TemperatureConditionType);

            // Enable temperature
            boundaryCondition.UseTemperature = true;

            Assert.IsNotNull(boundaryCondition.TemperatureTimeSeries);
            Assert.AreEqual(TemperatureBoundaryConditionType.Constant, boundaryCondition.TemperatureConditionType);

            // Turn if off again
            boundaryCondition.UseTemperature = false;

            // Should revert to condition before the temperature was enabled
            Assert.IsNull(boundaryCondition.TemperatureTimeSeries);
            Assert.AreEqual(TemperatureBoundaryConditionType.None, boundaryCondition.TemperatureConditionType);
        }


        [Test]
        public void GetSetValuesInSaltyBoundaryCondition()
        {
            var node = new HydroNode();
            var boundaryCondition = new WaterFlowModel1DBoundaryNodeData
                                        {
                                            Feature = node,
                                            DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries,
                                            UseSalt = true // This enables us to set values
                                        };

            var saltConcentrationTimeSeries = new TimeSeries();
            boundaryCondition.SaltConcentrationTimeSeries = saltConcentrationTimeSeries;
            boundaryCondition.SaltConditionType = SaltBoundaryConditionType.TimeDependent;
            boundaryCondition.SaltConcentrationConstant = 0.6;

            // Assert the values were stored OK
            Assert.AreEqual(saltConcentrationTimeSeries,boundaryCondition.SaltConcentrationTimeSeries);
            Assert.AreEqual(SaltBoundaryConditionType.TimeDependent,boundaryCondition.SaltConditionType);
            Assert.AreEqual(0.6,boundaryCondition.SaltConcentrationConstant);
        }
    }
}