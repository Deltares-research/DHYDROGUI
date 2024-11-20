using System.Collections.Generic;
using Deltares.Infrastructure.TestUtils;
using DHYDRO.Common.IO.InitialField;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.InitialField
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