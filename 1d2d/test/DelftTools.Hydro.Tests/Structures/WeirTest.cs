using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.SteerableProperties;
using DelftTools.Hydro.Structures.WeirFormula;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using ValidationAspects;

namespace DelftTools.Hydro.Tests.Structures
{
    [TestFixture]
    public class WeirTest
    {
        [Test]
        public void DefaultWeir()
        {
            IWeir weir = new Weir();

            Assert.IsTrue(weir.Validate().IsValid);
            Assert.IsFalse(weir.CanBeTimedependent);
            Assert.IsFalse(weir.UseCrestLevelTimeSeries);
            Assert.IsNull(weir.CrestLevelTimeSeries);
        }

        [Test]
        public void DefaultTimeDependentWeir()
        {
            IWeir weir = new Weir(true);

            Assert.IsTrue(weir.Validate().IsValid);
            Assert.IsTrue(weir.CanBeTimedependent);
            Assert.IsFalse(weir.UseCrestLevelTimeSeries);
            Assert.IsNotNull(weir.CrestLevelTimeSeries, "Time series should be initialized.");
        }

        
        [Test]
        public void UseTimeSeriesPreconditionThrows()
        {
            IWeir weir = new Weir();

            // Setting to false should not cause problems:
            weir.UseCrestLevelTimeSeries = false;

            Assert.Throws<NotSupportedException>(() => weir.UseCrestLevelTimeSeries = true);
        }

        [Test]
        public void Clone()
        {
            IWeir weir = new Weir("Weir one")
                             {
                                 Geometry = new Point(7, 0),
                                 OffsetY = 175,
                                 CrestWidth = 75,
                                 CrestLevel = -3,
                                 AllowNegativeFlow = true,
                             };
            IWeir clonedWeir = (IWeir) weir.Clone();

            Assert.AreEqual(clonedWeir.Name, weir.Name);
            Assert.AreEqual(clonedWeir.OffsetY, weir.OffsetY);
            Assert.AreEqual(clonedWeir.Geometry, weir.Geometry);
            Assert.AreEqual(clonedWeir.CrestWidth, weir.CrestWidth);
            Assert.AreEqual(clonedWeir.CrestLevel, weir.CrestLevel);
        }

        [Test]
        public void CopyFromTest()
        {
            //create two different weirs 
            //the target should copy all the property values form the source 
            //into the target, but not the name and geometry!! 
            IWeir sourceWeir = new Weir("Source Weir", true)
            {
                Geometry = new Point(7, 0),
                OffsetY = 175,
                CrestWidth = 75,
                CrestLevel = -3,
                Name = "Source Weir",
                AllowNegativeFlow = true,
                WeirFormula = new GatedWeirFormula(),
                UseCrestLevelTimeSeries = true,
            };
            sourceWeir.CrestLevelTimeSeries[new DateTime(2013, 1, 2, 3, 4, 5, 6)] = 7.8;
            IWeir targetWeir = new Weir("Target Weir", false)
            {
                Geometry = new Point(42, 0),
                OffsetY = 571,
                CrestWidth = 55,
                CrestLevel = -1,
                Name = "Target Weir",
                AllowNegativeFlow = false,
                WeirFormula = new FreeFormWeirFormula(),
                UseCrestLevelTimeSeries = false,
            };
            targetWeir.CopyFrom(sourceWeir);

            Assert.AreNotEqual(sourceWeir.Name, targetWeir.Name);
            Assert.AreEqual(sourceWeir.OffsetY, targetWeir.OffsetY);
            Assert.AreEqual(sourceWeir.CrestWidth, targetWeir.CrestWidth);
            Assert.AreEqual(sourceWeir.CrestLevel, targetWeir.CrestLevel);
            Assert.AreEqual(sourceWeir.AllowNegativeFlow, targetWeir.AllowNegativeFlow);
            Assert.AreEqual(sourceWeir.WeirFormula.HasFlowDirection, targetWeir.WeirFormula.HasFlowDirection);
            Assert.AreEqual(sourceWeir.IsGated, targetWeir.IsGated);
            Assert.AreEqual(sourceWeir.CanBeTimedependent, targetWeir.CanBeTimedependent);
            Assert.AreEqual(sourceWeir.UseCrestLevelTimeSeries, targetWeir.UseCrestLevelTimeSeries);
            Assert.AreEqual(sourceWeir.CrestLevelTimeSeries[new DateTime(2013, 1, 2, 3, 4, 5, 6)],
                            targetWeir.CrestLevelTimeSeries[new DateTime(2013, 1, 2, 3, 4, 5, 6)]);
        }

        [Test]
        public void IsGated()
        {
            IWeir simpleweir = new Weir("simple") { };
            IWeir gatedweir = new Weir("gated") { WeirFormula = new GatedWeirFormula()};

            Assert.IsFalse(simpleweir.IsGated);
            Assert.IsTrue(gatedweir.IsGated);

        }

        [Test]
        public void IsRectangle()
        {
            IWeir simpleweir = new Weir("simple") { };
            IWeir freeformweir = new Weir("freeform") { WeirFormula = new FreeFormWeirFormula() };

            Assert.IsTrue(simpleweir.IsRectangle);
            Assert.IsFalse(freeformweir.IsRectangle);
        }

        [Test]
        public void Allow()
        {
            IWeir simpleweir = new Weir("simple");

            simpleweir.AllowNegativeFlow = true;
            simpleweir.AllowPositiveFlow = true;

            Assert.IsTrue(simpleweir.AllowNegativeFlow);
            Assert.IsTrue(simpleweir.AllowPositiveFlow);

            simpleweir.AllowPositiveFlow = false;

            Assert.IsTrue(simpleweir.AllowNegativeFlow);
            Assert.IsFalse(simpleweir.AllowPositiveFlow);

            simpleweir.AllowNegativeFlow = false;

            Assert.IsFalse(simpleweir.AllowNegativeFlow);
            Assert.IsFalse(simpleweir.AllowPositiveFlow);

            simpleweir.AllowNegativeFlow = true;
            simpleweir.AllowPositiveFlow = true;

            Assert.IsTrue(simpleweir.AllowNegativeFlow);
            Assert.IsTrue(simpleweir.AllowPositiveFlow);

        }

        [Test]
        public void BindingTest()
        {
            var  pierweir = new Weir("pier") { WeirFormula = new PierWeirFormula() };
            int callCount = 0;

            ((INotifyPropertyChanged)pierweir).PropertyChanged += (s, e) =>
            //pierweir.PropertyChanged += (s, e) =>
                                            {
                                                callCount++;
                                                Assert.AreEqual(pierweir, s);
                                                Assert.AreEqual("CrestShape", e.PropertyName);
                                            };
            pierweir.CrestShape = CrestShape.Triangular;
            Assert.AreEqual(1,callCount);
        }

        [Test]
        public void PropertyChangedForFormula()
        {
            //translates the event so view etc don't have to know about formula. 
            //requires a hack in weir that can be removed with new PS (hopefully)
            var formula = new GatedWeirFormula();
            var pierweir = new Weir("pier") { WeirFormula = formula };

            int callCount = 0;
            ((INotifyPropertyChanged) pierweir).PropertyChanged += (s, e) =>
                                                                       {
                                                                           Assert.AreEqual("GateOpening", e.PropertyName);
                                                                           Assert.AreEqual(formula, s);
                                                                           callCount++;
                                                                       };

            formula.GateOpening = 22.0;
            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void CrestLevelAndCrestWidthAreDefinedInFormulaForGeneralStructureWeir()
        {
            double bedLevelStructureCentre = 5.0;
            double widthStructureCentre = 1.0;
            var weir = new Weir { WeirFormula = new GeneralStructureWeirFormula
                                                    {
                                                        BedLevelStructureCentre = bedLevelStructureCentre,
                                                        WidthStructureCentre = widthStructureCentre
                                                    } };
            Assert.AreEqual(bedLevelStructureCentre,weir.CrestLevel);
            Assert.AreEqual(widthStructureCentre, weir.CrestWidth);
            
        }

        [Test]
        public void RiverWeirClone()
        {
            var weir = new Weir {WeirFormula = new RiverWeirFormula()};

            var clonedWeir = (Weir) weir.Clone();

            Assert.AreNotSame(((RiverWeirFormula) weir.WeirFormula).SubmergeReductionNeg,
                              ((RiverWeirFormula) clonedWeir.WeirFormula).SubmergeReductionNeg);
        }

        [Test]
        public void SimpleWeirPolylineWidthTest()
        {
            var weir = new Weir()
            {
                WeirFormula = new SimpleWeirFormula(),
                Geometry = new LineString(new[] {new Coordinate(0, 0), new Coordinate(4, 3),})
            };

            Assert.AreEqual(5,weir.CrestWidth);
        }

        [Test]
        public void RetrieveSteerableProperties_ReturnsCrestLevelSteerableProperty()
        {
            // Setup
            var weir = new Weir(true);
            
            // Call
            List<SteerableProperty> steerableProperties = weir.RetrieveSteerableProperties().ToList();

            // Assert
            Assert.That(steerableProperties.Count, Is.EqualTo(1));
            SteerableProperty property = steerableProperties.First();
            Assert.That(property.TimeSeries.Name, Is.EqualTo("Crest level"));
        }

        [Test]
        public void SettingWeirFormulaToFreeFormWeir_EnsuresThatNoTimeSeriesIsUsedForCrestLevel()
        {
            // Setup
            const bool allowTimeVaryingData = true;
            var weir = new Weir(allowTimeVaryingData) { UseCrestLevelTimeSeries = true };
            
            // Precondition
            Assert.That(weir.UseCrestLevelTimeSeries, Is.True);

            // Call
            weir.WeirFormula = new FreeFormWeirFormula();

            // Assert
            Assert.That(weir.UseCrestLevelTimeSeries, Is.False);
        }

        [Test]
        public void RetrieveSteerableProperties_FreeFormWeirFormula_ReturnsZeroSteerableProperties()
        {
            // Setup
            const bool allowTimeVaryingData = true;
            var weir = new Weir(allowTimeVaryingData) { WeirFormula = new FreeFormWeirFormula() };

            // Call
            IEnumerable<SteerableProperty> steerableProperties = weir.RetrieveSteerableProperties();

            // Assert
            Assert.That(steerableProperties, Is.Empty);
        }

        [Test]
        [TestCaseSource(nameof(IsUsingTimeSeriesForCrestLevelTestCases))]
        public void IsUsingTimeSeriesForCrestLevel_ReturnsCorrectValue(
            bool canBeTimedependent,
            bool useCrestLevelTimeSeries,
            IWeirFormula weirFormula,
            bool expectedResult)
        {
            // Setup
            var weir = new Weir(canBeTimedependent)
            {
                WeirFormula = weirFormula,
                UseCrestLevelTimeSeries = useCrestLevelTimeSeries
            };

            // Call
            bool isUsingTimeSeriesForCrestLevel = weir.IsUsingTimeSeriesForCrestLevel();

            // Assert
            Assert.That(isUsingTimeSeriesForCrestLevel, Is.EqualTo(expectedResult));
        }

        private static IEnumerable<TestCaseData> IsUsingTimeSeriesForCrestLevelTestCases()
        {
            var freeFormWeirFormula = new FreeFormWeirFormula();
            var simpleWeirFormula = new SimpleWeirFormula();
            var generalStructureWeirFormula = new GeneralStructureWeirFormula();
            var gatedWeirFormula = new GatedWeirFormula();

            yield return new TestCaseData(true, true, freeFormWeirFormula, false);
            yield return new TestCaseData(true, false, freeFormWeirFormula, false);
            yield return new TestCaseData(false, false, freeFormWeirFormula, false);
            
            yield return new TestCaseData(true, true, simpleWeirFormula, true);
            yield return new TestCaseData(true, false, simpleWeirFormula, false);
            yield return new TestCaseData(false, false, simpleWeirFormula, false);
            
            yield return new TestCaseData(true, true, generalStructureWeirFormula, true);
            yield return new TestCaseData(true, false, generalStructureWeirFormula, false);
            yield return new TestCaseData(false, false, generalStructureWeirFormula, false);
            
            yield return new TestCaseData(true, true, gatedWeirFormula, true);
            yield return new TestCaseData(true, false, gatedWeirFormula, false);
            yield return new TestCaseData(false, false, gatedWeirFormula, false);
        }
    }
}
