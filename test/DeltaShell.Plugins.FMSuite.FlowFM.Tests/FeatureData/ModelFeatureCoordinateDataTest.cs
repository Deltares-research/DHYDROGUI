using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;

using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks;
using FixedWeir = DelftTools.Hydro.Structures.FixedWeir;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.FeatureData
{
    [TestFixture]
    public class ModelFeatureCoordinateDataTest
    {
        [Test]
        public void CreateFixedWeir()
        {

            var mocks = new MockRepository();
            var fixedWeir = mocks.StrictMock<FixedWeir>();
            var fmModel = mocks.StrictMultiMock<IWaterFlowFMModel>(typeof(INotifyPropertyChanged));
            var waterFlowFmModelDefinition = mocks.Stub<WaterFlowFMModelDefinition>();

            var waterFlowFmProperty = waterFlowFmModelDefinition.GetModelProperty(KnownProperties.FixedWeirScheme);
            var val = FMParser.FromString(waterFlowFmProperty.GetValueAsString(),
                waterFlowFmProperty.PropertyDefinition.DataType);
            waterFlowFmProperty.SetValueAsString("9");
            fixedWeir.Expect(fw => fw.Geometry).Return(new Point(0, 0)).Repeat.Any();
            fixedWeir.Expect(fw => fw.Name).Return("").Repeat.Any();
            fmModel.Expect(m => m.ModelDefinition).Return(waterFlowFmModelDefinition).Repeat.Any();

            mocks.ReplayAll();

            waterFlowFmProperty.SetValueAsString("9");
            
            var modelFeatureCoordinateData = new ModelFeatureCoordinateData(fixedWeir,"9");
            mocks.VerifyAll();
        }

        [Test]
        public void CreateFixedWeirAndChangeFeature()
        {

            var mocks = new MockRepository();
            var fixedWeir = mocks.Stub<FixedWeir>();
            fixedWeir.Geometry = new Point(0, 0);
            fixedWeir.Name = "";
            mocks.ReplayAll();


            var fmModel = new WaterFlowFMModel();
            fmModel.ModelDefinition.GetModelProperty(KnownProperties.FixedWeirScheme).SetValueAsString("9");
            fmModel.Area.FixedWeirs.Add(fixedWeir);

            fmModel.ModelDefinition.GetModelProperty(KnownProperties.FixedWeirScheme).SetValueAsString("6");

            var dataColumn = new DataColumn<double>("my column");
            var modelFeatureCoordinateData = new ModelFeatureCoordinateData(fixedWeir);


            modelFeatureCoordinateData.Feature = null;

            Assert.AreEqual(0, dataColumn.ValueList.Count);

            mocks.VerifyAll();
        }
        
        [Test]
        public void CreateFixedWeirAndChangeSchemeAndNumberOfCoordinates()
        {
            var lineGeomery = new LineString(new Coordinate[]
            {
                new Coordinate(0, 0),
                new Coordinate(10, 10),
                new Coordinate(10, 0),
                new Coordinate(0, 0)
            });

            var mocks = new MockRepository();
            /*var fixedWeir = (FixedWeir) mocks.StrictMultiMock(typeof(IFeature), typeof(FixedWeir), typeof(INotifyPropertyChanged));
            fixedWeir.Geometry = lineGeomery;
            fixedWeir.Name = "";
            ((INotifyPropertyChanged)fixedWeir)
                .Expect(f => f.PropertyChanged += Arg<PropertyChangedEventHandler>.Is.Anything).IgnoreArguments();
            */
            var fixedWeir = new FixedWeir {Geometry = lineGeomery};

            mocks.ReplayAll();
            var fmModel = new WaterFlowFMModel();
            fmModel.ModelDefinition.GetModelProperty(KnownProperties.FixedWeirScheme).SetValueAsString("8");
            fmModel.Area.FixedWeirs.Add(fixedWeir);

            var modelFeatureDatas = TypeUtils.GetField(fmModel, "modelDataForFeatures") as ModelDataForFeatures;
            var modelFeatureCoordinateDatas = TypeUtils.GetField(modelFeatureDatas, "modelFeatureCoordinateDatas") as IList<ModelFeatureCoordinateData>;

            Assert.That(modelFeatureCoordinateDatas.Count, Is.EqualTo(1));
            var modelFeatureCoordinateData = modelFeatureCoordinateDatas.First();
            Assert.That(modelFeatureCoordinateData.Feature, Is.EqualTo(fixedWeir));
            Assert.That(modelFeatureCoordinateData.DataColumns.Count, Is.EqualTo(3));
            Assert.That(modelFeatureCoordinateData.DataColumns.First().ValueList.Count, Is.EqualTo(4));

            fixedWeir.Geometry = new LineString(new Coordinate[]
            {
                new Coordinate(0, 0),
                new Coordinate(10, 10),
                new Coordinate(10, 0),
                new Coordinate(0, 0),
                new Coordinate(0, 100),
            });
            /*((INotifyPropertyChanged)fixedWeir).Raise(
                f => f.PropertyChanged += Arg<PropertyChangedEventHandler>.Is.Anything, fixedWeir,
                new PropertyChangedEventArgs(nameof(fixedWeir.Geometry)));
*/
            modelFeatureDatas = TypeUtils.GetField(fmModel, "modelDataForFeatures") as ModelDataForFeatures;
            modelFeatureCoordinateDatas = TypeUtils.GetField(modelFeatureDatas, "modelFeatureCoordinateDatas") as IList<ModelFeatureCoordinateData>;

            Assert.That(modelFeatureCoordinateDatas.Count, Is.EqualTo(1));
            modelFeatureCoordinateData = modelFeatureCoordinateDatas.First();
            Assert.That(modelFeatureCoordinateData.Feature, Is.EqualTo(fixedWeir));
            Assert.That(modelFeatureCoordinateData.DataColumns.Count, Is.EqualTo(3));
            Assert.That(modelFeatureCoordinateData.DataColumns.First().ValueList.Count, Is.EqualTo(5));

            fmModel.ModelDefinition.GetModelProperty(KnownProperties.FixedWeirScheme).SetValueAsString("9");

            modelFeatureDatas = TypeUtils.GetField(fmModel, "modelDataForFeatures") as ModelDataForFeatures;
            modelFeatureCoordinateDatas = TypeUtils.GetField(modelFeatureDatas, "modelFeatureCoordinateDatas") as IList<ModelFeatureCoordinateData>;

            Assert.That(modelFeatureCoordinateDatas.Count, Is.EqualTo(1));
            modelFeatureCoordinateData = modelFeatureCoordinateDatas.First();
            Assert.That(modelFeatureCoordinateData.Feature, Is.EqualTo(fixedWeir));
            Assert.That(modelFeatureCoordinateData.DataColumns.Count, Is.EqualTo(7));
            foreach (var dataColumn in modelFeatureCoordinateData.DataColumns)
            {
                Assert.That(dataColumn.ValueList.Count, Is.EqualTo(5));
                Assert.That(dataColumn.IsActive, Is.True);
            }


            fmModel.ModelDefinition.GetModelProperty(KnownProperties.FixedWeirScheme).SetValueAsString("6");

            modelFeatureDatas = TypeUtils.GetField(fmModel, "modelDataForFeatures") as ModelDataForFeatures;
            modelFeatureCoordinateDatas = TypeUtils.GetField(modelFeatureDatas, "modelFeatureCoordinateDatas") as IList<ModelFeatureCoordinateData>;

            Assert.That(modelFeatureCoordinateDatas.Count, Is.EqualTo(1));
            modelFeatureCoordinateData = modelFeatureCoordinateDatas.First();
            Assert.That(modelFeatureCoordinateData.Feature, Is.EqualTo(fixedWeir));
            Assert.That(modelFeatureCoordinateData.DataColumns.Count, Is.EqualTo(7));
            foreach (var dataColumn in modelFeatureCoordinateData.DataColumns)
            {
                Assert.That(dataColumn.ValueList.Count, Is.EqualTo(5));
                if (dataColumn.Name == "Crest Length" || dataColumn.Name == "Talud Up" ||
                    dataColumn.Name == "Talud Down" || dataColumn.Name == "Vegetation Coefficient")
                    Assert.That(dataColumn.IsActive, Is.False);
                else
                    Assert.That(dataColumn.IsActive, Is.True);
            }
            fixedWeir.Geometry = lineGeomery;
          
            modelFeatureDatas = TypeUtils.GetField(fmModel, "modelDataForFeatures") as ModelDataForFeatures;
            modelFeatureCoordinateDatas = TypeUtils.GetField(modelFeatureDatas, "modelFeatureCoordinateDatas") as IList<ModelFeatureCoordinateData>;

            Assert.That(modelFeatureCoordinateDatas.Count, Is.EqualTo(1));
            modelFeatureCoordinateData = modelFeatureCoordinateDatas.First();
            Assert.That(modelFeatureCoordinateData.Feature, Is.EqualTo(fixedWeir));
            Assert.That(modelFeatureCoordinateData.DataColumns.Count, Is.EqualTo(7));
            foreach (var dataColumn in modelFeatureCoordinateData.DataColumns)
            {
                Assert.That(dataColumn.ValueList.Count, Is.EqualTo(4));
            }

            fmModel.Area.FixedWeirs.Remove(fixedWeir);
            modelFeatureDatas = TypeUtils.GetField(fmModel, "modelDataForFeatures") as ModelDataForFeatures;
            modelFeatureCoordinateDatas = TypeUtils.GetField(modelFeatureDatas, "modelFeatureCoordinateDatas") as IList<ModelFeatureCoordinateData>;

            Assert.That(modelFeatureCoordinateDatas.Count, Is.EqualTo(0));

            mocks.VerifyAll();
        }
        
    }
}