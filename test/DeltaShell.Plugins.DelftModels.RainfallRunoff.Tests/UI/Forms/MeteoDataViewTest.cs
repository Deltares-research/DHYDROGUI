using System;
using System.Linq;
using System.Threading;
using DelftTools.Controls.Swf.Table;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI.Forms
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class MeteoDataViewTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowEmptyMeteoDataView()
        {

            var meteoDataView = new MeteoDataView();

            WindowsFormsTestHelper.ShowModal(meteoDataView);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowMeteoDataViewWithDataPerCatchment()
        {
            var meteoData = new MeteoData(MeteoDataAggregationType.Cumulative)
                {
                    DataDistributionType = MeteoDataDistributionType.PerFeature
                };
            var catchment1 = new Catchment { Name = "Catchment1" };
            var catchment2 = new Catchment { Name = "Catchment2" };
            var catchment3 = new Catchment { Name = "Catchment3" };

            var basin = new DrainageBasin { Catchments = { catchment1, catchment2, catchment3 } };

            ((IFeatureCoverage)meteoData.Data).Features.Add(catchment1);
            ((IFeatureCoverage)meteoData.Data).FeatureVariable.Values.Add(catchment1);

            ((IFeatureCoverage)meteoData.Data).Features.Add(catchment2);
            ((IFeatureCoverage)meteoData.Data).FeatureVariable.Values.Add(catchment2);

            ((IFeatureCoverage)meteoData.Data).Features.Add(catchment3);
            ((IFeatureCoverage)meteoData.Data).FeatureVariable.Values.Add(catchment3);

            var meteoDataView = new MeteoDataView { Data = meteoData };

            WindowsFormsTestHelper.ShowModal(meteoDataView);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithDataPerStation()
        {
            var meteoData = new MeteoData(MeteoDataAggregationType.Cumulative)
            {
                DataDistributionType = MeteoDataDistributionType.PerStation
            };

            meteoData.Data.Arguments[1].AddValues(new[] {"station1", "station2", "station3"});
            var now = DateTime.Now;
            meteoData.Data[now] = new[] { 1.0, 2.0, 3.0 };

            var meteoDataView = new MeteoDataView { Data = meteoData };

            WindowsFormsTestHelper.ShowModal(meteoDataView);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowMeteoDataViewWithDataPerCatchmentAndModify()
        {
            var meteoData = new MeteoData(MeteoDataAggregationType.Cumulative)
                {
                    DataDistributionType = MeteoDataDistributionType.PerFeature
                };
            IFeature catchment1 = new Catchment { Name = "Catchment1" };
            IFeature catchment2 = new Catchment { Name = "Catchment2" };

            var featureCoverage = (IFeatureCoverage)meteoData.Data;

            featureCoverage.Features.Add(catchment1);
            featureCoverage.FeatureVariable.Values.Add(catchment1);

            var meteoDataView = new MeteoDataView { Data = meteoData };

            WindowsFormsTestHelper.ShowModal(meteoDataView, f =>
                {
                    featureCoverage.Features.Add(catchment2);
                    featureCoverage.FeatureVariable.Values.Add(
                        catchment2);

                    featureCoverage.Features.Remove(catchment1);
                    featureCoverage.FeatureVariable.Values.
                                    Remove(
                                        catchment1);
                });
        }
    }
}
