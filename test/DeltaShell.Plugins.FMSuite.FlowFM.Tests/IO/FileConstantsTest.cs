using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class FileConstantsTest
    {
        [TestCase(FileConstants.InputDirectoryName, "input")]
        [TestCase(FileConstants.OutputDirectoryName, "output")]
        [TestCase(FileConstants.PrefixDelwaqDirectoryName, "DFM_DELWAQ_")]
        [TestCase(FileConstants.SnappedFeaturesDirectoryName, "snapped")]
        [TestCase(FileConstants.PliFileExtension, ".pli")]
        [TestCase(FileConstants.PlizFileExtension, ".pliz")]
        [TestCase(FileConstants.PolylineFileExtension, ".pol")]
        [TestCase(FileConstants.TimFileExtension, ".tim")]
        [TestCase(FileConstants.XyzFileExtension, ".xyz")]
        [TestCase(FileConstants.XynFileExtension, ".xyn")]
        [TestCase(FileConstants.IniFileExtension, ".ini")]
        [TestCase(FileConstants.NetCdfFileExtension, ".nc")]
        [TestCase(FileConstants.MduFileExtension, ".mdu")]
        [TestCase(FileConstants.MorphologyFileExtension, ".mor")]
        [TestCase(FileConstants.SedimentFileExtension, ".sed")]
        [TestCase(FileConstants.LandBoundaryFileExtension, ".ldb")]
        [TestCase(FileConstants.ExternalForcingFileExtension, ".ext")]
        [TestCase(FileConstants.GriddedHeatFluxModelFileExtension, ".htc")]
        [TestCase(FileConstants.WindFileExtension, ".wnd")]
        [TestCase(FileConstants.NetFileExtension, "_net" + FileConstants.NetCdfFileExtension)]
        [TestCase(FileConstants.MapFileExtension, "_map" + FileConstants.NetCdfFileExtension)]
        [TestCase(FileConstants.HisFileExtension, "_his" + FileConstants.NetCdfFileExtension)]
        [TestCase(FileConstants.ComFileExtension, "_com" + FileConstants.NetCdfFileExtension)]
        [TestCase(FileConstants.ClassMapFileExtension, "_clm" + FileConstants.NetCdfFileExtension)]
        [TestCase(FileConstants.RestartFileExtension, "_rst" + FileConstants.NetCdfFileExtension)]
        [TestCase(FileConstants.GeomFileExtension, "geom" + FileConstants.NetCdfFileExtension)]
        [TestCase(FileConstants.ThinDamPliFileExtension, "_thd" + FileConstants.PliFileExtension)]
        [TestCase(FileConstants.ThinDamPlizFileExtension, "_thd" + FileConstants.PlizFileExtension)]
        [TestCase(FileConstants.FixedWeirPlizFileExtension, "_fxw" + FileConstants.PlizFileExtension)]
        [TestCase(FileConstants.FixedWeirPliFileExtension, "_fxw" + FileConstants.PliFileExtension)]
        [TestCase(FileConstants.ObsCrossSectionPliFileExtension, "_crs" + FileConstants.PliFileExtension)]
        [TestCase(FileConstants.ObsCrossSectionPlizFileExtension, "_crs" + FileConstants.PlizFileExtension)]
        [TestCase(FileConstants.DryAreaFileExtension, "_dry" + FileConstants.PolylineFileExtension)]
        [TestCase(FileConstants.DryPointFileExtension, "_dry" + FileConstants.XyzFileExtension)]
        [TestCase(FileConstants.StructuresFileExtension, "_structures" + FileConstants.IniFileExtension)]
        [TestCase(FileConstants.ObsPointFileExtension, "_obs" + FileConstants.XynFileExtension)]
        [TestCase(FileConstants.EnclosureExtension, "_enc" + FileConstants.PolylineFileExtension)]
        [TestCase(FileConstants.EmbankmentFileExtension, "_bnk" + FileConstants.PlizFileExtension)]
        [TestCase(FileConstants.MeteoFileExtension, "_meteo" + FileConstants.TimFileExtension)]
        [TestCase(FileConstants.BoundaryExternalForcingFileExtension, "_bnd" + FileConstants.ExternalForcingFileExtension)]
        public void Field_HasCorrectValue(string resultValue, string expectedValue)
        {
            Assert.That(resultValue, Is.EqualTo(expectedValue),
                        $"Constant field within class {nameof(FileConstants)} did not have correct value.");
        }
    }
}