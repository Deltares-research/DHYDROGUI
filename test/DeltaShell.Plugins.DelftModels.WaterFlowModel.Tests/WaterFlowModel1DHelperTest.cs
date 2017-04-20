using DelftTools.Hydro.Helpers;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests
{
    [TestFixture]
    public class WaterFlowModel1DHelperTest
    {
       [Test]
       public void CanChangeIfNoInitialConditionsDefined()
       {
           //empty new model..should be ok
           var waterFlowModel1D = new WaterFlowModel1D();
           //create a branch with a a single crossection and initial condition on this branch
           var message = "";
           Assert.IsTrue(WaterFlowModel1DHelper.CanChangeInitialConditionsType(waterFlowModel1D,out message)); 
       }

       [Test]
       public void CanChangeIfCrossSectionAvailableForEachLocationInInitialCondition()
       {
           var waterFlowModel1D = new WaterFlowModel1D();
           var hydroNetwork = HydroNetworkHelper.GetSnakeHydroNetwork(1);
           waterFlowModel1D.Network = hydroNetwork;
           //define an initial condition on the first branch
           var networkLocation = new NetworkLocation(hydroNetwork.Branches[0], 10);
           waterFlowModel1D.InitialConditions[networkLocation] = 2.0;

           //cannot change because no crossections
           var expectedMessage = string.Format(
                    "Cannot change the type of the initial conditions. No cross-sections found on channel '{0}' for location '{1}'.", networkLocation.Branch.Name,
                        networkLocation.Name);
           var message = "";
           Assert.IsFalse(WaterFlowModel1DHelper.CanChangeInitialConditionsType(waterFlowModel1D,out message));
           Assert.AreEqual(expectedMessage, message);
       }
        
    }
}