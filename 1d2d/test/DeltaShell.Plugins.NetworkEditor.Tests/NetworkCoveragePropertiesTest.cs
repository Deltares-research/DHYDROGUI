using DelftTools.Functions.Generic;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests
{
    [TestFixture]
    public class NetworkCoveragePropertiesTest
    {
        [Test]
        public void InterpolationMethodCanBeSetWhenLocationArgumentsHasAllowSetInterpolationType()
        {
            var networkCoverage = new NetworkCoverage();
            networkCoverage.Locations.AllowSetExtrapolationType = true;
            networkCoverage.Locations.AllowSetInterpolationType = false;

            var properties = new NetworkCoverageProperties { Data = networkCoverage };

            //set them to a default value
            networkCoverage.Locations.InterpolationType = InterpolationType.Constant;
            networkCoverage.Locations.ExtrapolationType = ExtrapolationType.Constant;

            //use properties class to set them to linear.
            properties.InterpolationType = NetworkCoverageProperties.NetworkCoverageInterpolationType.Linear;

            //assert the interpolation did not change and the extra polation did
            Assert.AreEqual(InterpolationType.Constant, networkCoverage.Locations.InterpolationType);
            Assert.AreEqual(ExtrapolationType.Constant, networkCoverage.Locations.ExtrapolationType);
        }
    }
}