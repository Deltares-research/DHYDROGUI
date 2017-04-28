using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using NUnit.Framework;
using SharpTestsEx;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Domain
{
    [TestFixture]
    public class ConnectionPointTest
    {
         [Test]
         public void ChangingFeatureUpdatesName()
         {
             var feature = new RtcTestFeature { Name = "f" };

             var input = new Input { Feature = feature, ParameterName = "p" };

             input.Name
                 .Should("name is set correctly").Be.EqualTo("f_p");

             feature.Name = "f2";

             input.Name
                 .Should("name is updated on feature name change").Be.EqualTo("f2_p");
         }
    }
}