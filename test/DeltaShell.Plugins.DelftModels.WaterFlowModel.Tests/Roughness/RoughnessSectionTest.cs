using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.Tests.Helpers;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Editing;
using DelftTools.Utils.UndoRedo;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;
using Rhino.Mocks;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.Roughness
{
    [TestFixture]
    public class RoughnessSectionTest
    {
        [Test]
        public void SettingNetworkWillConvertExistingFunctionsToNewNetworkIfPossible()
        {
            // setup
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(2);
            var branch1 = network.Branches[0];
            var branch2 = network.Branches[1];
            var crossSectionSectionType = new CrossSectionSectionType { Name = "main" };
            var section = new RoughnessSection(crossSectionSectionType, network);

            var functionOfH = new Function("functionOfH");
            functionOfH.Arguments.Add(new Variable<double>() {Values = new MultiDimensionalArray<double>() {6.0, 8.0, 10.0}});
            functionOfH.Components.Add(new Variable<double>() { Values = new MultiDimensionalArray<double>() { 1.0, 5.0, 9.0 } });

            var functionOfQ = new Function("functionOfQ");
            functionOfQ.Arguments.Add(new Variable<double>() { Values = new MultiDimensionalArray<double>() { 2.0, 6.0, 10.0 } });
            functionOfQ.Components.Add(new Variable<double>() { Values = new MultiDimensionalArray<double>() { 3.0, 5.0, 7.0 } });
            
            // add functions
            section.AddHRoughnessFunctionToBranch(branch1, functionOfH);
            section.AddQRoughnessFunctionToBranch(branch2, functionOfQ);
            
            Assert.IsNotNull(section.FunctionOfH(branch1));
            Assert.IsNotNull(section.FunctionOfQ(branch2));
            
            // clone network and update section
            var clonedNetwork = (IHydroNetwork)network.Clone();
            section.Network = clonedNetwork;

            // compare functionOfH
            var clonedFunctionOfH = section.FunctionOfH(clonedNetwork.Branches[0]);

            var originalArguments = (IList<double>)functionOfH.Arguments[0].Values;
            var clonedArguments = (IList<double>)clonedFunctionOfH.Arguments[0].Values;
            Assert.IsTrue(CompareLists(originalArguments, clonedArguments));

            var originalComponents = (IList<double>)functionOfH.Components[0].Values;
            var clonedComponents = (IList<double>)clonedFunctionOfH.Components[0].Values;
            Assert.IsTrue(CompareLists(originalComponents, clonedComponents));

            // compare functionOfQ
            var clonedFunctionOfQ = section.FunctionOfQ(clonedNetwork.Branches[1]);

            originalArguments = (IList<double>)functionOfQ.Arguments[0].Values;
            clonedArguments = (IList<double>)clonedFunctionOfQ.Arguments[0].Values;
            Assert.IsTrue(CompareLists(originalArguments, clonedArguments));

            originalComponents = (IList<double>)functionOfQ.Components[0].Values;
            clonedComponents = (IList<double>)clonedFunctionOfQ.Components[0].Values;
            Assert.IsTrue(CompareLists(originalComponents, clonedComponents));
        }

        private static bool CompareLists<T>(IList<T> list1, IList<T> list2) where T : IComparable
        {
            if(list1.Count != list2.Count) return false;

            for (var i = 0; i < list1.Count; i++)
            {
                if (list1[i].CompareTo(list2[i]) != 0) return false;
            }
            return true;
        }

        [Test]
        public void NormalAndReverseRoughnessSectionsAreLinkedOnRoughnessTypePerBranch()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(2);
            var branch1 = network.Branches[0];
            var branch2 = network.Branches[1];
            var crossSectionSectionType = new CrossSectionSectionType { Name = "main" };
            var section = new RoughnessSection(crossSectionSectionType, network);
            var reverseSection = new ReverseRoughnessSection(section);

            reverseSection.UseNormalRoughness = false;

            var locationOne = new NetworkLocation(branch2, 0);
            reverseSection.RoughnessNetworkCoverage[locationOne] = new object[] { 100.0, RoughnessType.Manning };
            section.RoughnessNetworkCoverage[locationOne] = new object[] { 50.0, RoughnessType.Chezy };
            section.RoughnessNetworkCoverage[new NetworkLocation(branch1, 50)] = new object[] { 60.0, RoughnessType.DeBosAndBijkerk };
            section.RoughnessNetworkCoverage[new NetworkLocation(branch1, 0)] = new object[] { 70.0, RoughnessType.DeBosAndBijkerk };
            
            Assert.AreEqual(100, reverseSection.RoughnessNetworkCoverage.EvaluateRoughnessValue(locationOne));
            Assert.AreEqual(RoughnessType.Chezy, reverseSection.RoughnessNetworkCoverage.EvaluateRoughnessType(locationOne));
        }

        [Test]
        public void Clone()
        {
            var network = new Network();
            var crossSectionSectionType = new CrossSectionSectionType { Name = "main" };
            var section = new RoughnessSection(crossSectionSectionType, network);
            var clone = (RoughnessSection)section.Clone();

            Assert.AreEqual(section.CrossSectionSectionType, clone.CrossSectionSectionType);
            //a new coverage on the same network...is this correct?
            Assert.AreEqual(section.RoughnessNetworkCoverage.Network, clone.RoughnessNetworkCoverage.Network);
        }

        [Test]
        public void TestFunctionOfH()
        {
            //var network = CreateNetwork();

            var function = RoughnessSection.DefineFunctionOfQ();
            function[0.0, 0.0] = 1.1;
            function[0.0, 1000.0] = 2.1;
            function[0.0, 5000.0] = 3.1;
            function[0.0, 10000.0] = 2.1;

            function[2500.0, 0.0] = 11.1;
            function[2500.0, 8000.0] = 13.1;
            function[2500.0, 10000.0] = 12.1;

            var roughness = function[2500.0, 8000.0];
            Assert.AreEqual(13.1, (double)roughness, 1.0e-6);
        }

        [Test]
        public void NameOfCoverageStaysInSyncWithCrossSectionSectionTypeName()
        {
            var network = new Network();
            var crossSectionSectionType = new CrossSectionSectionType { Name = "main" };
            var section = new RoughnessSection(crossSectionSectionType, network);

            Assert.AreEqual("main", section.RoughnessNetworkCoverage.Name);

            //change name of sectiontype should update the name of the coverage
            string newname = "newName";
            crossSectionSectionType.Name = newname;

            Assert.AreEqual(newname, section.RoughnessNetworkCoverage.Name);

        }

        [Test]
        public void DataItemsIsRenamedWhenCrossSectionTypeRenames()
        {
            var network = new Network();
            var crossSectionSectionType = new CrossSectionSectionType { Name = "main" };
            var section = new RoughnessSection(crossSectionSectionType, network);

            var dataItem = new DataItem(section);
            Assert.AreEqual("main", dataItem.Name);

            string newName = "kees";
            crossSectionSectionType.Name = newName;

            Assert.AreEqual(newName, dataItem.Name);

        }

        [Test]
        public void MovingALocationInCoverageUpdatesTheRelatedFunction()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(100, 0));
            var branch = network.Branches[0];
            var crossSectionSectionType = new CrossSectionSectionType { Name = "main" };
            var section = new RoughnessSection(crossSectionSectionType, network);
            //define a location 
            const double offset = 10.0d;
            var networkLocation = new NetworkLocation(branch, offset);
            section.RoughnessNetworkCoverage[networkLocation] = new object[] { 100.0, RoughnessType.StricklerKn };
            
            //define q on the branch
            var q = 10.0d;
            var function = section.AddQRoughnessFunctionToBranch(branch);
            function[offset,q] = 11.0d; //should this not be automatic?

            //now move the location..just like a branch editor would
            networkLocation.Chainage = 30.0d;

            //check the function offset got updated
            Assert.AreEqual(new[]{30},function.Arguments[0].Values);
        }

        [Test]
        public void MovingALocationToADifferentBranchUpdatesTheRelatedFunction()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(100, 0),new Point(100,100));
            var branch = network.Branches[0];
            var crossSectionSectionType = new CrossSectionSectionType { Name = "main" };
            var section = new RoughnessSection(crossSectionSectionType, network);
            //define a location 
            const double offset = 10.0d;
            var networkLocation = new NetworkLocation(branch, offset);
            section.RoughnessNetworkCoverage[networkLocation] = new object[] { 100.0, RoughnessType.StricklerKn };

            //define q on the branch
            var q = 10.0d;
            var function = section.AddQRoughnessFunctionToBranch(branch);
            function[offset, q] = 11.0d; //should this not be automatic?

            //move to other branch
            networkLocation.Branch = network.Branches[1];

            Assert.AreEqual(0, function.Arguments[0].Values.Count);
        }

        [Test]
        public void SplittingBranchKeepFunctionsIntact()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(100, 0));
            var channel = network.Channels.First();
            var crossSectionSectionType = new CrossSectionSectionType {Name = "main"};
            var section = new RoughnessSection(crossSectionSectionType, network);
            //define two location 
            int offset = 0;
            int endOffset = 100;
            section.RoughnessNetworkCoverage[new NetworkLocation(channel, offset)] = new object[]
                                                                                         {
                                                                                             0.0, RoughnessType.StricklerKn
                                                                                         };
            section.RoughnessNetworkCoverage[new NetworkLocation(channel, endOffset)] = new object[]
                                                                                            {
                                                                                                100.0,
                                                                                                RoughnessType.StricklerKn
                                                                                            };

            //define values for two q on the branch
            const double qSlow = 10.0d;
            var function = section.AddQRoughnessFunctionToBranch(channel);
            function[10.0d, qSlow] = 10.0d;
            function[90.0d, qSlow] = 20.0d; 

            const double qFast = 15.0d;
            function[10.0d, qFast] = 5.0d;
            function[90.0d, qFast] = 15.0d; 


            HydroNetworkHelper.SplitChannelAtNode(channel, 50);
            //now move the location..just like a branch editor would
            //networkLocation.Offset = 30.0d;

            //check the original function got updated (function of b1)
            Assert.AreEqual(new[] {10, 50}, function.Arguments[0].Values);
            Assert.AreEqual(new[] { qSlow, qFast }, function.Arguments[1].Values);
            Assert.AreEqual(new[] {10, 5, 15, 10}, function.Components[0].Values);


            //check another function was created with the values previously defined on 80
            var newBranch = network.Channels.ElementAt(1);
            var newFunction = section.FunctionOfQ(newBranch);
            
            Assert.AreEqual(new[] {0, 40}, newFunction.Arguments[0].Values);
            Assert.AreEqual(new[] { qSlow, qFast }, newFunction.Arguments[1].Values);
            Assert.AreEqual(new[] { 15, 10, 20, 15 }, newFunction.Components[0].Values);
        }

        [Test]
        public void RougnessSectionIncludesCoverageInGetAllItemsRecursive()
        {
            //this is needed because when a branch is merged we need to access the coverage to check 
            //if there is data defined on the merged branches
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(100, 0));
            var crossSectionSectionType = new CrossSectionSectionType { Name = "main" };
            var section = new RoughnessSection(crossSectionSectionType, network);

            IEnumerable<object> items = section.GetAllItemsRecursive().ToList();
            Assert.AreEqual(1,items.OfType<INetworkCoverage>().Count());
        }

        [Test]
        public void ReplacingLocationUpdatesRoughessFunction()
        {
            //relates to 5109. If the table is used to alter the chainage of a location, the location is replaced.
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(2);
            var crossSectionSectionType = new CrossSectionSectionType {Name = "main"};
            var section = new RoughnessSection(crossSectionSectionType, network);
            var branch = network.Branches.First();

            //add a location to the branch
            section.RoughnessNetworkCoverage[new NetworkLocation(branch, 50)] = new object[]
                                                                                    {
                                                                                        100.0,
                                                                                        RoughnessType.StricklerKn
                                                                                    };

            //switch and sync the function (as is done in RoughnessSectionCoverageView
            var function = section.AddQRoughnessFunctionToBranch(branch);
            var chainages =
                section.RoughnessNetworkCoverage.Locations.Values.Where(l => l.Branch == branch).Select(l => l.Chainage);
            function.Arguments[0].SetValues(chainages);
            function[50.0d, 1.0] = 5.0d; //set a value

            //action! replace a value in the coverage
            section.RoughnessNetworkCoverage.Locations.Values[0] = new NetworkLocation(branch, 60);

            //check the function got the message
            Assert.AreEqual(new[] {60.0}, function.Arguments[0].Values); //offset got updated.
            Assert.AreEqual(5.0d, function[60.0d, 1.0]); //but the value remains

            //changing the branch should also work
            var secondBranch = network.Branches.Skip(1).First();
            section.RoughnessNetworkCoverage.Locations.Values[0] = new NetworkLocation(secondBranch, 60);

            Assert.AreEqual(0, function.Arguments[0].Values.Count);
        }

        [Test]
        public void MoveLocationToOtherBranch()
        {
            //relates to 5109. If the table is used to alter the chainage of a location, the location is replaced.
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(2);
            var crossSectionSectionType = new CrossSectionSectionType {Name = "main"};
            var section = new RoughnessSection(crossSectionSectionType, network);
            var first = network.Branches.First();
            var secondBranch= network.Branches.Skip(1).First();
            //make both q depent
            var firstFunction= section.AddQRoughnessFunctionToBranch(first);
            var secondFunction= section.AddQRoughnessFunctionToBranch(secondBranch);

            //add a location to the first branch
            section.RoughnessNetworkCoverage[new NetworkLocation(first, 50)] = new object[]
                                                                                    {
                                                                                        100.0,
                                                                                        RoughnessType.StricklerKn
                                                                                    };

            //move the location to the second branch
            Assert.AreEqual(1,firstFunction.Arguments[0].Values.Count);

            var coverage = section.RoughnessNetworkCoverage;
            coverage.Locations.Values[0] = new NetworkLocation(secondBranch, 50);

            //move the location to the second branch
            Assert.AreEqual(0, firstFunction.Arguments[0].Values.Count);
            Assert.AreEqual(1, secondFunction.Arguments[0].Values.Count);
        }

        [Test]
        public void UpdateCoverageForFunctionValidatesLocationChainage()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1);
            var crossSectionSectionType = new CrossSectionSectionType { Name = "main" };
            var roughnessSection = new RoughnessSection(crossSectionSectionType, network);
            var roughnessFunction = RoughnessBranchDataMerger.DefineConstantFunction();

            roughnessFunction[10.0] = 10.0;
            roughnessFunction[99.0] = 99.0;
            roughnessFunction[10000.0] = 10000.0;

            TestHelper.AssertLogMessageIsGenerated(() => roughnessSection.UpdateCoverageForFunction(network.Branches.First(), roughnessFunction, RoughnessType.Chezy), "Invalid chainage '10000' for branch 'branch1'; skipped.");
            Assert.AreEqual(2, roughnessSection.RoughnessNetworkCoverage.Arguments[0].Values.Count);
            Assert.AreEqual(10.0, ((NetworkLocation) roughnessSection.RoughnessNetworkCoverage.Arguments[0].Values[0]).Chainage);
            Assert.AreEqual(99.0, ((NetworkLocation) roughnessSection.RoughnessNetworkCoverage.Arguments[0].Values[1]).Chainage);
        }

        [Test]
        [Category(TestCategory.UndoRedo)]
        public void UndoRedoChangeBranchFunction()
        {
            var node1 = new HydroNode("1");
            var node2 = new HydroNode("2");
            var branch = new Channel(node1, node2);
            var network = new Network();
            network.Nodes.AddRange(new[]{node1, node2});
            network.Branches.Add(branch);

            var roughnessSection = new RoughnessSection(new CrossSectionSectionType(), network);

            using (var undoManager = new UndoRedoManager(roughnessSection))
            {
                roughnessSection.ChangeBranchFunction(branch, RoughnessFunction.FunctionOfH);

                Assert.AreEqual(1, undoManager.UndoStack.Count(), "#undo");
                Assert.AreEqual(RoughnessFunction.FunctionOfH, roughnessSection.GetRoughnessFunctionType(branch));

                undoManager.Undo();

                Assert.AreEqual(RoughnessFunction.Constant, roughnessSection.GetRoughnessFunctionType(branch));
                Assert.AreEqual(1, undoManager.RedoStack.Count(), "#redo");

                undoManager.Redo();

                Assert.AreEqual(1, undoManager.UndoStack.Count(), "#undo");
                Assert.AreEqual(RoughnessFunction.FunctionOfH, roughnessSection.GetRoughnessFunctionType(branch));
            }
        }

        [Test]
        public void GetFirstRoughnessSectionByNameFromRoughnessSectionExtensionsTest()
        {
            var node1 = new HydroNode("1");
            var node2 = new HydroNode("2");
            var branch = new Channel(node1, node2);
            var network = new Network();
            network.Nodes.AddRange(new[] { node1, node2 });
            network.Branches.Add(branch);

            var roughnessSection1 = new RoughnessSection(new CrossSectionSectionType { Name = RoughnessDataSet.MainSectionTypeName }, network);
            var roughnessSection2 = new RoughnessSection(new CrossSectionSectionType { Name = RoughnessDataSet.Floodplain1SectionTypeName }, network);
            var roughnessSection3 = new RoughnessSection(new CrossSectionSectionType { Name = RoughnessDataSet.Floodplain2SectionTypeName }, network);

            var roughnessSections = new List<RoughnessSection> {roughnessSection1, roughnessSection2, roughnessSection3};
            Assert.AreEqual(roughnessSection1, roughnessSections.GetMainRoughnessSection());
            Assert.AreEqual(roughnessSection2, roughnessSections.GetFloodplain1());
            Assert.AreEqual(roughnessSection3, roughnessSections.GetFloodplain2());
            Assert.AreEqual(roughnessSection1, roughnessSections.GetApplicableReverseRoughnessSection(roughnessSection1));
        }

        #region ReverseRoughnessSection

        [Test]
        public void ReverseRoughnessGoesBackToChezyIfNormalSectionHasNoRoughnessDefined()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(2);
            var branch1 = network.Branches[0];
            var crossSectionSectionType = new CrossSectionSectionType { Name = "main" };
            var section = new RoughnessSection(crossSectionSectionType, network);
            var reverseSection = new ReverseRoughnessSection(section);

            reverseSection.UseNormalRoughness = false;
            
            var locationOne = new NetworkLocation(branch1, 0);
            reverseSection.RoughnessNetworkCoverage[locationOne] = new object[] { 70.0, RoughnessType.Chezy }; // initialize to chezy
            section.RoughnessNetworkCoverage[locationOne] = new object[] { 70.0, RoughnessType.DeBosAndBijkerk };
            reverseSection.RoughnessNetworkCoverage[new NetworkLocation(branch1, 50)] = new object[] { 100.0, RoughnessType.Chezy }; //not possible, changed to DeBosAndBijkerk

            Assert.AreEqual(RoughnessType.DeBosAndBijkerk, reverseSection.RoughnessNetworkCoverage.EvaluateRoughnessType(locationOne));

            section.RoughnessNetworkCoverage.Locations.Values.Remove(locationOne);

            Assert.AreEqual(RoughnessType.Chezy, reverseSection.RoughnessNetworkCoverage.EvaluateRoughnessType(locationOne));
        }

        [Test, Category(TestCategory.Performance)]
        public void ReverseRoughnessGoesBackToChezyEfficientlyIfNormalSectionHasNoRoughnessDefined()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(2);
            var branch1 = network.Branches[0];
            var crossSectionSectionType = new CrossSectionSectionType {Name = "main"};
            var section = new RoughnessSection(crossSectionSectionType, network);
            var reverseSection = new ReverseRoughnessSection(section) {UseNormalRoughness = false};

            section.RoughnessNetworkCoverage.SkipInterpolationForNewLocation = true;
            reverseSection.RoughnessNetworkCoverage.SkipInterpolationForNewLocation = true;

            // initialize to chezy
            reverseSection.RoughnessNetworkCoverage[new NetworkLocation(branch1, 0.0)] = new object[] {70.0, RoughnessType.Chezy};

            const int numLocations = 10000;
            var factor = numLocations/branch1.Length;
            
            for (int i = 0; i < numLocations; i++)
            {
                var loc = new NetworkLocation(branch1, i * factor);
                section.RoughnessNetworkCoverage[loc] = new object[] {70.0, RoughnessType.DeBosAndBijkerk};
            }

            // expect it was changed to match normal branch
            Assert.AreEqual(RoughnessType.DeBosAndBijkerk,
                reverseSection.RoughnessNetworkCoverage.EvaluateRoughnessType(new NetworkLocation(branch1, 0.0)));

            TestHelper.AssertIsFasterThan(1000, () =>
            {
                section.RoughnessNetworkCoverage.BeginEdit(new DefaultEditAction("Removing branch"));
                for (int i = 0; i < numLocations; i++)
                {
                    var loc = new NetworkLocation(branch1, i * factor);
                    section.RoughnessNetworkCoverage.Locations.Values.Remove(loc);
                }
                section.RoughnessNetworkCoverage.EndEdit();

                // expect it was reset back to chezy
                Assert.AreEqual(RoughnessType.Chezy,
                    reverseSection.RoughnessNetworkCoverage.EvaluateRoughnessType(new NetworkLocation(branch1, 0.0)));
            });
        }

        [Test]
        public void ReverseRoughnessSwitchesWithDefaultOfNormalSection()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(2);
            var branch1 = network.Branches[0];
            var crossSectionSectionType = new CrossSectionSectionType { Name = "main" };
            var section = new RoughnessSection(crossSectionSectionType, network);
            var reverseSection = new ReverseRoughnessSection(section);

            reverseSection.UseNormalRoughness = false;

            var locationOne = new NetworkLocation(branch1, 0);
            reverseSection.RoughnessNetworkCoverage[new NetworkLocation(branch1, 50)] = new object[] { 100.0, RoughnessType.Chezy };

            Assert.AreEqual(RoughnessType.Chezy, reverseSection.RoughnessNetworkCoverage.EvaluateRoughnessType(locationOne));

            section.SetDefaults(RoughnessType.DeBosAndBijkerk, 10.0);

            Assert.AreEqual(RoughnessType.DeBosAndBijkerk, reverseSection.RoughnessNetworkCoverage.EvaluateRoughnessType(locationOne));
            Assert.AreEqual(reverseSection.GetDefaultRoughnessType(), section.GetDefaultRoughnessType());
            Assert.AreEqual(reverseSection.GetDefaultRoughnessValue(), section.GetDefaultRoughnessValue());
        }

        [Test]
        public void ReverseRoughnessUsesDefaultOfNormalSectionOnAdd()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(2);
            var branch1 = network.Branches[0];
            var crossSectionSectionType = new CrossSectionSectionType { Name = "main" };
            var section = new RoughnessSection(crossSectionSectionType, network);
            var reverseSection = new ReverseRoughnessSection(section);

            reverseSection.UseNormalRoughness = false;

            section.SetDefaults(RoughnessType.StricklerKs, 0.05);

            var locationOne = new NetworkLocation(branch1, 0);
            reverseSection.RoughnessNetworkCoverage[new NetworkLocation(branch1, 50)] = new object[] { 100.0, RoughnessType.DeBosAndBijkerk };

            Assert.AreEqual(RoughnessType.StricklerKs, reverseSection.RoughnessNetworkCoverage.EvaluateRoughnessType(locationOne));
        }

        [Test]
        public void ReverseRoughnessSectionSetDefaultsTest()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(2);
            var reverseSection = new ReverseRoughnessSection()
            {
                NormalSection = new RoughnessSection(new CrossSectionSectionType(), network)
            };
            reverseSection.SetDefaults(RoughnessType.Manning, 4.0);

            //If we set the defaults but then set the UseNormalRoughness to True set defauls will set it to false.
            reverseSection.UseNormalRoughness = true;
            reverseSection.SetDefaults(RoughnessType.Manning, 4.0);
            Assert.IsFalse(reverseSection.UseNormalRoughness);
            
            //However, if the normal section shares the values of the reverse roughness section, the flag will still stay as True
            reverseSection.NormalSection.SetDefaults(RoughnessType.Manning, 4.0);
            reverseSection.UseNormalRoughness = true;
            reverseSection.SetDefaults(RoughnessType.Manning, 4.0);
            Assert.IsTrue(reverseSection.UseNormalRoughness);
            Assert.AreEqual(reverseSection.GetDefaultRoughnessType(), reverseSection.NormalSection.GetDefaultRoughnessType());
            Assert.AreEqual(reverseSection.GetDefaultRoughnessValue(), reverseSection.NormalSection.GetDefaultRoughnessValue());
        }

        [Test]
        public void EvaluateRoughnessValueTest()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(2);
            var reverseSection = new ReverseRoughnessSection()
            {
                NormalSection = new RoughnessSection(new CrossSectionSectionType(), network)
            };
            reverseSection.SetDefaults(RoughnessType.Manning, 4.0);
            var locationA = new NetworkLocation(network.Branches.First(), 10);
            var firstValue = reverseSection.EvaluateRoughnessValue(locationA);
            Assert.NotNull(firstValue);
            Assert.LessOrEqual(0, firstValue);

            //With values in the network coverage the result should differ
            reverseSection.RoughnessNetworkCoverage[locationA] = new object[]
            {
                100.0,
                RoughnessType.StricklerKn
            };
            var secondValue = reverseSection.EvaluateRoughnessValue(locationA);
            Assert.NotNull(secondValue);
            Assert.AreEqual(100.0, secondValue);
            Assert.AreNotEqual(firstValue, secondValue);
        }

        [Test]
        public void GetFunctionOfHAndGetFunctionOfQForRoughnessSectionTest()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(2);
            var reverseSection = new ReverseRoughnessSection()
            {
                NormalSection = new RoughnessSection(new CrossSectionSectionType(), network)
            };

            var branch = network.Branches.First();
            reverseSection.NormalSection.AddHRoughnessFunctionToBranch(branch, new Function("TestFunction"));
            reverseSection.NormalSection.AddQRoughnessFunctionToBranch(branch, new Function("TestFunction2"));
            var locationA = new NetworkLocation(branch, 10);
            var firstFunctionOfH = reverseSection.FunctionOfH(branch);
            var firstFunctionOfQ = reverseSection.FunctionOfQ(branch);
            Assert.NotNull(firstFunctionOfH);
            Assert.NotNull(firstFunctionOfQ);

            //With values in the network coverage the result should differ
            reverseSection.RoughnessNetworkCoverage[locationA] = new object[]
            {
                100.0,
                RoughnessType.StricklerKn
            };
            reverseSection.AddHRoughnessFunctionToBranch(branch, new Function("TestFunction"));
            reverseSection.AddQRoughnessFunctionToBranch(branch, new Function("TestFunction2"));

            var secondFunctionOfH = reverseSection.FunctionOfH(branch);
            var secondFunctionOfQ = reverseSection.FunctionOfQ(branch);
            Assert.NotNull(secondFunctionOfH);
            Assert.NotNull(secondFunctionOfQ);

            Assert.AreNotEqual(firstFunctionOfH, secondFunctionOfH);
            Assert.AreNotEqual(firstFunctionOfQ, secondFunctionOfQ);
        }

        #endregion
    }
}