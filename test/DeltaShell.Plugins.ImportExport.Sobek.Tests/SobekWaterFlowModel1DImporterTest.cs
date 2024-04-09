using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests
{
    [TestFixture]
    public class SobekWaterFlowFMModelImporterTest
    {
        /// <summary>
        /// 04/03/2014 (SSD, i7-4900MQ): 
        /// original = 72669ms, optimized = 30083ms / 41% = 2.4x faster ( 36500ms / 50% = 2x faster, when not at Deltares - license :( )
        /// 
        /// == Update ==
        /// 31/05/2017 (i7-3630QM):
        /// Time to run locally: 72801
        /// Build server factor: x3.4 (approx)
        /// 
        /// Note: This test takes over twice as long to run since switching to the DIMR runner
        ///       Given the intention of this test is to test the efficiency of DeltaShell rather than DIMR:
        ///       I've increased the threshold to this new base-line
        /// 
        ///       *run away... run away...*
        /// 
        /// == /Update ==
        /// 
        /// for analysis reports see issue SOBEK3-1020
        /// 
        /// </summary>

        //See issue FM1D2D-533

        // [Test]
        // [Category(TestCategory.Slow)]
        // [Category(TestCategory.Performance)]
        // public void RunImportedModelShouldBeFast_039b()
        // {
        //     Dimr.DimrLogging.LogFileLevel = Level.Fatal;
        //     Dimr.DimrLogging.FeedbackLevel = Level.Fatal;
        //
        //     var path = TestHelper.GetTestDataDirectory() + @"\039b_000.lit\1\network.tp";
        //
        //     var model = GetWaterFlowFMModel(path);
        //
        //     Action initializeAndRunModel = () =>
        //     {
        //         model.Initialize();
        //         while (model.Status != ActivityStatus.Done)
        //         {
        //             model.Execute();
        //
        //             if (model.Status == ActivityStatus.Failed)
        //             {
        //                 Assert.Fail("Error during model run");
        //             }
        //         }
        //     };
        //
        //     TestHelper.AssertIsFasterThan(250000, initializeAndRunModel);
        // }
        //
        // [Test]
        // [Category(TestCategory.DataAccess)]
        // [Category(TestCategory.Slow)]
        // public void RemoveGridPointAtStructures()
        // {
        //     string pathToSobekModel = TestHelper.GetTestDataDirectory() + @"\BYPASS.lit\3\network.tp";
        //
        //     var flowModel1D = GetWaterFlowFMModel(pathToSobekModel);
        //
        //     var network = flowModel1D.Network;
        //     var discretization = flowModel1D.NetworkDiscretization;
        //     foreach (var structure in network.CompositeBranchStructures)
        //     {
        //         Assert.IsFalse(discretization.Locations.Values.Contains(new NetworkLocation(structure.Branch, structure.Chainage)));
        //     }
        // }
        //
        // [Test]
        // [Category(TestCategory.DataAccess)]
        // [Category(TestCategory.Slow)]
        // public void RemoveDoubleGridPointOnBranch()
        // {
        //     string pathToSobekModel = TestHelper.GetTestDataDirectory() + @"\ReModels\20110331_NDB.sbk\6\deftop.1";
        //
        //     var flowModel1D = GetWaterFlowFMModel(pathToSobekModel);
        //
        //     var discretization = flowModel1D.NetworkDiscretization;
        //     for (int i = 0; i < discretization.Locations.Values.LongCount() - 1; i++)
        //     {
        //         var location1 = discretization.Locations.Values[i];
        //         var location2 = discretization.Locations.Values[i + 1];
        //         if (location1.Branch == location2.Branch)
        //         {
        //             Assert.IsFalse(Math.Abs(location1.Chainage - location2.Chainage) < 1.0);
        //         }
        //     }
        // }
        //
        // [Test]
        // [Category(TestCategory.DataAccess)]
        // public void ImportNetworkWithNames()
        // {
        //     // import model and network.
        //     WaterFlowFMModel waterFlowFMModel = GetWaterFlowFMModel(TestHelper.GetTestDataDirectory() + @"\NameCalc.lit\1\network.tp");
        //     Assert.AreEqual("Node1", waterFlowFMModel.NetworkDiscretization.Locations.Values[1].LongName);
        //     Assert.AreEqual("1_1", waterFlowFMModel.NetworkDiscretization.Locations.Values[1].Name);
        //     // NodeZoveel is 20 gridpoint in file but gridpoint at 8339.69653108593 should be ignored because it is a structure
        //     Assert.AreEqual("NodeZoveel", waterFlowFMModel.NetworkDiscretization.Locations.Values[19].LongName);
        //     Assert.AreEqual("1_17", waterFlowFMModel.NetworkDiscretization.Locations.Values[19].Name);
        //
        //     //node
        //     IHydroNetwork hydroNetwork = waterFlowFMModel.Network;
        //     Assert.AreEqual("Node", hydroNetwork.HydroNodes.First().LongName);
        //     Assert.AreEqual("3", hydroNetwork.Nodes.First().Name);
        //
        //     //branch
        //     Assert.AreEqual("1", hydroNetwork.Channels.First().Name);
        //     Assert.AreEqual("Branch", hydroNetwork.Channels.First().LongName);
        //
        //     //crossection
        //     Assert.AreEqual("1", hydroNetwork.CrossSections.First().Name);
        //     Assert.AreEqual("CrossSection", hydroNetwork.CrossSections.First().LongName);
        //     //Assert.AreEqual("3", hydroNetwork.CrossSections.First().Name);
        //     
        //     //pump
        //     Assert.AreEqual("4",hydroNetwork.Pumps.First().Name);
        //     Assert.AreEqual("RiverPump", hydroNetwork.Pumps.First().LongName);
        // }
        //
        // [Test]
        // [Category(TestCategory.DataAccess)]
        // public void ImportInitialConditionsConstantOnBranch()
        // {
        //     //import a model with 2 branches..on each branch a constant is defined
        //     WaterFlowFMModel waterFlowFMModel = GetWaterFlowFMModel(TestHelper.GetTestDataDirectory() + @"\InitCond.lit\1\network.tp");
        //
        //     //check the first branch has a depth of 0.75
        //     var firstBranch = waterFlowFMModel.Network.Branches.First();
        //     var firstBranchLocation =
        //         waterFlowFMModel.InitialConditions.Locations.Values.FirstOrDefault(f => f.Branch == firstBranch);
        //     Assert.AreEqual(0.75, waterFlowFMModel.InitialConditions[firstBranchLocation]);
        //     //use evaluate cause we might be off by a little bit...
        //     Assert.AreEqual(-1,waterFlowFMModel.InitialFlow.Evaluate(firstBranchLocation));
        //
        //     //the second branch should get a 10.88 (0.88 waterlevel + a bedlevel of -10 in the cross-sections)
        //     var secondBranch = waterFlowFMModel.Network.Branches[1];
        //     var secondBranchLocation=
        //         waterFlowFMModel.InitialConditions.Locations.Values.FirstOrDefault(f => f.Branch == secondBranch);
        //     Assert.AreEqual(10.88, waterFlowFMModel.InitialConditions[secondBranchLocation]);
        //
        //     Assert.AreEqual(1, waterFlowFMModel.InitialFlow.Evaluate(secondBranchLocation));
        //
        //     //check the global waterlevel made it to the model
        //     Assert.AreEqual(1.25, waterFlowFMModel.DefaultInitialWaterLevel);
        //     //no global initial depth...so 0
        //     Assert.AreEqual(0,waterFlowFMModel.DefaultInitialDepth);
        //     //default flow is defined on the coverage itself
        //     Assert.AreEqual(15.0, waterFlowFMModel.InitialFlow.DefaultValue);
        // }
        //
        // [Test]
        // [Category(TestCategory.DataAccess)]
        // public void ImportInitialConditionsAlongBranch()
        // {
        //     //import a model with 2 branches..one branch a func for Depth the other Func waterlevel
        //     var waterFlowFMModel = GetWaterFlowFMModel(TestHelper.GetTestDataDirectory() + @"\CondFunc.lit\1\network.tp");
        //
        //     //check the depth on the first branch
        //     var firstBranch = waterFlowFMModel.Network.Branches.First();
        //     IList<INetworkLocation> firstBranchLocations =
        //         waterFlowFMModel.InitialConditions.Locations.Values.Where(l => l.Branch == firstBranch).ToList();
        //
        //     //4 values on the first branch
        //     Assert.AreEqual(4, firstBranchLocations.Count);
        //     Assert.AreEqual(new[] { 0.5, 1.5, 1.0, 0.75 }, firstBranchLocations
        //         .Select(loc => waterFlowFMModel.InitialConditions[loc]).ToArray());
        //     
        //     //the second use waterlevel..2 locations here
        //     var secondBranch = waterFlowFMModel.Network.Branches[1];
        //     var secondBranchLocations =
        //         waterFlowFMModel.InitialConditions.Locations.Values.Where(f => f.Branch == secondBranch).ToList();
        //     
        //     Assert.AreEqual(4,secondBranchLocations.Count);
        //     Assert.AreEqual(new[] { 11, 11.641956799085495, 12, 12 }, secondBranchLocations
        //         .Select(loc => waterFlowFMModel.InitialConditions[loc]).ToArray());
        // }
        //
        // [Test]
        // [Category(TestCategory.DataAccess)]
        // public void ImportBoundaryConditionQh()
        // {
        //     WaterFlowFMModel waterFlowFMModel = GetWaterFlowFMModel(TestHelper.GetTestDataDirectory() + @"\QHBound.lit\1\network.tp");
        //     Assert.AreEqual(2, waterFlowFMModel.BoundaryConditions.Count);
        //
        //     // first is Q(h)
        //     var QhBoundaryData = waterFlowFMModel.BoundaryConditions[0];
        //     Assert.AreEqual(Model1DBoundaryNodeDataType.FlowWaterLevelTable, QhBoundaryData.DataType);
        //     // check the argument H
        //     Assert.AreEqual(new[] {0, 1, 3, 4}, QhBoundaryData.Data.Arguments[0].Values);
        //     // the component Q
        //     Assert.AreEqual(new[] {1, 2, 4, 5}, QhBoundaryData.Data.Components[0].Values);
        //
        //     // first is default is none
        //     Assert.AreEqual(Model1DBoundaryNodeDataType.None, waterFlowFMModel.BoundaryConditions[1].DataType);
        // }
        //
        // [Test]
        // [Category(TestCategory.DataAccess)]
        // public void ImportBoundaryConditionDeadEnd()
        // {
        //     var waterFlowFMModel = GetWaterFlowFMModel(TestHelper.GetTestDataDirectory() + @"\036_000.lit\1\network.tp");
        //     Assert.AreEqual(2, waterFlowFMModel.BoundaryConditions.Count);
        //     // second is default is dead end
        //     Assert.AreEqual(Model1DBoundaryNodeDataType.None,
        //                     waterFlowFMModel.BoundaryConditions[1].DataType);
        //
        // }
        //
        // [Test]
        // [Category(TestCategory.Integration)]
        // public void ImportBoundaryWhereIdIsNotCarrierId()
        // {
        //     // In al Sobek212 test model id and ci in network.cn are identical
        //     // IN SobekRe models this is in most cases not true
        //     var waterFlowFMModel = GetWaterFlowFMModel(TestHelper.GetTestDataDirectory() + @"\ReModels\20110331_BfgRhein.sbk\1\deftop.1");
        //     // defcnd.1
        //     // FLBO id '5576325' nm '(null)' ci 'AL2_05' flbo
        //     // defcnd.2
        //     // FLBO id '5576325' ty 1 se 0 h0 9.9999e+009 w0 0 q_ dt 1 9.9999e+009 9.9999e+009 'Discharge at Boundary' PDIN 0 0 '' pdin CLTT 'Time' 'Q [m3/s]' cltt CLID '(null)' '(null)' clid TBLE 
        //     // '2002/12/15;00:00:00' 2200 < 
        //     // ...
        //     // thus at node AL2_05 must be Q boundary Time series first datetime is 2002/12/15;00:00:00
        //     var boundaryNode = waterFlowFMModel.BoundaryConditions.Where(bc => bc.Feature.Name == "AL2_05").FirstOrDefault();
        //     Assert.AreEqual(Model1DBoundaryNodeDataType.FlowTimeSeries, boundaryNode.DataType);
        //     Assert.Greater(boundaryNode.Data.Arguments[0].Values.Count, 0);
        //     Assert.AreEqual(new DateTime(2002, 12, 15, 0, 0, 0), boundaryNode.Data.Arguments[0].Values[0]);
        // }
        //
        // [Test]
        // [Category(TestCategory.Integration)]
        // [Category(TestCategory.Slow)]
        // public void ImportBoundarySaltAndFlowUseIdenticalBoundaryId()
        // {
        //     // In all Sobek212 test models id and ci in network.cn are identical
        //     // IN SobekRe models this is in most cases not true
        //     var waterFlowFMModel = GetWaterFlowFMModel(TestHelper.GetTestDataDirectory() + @"\ReModels\20110331_NDB.sbk\6\deftop.1");
        //     // defcnd.1
        //     // FLBO id '9' nm '(null)' ci '22' flbo
        //     // FLBO id '11' nm '(null)' ci '28' flbo
        //     // STBO id '12' nm '(null)' ci '28' stbo
        //     // STBO id '11' nm '(null)' ci '22' stbo
        //     // defcnd.2
        //     // FLBO id '9' ty 1 se 0 h0 9.9999e+009 w0 0 q_ dw 0 0 9.9999e+009 h_ wd 0 9.9999e+009 9.9999e+009 qs dm 0 9.9999e+009 9.9999e+009 flbo
        //     // FLBO id '11' ty 1 se 0 h0 9.9999e+009 w0 0 q_ dw 0 0 9.9999e+009 h_ wd 0 9.9999e+009 9.9999e+009 qs dm 0 9.9999e+009 9.9999e+009 flbo
        //     // defcnd.6
        //     // STBO id '11' ty 1 co co 0 0.2 9.9999e+009 tl 9.9999e+009 tu 0 stbo
        //     // STBO id '12' ty 1 co co 0 0.2 9.9999e+009 tl 9.9999e+009 tu 0 stbo
        //     
        //     var boundaryNode = waterFlowFMModel.BoundaryConditions.Where(bc => bc.Feature.Name == "28").FirstOrDefault();
        //     Assert.AreEqual(Model1DBoundaryNodeDataType.FlowConstant, boundaryNode.DataType);
        //     Assert.AreEqual(0.0, boundaryNode.Flow, 1.0e-6);
        //     Assert.IsTrue(waterFlowFMModel.UseSalt);
        //     // ty 1 is zeroflux = none
        //     Assert.AreEqual(SaltBoundaryConditionType.None, boundaryNode.SaltConditionType);
        //     // defcnd.1
        //     // STBO id '34' nm '(null)' ci '91' stbo
        //     // defcnd.6
        //     // STBO id '34' ty 0 co co 0 34 9.9999e+009 tl 5400 tu 1 stbo
        //     boundaryNode = waterFlowFMModel.BoundaryConditions.FirstOrDefault(bc => bc.Feature.Name == "91");
        //     // ty 0 is concentration co co 0 is constant
        //     Assert.AreEqual(SaltBoundaryConditionType.Constant, boundaryNode.SaltConditionType);
        //     Assert.AreEqual(34, boundaryNode.SaltConcentrationConstant, 1.0e-6);
        // }
        //
        // [Test]
        // [Category(TestCategory.DataAccess)]
        // [Category(TestCategory.Slow)]
        // public void InvertReboundariesAtEndOfBranch()
        // {
        //     var waterFlowFMModel = GetWaterFlowFMModel(TestHelper.GetTestDataDirectory() + @"\ReModels\20110331_NDB.sbk\6\deftop.1");
        //     var boundaryData = waterFlowFMModel.BoundaryConditions.FirstOrDefault(bc => bc.Feature.Name == "P_P_P_Volk_91");
        //     Assert.AreEqual(Model1DBoundaryNodeDataType.FlowWaterLevelTable, boundaryData.DataType);
        //     // check the argument H
        //     Assert.AreEqual(new[] { 0, 0.79, 0.8 }, boundaryData.Data.Arguments[0].Values);
        //     Assert.AreEqual(new[] { 0, 0, -200 }, boundaryData.Data.Components[0].Values);
        // }
        //
        // [Test]
        // [Category(TestCategory.DataAccess)]
        // [Category(TestCategory.Slow)]
        // public void ImportExtraResistanceForRe()
        // {
        //     var pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\ReModels\20110331_BfgRhein.sbk\1\DEFTOP.1";
        //     var waterFlowFMModel = GetWaterFlowFMModel(pathToSobekNetwork);
        //     Assert.AreEqual(2, waterFlowFMModel.Network.ExtraResistances.Count());
        // }
        //
        // [Test]
        // [Category(TestCategory.DataAccess)]
        // public void ImportWindConstant()
        // {
        //     var waterFlowFMModel = GetWaterFlowFMModel(TestHelper.GetTestDataDirectory() + @"\wind.lit\2\network.tp");
        //     Assert.AreEqual(0, waterFlowFMModel.Wind.Direction.Values.Count);
        //     Assert.AreEqual(181.0, (double)waterFlowFMModel.Wind.Direction.DefaultValue, 1.0e-6);
        // }
        //
        // [Test]
        // [Category(TestCategory.DataAccess)]
        // public void ImportWindTimeSeries()
        // {
        //     var waterFlowFMModel = GetWaterFlowFMModel(TestHelper.GetTestDataDirectory() + @"\wind.lit\4\network.tp");
        //     Assert.AreEqual(2, waterFlowFMModel.Wind.Direction.Values.Count);
        // }
        //
        // /// <summary>
        // /// Sobek modeldatabase files may contain references to unused data; they should be ignored
        // /// test to check for TOOLS-2070: Flow 1D gives error message when running because of unused data.
        // /// </summary>
        // [Test]
        // [Category(TestCategory.Integration)]
        // public void ReadReducedModel()
        // {
        //     string path = TestHelper.GetTestDataDirectory() + @"\ReducedModel\3\NETWORK.TP";
        //
        //     //import existing network and model + default boundary conditions
        //     var importer = new SobekNetworkImporter();
        //     var network = (HydroNetwork)importer.ImportItem(path);
        //
        //     // profile.dat 10 records (CRSN)
        //     // network.cr 4 records (CRSN)
        //     // --> result is 4 cross sections
        //     Assert.AreEqual(4, network.CrossSections.Count());
        //
        //     // boundary.dat 4 records (FLBO)
        //     // lateral.dat 5 records (FLBR)
        //     // network.cn 4 records ( 2 FLBO, 2 FLBR)
        //     // --> result is 2 nodes with boundary and 2 lateral sources
        //     Assert.AreEqual(2, network.Nodes.Count(n => !n.IsConnectedToMultipleBranches));
        //     Assert.AreEqual(2, network.LateralSources.Count());
        //
        //     // struct.def 3 records (STDS)
        //     // network.st 1 records (STRU)
        //     // --> result is 2 structures (+1 for composite)
        //     Assert.AreEqual(2, network.Structures.Count());
        //     var compositeStructures = network.Structures.Where(s => s is ICompositeBranchStructure);
        //
        //     var compositeStructure = compositeStructures.First() as CompositeBranchStructure;
        //     Assert.AreEqual("weir 3", compositeStructure.Structures[0].LongName);
        //     
        //     Assert.AreEqual(1, compositeStructures.Count());
        // }
        //
        // [Test]
        // [Category(TestCategory.DataAccess)]
        // [Category(TestCategory.WorkInProgress)]//ToDo: find out why absolute path is needed
        // public void ImportWindConstantFileAbsolutePath()
        // {
        //     string casedesc = Environment.CurrentDirectory + "tmpcasedesc.cmt";
        //     string wind = Environment.CurrentDirectory + "WINDCNST.WDC";
        //     TextWriter textWriter = new StreamWriter(casedesc);
        //     textWriter.WriteLine(@"I {0}WINDCNST.WDC 234 '1288628629'", Environment.CurrentDirectory);
        //     textWriter.Close();
        //
        //     textWriter = new StreamWriter(wind);
        //     textWriter.WriteLine(@"GLMT MTEO nm '(null)' ss 0 id '0' ci '-1' lc 9.9999e+009 wu 1");
        //     textWriter.WriteLine(@"wv tv 0 0.67 9.9999e+009 wd td 0 241.11 9.9999e+009 su 0 sh ts");
        //     textWriter.WriteLine(@"0 9.9999e+009 9.9999e+009 tu 0 tp tw 0 9.9999e+009 9.9999e+009 au 0 at ta 0");
        //     textWriter.WriteLine(@"9.9999e+009 9.9999e+009 mteo glmt");
        //     textWriter.Close();
        //
        //     // import model as base
        //     var waterFlowFMModel = GetWaterFlowFMModel(TestHelper.GetTestDataDirectory() + @"\wind.lit\2\network.tp"); 
        //     //now check wind data with absolute path set in case file
        //     //SobekCaseData sobekCaseData = SobekCaseDataReader.ReadCaseData(casedesc);
        //     //SobekwaterFlowFMModelReader.ImportWind("C:\\", waterFlowFMModel, sobekCaseData.WindDataPath);
        //
        //     File.Delete(casedesc);
        //     File.Delete(wind);
        //
        //     // NOTE: due to fix in revision 19477, D:\New_Deltashell\delta-shell\test-data\Plugins\DelftModels\DeltaShell.Plugins.ImportExport.Sobek.Tests\FIXED\WINDCNST.WDC will be read, 
        //     // and custom written file is ignored!
        //
        //     //Assert.AreEqual(0, waterFlowFMModel.Wind.Velocity.Values.Count);
        //     //Assert.AreEqual(0.67, (double)waterFlowFMModel.Wind.Velocity.DefaultValue, 1.0e-6);
        // }
        //
        // private static WaterFlowFMModel GetWaterFlowFMModel(string path)
        // {
        //     var importer = new SobekModelToIntegratedModelImporter();
        //     var model = importer.ImportItem(path);
        //
        //     if(model is ICompositeActivity)
        //     {
        //         return ((ICompositeActivity)model).Activities.OfType<WaterFlowFMModel>().First();
        //     }
        //
        //     //calculation grid points
        //     return (WaterFlowFMModel)model;
        // }
        //
        // [Test]
        // [Category(TestCategory.Integration)]
        // public void ImportReModelWithSalt()
        // {
        //     // import model and network.
        //     waterFlowFMModel waterFlowFMModel = GetWaterFlowFMModel(TestHelper.GetTestDataDirectory() + @"\ReModels\zoutig.sbk\1\deftop.1");
        //     Assert.IsTrue(waterFlowFMModel.UseSalt);
        //
        //     // simple network; 
        //     //  - 2 branches, 
        //     //  - 2 boundary node; 1 zero flux, 1 time series salt
        //     //  - 3 lateral sources; 1 flow and salt concentration, 1 no salt, 1 dry load (= no flow)
        //     var boundaryNode29 = waterFlowFMModel.BoundaryConditions.Where(bc => bc.Feature.Name == "29").FirstOrDefault();
        //     Assert.IsTrue(boundaryNode29.UseSalt);
        //     Assert.AreEqual(SaltBoundaryConditionType.TimeDependent, boundaryNode29.SaltConditionType);
        //     var boundaryNode0 = waterFlowFMModel.BoundaryConditions.Where(bc => bc.Feature.Name == "0").FirstOrDefault();
        //     Assert.IsTrue(boundaryNode0.UseSalt);
        //     Assert.AreEqual(SaltBoundaryConditionType.None, boundaryNode0.SaltConditionType);
        //
        //     Assert.AreEqual(3, waterFlowFMModel.Network.LateralSources.Count());
        //
        //     var lateralSource15 = waterFlowFMModel.LateralSourceData.Where(lsd => lsd.Feature.Name == "15").FirstOrDefault();
        //     Assert.IsTrue(lateralSource15.UseSalt);
        //     Assert.AreEqual(SaltLateralDischargeType.ConcentrationTimeSeries, lateralSource15.SaltLateralDischargeType);
        //
        //     var lateralSource19 = waterFlowFMModel.LateralSourceData.Where(lsd => lsd.Feature.Name == "19").FirstOrDefault();
        //     Assert.IsTrue(lateralSource19.UseSalt); // is a model wide setting
        //     Assert.AreEqual(SaltLateralDischargeType.Default, lateralSource19.SaltLateralDischargeType);
        //
        //     var lateralSource21 = waterFlowFMModel.LateralSourceData.Where(lsd => lsd.Feature.Name == "21").FirstOrDefault();
        //     Assert.IsTrue(lateralSource21.UseSalt);
        //     Assert.AreEqual(SaltLateralDischargeType.MassTimeSeries, lateralSource21.SaltLateralDischargeType);
        //
        //     Assert.AreEqual(6, waterFlowFMModel.InitialSaltConcentration.Locations.Values.Count);
        //     Assert.AreEqual(8, waterFlowFMModel.DispersionCoverage.Locations.Values.Count);
        // }
        //
        // [Test]
        // [Category(TestCategory.Integration)]
        // [Category(TestCategory.Slow)]
        // public void OnlyMainFp1Fp2RoughNessSections()
        // {
        //     // import model and network.
        //     var waterFlowFMModel = GetWaterFlowFMModel(TestHelper.GetTestDataDirectory() + @"\BYPASS.lit\3\network.tp");
        //     
        //     Assert.AreEqual(3, waterFlowFMModel.RoughnessSections.Count);
        //     Assert.AreEqual(1, waterFlowFMModel.RoughnessSections.Where(rs => rs.Name.ToUpper().Contains("MAIN")).Count());
        //     Assert.AreEqual(1, waterFlowFMModel.RoughnessSections.Where(rs => rs.Name.ToUpper().Contains("PLAIN1")).Count());
        //     Assert.AreEqual(1, waterFlowFMModel.RoughnessSections.Where(rs => rs.Name.ToUpper().Contains("PLAIN2")).Count());
        // }
        //
        // [Test]
        // [Category(TestCategory.Integration)]
        // public void QRelatedRoughnessShouldAlsoAddNetworkLocationToCoverage()
        // {
        //     var pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\ReModels\20110331_BfgRhein.sbk\1\DEFTOP.1";
        //     var waterFlowFMModel = GetWaterFlowFMModel(pathToSobekNetwork);
        //     // offset for branchfeature are always relative to the geometry of the branch
        //     var mainCoverage =
        //         waterFlowFMModel.RoughnessSections.FirstOrDefault(rs => rs.Name.ToUpper().Contains("MAIN"));
        //     // branch "AL2_06" has roughnees for Q at 4 location: 'Q' '0' '7000' '12000' '34524'
        //     var branch = waterFlowFMModel.Network.Branches.Where(b => b.Name == "AL2_06").FirstOrDefault();
        //     Assert.AreEqual(RoughnessFunction.FunctionOfQ, mainCoverage.GetRoughnessFunctionType(branch));
        //     Assert.AreEqual(4, mainCoverage.RoughnessNetworkCoverage.Locations.Values.Where(nl => nl.Branch == branch).Count());
        // }
        //
        // /// <summary>
        // /// Test for branch with user defined length
        // /// from SobekRe files:
        // /// NODE id '0' nm 'node001' px 64040.0 py 444970.0 node
        // /// NODE id '1' nm 'Maasmond' px 62730.0 py 445440.0 node
        // /// BRCH id '0' nm 'MAMO001' bn '0' en '1' al 1405 brch
        // /// GRID id '0' nm '(null)' ci '0' lc 9.9999e+009 se 0 oc 0 gr gr 'GridPoints on Branch <MAMO001> with length: 1405.0' PDIN 0 0 '' pdin CLTT 'Location' '1/R' cltt CLID '(null)' '(null)' clid TBLE 
        // /// 0 9.9999e+009 < 
        // /// 351.25 9.9999e+009 < 
        // /// 702.5 9.9999e+009 < 
        // /// 1053.75 9.9999e+009 < 
        // /// 1405 9.9999e+009 < 
        // /// tble
        // ///  grid
        // /// </summary>
        // [Test]
        // [Category(TestCategory.Integration)]
        // [Category(TestCategory.Slow)]
        // public void SobekReCalcGridWithUserLength()
        // {
        //     var waterFlowFMModel = GetWaterFlowFMModel(TestHelper.GetTestDataDirectory() + @"\ReModels\20110331_NDB.sbk\6\deftop.1");
        //     var computationalPoints =
        //         waterFlowFMModel.NetworkDiscretization.Locations.Values.Where(nl => nl.Branch.Name == "0").OrderBy(nl =>nl.Chainage).ToArray();
        //     Assert.AreEqual(5, computationalPoints.Count());
        //     // expect gridpoint to cover branch and scaled
        //     Assert.AreEqual(0.0, computationalPoints[0].Chainage, 1.0e-6);
        //     Assert.AreEqual(351.25, computationalPoints[1].Chainage, 1.0e-6);
        //     Assert.AreEqual(702.5, computationalPoints[2].Chainage, 1.0e-6);
        //     Assert.AreEqual(1053.75, computationalPoints[3].Chainage, 1.0e-6);
        //     Assert.AreEqual(1405.0, computationalPoints[4].Chainage, 1.0e-6);
        // }
        //
        // /// GRID id '109' nm '(null)' ci '111' lc 9.9999e+009 se 0 oc 0 gr gr 'GridPoints on Branch <MIHA094> with length: 2056.9' PDIN 0 0 '' pdin CLTT 'Location [m]' '1/R [1/m]' cltt CLID '(null)' '(null)' clid TBLE 
        // /// 0 9.9999e+009 < 
        // /// 497.5 9.9999e+009 < 
        // /// 995 9.9999e+009 < 
        // /// 1525.9 9.9999e+009 < 
        // /// 1791.35 9.9999e+009 < 
        // /// 2056.8 9.9999e+009 <    <-----
        // /// tble
        // /// BRCH id '111' nm 'MIHA094' bn '82' en '81' al 2056.87 brch
        // /// 
        // /// Gridpoint at 2056.8 is 0.07 m before end should be ignored (or shifted to end)
        // [Test]
        // [Category(TestCategory.Integration)]
        // [Category(TestCategory.Slow)]
        // public void SobekReCalcGridWithUserLengthAndSmallSegment()
        // {
        //     var waterFlowFMModel = GetWaterFlowFMModel(TestHelper.GetTestDataDirectory() + @"\ReModels\20110331_NDB.sbk\6\deftop.1");
        //     
        //     var computationalPoints =
        //         waterFlowFMModel.NetworkDiscretization.Locations.Values.Where(nl => nl.Branch.Name == "111").OrderBy(nl => nl.Chainage).ToArray();
        //     Assert.AreEqual(6, computationalPoints.Count());
        //     // expect gridpoint to cover branch and scaled
        //     Assert.AreEqual(0.0, computationalPoints[0].Chainage, 1.0e-6);
        //     Assert.AreEqual(497.5, computationalPoints[1].Chainage, 1.0e-6);
        //     Assert.AreEqual(995, computationalPoints[2].Chainage, 1.0e-6);
        //     Assert.AreEqual(1525.9, computationalPoints[3].Chainage, 1.0e-6);
        //     Assert.AreEqual(1791.35, computationalPoints[4].Chainage, 1.0e-6);
        //     // gridpoint near end branch should be removed
        //     // Assert.AreEqual(factor * 2056.8, computationalPoints[5].Offset, 1.0e-6);
        //     Assert.AreEqual(2056.87, computationalPoints[5].Chainage, 1.0e-6);
        //
        //     var computationalSegments =
        //         waterFlowFMModel.NetworkDiscretization.Segments.Values.Where(s => s.Branch.Name == "111").OrderBy(nl => nl.Chainage).ToArray();
        //     Assert.AreEqual(5, computationalSegments.Count());
        //     Assert.AreEqual(1791.35, computationalSegments[4].Chainage, 1.0e-6);
        //     Assert.AreEqual(2056.87, computationalSegments[4].EndChainage, 1.0e-6);
        // }
        //
        // [Test]
        // [Category(TestCategory.DataAccess)]
        // [Category(TestCategory.Slow)]
        // public void ReadNetworkWithHeightFlowStorageWidthCrossSections()
        // {
        //     string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\SW_max_1.lit\3\NETWORK.TP";
        //     var importer = new SobekModelToIntegratedModelImporter();
        //     var flowModel1D = (waterFlowFMModel)importer.ImportItem(pathToSobekNetwork);
        //
        //     var heightFlowStorageWidthCrossSections = flowModel1D.Network.CrossSections.Where(cs => cs.CrossSectionType == CrossSectionType.ZW);
        //
        //     //CrossSections with r_ are not ignored anymore
        //     Assert.AreEqual(13, heightFlowStorageWidthCrossSections.Count());
        //
        //     var crossSection = heightFlowStorageWidthCrossSections.First(cs => cs.Name == "prof_ZGN2");
        //
        //     Assert.AreEqual(1, crossSection.Definition.Sections.Count);
        //     var sectionType = crossSection.Definition.Sections[0].SectionType;
        //     var roughnessSection = flowModel1D.RoughnessSections.Where(rs => rs.CrossSectionSectionType == sectionType).FirstOrDefault();
        //     var coverage = roughnessSection.RoughnessNetworkCoverage;
        //     Assert.AreEqual(RoughnessType.StricklerKs, coverage.EvaluateRoughnessType(new NetworkLocation(crossSection.Branch, crossSection.Chainage)));
        //     Assert.AreEqual(30.0, coverage.EvaluateRoughnessValue(new NetworkLocation(crossSection.Branch, crossSection.Chainage)), 1.0e-6);
        //     Assert.AreEqual(crossSection.Definition.Profile.Max(yz => yz.X), crossSection.Definition.Sections[0].MaxY);
        // }
        //
        // [Test]
        // [Category(TestCategory.DataAccess)]
        // public void ReadModelWithRougnessInterpolationTypeIsBlock()
        // {
        //     string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\254_000.lit\1\NETWORK.TP";
        //     var importer = new SobekModelToIntegratedModelImporter();
        //     var flowModel1D = (waterFlowFMModel)importer.ImportItem(pathToSobekNetwork);
        //
        //     var mainSection = flowModel1D.RoughnessSections.FirstOrDefault(rs => rs.Name.ToLower() == "main");
        //
        //     Assert.IsNotNull(mainSection);
        //     Assert.AreEqual(InterpolationType.Constant, mainSection.RoughnessNetworkCoverage.Arguments[0].InterpolationType);
        //
        // }
        //
        // /// <summary>
        // /// 500	525
        // /// 525	575
        // /// 575	600
        // /// 600	650
        // /// </summary>
        // [Test]
        // [Category(TestCategory.DataAccess)]
        // public void ReadShiftedProfile()
        // {
        //     var pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\profshft.lit\1\NETWORK.TP";
        //     var importer = new SobekModelToIntegratedModelImporter();
        //     var model = (waterFlowFMModel)importer.ImportItem(pathToSobekNetwork);
        //
        //     var network = model.Network;
        //     var crossSection = network.CrossSections.FirstOrDefault().Definition;
        //     Assert.AreEqual(4, crossSection.Sections.Count);
        //     Assert.AreEqual(500, crossSection.Sections[0].MinY, 1.0e-3);
        //     Assert.AreEqual(525, crossSection.Sections[0].MaxY, 1.0e-3);
        //     Assert.AreEqual(525, crossSection.Sections[1].MinY, 1.0e-3);
        //     Assert.AreEqual(575, crossSection.Sections[1].MaxY, 1.0e-3);
        //     Assert.AreEqual(575, crossSection.Sections[2].MinY, 1.0e-3);
        //     Assert.AreEqual(600, crossSection.Sections[2].MaxY, 1.0e-3);
        //     Assert.AreEqual(600, crossSection.Sections[3].MinY, 1.0e-3);
        //     Assert.AreEqual(650, crossSection.Sections[3].MaxY, 1.0e-3);
        // }
        //
        // [Test]
        // [Category(TestCategory.DataAccess)]
        // [Category(TestCategory.Slow)]
        // public void ReadInitialConditionFromSettings()
        // {
        //     var pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\3testjz\NETWORK.TP";
        //     var model = GetWaterFlowFMModel(pathToSobekNetwork);
        //
        //     Assert.AreEqual(InitialConditionsType.Depth, model.InitialConditionsType);
        //     Assert.AreEqual(2.0, model.DefaultInitialWaterLevel, 1.0e-6);
        //     Assert.AreEqual(0.25, model.DefaultInitialDepth, 1.0e-6);
        // }
        //
        // /// <summary>
        // /// see related test in SobekNetworkFileReaderTest::ReadPoNetwork
        // /// SobekwaterFlowFMModelReader splts for linkage nodes but this results in branches without any crosssections
        // /// </summary>
        // [Test]
        // [Category(TestCategory.DataAccess)]
        // [Category(TestCategory.VerySlow)]
        // public void ReadPoNetwork()
        // {
        //     var path = TestHelper.GetTestDataDirectory() + @"\POup_GV.lit\7\network.tp";
        //
        //     var importer = new SobekModelToIntegratedModelImporter();
        //     var model = (waterFlowFMModel)importer.ImportItem(path);
        //
        //     var branchehWithoutCrossSections = model.Network.Channels.Where(c => c.CrossSections.Count() == 0);
        //     Assert.Greater(branchehWithoutCrossSections.Count(), 0);
        //     // TODO: add asserts
        // }
        //
        // [Test]
        // [Category(TestCategory.DataAccess)]
        // [Category(TestCategory.Slow)]
        // public void ReadReModelWithLateralSourceData()
        // {
        //     string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\REModels\J_10BANK_v2.sbk\6\DEFTOP.1";
        //     
        //     var importer = new SobekModelToIntegratedModelImporter();
        //     var model = importer.ImportItem(pathToSobekNetwork);
        //
        //     Assert.IsNotNull(model);
        //     Assert.IsTrue(model is ICompositeActivity);
        //     var flowModel = (model as ICompositeActivity).Activities.OfType<waterFlowFMModel>().First();
        //     Assert.AreEqual(50, flowModel.LateralSourceData.Count);
        // }
        //
        // [Test]
        // [Category(TestCategory.DataAccess)]
        // [Category(TestCategory.Slow)]
        // public void ReadReModelIntowaterFlowFMModelWithLateralSourceData()
        // {
        //     string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\REModels\J_10BANK_v2.sbk\6\DEFTOP.1";
        //
        //     var importer = new SobekModelToWaterFlowFMImporter();
        //     importer.TargetItem = new waterFlowFMModel();
        //     var model = importer.ImportItem(pathToSobekNetwork);
        //
        //     Assert.IsNotNull(model);
        //     Assert.IsInstanceOfType(typeof(waterFlowFMModel), model);
        //     var flowModel = ((waterFlowFMModel) model);
        //     Assert.AreEqual(50, flowModel.LateralSourceData.Count);
        // }
        //
        // [Test]
        // [Category(TestCategory.DataAccess)]
        // [Category(TestCategory.Slow)]
        // public void ReadCaseSettingsSteadyViaNetworkTpFile()
        // {
        //     var culture1 = Thread.CurrentThread.CurrentCulture;
        //     Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("nl-nl");
        //
        //     var modelPath = TestHelper.GetTestDataDirectory() + @"\\steady.lit\2\NETWORK.TP";
        //     var settingsPath = TestHelper.GetTestDataDirectory() + @"\\steady.lit\2\SETTINGS.DAT";
        //     var importer = new SobekModelToIntegratedModelImporter();
        //     var model1D = ((HydroModel)importer.ImportItem(modelPath)).Models.OfType<waterFlowFMModel>().First();
        //
        //     var parameterSettings = model1D.ParameterSettings;
        //
        //     var sobekCaseSettings = SobekCaseSettingsReader.GetSobekCaseSettings(settingsPath);
        //     //ramdon samples
        //     foreach (var modelApiParameter in parameterSettings)
        //     {
        //         if (modelApiParameter.Name == "CourantNumber")
        //         {
        //             Assert.AreEqual(sobekCaseSettings.CourantNumber, double.Parse(modelApiParameter.Value, CultureInfo.InvariantCulture));
        //         }
        //         if (modelApiParameter.Name == "DtMinimum")
        //         {
        //             Assert.AreEqual(sobekCaseSettings.DtMinimum, double.Parse(modelApiParameter.Value, CultureInfo.InvariantCulture));
        //         }
        //         if (modelApiParameter.Name == "AccurateVersusSpeed")
        //         {
        //             Assert.AreEqual(sobekCaseSettings.AccurateVersusSpeed, double.Parse(modelApiParameter.Value, CultureInfo.InvariantCulture));
        //         }
        //     }
        //     Thread.CurrentThread.CurrentCulture = culture1;
        // }
        //
        // [Test]
        // [Category(TestCategory.DataAccess)]
        // public void ReadFlowModelTimersFromMeteo()
        // {
        //     string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\FlowTimersFromMeteo\TimersFromMeteo.lit\1\NETWORK.TP";
        //     var importer = new SobekModelToIntegratedModelImporter();
        //     var flowModel1D = (waterFlowFMModel)importer.ImportItem(pathToSobekNetwork);
        //
        //     var startTime = new DateTime(1996, 1, 1, 0, 0, 0);
        //     var stopTime = new DateTime(1997, 1, 1, 1, 0, 0);
        //
        //     Assert.AreEqual(startTime, flowModel1D.StartTime);
        //     Assert.AreEqual(stopTime, flowModel1D.StopTime);
        // }
        //
        // [Test]
        // [Category(TestCategory.DataAccess)]
        // public void ReadFlowModelTimersNotFromMeteo()
        // {
        //     string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\FlowTimersFromMeteo\TimersFromMeteo.lit\2\NETWORK.TP";
        //     var importer = new SobekModelToIntegratedModelImporter();
        //     var flowModel1D = (waterFlowFMModel)importer.ImportItem(pathToSobekNetwork);
        //
        //     var startTime = new DateTime(1996, 1, 1, 1, 0, 0);
        //     var stopTime = new DateTime(1996, 1, 2, 15, 0, 0);
        //
        //     Assert.AreEqual(startTime, flowModel1D.StartTime);
        //     Assert.AreEqual(stopTime, flowModel1D.StopTime);
        // }

    }
}