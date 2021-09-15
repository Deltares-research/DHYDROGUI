using DeltaShell.Plugins.FMSuite.Common.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class FileConstantsTest
    {
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
        [TestCase(FileConstants.NetFileExtension, "_net.nc")]
        [TestCase(FileConstants.MapFileExtension, "_map.nc")]
        [TestCase(FileConstants.HisFileExtension, "_his.nc")]
        [TestCase(FileConstants.ComFileExtension, "_com.nc")]
        [TestCase(FileConstants.ClassMapFileExtension, "_clm.nc")]
        [TestCase(FileConstants.RestartFileExtension, "_rst.nc")]
        [TestCase(FileConstants.GeomFileExtension, "geom.nc")]
        [TestCase(FileConstants.ThinDamPliFileExtension, "_thd.pli")]
        [TestCase(FileConstants.ThinDamPlizFileExtension, "_thd.pliz")]
        [TestCase(FileConstants.FixedWeirPlizFileExtension, "_fxw.pliz")]
        [TestCase(FileConstants.FixedWeirPliFileExtension, "_fxw.pli")]
        [TestCase(FileConstants.ObsCrossSectionPliFileExtension, "_crs.pli")]
        [TestCase(FileConstants.ObsCrossSectionPlizFileExtension, "_crs.pliz")]
        [TestCase(FileConstants.DryAreaFileExtension, "_dry.pol")]
        [TestCase(FileConstants.DryPointFileExtension, "_dry.xyz")]
        [TestCase(FileConstants.StructuresFileExtension, "_structures.ini")]
        [TestCase(FileConstants.ObsPointFileExtension, "_obs.xyn")]
        [TestCase(FileConstants.EnclosureExtension, "_enc.pol")]
        [TestCase(FileConstants.MeteoFileExtension, "_meteo.tim")]
        [TestCase(FileConstants.BoundaryExternalForcingFileExtension, "_bnd.ext")]
        public void Field_HasCorrectValue(string resultValue, string expectedValue)
        {
            Assert.That(resultValue, Is.EqualTo(expectedValue),
                        $"Constant field within class {nameof(FileConstants)} did not have correct value.");
        }
    }
}