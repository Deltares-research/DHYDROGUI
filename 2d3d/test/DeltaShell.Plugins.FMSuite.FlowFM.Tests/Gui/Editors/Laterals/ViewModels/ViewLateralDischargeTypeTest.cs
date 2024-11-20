using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors.Laterals.ViewModels;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Editors.Laterals.ViewModels
{
    [TestFixture]
    public class ViewLateralDischargeTypeTest : EnumDescriptionTestFixture<ViewLateralDischargeType>
    {
        protected override IDictionary<ViewLateralDischargeType, string> ExpectedDescriptionForEnumValues =>
            new Dictionary<ViewLateralDischargeType, string>
            {
                { ViewLateralDischargeType.Constant, "Constant discharge" },
                { ViewLateralDischargeType.TimeSeries, "Discharge time series" },
                { ViewLateralDischargeType.RealTime, "Real time" }
            };
    }
}