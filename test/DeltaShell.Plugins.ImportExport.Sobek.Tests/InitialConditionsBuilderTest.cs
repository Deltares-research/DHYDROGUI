using System;
using System.Data;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.TestUtils;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NetTopologySuite.IO;
using log4net.Core;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests
{
    [TestFixture]
    public class InitialConditionsBuilderTest
    {
        
        [Test]
        public void GlobalDefinitions()
        {
            var definition = new FlowInitialCondition
                                 {
                                     WaterLevelType = FlowInitialCondition.FlowConditionType.WaterDepth,
                                     IsGlobalDefinition = true,
                                     Level =
                                         {
                                             Constant = 10
                                         }
                                     ,
                                     Discharge =
                                         {
                                             Constant = 4
                                         }
                                 };

            var builder = new InitialConditionsBuilder(new[] {definition}, new HydroNetwork());
            builder.Build();
            
            Assert.AreEqual(10, builder.InitialDepth.DefaultValue);
            Assert.AreEqual(4, builder.InitialFlow.DefaultValue);
        }

        [Test]
        public void ConstantInitialConditionsAreRead()
        {
            //a level of 4 is defined on branch 0
            var waterLevelDefinition = new FlowInitialCondition
                                           {
                                               WaterLevelType = FlowInitialCondition.FlowConditionType.WaterLevel,
                                               IsLevelBoundary = true,

                                               Level =
                                                   {
                                                       IsConstant = true,
                                                       Constant = 4
                                                   }
                                               ,
                                               BranchID = "1"
                                           };

            var depthDefinition = new FlowInitialCondition
                                      {
                                          WaterLevelType = FlowInitialCondition.FlowConditionType.WaterDepth,
                                          IsLevelBoundary = true,
                                          Level =
                                              {
                                                  IsConstant = true,
                                                  Constant = 21
                                              }
                                          ,
                                          BranchID = "2"
                                      };

            //create network of 2 branches 1,2
            var hydroNetwork = HydroNetworkHelper.GetSnakeHydroNetwork(2, true);
            //fix the names so the builder can match the channels and the initial conditions
            hydroNetwork.Branches[0].Name = "1";
            hydroNetwork.Branches[1].Name = "2";

            //add a crossection to make conversion to deptp possible.
            CrossSectionHelper.AddCrossSection((IChannel) hydroNetwork.Branches[0], 10, -20);
            CrossSectionHelper.AddCrossSection((IChannel) hydroNetwork.Branches[1], 10, -20);


            var builder = new InitialConditionsBuilder(new[] {waterLevelDefinition, depthDefinition}, hydroNetwork);

            builder.Build();

            //should result in a depth coverage
            var initialDepth = builder.InitialDepth;
            //should have a single location halfway the branch
            //crossection has -20 and a level of 4 is defined ..hence the depth should be 24
            Assert.AreEqual(3, initialDepth.Components[0].Values.Count);
            
            //use evaluate cause we might be off by a bit..
            Assert.AreEqual(initialDepth.Evaluate(new NetworkLocation(hydroNetwork.Branches[0], 50)), 24);
            Assert.AreEqual(initialDepth.Evaluate(new NetworkLocation(hydroNetwork.Branches[1], 50)), 21);
        }

        [Test]
        public void InterpolationConditionsAreRead()
        {
            //a level of 4 is defined on branch 0
            var waterLevelDefinition = new FlowInitialCondition
                                           {
                                               WaterLevelType = FlowInitialCondition.FlowConditionType.WaterLevel,
                                               Level =
                                                   {
                                                       IsConstant = true,
                                                       Constant = 45.12
                                                    },

                                               IsQBoundary = true,
                                               Discharge = 
                                               {
                                                   IsConstant = false,
                                                   Data = new DataTable(),
                                                   Interpolation = InterpolationType.Constant
                                               },
                BranchID = "1"
            };

            var depthDefinition = new FlowInitialCondition
            {
                WaterLevelType = FlowInitialCondition.FlowConditionType.WaterDepth,
                IsLevelBoundary = true,
                Level =
                {
                    IsConstant = false,
                    Data = new DataTable(),
                    Interpolation = InterpolationType.Linear
                }
                ,
                BranchID = "2"
            };

            //create network of 2 branches 1,2
            var hydroNetwork = HydroNetworkHelper.GetSnakeHydroNetwork(2, true);
            //fix the names so the builder can match the channels and the initial conditions
            hydroNetwork.Branches[0].Name = "1";
            hydroNetwork.Branches[1].Name = "2";

            //add a crossection to make conversion to deptp possible.
            CrossSectionHelper.AddCrossSection((IChannel)hydroNetwork.Branches[0], 10, -20);
            CrossSectionHelper.AddCrossSection((IChannel)hydroNetwork.Branches[1], 10, -20);

            var builder = new InitialConditionsBuilder(new[] { waterLevelDefinition, depthDefinition }, hydroNetwork);

            builder.Build();

            var initialDepth = builder.InitialDepth;
            var initialFlow = builder.InitialFlow;

            Assert.IsNotNull(initialDepth);
            Assert.AreEqual(InterpolationType.Linear, initialDepth.Arguments[0].InterpolationType);

            Assert.IsNotNull(initialFlow);
            Assert.AreEqual(InterpolationType.Constant, initialFlow.Arguments[0].InterpolationType);

        }

        [Test]
        public void WarnForUnusedConditions()
        {
            //create a level condition for a branch that is not in the network
            var builder = new InitialConditionsBuilder(new[]
                                                           {
                                                               new FlowInitialCondition
                                                                   {
                                                                       IsLevelBoundary = true,
                                                                       ID = "kees",
                                                                       BranchID = "truusje"
                                                                   },
                                                           }, new HydroNetwork());
            const string message = "Channel truusje for initial condition kees not found; skipped";

            LogHelper.SetLoggingLevel(Level.Debug);
            TestHelper.AssertLogMessageIsGenerated(builder.Build,message);
            LogHelper.SetLoggingLevel(Level.Error);
        }

        [Test]
        public void WarnIfConditionIsSkippedDueToMissingCrossSection()
        {
            var hydroNetwork = new HydroNetwork();
            string branchId = "BranchID";
            hydroNetwork.Branches.Add(new Channel { Name = branchId, Geometry = new WKTReader().Read("LINESTRING (0 0, 100 0)") });
            //create a builder with a waterlevel constant condition on the branch..
            var flowInitialCondition = new FlowInitialCondition
                                           {
                                               IsLevelBoundary = true,
                                               WaterLevelType = FlowInitialCondition.FlowConditionType.WaterLevel,
                                               ID = "kees",
                                               BranchID = branchId,
                                               Level =
                                                   {
                                                       IsConstant = true
                                                   }
                                           };
            
            
            var builder = new InitialConditionsBuilder(new[]
                                                           {
                                                               flowInitialCondition,
                                                           }, hydroNetwork);


            string message = String.Format("Initial condition with ID {0} skipped because no crossections are defined on branch {1} and no conversion to depth could be made",
                "kees",branchId);


            LogHelper.ConfigureLogging();
            LogHelper.SetLoggingLevel(Level.Debug);
            TestHelper.AssertLogMessageIsGenerated(builder.Build, message);
            LogHelper.SetLoggingLevel(Level.Error);
        }

        [Test]
        public void InitialConditionsDefinedAtOnePoint()
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

            string branchId = "BranchID";
            var hydroNetwork = new HydroNetwork();
            hydroNetwork.Branches.Add(new Channel { Name = branchId, Geometry = new WKTReader().Read("LINESTRING (0 0, 5000 0)") });
            

            //create a level condition for a branch that is not in the network
            var builder = new InitialConditionsBuilder(new[]
                                                           {
                                                               new FlowInitialCondition
                                                                   {
                                                                       IsLevelBoundary = true,
                                                                       ID = "1",
                                                                       BranchID = branchId, 
                                                                       Level = 
                                                                           {
                                                                               IsConstant = false,
                                                                               Data = dataTable,
                                                                               Interpolation = InterpolationType.Linear
                                                                           }
                                                                   },
                                                           }, hydroNetwork);
            builder.Build();
            Assert.AreEqual(dataTable.Rows.Count, builder.InitialDepth.Locations.Values.Count);

            Assert.AreEqual(0.0, builder.InitialDepth.Locations.Values[0].Chainage);
            Assert.AreEqual(5.0, (double)builder.InitialDepth[builder.InitialDepth.Locations.Values[0]]);

            Assert.AreEqual(2500.0, builder.InitialDepth.Locations.Values[1].Chainage);
            Assert.AreEqual(5.0, (double)builder.InitialDepth[builder.InitialDepth.Locations.Values[1]]);

            Assert.AreEqual(2500.01, builder.InitialDepth.Locations.Values[2].Chainage);
            Assert.AreEqual(3.0, (double)builder.InitialDepth[builder.InitialDepth.Locations.Values[2]]);

            Assert.AreEqual(5000.0, builder.InitialDepth.Locations.Values[3].Chainage);
            Assert.AreEqual(3.0, (double)builder.InitialDepth[builder.InitialDepth.Locations.Values[3]]);


        }
    }
}