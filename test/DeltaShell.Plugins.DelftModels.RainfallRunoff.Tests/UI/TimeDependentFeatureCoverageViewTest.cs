using System;
using System.Threading;
using System.Windows.Forms;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms.CoverageViews;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI
{
    [TestFixture, Apartment(ApartmentState.STA)]
    [Category(TestCategory.WindowsForms)]
    public class TimeDependentFeatureCoverageViewTest
    {
        [Test]
        public void ShowWithData()
        {
            var timeDepFeatureCoverage = new FeatureCoverage("TimeDepFeatCov") {IsTimeDependent = true};
            timeDepFeatureCoverage.Arguments.Add(new Variable<IFeature>("Catchment"));
            timeDepFeatureCoverage.Components.Add(new Variable<double>("Value"));

            var catchment1 = new Catchment {Name = "catch1"};
            var catchment2 = new Catchment {Name = "catch2"};
            var catchment3 = new Catchment {Name = "catch3"};

            timeDepFeatureCoverage.Features.Add(catchment1);
            timeDepFeatureCoverage.Features.Add(catchment2);
            timeDepFeatureCoverage.Features.Add(catchment3);

            timeDepFeatureCoverage.Time.Values.Add(new DateTime(2000, 1, 1));
            timeDepFeatureCoverage.Time.Values.Add(new DateTime(2001, 1, 1));
            timeDepFeatureCoverage.Time.Values.Add(new DateTime(2002, 1, 1));
            timeDepFeatureCoverage.Time.Values.Add(new DateTime(2003, 1, 1));

            timeDepFeatureCoverage.FeatureVariable.Values.Add(catchment1);
            timeDepFeatureCoverage.FeatureVariable.Values.Add(catchment2);
            timeDepFeatureCoverage.FeatureVariable.Values.Add(catchment3);

            var view = new CoverageView{Data = timeDepFeatureCoverage} as Control;
            
            WindowsFormsTestHelper.ShowModal(view);
        }
    }
}
