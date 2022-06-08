using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.SteerableProperties;
using DelftTools.TestUtils;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.Structures
{
    [TestFixture]
    public class CulvertTest
    {
        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var culvert = new Culvert();
            
            // Assert
            Assert.That(culvert.Name, Is.EqualTo("Culvert"));
            Assert.That(culvert.GateOpeningLossCoefficientFunction, Is.Not.Null);
            Assert.That(culvert.Width, Is.EqualTo(1.0));
            Assert.That(culvert.Height, Is.EqualTo(1.0));
            Assert.That(culvert.TabulatedCrossSectionDefinition, Is.Not.Null);
            Assert.That(culvert.GeometryType, Is.EqualTo(CulvertGeometryType.Round));
            Assert.That(culvert.CulvertType, Is.EqualTo(CulvertType.Culvert));
            Assert.That(culvert.BendLossCoefficient, Is.EqualTo(1.0));
        }

        [Test]
        public void CreateDefault_CreatesCorrectInstance()
        {
            // Call
            var culvert = Culvert.CreateDefault();
            
            // Assert
            Assert.That(culvert.InletLevel, Is.EqualTo(-5.0));
            Assert.That(culvert.OutletLevel, Is.EqualTo(-5.0));
            Assert.That(culvert.Width, Is.EqualTo(1.0));
            Assert.That(culvert.Height, Is.EqualTo(1.0));
            Assert.That(culvert.Length, Is.EqualTo(10.0));
            Assert.That(culvert.InletLossCoefficient, Is.EqualTo(0.1));
            Assert.That(culvert.OutletLossCoefficient, Is.EqualTo(0.1));
            Assert.That(culvert.BendLossCoefficient, Is.EqualTo(1.0));
            Assert.That(culvert.ArcHeight, Is.EqualTo(0.25));
            Assert.That(culvert.Diameter, Is.EqualTo(4.0));
            Assert.That(culvert.Radius, Is.EqualTo(0.5));
            Assert.That(culvert.Radius1, Is.EqualTo(0.8));
            Assert.That(culvert.Radius2, Is.EqualTo(0.2));
            Assert.That(culvert.Radius3, Is.EqualTo(0));
            Assert.That(culvert.Angle, Is.EqualTo(28));
            Assert.That(culvert.Angle1, Is.EqualTo(0));
            Assert.That(culvert.Friction, Is.EqualTo(45.0));
            Assert.That(culvert.FrictionDataType, Is.EqualTo(Friction.Chezy));
        }
        
        [Test]
        public void AbsoluteCrossSectionIncludesInletLevelForRectangle()
        {
            //set it up as rectangle
            var culvert = new Culvert();
            culvert.GeometryType = CulvertGeometryType.Rectangle;
            culvert.Width = 20;
            culvert.Height = 10;
            culvert.InletLevel = 5;

            //TODO: add a small spike on top of the crossection (for modelapi only)
            Assert.AreEqual(2, culvert.CrossSectionDefinitionAtInletAbsolute.ZWDataTable.Count);

            //the inletlevel is included for the crossection. Is this ok for model api?
            Assert.AreEqual(5, culvert.CrossSectionDefinitionAtInletAbsolute.ZWDataTable[1].Z);
            Assert.AreEqual(15, culvert.CrossSectionDefinitionAtInletAbsolute.ZWDataTable[0].Z);

            Assert.AreEqual(0, culvert.CrossSectionDefinitionAtInletAbsolute.ZWDataTable[1].StorageWidth);
            Assert.AreEqual(0, culvert.CrossSectionDefinitionAtInletAbsolute.ZWDataTable[0].StorageWidth);

            Assert.AreEqual(20, culvert.CrossSectionDefinitionAtInletAbsolute.ZWDataTable[1].Width);
            Assert.AreEqual(20, culvert.CrossSectionDefinitionAtInletAbsolute.ZWDataTable[0].Width);
        }

        [Test]
        public void AbsoluteCrossSectionIncludesInletLevelForTabulated()
        {
            //set it up as rectangle
            var culvert = new Culvert();
            culvert.GeometryType = CulvertGeometryType.Tabulated;
            culvert.TabulatedCrossSectionDefinition.SetWithHfswData(new[]
                                                              {
                                                                  new HeightFlowStorageWidth(0, 20, 20),
                                                                  new HeightFlowStorageWidth(10, 20, 20)
                                                              });
            culvert.InletLevel = 5;

            //TODO: add a small spike on top of the crossection (for modelapi only)
            Assert.AreEqual(2, culvert.CrossSectionDefinitionAtInletAbsolute.ZWDataTable.Count);

            //the inletlevel is included for the crossection. Is this ok for model api?
            Assert.AreEqual(5, culvert.CrossSectionDefinitionAtInletAbsolute.ZWDataTable[1].Z);
            Assert.AreEqual(15, culvert.CrossSectionDefinitionAtInletAbsolute.ZWDataTable[0].Z);

            Assert.AreEqual(0, culvert.CrossSectionDefinitionAtInletAbsolute.ZWDataTable[1].StorageWidth);
            Assert.AreEqual(0, culvert.CrossSectionDefinitionAtInletAbsolute.ZWDataTable[0].StorageWidth);

            Assert.AreEqual(20, culvert.CrossSectionDefinitionAtInletAbsolute.ZWDataTable[1].Width);
            Assert.AreEqual(20, culvert.CrossSectionDefinitionAtInletAbsolute.ZWDataTable[0].Width);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.WorkInProgress)]
        public void PropertyChangedForTabulatedCrossection()
        {
            //TS: CrossSection is no longer sending property changed. It does have a manual property, but this would
            //require some hacking in Culvert to propogate this event through PostSharp. Since this seems only used
            //by the view, for now it has been solved there.

            //since structureview only listens to changes in the structure itself a change in the crossection 
            //should cause a PC in the Culvert itself

            int callCount = 0;
            //use a default 
            var culvert = new Culvert();
            culvert.TabulatedCrossSectionDefinition.ZWDataTable.AddCrossSectionZWRow(0, 0, 0);

            ((INotifyPropertyChanged)culvert).PropertyChanged += (s, e) =>
            {
                Assert.AreEqual(
                    culvert.TabulatedCrossSectionDefinition.
                        ZWDataTable[0], s);
                Assert.AreEqual("Width", e.PropertyName);
                callCount++;
            };

            culvert.TabulatedCrossSectionDefinition.ZWDataTable[0].Width = 22;

            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void CopyInto()
        {
            var culvert = new Culvert {CulvertType = CulvertType.Culvert, FlowDirection = FlowDirection.Both};
            var invertedSiphon = new Culvert { CulvertType = CulvertType.InvertedSiphon, FlowDirection = FlowDirection.Negative };

            var target = new Culvert();
            
            target.CopyFrom(culvert);
            Assert.AreEqual(culvert.CulvertType, target.CulvertType);
            Assert.AreEqual(culvert.FlowDirection, target.FlowDirection);

            target.CopyFrom(invertedSiphon);
            Assert.AreEqual(invertedSiphon.CulvertType, target.CulvertType);
            Assert.AreEqual(invertedSiphon.FlowDirection, target.FlowDirection);
        }

        [Test]
        public void CopyFrom()
        {
            var targetCulvert = new Culvert("target");
            var sourceCulvert = new Culvert("source")
            
                                    {
                                        Diameter = 20.0,
                                        FlowDirection = FlowDirection.Positive,
                                        Friction = 3.0,
                                        FrictionType = CulvertFrictionType.WhiteColebrook,
                                        GateInitialOpening = 4.2,
                                        //GateOpeningLossCoefficientFunction = ,
                                        GeometryType = CulvertGeometryType.SteelCunette,
                                        Height = 5.0,
                                        InletLevel = 3.11,
                                        InletLossCoefficient = 0.42,
                                        IsGated = true,
                                        CulvertType = CulvertType.InvertedSiphon,
                                        OutletLevel = 0.42,
                                        OutletLossCoefficient = 0.42,
                                        Radius = 42.0,
                                        Radius1 = 42.1,
                                        Radius2 = 42.3,
                                        Radius3 = 42.4,
                                        TabulatedCrossSectionDefinition =
                                            new CrossSectionDefinitionZW(),
                                        Width = 14.0,
                                        GroundLayerRoughness = 0.42,
                                        GroundLayerThickness = 4.2
                                    };
            targetCulvert.CopyFrom(sourceCulvert);
            Assert.AreEqual(sourceCulvert.Diameter, targetCulvert.Diameter);
            Assert.AreEqual(sourceCulvert.FlowDirection, targetCulvert.FlowDirection);
            Assert.AreEqual(sourceCulvert.Friction, targetCulvert.Friction);
            Assert.AreEqual(sourceCulvert.FrictionType, targetCulvert.FrictionType);
            Assert.AreEqual(sourceCulvert.GateInitialOpening, targetCulvert.GateInitialOpening);
            Assert.AreEqual(sourceCulvert.GeometryType, targetCulvert.GeometryType);
            Assert.AreEqual(sourceCulvert.Height, targetCulvert.Height);
            Assert.AreEqual(sourceCulvert.InletLevel, targetCulvert.InletLevel);
            Assert.AreEqual(sourceCulvert.OutletLossCoefficient, targetCulvert.OutletLossCoefficient);
            Assert.AreEqual(sourceCulvert.Radius, targetCulvert.Radius);
            Assert.AreEqual(sourceCulvert.TabulatedCrossSectionDefinition.CrossSectionType, targetCulvert.TabulatedCrossSectionDefinition.CrossSectionType);
            Assert.AreEqual(sourceCulvert.Width, targetCulvert.Width);
            Assert.AreEqual(sourceCulvert.GroundLayerThickness, targetCulvert.GroundLayerThickness);
            Assert.AreEqual(sourceCulvert.GroundLayerRoughness, targetCulvert.GroundLayerRoughness);
            Assert.AreNotEqual(sourceCulvert.Name, targetCulvert.Name);
        }

        [Test]
        public void RetrieveSteerableProperties_ReturnsValveOpeningHeightSteerableProperty()
        {
            // Setup
            const string defaultCulvertTimeSeriesName = "Valve Opening Height";
            var culvert = new Culvert();
            
            // Call
            List<SteerableProperty> steerableProperties = culvert.RetrieveSteerableProperties().ToList();

            // Assert
            Assert.That(steerableProperties.Count, Is.EqualTo(1));
            SteerableProperty property = steerableProperties.First();
            Assert.That(property.TimeSeries.Name, Is.EqualTo(defaultCulvertTimeSeriesName));
        }
    }
}