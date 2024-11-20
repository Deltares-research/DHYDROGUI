using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using GeoAPI.Extensions.Networks;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests
{
    [TestFixture]
    public class InitialConditionsBuilderTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var network = new HydroNetwork();

            // Call
            var builder = new InitialConditionsBuilder(Enumerable.Empty<FlowInitialCondition>(), network);

            // Assert
            Assert.That(builder.ChannelInitialConditionDefinitionsDict, Is.Not.Null);
            Assert.That(builder.HasSetGlobals, Is.False);
            Assert.That(builder.GlobalValue, Is.EqualTo(default(double)));
            Assert.That(builder.GlobalQuantity, Is.EqualTo(default(InitialConditionQuantity)));
        }

        [Test]
        [TestCase(FlowInitialCondition.FlowConditionType.WaterDepth, InitialConditionQuantity.WaterDepth, 10)]
        [TestCase(FlowInitialCondition.FlowConditionType.WaterLevel, InitialConditionQuantity.WaterLevel, 8.5)]
        public void GivenACollectionOfFlowInitialConditionsWithGlobalDefinition_WhenCallingBuild_ThenCorrectlySetsGlobalProperties(
            FlowInitialCondition.FlowConditionType flowConditionType,
            InitialConditionQuantity expectedQuantity,
            double expectedValue)
        {
            // Given
            var definition = new FlowInitialCondition
            {
                WaterLevelType = flowConditionType,
                IsGlobalDefinition = true,
                Level =
                {
                    Constant = expectedValue
                }
            };

            // When
            var builder = new InitialConditionsBuilder(new[] { definition }, new HydroNetwork());
            builder.Build();

            // Then
            Assert.That(builder.GlobalQuantity, Is.EqualTo(expectedQuantity));
            Assert.That(builder.GlobalValue, Is.EqualTo(expectedValue));
            Assert.That(builder.HasSetGlobals, Is.EqualTo(true));
        }

        [Test]
        public void GivenACollectionOfFlowInitialConditionsWithoutGlobalDefinition_WhenCallingBuild_ThenProvidesWarning()
        {
            // Given
            var network = new HydroNetwork();
            var collectionWithoutGlobal = new []
            {
                new FlowInitialCondition
                {
                    WaterLevelType = FlowInitialCondition.FlowConditionType.WaterDepth,
                    IsGlobalDefinition = false
                }
            };

            // When
            var builder = new InitialConditionsBuilder(collectionWithoutGlobal, network);
            Action action = () => builder.Build();

            // Then
            TestHelper.AssertLogMessageIsGenerated(action, "Globally defined flow conditions are not imported yet.");
            Assert.That(builder.GlobalValue, Is.EqualTo(default(double)));
            Assert.That(builder.GlobalQuantity, Is.EqualTo(default(InitialConditionQuantity)));
            Assert.That(builder.HasSetGlobals, Is.False);
        }

        [Test]
        public void GivenANetworkWithoutBranches_WhenCallingBuild_ThenNoInitialConditionsDefinitionsAreGenerated()
        {
            // Given
            var initialConditions = GenerateFlowInitialConditions(CreateDataTable);
            var network = new HydroNetwork();

            // Precondition
            Assert.That(network.Branches, Has.Count.EqualTo(0));

            // When
            var builder = new InitialConditionsBuilder(initialConditions, network);
            builder.Build();

            // Then
            Assert.That(builder.ChannelInitialConditionDefinitionsDict, Is.Not.Null);
            Assert.That(builder.ChannelInitialConditionDefinitionsDict, Is.Empty);
        }

        [Test]
        public void GivenFlowInitialConditionForDischarge_WhenCallingBuild_ThenNoDefinitionsAreCreatedButWarningIsGiven()
        {
            // Given
            var initialConditionId = "InitialCondition1";
            var listOfInitialConditions = new List<FlowInitialCondition>
            {
                new FlowInitialCondition
                {
                    WaterLevelType = FlowInitialCondition.FlowConditionType.WaterLevel,
                    IsGlobalDefinition = false,
                    IsLevelBoundary = false,
                    ID = initialConditionId
                }
            };
            var network = new HydroNetwork();
            network.Branches.Add(new Channel());

            // When
            var builder = new InitialConditionsBuilder(listOfInitialConditions, network);
            Action action = () => builder.Build();

            // Then
            TestHelper.AssertLogMessageIsGenerated(action, $"While importing initial conditions we encountered the following 1 warnings: \r\nCannot import {initialConditionId}. Only WaterDepth and WaterLevel initial conditions are currently supported.");
            Assert.That(builder.ChannelInitialConditionDefinitionsDict, Is.Not.Null);
            Assert.That(builder.ChannelInitialConditionDefinitionsDict.Count, Is.EqualTo(0));
        }

        [Test]
        public void GivenAFlowInitialConditionThatTargetsANonExistingBranch_WhenCallingBuild_ThenNoInitialConditionDefinitionIsGenerated()
        {
            // Given
            var listOfInitialConditions = new List<FlowInitialCondition>();

            var nonExistingName = "NonExistingName";
            var initialConditionId = "InitialCondition1";
            listOfInitialConditions.Add(new FlowInitialCondition
            {
                WaterLevelType = FlowInitialCondition.FlowConditionType.WaterLevel,
                IsGlobalDefinition = false,
                IsLevelBoundary = true,
                Level =
                {
                    Constant = 10
                },
                Discharge =
                {
                    Constant = 4
                },
                BranchID = nonExistingName,
                ID = initialConditionId
            });

            var network = new HydroNetwork();
            network.Branches.Add(new Channel(){Name = "Channel1"});

            // When
            var builder = new InitialConditionsBuilder(listOfInitialConditions, network);
            Action action = () => builder.Build();

            // Then
            TestHelper.AssertLogMessageIsGenerated(action, $"For the following initial conditions the channels where not found; skipped \r\n(branch \"{nonExistingName}\", cond. id \"{initialConditionId}\")");
            Assert.That(builder.ChannelInitialConditionDefinitionsDict, Is.Not.Null);
            Assert.That(builder.ChannelInitialConditionDefinitionsDict.Count, Is.EqualTo(0));
        }

        [Test]
        public void GivenACollectionOfFlowInitialConditions_WhenCallingBuild_ThenCorrectlyCreatesChannelInitialConditionDefinitions()
        {
            // Given
            var listOfInitialConditions = GenerateFlowInitialConditions(CreateDataTable);

            // set all initial condition to be WaterLevel, otherwise they will be filtered out due to being different from global quantity
            foreach (var inititialCondition in listOfInitialConditions) 
            {
                inititialCondition.WaterLevelType = FlowInitialCondition.FlowConditionType.WaterLevel;
            }

            var listOfBranches = new List<IBranch>
            {
                new Channel() {Name = "Branch1"},
                new Channel() {Name = "Branch2"},
                new Channel() {Name = "Branch3"},
                new Channel() {Name = "Branch4"}
            };

            var network = new HydroNetwork();
            network.Branches.AddRange(listOfBranches);
            Assert.That(network.Branches.Count, Is.EqualTo(4));

            // When
            var builder = new InitialConditionsBuilder(listOfInitialConditions, network);
            builder.Build();

            // Then
            Assert.That(builder.ChannelInitialConditionDefinitionsDict.Count, Is.EqualTo(4));
            AssertThatDefinitionsAreCorrect(builder, listOfBranches);
        }

        [Test]
        public void GivenACollectionOfFlowInitialConditionsWithDuplicateOffset_WhenCallingBuild_ThenCorrectlyCreatesChannelInitialConditionDefinitions()
        {
            // Given
            var listOfInitialConditions = GenerateFlowInitialConditions(CreateDataTableWithDuplicateOffset);

            // set all initial condition to be WaterLevel, otherwise they will be filtered out due to being different from global quantity
            foreach (var initialCondition in listOfInitialConditions)
            {
                initialCondition.WaterLevelType = FlowInitialCondition.FlowConditionType.WaterLevel;
            }

            var listOfBranches = new List<IBranch>
            {
                new Channel() {Name = "Branch1"},
                new Channel() {Name = "Branch2"},
                new Channel() {Name = "Branch3"},
                new Channel() {Name = "Branch4"}
            };

            var network = new HydroNetwork();
            network.Branches.AddRange(listOfBranches);
            Assert.That(network.Branches.Count, Is.EqualTo(4));

            // When
            var builder = new InitialConditionsBuilder(listOfInitialConditions, network);
            builder.Build();

            // Then
            Assert.That(builder.ChannelInitialConditionDefinitionsDict.Count, Is.EqualTo(4));
            
            var branch3 = builder.ChannelInitialConditionDefinitionsDict["Branch3"];
            Assert.That(branch3.SpatialChannelInitialConditionDefinition.ConstantSpatialChannelInitialConditionDefinitions[1].Chainage, Is.EqualTo(2500));
            Assert.That(branch3.SpatialChannelInitialConditionDefinition.ConstantSpatialChannelInitialConditionDefinitions[2].Chainage, Is.EqualTo(2500.01)); // 2500 + 0.01 offset
        }


        [Test]
        [TestCase(FlowInitialCondition.FlowConditionType.WaterLevel, InitialConditionQuantity.WaterLevel, new [] {"Branch2", "Branch4"})]
        [TestCase(FlowInitialCondition.FlowConditionType.WaterDepth, InitialConditionQuantity.WaterDepth, new[] { "Branch1", "Branch3" })]
        public void GivenACollectionOfFlowInitialConditions_WhenCallingBuild_ThenCorrectlyFiltersChannelInitialConditionDefinitionsBasedOnGlobalQuantity(
            FlowInitialCondition.FlowConditionType globalQuantity,
            InitialConditionQuantity expectedQuantity,
            string[] invalidBranches)
        {
            // Given
            var listOfInitialConditions = GenerateFlowInitialConditions(CreateDataTable);
            var globalInitialCondition = listOfInitialConditions.First();
            globalInitialCondition.WaterLevelType = globalQuantity;
            
            var network = new HydroNetwork();
            network.Branches.AddRange(new[]
            {
                new Channel{Name="Branch1"},
                new Channel{Name="Branch2"},
                new Channel{Name="Branch3"},
                new Channel{Name="Branch4"},
            });
            Assert.That(network.Branches.Count, Is.EqualTo(4));

            // When
            var builder = new InitialConditionsBuilder(listOfInitialConditions, network);
            Action action = () => builder.Build();

            // Then
            var expectedLogMessages = new[]
            {
                $"Initial condition definition does match the global quantity. Skipping import.\r\ndefinition \"{invalidBranches[0]}\" - global \"{globalQuantity}\"\r\ndefinition \"{invalidBranches[1]}\" - global \"{globalQuantity}\""
            };
            TestHelper.AssertLogMessagesAreGenerated(action, expectedLogMessages);
            
            Assert.That(builder.ChannelInitialConditionDefinitionsDict.Count, Is.EqualTo(2));
            foreach (var channelInitialConditionDefinition in builder.ChannelInitialConditionDefinitionsDict.Values)
            {
                InitialConditionQuantity quantity;
                if (channelInitialConditionDefinition.ConstantChannelInitialConditionDefinition != null)
                {
                    quantity = channelInitialConditionDefinition.ConstantChannelInitialConditionDefinition.Quantity;
                }
                else
                {
                    quantity = channelInitialConditionDefinition.SpatialChannelInitialConditionDefinition.Quantity;
                }
                Assert.That(quantity, Is.EqualTo(expectedQuantity));
            }

        }



        /// <summary>
        /// Creates a collection of FlowInitialCondition, containing:
        /// A global initial condition.
        /// A constant WaterLevel initial condition.
        /// A constant WaterDepth initial condition.
        /// A spatial WaterLevel initial condition.
        /// A spatial WaterDepth initial condition.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<FlowInitialCondition> GenerateFlowInitialConditions(Func<DataTable> createDataTable)
        {
            var listOfInitialConditions = new List<FlowInitialCondition>();

            var globalDefinition = new FlowInitialCondition
            {
                WaterLevelType = FlowInitialCondition.FlowConditionType.WaterDepth,
                IsGlobalDefinition = true,
                Level =
                {
                    Constant = 10
                }
            };
            listOfInitialConditions.Add(globalDefinition);

            var constantWaterLevelDefinition = new FlowInitialCondition
            {
                WaterLevelType = FlowInitialCondition.FlowConditionType.WaterLevel,
                IsGlobalDefinition = false,
                IsLevelBoundary = true,
                Level =
                {
                    IsConstant = true,
                    Constant = 88
                },
                BranchID = "Branch1",
                ID = "InitialCondition1"
            };
            listOfInitialConditions.Add(constantWaterLevelDefinition);

            var constantWaterDepthDefinition = new FlowInitialCondition
            {
                WaterLevelType = FlowInitialCondition.FlowConditionType.WaterDepth,
                IsGlobalDefinition = false,
                IsLevelBoundary = true,
                Level =
                {
                    IsConstant = true,
                    Constant = 77
                },
                BranchID = "Branch2",
                ID = "InitialCondition2"
            };
            listOfInitialConditions.Add(constantWaterDepthDefinition);

            var spatialWaterLevelDefinition = new FlowInitialCondition
            {
                WaterLevelType = FlowInitialCondition.FlowConditionType.WaterLevel,
                IsLevelBoundary = true,
                IsGlobalDefinition = false,
                Level =
                {
                    IsConstant = false,
                    Data = createDataTable()
                },
                BranchID = "Branch3",
                ID = "InitialCondition3"
            };
            listOfInitialConditions.Add(spatialWaterLevelDefinition);

            var spatialWaterDepthDefinition = new FlowInitialCondition
            {
                WaterLevelType = FlowInitialCondition.FlowConditionType.WaterDepth,
                IsGlobalDefinition = false,
                IsLevelBoundary = true,
                Level =
                {
                    IsConstant = false,
                    Data = createDataTable()
                },
                BranchID = "Branch4",
                ID = "InitialCondition4"
            };
            listOfInitialConditions.Add(spatialWaterDepthDefinition);
            
            Assert.That(listOfInitialConditions.Count(), Is.EqualTo(5));
            return listOfInitialConditions;
        }

        /// <summary>
        /// Creates spatial data with a duplicate offset value.
        /// </summary>
        /// <returns></returns>
        private DataTable CreateDataTableWithDuplicateOffset()
         {
             var dataTable = new DataTable();
             dataTable.Columns.Add(new DataColumn("offset", typeof(double)));
             dataTable.Columns.Add(new DataColumn("value", typeof(double)));

             var row = dataTable.NewRow();
             row[0] = 0.0;
             row[1] = 5.0;
             dataTable.Rows.Add(row);

             row = dataTable.NewRow();
             row[0] = 2500.0;
             row[1] = 5.0;
             dataTable.Rows.Add(row);

             row = dataTable.NewRow();
             row[0] = 2500.0;
             row[1] = 3.0;
             dataTable.Rows.Add(row);

             row = dataTable.NewRow();
             row[0] = 5000.0;
             row[1] = 3.0;
             dataTable.Rows.Add(row);

             return dataTable;
         }
        
        /// <summary>
        /// Creates spatial data.
        /// </summary>
        /// <returns></returns>
        private DataTable CreateDataTable()
         {
             var dataTable = new DataTable();
             dataTable.Columns.Add(new DataColumn("offset", typeof(double)));
             dataTable.Columns.Add(new DataColumn("value", typeof(double)));

             var row = dataTable.NewRow();
             row[0] = 0.0;
             row[1] = 5.0;
             dataTable.Rows.Add(row);

             row = dataTable.NewRow();
             row[0] = 2500.0;
             row[1] = 5.0;
             dataTable.Rows.Add(row);

             row = dataTable.NewRow();
             row[0] = 3500.0;
             row[1] = 3.0;
             dataTable.Rows.Add(row);

             row = dataTable.NewRow();
             row[0] = 5000.0;
             row[1] = 3.0;
             dataTable.Rows.Add(row);

             return dataTable;
         }

        private void AssertThatDefinitionsAreCorrect(InitialConditionsBuilder builder, ICollection<IBranch> branches)
        {
            var branch = branches.FirstOrDefault(b => string.Equals(b.Name, "Branch1"));
            Assert.That(branch, Is.Not.Null);
            var branchName = branch.Name;
            var buildDefinition = builder.ChannelInitialConditionDefinitionsDict[branchName];
            Assert.That(buildDefinition, Is.Not.Null);
            Assert.That(buildDefinition.Channel, Is.EqualTo(branch));
            Assert.That(buildDefinition.SpecificationType == ChannelInitialConditionSpecificationType.ConstantChannelInitialConditionDefinition);
            Assert.That(buildDefinition.SpatialChannelInitialConditionDefinition, Is.Null);
            Assert.That(buildDefinition.ConstantChannelInitialConditionDefinition.Value, Is.EqualTo(88));
            Assert.That(buildDefinition.ConstantChannelInitialConditionDefinition.Quantity, Is.EqualTo(InitialConditionQuantity.WaterLevel));

            branch = branches.FirstOrDefault(b => string.Equals(b.Name, "Branch2"));
            branchName = branch.Name;
            buildDefinition = builder.ChannelInitialConditionDefinitionsDict[branchName];
            Assert.That(buildDefinition, Is.Not.Null);
            Assert.That(buildDefinition.Channel, Is.EqualTo(branch));
            Assert.That(buildDefinition.SpecificationType == ChannelInitialConditionSpecificationType.ConstantChannelInitialConditionDefinition);
            Assert.That(buildDefinition.SpatialChannelInitialConditionDefinition, Is.Null);
            Assert.That(buildDefinition.ConstantChannelInitialConditionDefinition.Value, Is.EqualTo(77));
            Assert.That(buildDefinition.ConstantChannelInitialConditionDefinition.Quantity, Is.EqualTo(InitialConditionQuantity.WaterLevel));

            branch = branches.FirstOrDefault(b => string.Equals(b.Name, "Branch3"));
            branchName = branch.Name;
            buildDefinition = builder.ChannelInitialConditionDefinitionsDict[branchName];
            Assert.That(buildDefinition, Is.Not.Null);
            Assert.That(buildDefinition.Channel, Is.EqualTo(branch));
            Assert.That(buildDefinition.SpecificationType == ChannelInitialConditionSpecificationType.SpatialChannelInitialConditionDefinition);
            Assert.That(buildDefinition.ConstantChannelInitialConditionDefinition, Is.Null);
            var spatialDefinition = buildDefinition.SpatialChannelInitialConditionDefinition;
            Assert.That(spatialDefinition.ConstantSpatialChannelInitialConditionDefinitions.Count, Is.EqualTo(4));
            Assert.That(spatialDefinition.ConstantSpatialChannelInitialConditionDefinitions[0].Chainage, Is.EqualTo(0.0));
            Assert.That(spatialDefinition.ConstantSpatialChannelInitialConditionDefinitions[1].Chainage, Is.EqualTo(2500.0));
            Assert.That(spatialDefinition.ConstantSpatialChannelInitialConditionDefinitions[2].Chainage, Is.EqualTo(3500.0));
            Assert.That(spatialDefinition.ConstantSpatialChannelInitialConditionDefinitions[3].Chainage, Is.EqualTo(5000.0));
            Assert.That(spatialDefinition.ConstantSpatialChannelInitialConditionDefinitions[0].Value, Is.EqualTo(5.0));
            Assert.That(spatialDefinition.ConstantSpatialChannelInitialConditionDefinitions[1].Value, Is.EqualTo(5.0));
            Assert.That(spatialDefinition.ConstantSpatialChannelInitialConditionDefinitions[2].Value, Is.EqualTo(3.0));
            Assert.That(spatialDefinition.ConstantSpatialChannelInitialConditionDefinitions[3].Value, Is.EqualTo(3.0));

            branch = branches.FirstOrDefault(b => string.Equals(b.Name, "Branch4"));
            branchName = branch.Name;
            buildDefinition = builder.ChannelInitialConditionDefinitionsDict[branchName];
            Assert.That(buildDefinition, Is.Not.Null);
            Assert.That(buildDefinition.Channel, Is.EqualTo(branch));
            Assert.That(buildDefinition.SpecificationType == ChannelInitialConditionSpecificationType.SpatialChannelInitialConditionDefinition);
            Assert.That(buildDefinition.ConstantChannelInitialConditionDefinition, Is.Null);
            spatialDefinition = buildDefinition.SpatialChannelInitialConditionDefinition;
            Assert.That(spatialDefinition.ConstantSpatialChannelInitialConditionDefinitions.Count, Is.EqualTo(4));
            Assert.That(spatialDefinition.ConstantSpatialChannelInitialConditionDefinitions[0].Chainage, Is.EqualTo(0.0));
            Assert.That(spatialDefinition.ConstantSpatialChannelInitialConditionDefinitions[1].Chainage, Is.EqualTo(2500.0));
            Assert.That(spatialDefinition.ConstantSpatialChannelInitialConditionDefinitions[2].Chainage, Is.EqualTo(3500.0));
            Assert.That(spatialDefinition.ConstantSpatialChannelInitialConditionDefinitions[3].Chainage, Is.EqualTo(5000.0));
            Assert.That(spatialDefinition.ConstantSpatialChannelInitialConditionDefinitions[0].Value, Is.EqualTo(5.0));
            Assert.That(spatialDefinition.ConstantSpatialChannelInitialConditionDefinitions[1].Value, Is.EqualTo(5.0));
            Assert.That(spatialDefinition.ConstantSpatialChannelInitialConditionDefinitions[2].Value, Is.EqualTo(3.0));
            Assert.That(spatialDefinition.ConstantSpatialChannelInitialConditionDefinitions[3].Value, Is.EqualTo(3.0));
        }
    }
}