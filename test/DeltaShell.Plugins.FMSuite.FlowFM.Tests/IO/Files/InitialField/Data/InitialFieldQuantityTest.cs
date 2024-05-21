using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialField.Data;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.InitialField.Data
{
    [TestFixture]
    public class InitialFieldQuantityTest : EnumDescriptionTestFixture<InitialFieldQuantity>
    {
        protected override IDictionary<InitialFieldQuantity, string> ExpectedDescriptionForEnumValues => new Dictionary<InitialFieldQuantity, string>
        {
            { InitialFieldQuantity.BedLevel, "bedlevel" },
            { InitialFieldQuantity.WaterLevel, "waterlevel" },
            { InitialFieldQuantity.WaterDepth, "waterdepth" },
            { InitialFieldQuantity.InterceptionLayerThickness, "InterceptionLayerThickness" },
            { InitialFieldQuantity.PotentialEvaporation, "PotentialEvaporation" },
            { InitialFieldQuantity.InfiltrationCapacity, "InfiltrationCapacity" },
            { InitialFieldQuantity.HortonMaxInfCap, "HortonMaxInfCap" },
            { InitialFieldQuantity.HortonMinInfCap, "HortonMinInfCap" },
            { InitialFieldQuantity.HortonDecreaseRate, "HortonDecreaseRate" },
            { InitialFieldQuantity.HortonRecoveryRate, "HortonRecoveryRate" },
            { InitialFieldQuantity.FrictionCoefficient, "frictioncoefficient" },
            { InitialFieldQuantity.None, "" }
        };
    }
}