using System.Linq;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport
{
    [TestFixture]
    public class WaterFlowModel1DImporterHelperTest
    {
        [Test]
        public void TestRemovePreviousVersionOfDischargeAtLateralsCoverage()
        {
            var flow1DModel = new WaterFlowModel1D();

            var previousDischargeAtLateralOutputDataItemTag = "Discharge (l)";
            flow1DModel.DataItems.Add(new DataItem(new FeatureCoverage(), DataItemRole.Output, previousDischargeAtLateralOutputDataItemTag));
            
            WaterFlowModel1DImporterHelper.RemovePreviousVersionOfDischargeAtLateralsCoverage(flow1DModel);
            Assert.False(flow1DModel.DataItems.Any(di => di.Tag == previousDischargeAtLateralOutputDataItemTag));
        }
    }
}
