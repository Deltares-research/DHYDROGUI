using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.TestUtils;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.CrossSectionView
{
    /// <summary>
    /// Since most functionality is kept on CrossSectionDefinitionView the tests here should only be 
    /// about the wrapper view : CrossSectionView
    /// </summary>
    [TestFixture, Apartment(ApartmentState.STA)]
    [Category(TestCategory.WindowsForms)]
    public class CrossSectionViewTest
    {
        [Test]
        public void ShowWithYZCrossSection()
        {
            ICrossSection cs = GetYZCrossSectionOnHydroNetwork();
            cs.Name = "Rijn";

            var crossSectionDefinitionYZ = CrossSectionDefinitionYZ.CreateDefault();
            crossSectionDefinitionYZ.Name = "Poldersloot";

            var bigYz= CrossSectionDefinitionYZ.CreateDefault();
            bigYz.Name = "Suez kanaal";
            bigYz.ShiftLevel(10);

            cs.HydroNetwork.SharedCrossSectionDefinitions.Add(crossSectionDefinitionYZ);
            cs.HydroNetwork.SharedCrossSectionDefinitions.Add(bigYz);

            var crossSectionView = new Gui.Forms.CrossSectionView.CrossSectionView { Data = cs };

            WindowsFormsTestHelper.ShowModal(crossSectionView,(f)=>cs.HydroNetwork.SharedCrossSectionDefinitions.Add(new CrossSectionDefinitionZW()));
        }

        [Test]
        public void ShowWithXYZCrossSection()
        {
            ICrossSection cs = GetXYZCrossSectionOnHydroNetwork();
            cs.Name = "Rijn";

            var crossSectionView = new Gui.Forms.CrossSectionView.CrossSectionView { Data = cs };

            WindowsFormsTestHelper.ShowModal(crossSectionView, (f) => cs.HydroNetwork.SharedCrossSectionDefinitions.Add(new CrossSectionDefinitionZW()));
        }

        [Test]
        public void  ShowWithProxyDefinitions()
        {
            //TODO : add checks for unsubscribtion
            var definitionYZ = CrossSectionDefinitionYZ.CreateDefault("yz");
            var definitionZW = CrossSectionDefinitionZW.CreateDefault("zw");
            var hydroNetwork = HydroNetworkHelper.GetSnakeHydroNetwork(1);
            hydroNetwork.SharedCrossSectionDefinitions.AddRange(new ICrossSectionDefinition[]{definitionYZ,definitionZW});
            var channel = hydroNetwork.Channels.First();
            var proxyDefinition = new CrossSectionDefinitionProxy(definitionYZ) {LevelShift = 1.1};

            var cs = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(channel, proxyDefinition, 10);

            var crossSectionView = new Gui.Forms.CrossSectionView.CrossSectionView { Data = cs };
            WindowsFormsTestHelper.ShowModal(crossSectionView);
        }


        [Test]
        public void ShowWithSecondProxyDefinition()
        {
            //TODO : add checks for unsubscribtion
            var first = CrossSectionDefinitionYZ.CreateDefault();
            var second = CrossSectionDefinitionYZ.CreateDefault();
            first.Name = "first";
            second.Name = "second";
            var hydroNetwork = HydroNetworkHelper.GetSnakeHydroNetwork(1);
            hydroNetwork.SharedCrossSectionDefinitions.Add(first);
            hydroNetwork.SharedCrossSectionDefinitions.Add(second);

            var channel = hydroNetwork.Channels.First();
            var proxyDefinition = new CrossSectionDefinitionProxy(second) { LevelShift = 1.1 };

            var cs = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(channel, proxyDefinition, 10);

            var crossSectionView = new Gui.Forms.CrossSectionView.CrossSectionView { Data = cs };
            WindowsFormsTestHelper.ShowModal(crossSectionView);
        }

        private static ICrossSection GetYZCrossSectionOnHydroNetwork()
        {
            var crossSectionDefinitionYZ = new CrossSectionDefinitionYZ();
            crossSectionDefinitionYZ.YZDataTable.AddCrossSectionYZRow(0, 8);
            crossSectionDefinitionYZ.YZDataTable.AddCrossSectionYZRow(1, 6);
            crossSectionDefinitionYZ.YZDataTable.AddCrossSectionYZRow(2, 4);
            crossSectionDefinitionYZ.YZDataTable.AddCrossSectionYZRow(3, 3);
            crossSectionDefinitionYZ.YZDataTable.AddCrossSectionYZRow(4, 4);
            crossSectionDefinitionYZ.YZDataTable.AddCrossSectionYZRow(5, 6);
            crossSectionDefinitionYZ.YZDataTable.AddCrossSectionYZRow(6, 8);

            
            var hydroNetwork = HydroNetworkHelper.GetSnakeHydroNetwork(1);
            var channel = hydroNetwork.Channels.First();
            return HydroNetworkHelper.AddCrossSectionDefinitionToBranch(channel, crossSectionDefinitionYZ, 10);
        }

        private static ICrossSection GetXYZCrossSectionOnHydroNetwork()
        {
            var crossSectionDefinitionXYZ = CrossSectionDefinitionXYZ.CreateDefault();
            var coordinates = new[]
                         {
                             new Coordinate(0, 0),
                             new Coordinate(2, 0),
                             new Coordinate(4, -10),
                             new Coordinate(6, -10),
                             new Coordinate(8, 0),
                             new Coordinate(10, 0)
                         };

            //make geometry on the y/z plane
            crossSectionDefinitionXYZ.Geometry = new LineString(coordinates.Select(c => new Coordinate(0, c.X, c.Y)).ToArray());

            var hydroNetwork = HydroNetworkHelper.GetSnakeHydroNetwork(1);
            var channel = hydroNetwork.Channels.First();
            return HydroNetworkHelper.AddCrossSectionDefinitionToBranch(channel, crossSectionDefinitionXYZ, 10);
        }

        [Test]
        public void TestShowConveyanceButton()
        {
            var hydroNetwork = HydroNetworkHelper.GetSnakeHydroNetwork(1);
            var channel = hydroNetwork.Channels.First();
            var crossSectionView = new Gui.Forms.CrossSectionView.CrossSectionView
                {
                    GetConveyanceCalculators = c =>
                        {
                            var mocks = new MockRepository();
                            var calculatorMock = mocks.Stub<IConveyanceCalculator>();
                            return new List<IConveyanceCalculator> {calculatorMock};
                        }
                };
            var controls = crossSectionView.Controls.Find("panelForConveyanceBtn", true);

            var showConveyancePanel = controls.FirstOrDefault();
            Assert.IsNotNull(showConveyancePanel);

            //YZ
            var cs = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(channel,
                                                                          CrossSectionDefinitionYZ.CreateDefault(), 10);
            crossSectionView.Data = cs;

            Assert.IsTrue(showConveyancePanel.Visible);

            //XYZ
            cs = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(channel, CrossSectionDefinitionXYZ.CreateDefault(),
                                                                      10);
            crossSectionView.Data = cs;
            Assert.IsTrue(showConveyancePanel.Visible);

            //ZW
            cs = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(channel, CrossSectionDefinitionZW.CreateDefault(),
                                                                      10);
            crossSectionView.Data = cs;
            Assert.IsFalse(showConveyancePanel.Visible);

            //YZ + No Calculator
            crossSectionView.GetConveyanceCalculators = c => new List<IConveyanceCalculator>();

            cs = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(channel, CrossSectionDefinitionYZ.CreateDefault(),
                                                                      10);
            crossSectionView.Data = cs;
            Assert.IsFalse(showConveyancePanel.Visible);
        }
    }
}