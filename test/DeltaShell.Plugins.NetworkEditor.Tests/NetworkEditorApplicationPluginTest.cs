using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using GeoAPI.Extensions.Networks;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;

namespace DeltaShell.Plugins.NetworkEditor.Tests
{
    [TestFixture]
    public class NetworkEditorApplicationPluginTest
    {
        [Test]
        [Category(TestCategory.Integration)]
        public void AddChildRegionsWithUniqueNames()
        {
            // Create an application plugin instance
            var applicationPlugin = new NetworkEditorApplicationPlugin();

            // Obtain the data item info for creating hydro networks
            var dataItemInfo = applicationPlugin.GetDataItemInfos().First(dii => dii.ValueType == typeof(HydroNetwork));

            // Create two hydro networks based on the data item info
            var region = new HydroRegion();
            var hydroNetwork1 = (HydroNetwork) dataItemInfo.CreateData(region);
            var hydroNetwork2 = (HydroNetwork) dataItemInfo.CreateData(region);

            // The name of the second network should differ from the name of the first network
            hydroNetwork1.Name.Should().Not.Be.EqualTo(hydroNetwork2.Name);
        }

        [Test]
        public void GetDataItemsInfoHydroNetworkAdditionalOwnersCheckTest()
        {
            // Create an application plugin instance
            var applicationPlugin = new NetworkEditorApplicationPlugin();

            // Obtain the data item info for creating hydro networks
            var dataItemInfo = applicationPlugin.GetDataItemInfos().First(dii => dii.ValueType == typeof(HydroNetwork));

            var region = new HydroRegion();
            var hydroNetwork = (HydroNetwork) dataItemInfo.CreateData(region);
            Assert.IsTrue(dataItemInfo.AdditionalOwnerCheck(hydroNetwork));
            
        }

        [Test]
        public void GetDataItemsInfoHydroNetworAddExampleDataTest()
        {
            // Create an application plugin instance
            var applicationPlugin = new NetworkEditorApplicationPlugin();

            // Obtain the data item info for creating hydro networks
            var dataItemInfo = applicationPlugin.GetDataItemInfos().First(dii => dii.ValueType == typeof(HydroNetwork));

            var region = new HydroRegion();
            var hydroNetwork = (HydroNetwork)dataItemInfo.CreateData(region);
            dataItemInfo.AddExampleData(hydroNetwork);
            Assert.That(hydroNetwork.LateralSources.Count(), Is.EqualTo(1));
        }
        [Test]
        public void GetDataItemsInfoHydroNetworkCreateDataOnFolderTest()
        {
            // Create an application plugin instance
            var applicationPlugin = new NetworkEditorApplicationPlugin();

            // Obtain the data item info for creating hydro networks
            var dataItemInfo = applicationPlugin.GetDataItemInfos().First(dii => dii.ValueType == typeof(HydroNetwork));

            var folder = new Folder();
            var hydroNetwork = (HydroNetwork)dataItemInfo.CreateData(folder);
            Assert.IsTrue(hydroNetwork.Name.StartsWith(Properties.Resources.NetworkEditorApplicationPlugin_GetDataItemInfos_Network));
        }

        [Test]
        public void GetDataItemsInfoDrainageBasinCreateDataOnFolderTest()
        {
            // Create an application plugin instance
            var applicationPlugin = new NetworkEditorApplicationPlugin();

            // Obtain the data item info for creating hydro networks
            var dataItemInfo = applicationPlugin.GetDataItemInfos().First(dii => dii.ValueType == typeof(IDrainageBasin));

            var folder = new Folder();
            var basin = (IDrainageBasin)dataItemInfo.CreateData(folder);
            Assert.IsTrue(basin.Name.StartsWith(Properties.Resources.NetworkEditorApplicationPlugin_GetDataItemInfos_Basin));
        }

        [Test]
        public void GetDataItemsInfoDrainageBasinCreateDataOnHydroRegionTest()
        {
            // Create an application plugin instance
            var applicationPlugin = new NetworkEditorApplicationPlugin();

            // Obtain the data item info for creating hydro networks
            var dataItemInfo = applicationPlugin.GetDataItemInfos().First(dii => dii.ValueType == typeof(IDrainageBasin));

            var hydroRegion = new HydroRegion();
            var basin = (IDrainageBasin)dataItemInfo.CreateData(hydroRegion);
            Assert.IsTrue(basin.Name.StartsWith(Properties.Resources.NetworkEditorApplicationPlugin_GetDataItemInfos_Basin));
            Assert.That(hydroRegion.SubRegions[0], Is.EqualTo(basin));
        }

        [Test]
        public void GetDataItemsInfoDrainageNetworkAdditionalOwnersCheckTest()
        {
            // Create an application plugin instance
            var applicationPlugin = new NetworkEditorApplicationPlugin();

            // Obtain the data item info for creating hydro networks
            var dataItemInfo = applicationPlugin.GetDataItemInfos().First(dii => dii.ValueType == typeof(IDrainageBasin));

            var region = new HydroRegion();
            var drainageBasin = (IDrainageBasin)dataItemInfo.CreateData(region);
            Assert.IsTrue(dataItemInfo.AdditionalOwnerCheck(drainageBasin));

        }

        [Test]
        public void GetDataItemsInfoDrainageBasinAddExampleDataTest()
        {
            // Create an application plugin instance
            var applicationPlugin = new NetworkEditorApplicationPlugin();

            // Obtain the data item info for creating hydro networks
            var dataItemInfo = applicationPlugin.GetDataItemInfos().First(dii => dii.ValueType == typeof(IDrainageBasin));

            var region = new HydroRegion();
            var drainageBasin = (IDrainageBasin)dataItemInfo.CreateData(region);
            dataItemInfo.AddExampleData(drainageBasin);
            Assert.That(drainageBasin.Catchments.Count(), Is.EqualTo(1));
        }

        [Test]
        public void CheckNetworkEditorApplicationPluginProperties()
        {
            // Create an application plugin instance
            var applicationPlugin = new NetworkEditorApplicationPlugin();
            Assert.That(applicationPlugin.Name,
                Is.EqualTo(Properties.Resources.NetworkEditorApplicationPlugin_GetDataItemInfos_Network));
            Assert.That(applicationPlugin.DisplayName,
                Is.EqualTo(Properties.Resources.NetworkEditorApplicationPlugin_DisplayName_Hydro_Region_Plugin));
            Assert.That(applicationPlugin.Description,
                Is.EqualTo(Properties.Resources.NetworkEditorApplicationPlugin_Description));
            Assert.That(applicationPlugin.Version,
                Is.EqualTo(applicationPlugin.GetType().Assembly.GetName().Version.ToString()));
            Assert.IsTrue(new Regex(@"\d.\d.\d.\d").IsMatch(applicationPlugin.FileFormatVersion));
        }

        [Test]
        public void AddExampleHydroNetworkDataTest()
        {
            var mocks = new MockRepository();
            var network = mocks.DynamicMock<IHydroNetwork>();
            var branches = new EventedList<IBranch>();
            var nodes = new EventedList<INode>();
            network.Expect(n => n.Branches).Return(branches).Repeat.Any();
            network.Expect(n => n.Nodes).Return(nodes).Repeat.Any();
            mocks.ReplayAll();

            TypeUtils.CallPrivateStaticMethod(typeof(NetworkEditorApplicationPlugin), "AddExampleHydroNetworkData",
                network);

            mocks.VerifyAll();

            Assert.That(branches.Count, Is.EqualTo(1));
            var channel = branches.FirstOrDefault();
            Assert.IsNotNull(channel);
            Assert.That(channel.Name, Is.EqualTo("Channel1"));
            Assert.That(channel.BranchFeatures.Count, Is.EqualTo(1));
            Assert.That(channel.Geometry.ToString(), Is.EqualTo("LINESTRING (0 0, 10 0)"));
            var lateral = channel.BranchFeatures.FirstOrDefault();
            Assert.IsNotNull(lateral);
            Assert.IsInstanceOf<LateralSource>(lateral);
            Assert.That(lateral.Chainage, Is.EqualTo(5));
            Assert.That(lateral.Geometry.ToString(), Is.EqualTo("POINT (5 0)"));
            Assert.That(nodes.Count, Is.EqualTo(2));
            var node1 = nodes.ElementAtOrDefault(0);
            Assert.IsNotNull(node1);
            Assert.That(node1.Name, Is.EqualTo("Node1"));

            var node2 = nodes.ElementAtOrDefault(1);
            Assert.IsNotNull(node2);
            Assert.That(node2.Name, Is.EqualTo("Node2"));

            Assert.That(channel.Source, Is.EqualTo(node1));
            Assert.That(channel.Target, Is.EqualTo(node2));

            Assert.That(node1.Geometry.ToString(), Is.EqualTo("POINT (0 0)"));
            Assert.That(node2.Geometry.ToString(), Is.EqualTo("POINT (10 0)"));
        }

        [Test]
        public void AddExampleDrainageBasinDataTest()
        {
            var drainageBasin = new DrainageBasin();

            TypeUtils.CallPrivateStaticMethod(typeof(NetworkEditorApplicationPlugin), "AddExampleDrainageBasinData",
                                              drainageBasin);
            

            Assert.That(drainageBasin.Catchments.Count, Is.EqualTo(1));
            var catchment = drainageBasin.Catchments.FirstOrDefault();
            Assert.IsNotNull(catchment);
            Assert.That(catchment.Name, Is.EqualTo("Catchment1"));
            Assert.That(catchment.CatchmentType, Is.EqualTo(CatchmentType.Unpaved));
            Assert.That(catchment.Geometry.ToString(), Is.EqualTo("POLYGON ((0 6, 0 12, 10 12, 10 6, 0 6))"));
            Assert.That(catchment.Basin, Is.EqualTo(drainageBasin));

            Assert.That(drainageBasin.WasteWaterTreatmentPlants.Count, Is.EqualTo(1));
            var wasteWaterTreatmentPlant = drainageBasin.WasteWaterTreatmentPlants.FirstOrDefault();
            Assert.IsNotNull(wasteWaterTreatmentPlant);
            Assert.That(wasteWaterTreatmentPlant.Name, Is.EqualTo("Plant1"));
            Assert.That(wasteWaterTreatmentPlant.Geometry.ToString(), Is.EqualTo("POINT (5 5)"));

            Assert.That(drainageBasin.Links.Single().Geometry.ToString(), Is.EqualTo("LINESTRING (5 9 0, 5 5)"));
        }

        [Test]
        public void AddChildRegionDataItemsRegionIsNullSoReturnTest()
        {
            var mocks = new MockRepository();
            var dataItemWithoutRegion = mocks.DynamicMock<IDataItem>();
            dataItemWithoutRegion.Expect(d => d.Value).Return(null).Repeat.Once();
            mocks.ReplayAll();
            TypeUtils.CallPrivateStaticMethod(typeof(NetworkEditorApplicationPlugin), "AddChildRegionDataItems",
                dataItemWithoutRegion);
            mocks.VerifyAll();
        }

        [Test]
        public void GetDataItemInfosHydroRegionCreateDataTest()
        {
            var appPlugin = new NetworkEditorApplicationPlugin();
            var hydroRegionDataItemInfo =
                appPlugin.GetDataItemInfos().FirstOrDefault(dii => dii.ValueType == typeof(HydroRegion));

            Assert.IsNotNull(hydroRegionDataItemInfo);
            var region = new HydroRegion();
            var hydroRegion = (HydroRegion) hydroRegionDataItemInfo.CreateData(region);
            Assert.IsTrue(hydroRegion.Name.StartsWith(Properties.Resources.NetworkEditorApplicationPlugin_GetDataItemInfos_Region));
            Assert.That(hydroRegion.SubRegions.Count, Is.EqualTo(2));
            Assert.That(hydroRegion.SubRegions[0].Name,
                Is.EqualTo(Properties.Resources.NetworkEditorApplicationPlugin_GetDataItemInfos_Network));
            Assert.That(hydroRegion.SubRegions[1].Name,
                Is.EqualTo(Properties.Resources.NetworkEditorApplicationPlugin_GetDataItemInfos_Basin));
        }
        [Test]
        public void GetDataItemInfosHydroRegionAddExampleDataTest()
        {
            var appPlugin = new NetworkEditorApplicationPlugin();
            var hydroRegionDataItemInfo =
                appPlugin.GetDataItemInfos().FirstOrDefault(dii => dii.ValueType == typeof(HydroRegion));

            Assert.IsNotNull(hydroRegionDataItemInfo);
            var region = new HydroRegion();
            var hydroRegion = (HydroRegion)hydroRegionDataItemInfo.CreateData(region);
            hydroRegionDataItemInfo.AddExampleData(hydroRegion);
            Assert.That(hydroRegion.Links[0].Geometry.ToString(), Is.EqualTo("LINESTRING (5 5, 5 0)"));
        }
    }
}