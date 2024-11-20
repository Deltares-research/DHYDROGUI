using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.IntegrationTests
{
    //TODO: class has very large story like tests..split up and make more small tests
    [TestFixture]
    public class HydroNetworkCopyAndPasteHelperTest
    {
        private ClipboardMock clipboard;

        [SetUp]
        public void Setup()
        {
            if (!GuiTestHelper.IsBuildServer) return;
            clipboard = new ClipboardMock();
            clipboard.GetText_Returns_SetText();
            clipboard.GetData_Returns_SetData();
        }

        [TearDown]
        public void TearDown()
        {
            HydroNetworkCopyAndPasteHelper.ReleaseCopiedNetworkFeature();
            if (!GuiTestHelper.IsBuildServer) return;
            clipboard.Dispose();
        }

        [Test]
        public void SetAndGetClipBoardChannel()
        {
            var channel = new Channel();

            HydroNetworkCopyAndPasteHelper.SetNetworkFeatureToClipBoard(channel);
            Assert.IsTrue(HydroNetworkCopyAndPasteHelper.IsChannelSetToClipBoard());
            Assert.AreSame(channel, HydroNetworkCopyAndPasteHelper.GetChannelFromClipBoard());
            
            HydroNetworkCopyAndPasteHelper.ReleaseCopiedNetworkFeature();
            Assert.IsFalse(HydroNetworkCopyAndPasteHelper.IsChannelSetToClipBoard());
            Assert.IsNull(HydroNetworkCopyAndPasteHelper.GetChannelFromClipBoard());
        }

        [Test]
        public void SetAndGetClipBoardBranchFeature()
        {
            var pump = new Pump();

            HydroNetworkCopyAndPasteHelper.SetNetworkFeatureToClipBoard(pump);
            Assert.IsTrue(HydroNetworkCopyAndPasteHelper.IsBranchFeatureSetToClipBoard());
            Assert.AreSame(pump, HydroNetworkCopyAndPasteHelper.GetBranchFeatureFromClipBoard());

            HydroNetworkCopyAndPasteHelper.ReleaseCopiedNetworkFeature();
            Assert.IsFalse(HydroNetworkCopyAndPasteHelper.IsBranchFeatureSetToClipBoard());
            Assert.IsNull(HydroNetworkCopyAndPasteHelper.GetChannelFromClipBoard());
        }

        [Test]
        public void IsCrossSectionPastableInNetwork()
        {
            var crossSectionType1 = new CrossSectionSectionType { Name = "crossSectionType1" };
            var crossSectionType2 = new CrossSectionSectionType { Name = "crossSectionType2" };
            var crossSectionType3 = new CrossSectionSectionType { Name = "crossSectionType3" };
            var crossSectionType4 = new CrossSectionSectionType { Name = "crossSectionType3" }; // Same name as crossSectionType3

            var section1 = new CrossSectionSection { SectionType = crossSectionType1 };
            var section2 = new CrossSectionSection { SectionType = crossSectionType2 };
            var section3 = new CrossSectionSection { SectionType = crossSectionType2 };
            var section4 = new CrossSectionSection { SectionType = crossSectionType3 };

            var network = new HydroNetwork();
            var crossSectionDefinition = new CrossSectionDefinitionYZ
                                   {
                                       Name = "crossSection"
                                   };
            crossSectionDefinition.Sections.AddRange(new[] {section1, section2, section3, section4});

            string errorMessage;
            var crossSection = new CrossSection(crossSectionDefinition);
            Assert.IsFalse(HydroNetworkCopyAndPasteHelper.IsCrossSectionPastableInNetwork(network, crossSection, out errorMessage));
            Assert.AreEqual("Cannot paste the cross section with name \"crossSection\" in this network because SectionTypes with the following name are missing: crossSectionType1, crossSectionType2, crossSectionType3", errorMessage);

            network.CrossSectionSectionTypes.Add(crossSectionType1);
            network.CrossSectionSectionTypes.Add(crossSectionType2);

            Assert.IsFalse(HydroNetworkCopyAndPasteHelper.IsCrossSectionPastableInNetwork(network, crossSection, out errorMessage));
            Assert.AreEqual("Cannot paste the cross section with name \"crossSection\" in this network because a SectionType with the following name is missing: crossSectionType3", errorMessage);

            network.CrossSectionSectionTypes.Add(crossSectionType4);

            Assert.IsTrue(HydroNetworkCopyAndPasteHelper.IsCrossSectionPastableInNetwork(network, crossSection, out errorMessage));
            Assert.AreEqual("", errorMessage);
        }

        [Test]
        public void AdaptCrossSectionBeforePastingInNetwork()
        {
            var crossSectionType1 = new CrossSectionSectionType { Name = "crossSectionType1" };
            var crossSectionType2 = new CrossSectionSectionType { Name = "crossSectionType2" };
            var crossSectionType3 = new CrossSectionSectionType { Name = "crossSectionType2" }; // Same name as crossSectionType2

            var section1 = new CrossSectionSection { SectionType = crossSectionType3 };
            var section2 = new CrossSectionSection { SectionType = crossSectionType3 };

            var network = new HydroNetwork();
            ICrossSectionDefinition crossSectionDefinition = new CrossSectionDefinitionXYZ();
            crossSectionDefinition.Sections.Add(section1);
            crossSectionDefinition.Sections.Add(section2);

            network.CrossSectionSectionTypes.Add(crossSectionType1);
            var crossSection = new CrossSection(crossSectionDefinition);
            Assert.IsFalse(HydroNetworkCopyAndPasteHelper.AdaptCrossSectionBeforePastingInNetwork(network, crossSection));

            network.CrossSectionSectionTypes.Add(crossSectionType2);
            Assert.IsTrue(HydroNetworkCopyAndPasteHelper.AdaptCrossSectionBeforePastingInNetwork(network, crossSection));
            Assert.AreSame(crossSectionType2, crossSectionDefinition.Sections[0].SectionType);
            Assert.AreSame(crossSectionType2, crossSectionDefinition.Sections[1].SectionType);
        }

        [Test]
        public void PasteChannelToSameNetwork()
        {
            string errorMessage;

            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(200, 0));

            var channel = network.Channels.First();

            var pump = new Pump(false);
            BranchStructure.AddStructureToNetwork(pump, channel);

            channel.BranchFeatures.Add(pump);
            channel.BranchFeatures.Add(CrossSection.CreateDefault(CrossSectionType.YZ, channel));

            Assert.IsFalse(HydroNetworkCopyAndPasteHelper.PasteChannelToNetwork(network, out errorMessage));
            Assert.AreEqual("No branch is copied", errorMessage);

            HydroNetworkCopyAndPasteHelper.SetNetworkFeatureToClipBoard(channel);
            Assert.IsTrue(HydroNetworkCopyAndPasteHelper.PasteChannelToNetwork(network, out errorMessage));
            
            // Check the number of features in the network
            Assert.AreEqual(2, network.Branches.Count);
            Assert.AreEqual(4, network.Nodes.Count);
            Assert.AreEqual(2, network.Pumps.Count());
            Assert.AreEqual(2, network.CrossSections.Count());
            
            // Check the branch references of the pasted branch features
            var pastedChannel = network.Branches[1];
            Assert.AreSame(pastedChannel, network.Pumps.ElementAt(1).Branch);
            Assert.AreSame(pastedChannel, network.CrossSections.ElementAt(1).Branch);

            // Check the network references of the pasted channel and branch features
            Assert.AreSame(network, network.Branches.ElementAt(1).Network);
            Assert.AreSame(network, network.Pumps.ElementAt(1).Network);
            Assert.AreSame(network, network.CrossSections.ElementAt(1).Network);

            // Check the uniqueness of name of the pasted channel and branch features
            Assert.AreNotEqual(network.Branches.ElementAt(0).Name, network.Branches.ElementAt(1).Name);
            Assert.AreNotEqual(network.Pumps.ElementAt(0).Name, network.Pumps.ElementAt(1).Name);
            Assert.AreNotEqual(network.CrossSections.ElementAt(0).Name, network.CrossSections.ElementAt(1).Name);

            // Check that channel has moved by 100 (no units). 
            Assert.AreEqual(channel.Geometry.Coordinates[0].X + 100.0d, pastedChannel.Geometry.Coordinates[0].X);
            Assert.AreEqual(channel.Geometry.Coordinates[0].Y + 100.0d, pastedChannel.Geometry.Coordinates[0].Y);
        }

        [Test]
        public void PasteChannelToOtherNetwork()
        {
            string errorMessage;
            
            var network1 = new HydroNetwork();
            var network2 = new HydroNetwork();
            
            var channel = new Channel
                              {
                                  Name = "channel1",
                                  Geometry = new LineString(new[]
                                                                {
                                                                    new Coordinate(0, 0),
                                                                    new Coordinate(200, 0)
                                                                })
                              };

            network1.CrossSectionSectionTypes[0].Name = "CrossSectionType1";
            network2.CrossSectionSectionTypes[0].Name = "CrossSectionType2";
            NetworkHelper.AddChannelToHydroNetwork(network1, channel);
            
            var pump = new Pump(false);
            BranchStructure.AddStructureToNetwork(pump, channel);

            channel.BranchFeatures.Add(pump);
            var crossSectionYz = CrossSection.CreateDefault(CrossSectionType.YZ, channel);
            

            channel.BranchFeatures.Add(crossSectionYz);
            network1.CrossSections.ElementAt(0).Definition.Sections.Add(new CrossSectionSection { SectionType = network1.CrossSectionSectionTypes[0] });

            Assert.IsFalse(HydroNetworkCopyAndPasteHelper.PasteChannelToNetwork(network2, out errorMessage));
            Assert.AreEqual("No branch is copied", errorMessage);

            HydroNetworkCopyAndPasteHelper.SetNetworkFeatureToClipBoard(channel);

            Assert.IsFalse(HydroNetworkCopyAndPasteHelper.PasteChannelToNetwork(network2, out errorMessage));
            Assert.AreEqual("Cannot paste the channel because one of its features cannot be pasted:\n\nCannot paste the cross section with name \"CrossSection_1D_1\" in this network because a SectionType with the following name is missing: CrossSectionType1", errorMessage);

            network2.CrossSectionSectionTypes[0].Name = "CrossSectionType1";
            Assert.IsTrue(HydroNetworkCopyAndPasteHelper.PasteChannelToNetwork(network2, out errorMessage));

            // Check the number of features in the network
            Assert.AreEqual(1, network2.Branches.Count);
            Assert.AreEqual(2, network2.Nodes.Count);
            Assert.AreEqual(1, network2.Pumps.Count());
            Assert.AreEqual(1, network2.CrossSections.Count());
            Assert.AreEqual(1, network2.CrossSections.ElementAt(0).Definition.Sections.Count);

            // Check the branch references of the pasted branch features
            var pastedChannel = network2.Branches[0];
            Assert.AreSame(pastedChannel, network2.Pumps.ElementAt(0).Branch);
            Assert.AreSame(pastedChannel, network2.CrossSections.ElementAt(0).Branch);

            // Check the network references of the pasted channel and branch features
            Assert.AreSame(network2, network2.Branches.ElementAt(0).Network);
            Assert.AreSame(network2, network2.Pumps.ElementAt(0).Network);
            Assert.AreSame(network2, network2.CrossSections.ElementAt(0).Network);

            // Check if the cross section is adapted
            Assert.AreSame(network2.CrossSectionSectionTypes[0], network2.CrossSections.ElementAt(0).Definition.Sections[0].SectionType);

            // Check that channel has not moved. 
            Assert.AreEqual(channel.Geometry.Coordinates[0].X, pastedChannel.Geometry.Coordinates[0].X);
            Assert.AreEqual(channel.Geometry.Coordinates[0].Y, pastedChannel.Geometry.Coordinates[0].Y);
        }

        [Test]
        public void PasteBranchFeatureToBranchInSameNetwork()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(100, 0), new Point(100, 100));
            var channel1 = network.Channels.First();
            var channel2 = network.Channels.ElementAt(1);

            var pump = new Pump(false);
            BranchStructure.AddStructureToNetwork(pump, channel1);

            channel1.BranchFeatures.Add(pump);
            channel1.BranchFeatures.Add(CrossSection.CreateDefault(CrossSectionType.YZ, channel1));

            HydroNetworkCopyAndPasteHelper.SetNetworkFeatureToClipBoard(network.Pumps.ElementAt(0));
            Assert.IsTrue(HydroNetworkCopyAndPasteHelper.PasteBranchFeatureFromClipboardToBranch(channel1, 10, out string _));

            HydroNetworkCopyAndPasteHelper.SetNetworkFeatureToClipBoard(network.CrossSections.ElementAt(0));
            Assert.IsTrue(HydroNetworkCopyAndPasteHelper.PasteBranchFeatureFromClipboardToBranch(channel2, 10, out string _));

            // Check the number of features in the network
            Assert.AreEqual(2, network.Pumps.Count());
            Assert.AreEqual(2, network.CrossSections.Count());

            // Check the branch references of the pasted branch features
            Assert.AreSame(network.Branches[0], network.Pumps.ElementAt(1).Branch);
            Assert.AreSame(network.Branches[1], network.CrossSections.ElementAt(1).Branch);

            // Check the network references of the pasted channel and branch features
            Assert.AreSame(network, network.Pumps.ElementAt(1).Network);
            Assert.AreSame(network, network.CrossSections.ElementAt(1).Network);

            // Check the uniqueness of name of the pasted branch features
            Assert.AreNotEqual(network.Pumps.ElementAt(0).Name, network.Pumps.ElementAt(1).Name);
            Assert.AreNotEqual(network.CrossSections.ElementAt(0).Name, network.CrossSections.ElementAt(1).Name);
        }

        [Test]
        public void PasteBranchFeatureToBranchInOtherNetwork()
        {
            string errorMessage;

            var sourceNetwork = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(200, 0));
            var sourceChannel = sourceNetwork.Channels.First();
            
            var targetNetwork = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(200, 0));
            var targetChannel = targetNetwork.Channels.First();
            
            sourceNetwork.CrossSectionSectionTypes[0].Name = "CrossSectionType1";
            targetNetwork.CrossSectionSectionTypes[0].Name = "CrossSectionType2";

            var pump = new Pump(false);
            BranchStructure.AddStructureToNetwork(pump, sourceChannel);

            sourceChannel.BranchFeatures.Add(pump);
            var sourceCrossSection = CrossSection.CreateDefault(CrossSectionType.YZ, sourceChannel);
            sourceCrossSection.Name = "SourceCrossSection";
            sourceCrossSection.Definition.Name = "SourceCrossSection";
            sourceChannel.BranchFeatures.Add(sourceCrossSection);

            sourceNetwork.CrossSections.ElementAt(0).Definition.Sections.Add(new CrossSectionSection { SectionType = sourceNetwork.CrossSectionSectionTypes[0] });

            Assert.IsFalse(HydroNetworkCopyAndPasteHelper.PasteBranchFeatureFromClipboardToBranch(targetChannel, 10, out errorMessage));
            Assert.AreEqual("No branch feature in clipboard", errorMessage);

            HydroNetworkCopyAndPasteHelper.SetNetworkFeatureToClipBoard(sourceNetwork.Pumps.ElementAt(0));
            Assert.IsTrue(HydroNetworkCopyAndPasteHelper.PasteBranchFeatureFromClipboardToBranch(targetChannel, 10, out errorMessage));

            HydroNetworkCopyAndPasteHelper.SetNetworkFeatureToClipBoard(sourceNetwork.CrossSections.ElementAt(0));
            Assert.IsFalse(HydroNetworkCopyAndPasteHelper.PasteBranchFeatureFromClipboardToBranch(targetChannel, 10, out errorMessage));
            Assert.AreEqual("Cannot paste the cross section with name \"CrossSection_1D_1\" in this network because a SectionType with the following name is missing: CrossSectionType1", errorMessage);

            targetNetwork.CrossSectionSectionTypes[0].Name = "CrossSectionType1";
            Assert.IsTrue(HydroNetworkCopyAndPasteHelper.PasteBranchFeatureFromClipboardToBranch(targetChannel, 10, out errorMessage));
            
            // Check the number of features in the network
            Assert.AreEqual(1, targetNetwork.Pumps.Count());
            Assert.AreEqual(1, targetNetwork.CrossSections.Count());

            // Check the branch references of the pasted branch features
            Assert.AreSame(targetNetwork.Branches[0], targetNetwork.Pumps.ElementAt(0).Branch);
            Assert.AreSame(targetNetwork.Branches[0], targetNetwork.CrossSections.ElementAt(0).Branch);

            // Check the network references of the pasted channel and branch features
            Assert.AreSame(targetNetwork, targetNetwork.Pumps.ElementAt(0).Network);
            Assert.AreSame(targetNetwork, targetNetwork.CrossSections.ElementAt(0).Network);

            // Check if the cross section is adapted
            Assert.AreSame(targetNetwork.CrossSectionSectionTypes[0], targetNetwork.CrossSections.ElementAt(0).Definition.Sections[0].SectionType);
        }

        [Test]
        public void PasteBranchFeatureIntoBranchFeatureInSameNetwork()
        {
            string errorMessage;

            var network = new HydroNetwork();

            var channel = new Channel
                              {
                                  Name = "channel1",
                                  Geometry = new LineString(new[]
                                                                {
                                                                    new Coordinate(0, 0),
                                                                    new Coordinate(200, 0)
                                                                })
                              };

            NetworkHelper.AddChannelToHydroNetwork(network, channel);
            var pump1 = new Pump(false);
            BranchStructure.AddStructureToNetwork(pump1, channel);
            var pump2 = new Pump(false);
            BranchStructure.AddStructureToNetwork(pump2, channel);

            channel.BranchFeatures.Add(pump1);
            channel.BranchFeatures.Add(pump2);
            channel.BranchFeatures.Add(CrossSection.CreateDefault(CrossSectionType.YZ, channel));
            channel.BranchFeatures.Add(CrossSection.CreateDefault(CrossSectionType.YZ, channel));

            Assert.IsFalse(HydroNetworkCopyAndPasteHelper.PasteBranchFeatureIntoBranchFeature(network.Pumps.ElementAt(1), out errorMessage));
            Assert.AreEqual("No branch feature is copied", errorMessage);

            HydroNetworkCopyAndPasteHelper.SetNetworkFeatureToClipBoard(network.Pumps.ElementAt(0));

            Assert.IsFalse(HydroNetworkCopyAndPasteHelper.PasteBranchFeatureIntoBranchFeature(network.CrossSections.ElementAt(1), out errorMessage));
            Assert.AreEqual("The copied branch feature is of a different type as the target branch feature", errorMessage);

            Assert.IsTrue(HydroNetworkCopyAndPasteHelper.PasteBranchFeatureIntoBranchFeature(network.Pumps.ElementAt(1), out errorMessage));

            HydroNetworkCopyAndPasteHelper.SetNetworkFeatureToClipBoard(network.CrossSections.ElementAt(0));
            Assert.IsTrue(HydroNetworkCopyAndPasteHelper.PasteBranchFeatureIntoBranchFeature(network.CrossSections.ElementAt(1), out errorMessage));

            // Check the number of features in the network
            Assert.AreEqual(2, network.Pumps.Count());
            Assert.AreEqual(2, network.CrossSections.Count());
        }

        [Test]
        public void PasteBranchFeatureIntoBranchFeatureInOtherNetwork()
        {
            string errorMessage;

            var network1 = new HydroNetwork();
            var targetNetwork = new HydroNetwork();

            var channel1 = new Channel
                               {
                                   Name = "channel1",
                                   Geometry = new LineString(new[]
                                                                 {
                                                                     new Coordinate(0, 0),
                                                                     new Coordinate(200, 0)
                                                                 })
                               };

            var channel2 = new Channel
                               {
                                   Name = "channel2",
                                   Geometry = new LineString(new[]
                                                                 {
                                                                     new Coordinate(0, 0),
                                                                     new Coordinate(200, 0)
                                                                 })
                               };

            network1.CrossSectionSectionTypes[0].Name = "CrossSectionType1";
            targetNetwork.CrossSectionSectionTypes[0].Name = "CrossSectionType2";
            NetworkHelper.AddChannelToHydroNetwork(network1, channel1);
            NetworkHelper.AddChannelToHydroNetwork(targetNetwork, channel2);

            var pump1 = new Pump(false);
            BranchStructure.AddStructureToNetwork(pump1, channel1);

            channel1.BranchFeatures.Add(pump1);
            var crossSectionYz = CrossSection.CreateDefault(CrossSectionType.YZ, channel1);
            crossSectionYz.Name = "CrossSection001";
            crossSectionYz.Definition.Name = "CrossSection001";
            channel1.BranchFeatures.Add(crossSectionYz);

            var pump2 = new Pump(false);
            BranchStructure.AddStructureToNetwork(pump2, channel2);

            channel2.BranchFeatures.Add(pump2);
            var crossSection2 = CrossSection.CreateDefault(CrossSectionType.YZ, channel2);
            crossSection2.Name = "CrossSection002";
            channel2.BranchFeatures.Add(crossSection2);
            var sourceCrossSection = network1.CrossSections.ElementAt(0);

            sourceCrossSection.Definition.Sections.Add(new CrossSectionSection { SectionType = network1.CrossSectionSectionTypes[0] });

            Assert.IsFalse(HydroNetworkCopyAndPasteHelper.PasteBranchFeatureIntoBranchFeature(targetNetwork.Pumps.ElementAt(0), out errorMessage));
            Assert.AreEqual("No branch feature is copied", errorMessage);

            HydroNetworkCopyAndPasteHelper.SetNetworkFeatureToClipBoard(network1.Pumps.ElementAt(0));

            var targetCrossSection = targetNetwork.CrossSections.ElementAt(0);

            Assert.IsFalse(HydroNetworkCopyAndPasteHelper.PasteBranchFeatureIntoBranchFeature(targetCrossSection, out errorMessage));
            Assert.AreEqual("The copied branch feature is of a different type as the target branch feature", errorMessage);

            Assert.IsTrue(HydroNetworkCopyAndPasteHelper.PasteBranchFeatureIntoBranchFeature(targetNetwork.Pumps.ElementAt(0), out errorMessage));

            HydroNetworkCopyAndPasteHelper.SetNetworkFeatureToClipBoard(sourceCrossSection);

            Assert.IsFalse(HydroNetworkCopyAndPasteHelper.PasteBranchFeatureIntoBranchFeature(targetCrossSection, out errorMessage));
            Assert.AreEqual("Cannot paste the cross section with name \"CrossSection001\" in this network because a SectionType with the following name is missing: CrossSectionType1", errorMessage);

            targetNetwork.CrossSectionSectionTypes[0].Name = "CrossSectionType1";
            Assert.IsTrue(HydroNetworkCopyAndPasteHelper.PasteBranchFeatureIntoBranchFeature(targetCrossSection, out errorMessage));

            // Check the number of features in the network
            Assert.AreEqual(1, targetNetwork.Pumps.Count());
            Assert.AreEqual(1, targetNetwork.CrossSections.Count());

            // Check if the cross section is adapted
            Assert.AreSame(targetNetwork.CrossSectionSectionTypes[0], targetCrossSection.Definition.Sections[0].SectionType);
        }

        [Test]
        public void PasteProxiedCrossSection()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(200, 0));
            var channel = network.Channels.First();

            var crossSectionDefinitionYZ = CrossSectionDefinitionYZ.CreateDefault();            
            crossSectionDefinitionYZ.Name = "Rijn";
            network.SharedCrossSectionDefinitions.Add(crossSectionDefinitionYZ);

            var crossSectionDefinitionProxy = new CrossSectionDefinitionProxy(crossSectionDefinitionYZ);
            var crossSection = new CrossSection(crossSectionDefinitionProxy) {Name = "kees"};
            
            NetworkHelper.AddBranchFeatureToBranch(crossSection,channel, 10);

            HydroNetworkCopyAndPasteHelper.PastBranchFeatureToBranch(channel, crossSection, 10, out string _);

            Assert.AreEqual(2, channel.CrossSections.Count());
            var copy = channel.CrossSections.First(c => c.Name != "kees");

            Assert.AreEqual("CrossSection_1D_1", copy.Name);
            
            Assert.IsTrue(copy.Definition.IsProxy);
            var copiedProxy = (CrossSectionDefinitionProxy) copy.Definition;
            Assert.AreEqual(crossSectionDefinitionYZ, copiedProxy.InnerDefinition);

        }

        [Test]
        public void CanNotPasteProxyCrossSectionsFromOtherNetwork()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(200, 0));
            var channel = network.Channels.First();

            var crossSectionDefinitionYZ = CrossSectionDefinitionYZ.CreateDefault();
            var crossSectionDefinitionProxy = new CrossSectionDefinitionProxy(crossSectionDefinitionYZ);

            var crossSection = new CrossSection(crossSectionDefinitionProxy) { Name = "kees" };
            //this sets the network of the crossection
            NetworkHelper.AddBranchFeatureToBranch(crossSection, channel, 10);

            var hydroNetwork = new HydroNetwork();
            string message = "";
            Assert.IsFalse(HydroNetworkCopyAndPasteHelper.IsCrossSectionPastableInNetwork(hydroNetwork,crossSection,out message));
            Assert.AreEqual("Can not paste cross section with name \"kees\" because it uses a shared definition of other network.",message);
        }

        [Test]
        public void CanNotPasteXYZCrossSections()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(200, 0));
            var channel = network.Channels.First();

            var crossSectionDefinitionXYZ = CrossSectionDefinitionXYZ.CreateDefault();


            var crossSection = new CrossSection(crossSectionDefinitionXYZ) { Name = "kees" };
            //this sets the network of the crossection
            NetworkHelper.AddBranchFeatureToBranch(crossSection, channel, 10);

            var hydroNetwork = new HydroNetwork();
            string message = "";
            Assert.IsFalse(HydroNetworkCopyAndPasteHelper.IsCrossSectionPastableInNetwork(hydroNetwork, crossSection, out message));
            Assert.AreEqual("Can not paste cross section with name \"kees\" because it is geometry based.", message);
        }
    }
}