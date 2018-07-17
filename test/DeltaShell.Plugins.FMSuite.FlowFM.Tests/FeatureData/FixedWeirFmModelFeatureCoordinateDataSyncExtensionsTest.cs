using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.FeatureData
{
    [TestFixture]
    public class FixedWeirFmModelFeatureCoordinateDataSyncExtensionsTest
    {
        [Test]
        public void GivenModelFeatureCoordinateData_WhenSchemeIsChanging_ThenTheColumnsShouldUpdate()
        {
            var data = new ModelFeatureCoordinateData<FixedWeir>();
            data.UpdateDataColumns("Scheme8");

            Assert.AreEqual(3, data.DataColumns.Count);
            Assert.AreEqual("Crest Levels",data.DataColumns[0].Name);
            Assert.AreEqual("Ground Levels Left", data.DataColumns[1].Name);
            Assert.AreEqual("Ground Levels Right", data.DataColumns[2].Name);
            
            data.UpdateDataColumns("Scheme9");
            Assert.AreEqual(7, data.DataColumns.Count);
            Assert.AreEqual("Crest Levels", data.DataColumns[0].Name);
            Assert.AreEqual("Ground Levels Left", data.DataColumns[1].Name);
            Assert.AreEqual("Ground Levels Right", data.DataColumns[2].Name);
            Assert.AreEqual("Crest Length", data.DataColumns[3].Name);
            Assert.AreEqual("Talud Up", data.DataColumns[4].Name);
            Assert.AreEqual("Talud Down", data.DataColumns[5].Name);
            Assert.AreEqual("Vegetation Coefficient", data.DataColumns[6].Name);
            
            data.UpdateDataColumns("Scheme6");
            Assert.AreEqual(7, data.DataColumns.Count);
            Assert.AreEqual(7, data.DataColumns.Count);
            Assert.AreEqual("Crest Levels", data.DataColumns[0].Name);
            Assert.AreEqual("Ground Levels Left", data.DataColumns[1].Name);
            Assert.AreEqual("Ground Levels Right", data.DataColumns[2].Name);
            Assert.AreEqual("Crest Length", data.DataColumns[3].Name);
            Assert.AreEqual("Talud Up", data.DataColumns[4].Name);
            Assert.AreEqual("Talud Down", data.DataColumns[5].Name);
            Assert.AreEqual("Vegetation Coefficient", data.DataColumns[6].Name);
        }
        [Test]
        public void GivenModelFeatureCoordinateData_WhenFeatureIsAddedAndSchemeIsChanging_ThenAllDefaultValuesCouldBeFound()
        {
            var lineGeomery = new LineString(new Coordinate[]
            {
                new Coordinate(0,0),
                new Coordinate(10,10)
            });

            var fixedWeir = new FixedWeir { Geometry = lineGeomery };
            
            var data = new ModelFeatureCoordinateData<FixedWeir> { Feature = fixedWeir };
            data.UpdateDataColumns("Scheme9");
            
            Assert.AreEqual(0,data.DataColumns[0].ValueList[0]);
            Assert.AreEqual(0, data.DataColumns[0].ValueList[1]);

            Assert.AreEqual(0, data.DataColumns[1].ValueList[0]);
            Assert.AreEqual(0, data.DataColumns[1].ValueList[1]);

            Assert.AreEqual(0, data.DataColumns[2].ValueList[0]);
            Assert.AreEqual(0, data.DataColumns[2].ValueList[1]);

            Assert.AreEqual(3, data.DataColumns[3].ValueList[0]);
            Assert.AreEqual(3, data.DataColumns[3].ValueList[1]);

            Assert.AreEqual(4, data.DataColumns[4].ValueList[0]);
            Assert.AreEqual(4, data.DataColumns[4].ValueList[1]);

            Assert.AreEqual(4, data.DataColumns[5].ValueList[0]);
            Assert.AreEqual(4, data.DataColumns[5].ValueList[1]);

            Assert.AreEqual(0, data.DataColumns[6].ValueList[0]);
            Assert.AreEqual(0, data.DataColumns[6].ValueList[1]);
        }

        
    }
}