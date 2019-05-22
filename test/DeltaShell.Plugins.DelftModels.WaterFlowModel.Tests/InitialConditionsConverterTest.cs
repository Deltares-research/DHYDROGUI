using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests
{
    [TestFixture]
    public class InitialConditionsConverterTest
    {
        private WaterFlowModel1D waterFlowModel1D;
        private IHydroNetwork hydroNetwork;
        private INetworkCoverage initialConditions;
        private IChannel firstChannel;
        
        [SetUp]
        public void SetUp()
        {
            //setup a model with a initial condition   
            waterFlowModel1D = new WaterFlowModel1D
                                   {
                                       Network = HydroNetworkHelper.GetSnakeHydroNetwork(1)
                                   };

            initialConditions = waterFlowModel1D.InitialConditions;
            hydroNetwork = waterFlowModel1D.Network;
            
            firstChannel = hydroNetwork.Channels.First();
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Can not switch to target type. Model is already of initial conditions type WaterLevel.")]
        public void ThrowExceptionIfInvalidTargetType()
        {
            waterFlowModel1D.InitialConditionsType = InitialConditionsType.WaterLevel;

            InitialConditionsConverter.ChangeInitialConditionsType(waterFlowModel1D,InitialConditionsType.WaterLevel);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Unable to change initial condition type. Insufficient crossections.")]
        public void ThrowExceptionIfInsufficientCrossSections()
        {
            waterFlowModel1D.InitialConditionsType = InitialConditionsType.Depth;

            //define a value so we get an exception
            initialConditions[new NetworkLocation(hydroNetwork.Branches[0], 10)] = 4.0;

            InitialConditionsConverter.ChangeInitialConditionsType(waterFlowModel1D,InitialConditionsType.WaterLevel);
        }

        [Test]
        public void SwitchChangesCoverageName()
        {
            //make sure the type is change..so depth first
            waterFlowModel1D.InitialConditionsType = InitialConditionsType.Depth;

            //go to waterlevel
            InitialConditionsConverter.ChangeInitialConditionsType(waterFlowModel1D, InitialConditionsType.WaterLevel);
            Assert.AreEqual("Initial Water Level", initialConditions.Name);
            //assert the component got changed as well..
            Assert.AreEqual("Water Level".ToUpper(), initialConditions.Components[0].Name.ToUpper());
        }

        [Test]
        public void SwitchUpdatesValues()
        {
            waterFlowModel1D.InitialConditionsType = InitialConditionsType.Depth;
            
            //add a crossection with a bedlevel of -10 at ofset 15
            CrossSectionHelper.AddCrossSection(firstChannel, 15, -10);

            var networkLocation = new NetworkLocation(firstChannel, 10);
            initialConditions[networkLocation] = 4.0;

            //so a depth of 4 and a bed of -10 should result in a level at -6
            InitialConditionsConverter.ChangeInitialConditionsType(waterFlowModel1D, InitialConditionsType.WaterLevel);
            
            Assert.AreEqual(-6.0,initialConditions[networkLocation]);
        }

        [Test]
        public void SwitchUpdatesComplexValues()
        {
            waterFlowModel1D.InitialConditionsType = InitialConditionsType.Depth;
            
            //add a crossection with a bedlevel of -10 at ofset 15
            CrossSectionHelper.AddCrossSection(firstChannel, 20, -10);
            CrossSectionHelper.AddCrossSection(firstChannel, 40, -15);

            initialConditions[new NetworkLocation(firstChannel, 10)] = 4.0;
            initialConditions[new NetworkLocation(firstChannel, 30)] = 5.0;

            //so a depth of 4 and a bed of -10 should result in a level at -6
            InitialConditionsConverter.ChangeInitialConditionsType(waterFlowModel1D, InitialConditionsType.WaterLevel);

            Assert.AreEqual(4, initialConditions.Locations.Values.Count);
            Assert.AreEqual(new []{-6.0,-5.5,-7.5, -10.0}, initialConditions.Components[0].Values);
        }

        [Test]
        public void SwitchUpdatesDefaultValues()
        {
            //make sure the type is change..so depth first
            waterFlowModel1D.InitialConditionsType = InitialConditionsType.Depth;
            waterFlowModel1D.DefaultInitialWaterLevel = 33.4;

            InitialConditionsConverter.ChangeInitialConditionsType(waterFlowModel1D, InitialConditionsType.WaterLevel);
            //the default waterlevel is now the default
            Assert.AreEqual(33.4, initialConditions.DefaultValue);
        }
    }
}