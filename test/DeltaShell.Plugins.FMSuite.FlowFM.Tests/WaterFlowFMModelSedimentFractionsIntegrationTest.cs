using System.Collections.Generic;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Sediment;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [Category(TestCategory.Integration)]
    [Category(TestCategory.Slow)]
    public class WaterFlowFMModelSedimentFractionsIntegrationTest
    {
        [Test]
        public void TestRemoveSedimentFractionAlsoRemovesCoverages()
        {
            // setup
            WaterFlowFMModel model = CreateSimpleBoxModel();
            Assert.IsEmpty(model.SpatialData.InitialFractions.ToList());
            var fraction = new SedimentFraction() {Name = "Frac1"};
            model.SedimentFractions.Add(fraction);

            List<ISpatiallyVaryingSedimentProperty> spatiallyVaryingProperties = fraction.CurrentSedimentType.Properties.OfType<ISpatiallyVaryingSedimentProperty>().ToList();
            Assert.IsTrue(spatiallyVaryingProperties.Any());

            // TODO: delete the next two lines and uncomment the foreach-loop when Initial Condition is supported in ext-files (DELFT3DFM-996)
            ISpatiallyVaryingSedimentProperty initialConditionProperty = spatiallyVaryingProperties.FirstOrDefault();
            Assert.NotNull(initialConditionProperty);

            foreach (ISpatiallyVaryingSedimentProperty spatiallyVaryingProperty in spatiallyVaryingProperties)
            {
                // Set spatially varying property to true, DataItem for coverage should now exist
                spatiallyVaryingProperty.IsSpatiallyVarying = true;
                Assert.NotNull(model.AllDataItems.FirstOrDefault(di => di.Name == spatiallyVaryingProperty.SpatiallyVaryingName));
            }

            // Remove the Fraction (with a spatially varying property set to true) the DataItem for the coverage should also be removed
            model.SedimentFractions.Remove(fraction);
            foreach (ISpatiallyVaryingSedimentProperty spatiallyVaryingProperty in spatiallyVaryingProperties)
            {
                Assert.Null(model.DataItems.FirstOrDefault(di => di.Name == spatiallyVaryingProperty.SpatiallyVaryingName));
            }

            // Re-add the fraction (with a spatially varying property set to true) the DataItem for the coverage should be restored
            model.SedimentFractions.Add(fraction);
            foreach (ISpatiallyVaryingSedimentProperty spatiallyVaryingProperty in spatiallyVaryingProperties)
            {
                Assert.NotNull(model.AllDataItems.FirstOrDefault(di => di.Name == spatiallyVaryingProperty.SpatiallyVaryingName));
            }
        }

        [Test]
        public void AddRemoveSedimentConcentrationFractionShouldAddRemoveData()
        {
            // create
            WaterFlowFMModel model = CreateSimpleBoxModel();
            model.BoundaryConditionSets[0].BoundaryConditions.Clear();
            Assert.IsEmpty(model.SpatialData.InitialFractions.ToList());
            Assert.IsEmpty(model.BoundaryConditionSets[0].BoundaryConditions.ToList());
            Assert.IsEmpty(
                model.BoundaryConditions.OfType<FlowBoundaryCondition>()
                     .Where(bc => bc.FlowQuantity == FlowBoundaryQuantityType.SedimentConcentration)
                     .ToList());

            model.SedimentFractions.Add(new SedimentFraction() {Name = "Frac1"});
            model.SedimentFractions.Add(new SedimentFraction() {Name = "Frac2"});

            //The fraction does no longer imply an initial value
            Assert.AreEqual(2, model.SpatialData.InitialFractions.Count());

            //add sediment bc

            model.BoundaryConditionSets[0].BoundaryConditions.Add(
                new FlowBoundaryCondition(FlowBoundaryQuantityType.SedimentConcentration, BoundaryConditionDataType.TimeSeries) {SedimentFractionName = "Frac1"});

            Assert.IsNotEmpty(model.BoundaryConditionSets.ToList());
            Assert.IsNotEmpty(model.BoundaryConditions.OfType<FlowBoundaryCondition>()
                                   .Where(bc => bc.FlowQuantity == FlowBoundaryQuantityType.SedimentConcentration)
                                   .ToList());

            //clearing
            model.SedimentFractions.Clear(); // All boundary conditions with these fractions should be removed.
            Assert.IsEmpty(model.SpatialData.InitialFractions);
            Assert.IsEmpty(model.BoundaryConditions.OfType<FlowBoundaryCondition>()
                                .Where(bc => bc.FlowQuantity == FlowBoundaryQuantityType.SedimentConcentration)
                                .ToList());
        }

        [Test]
        public void AddRemoveSedimentMorphologyFractionShouldAddRemoveData()
        {
            // create
            WaterFlowFMModel model = CreateSimpleBoxModel();
            model.BoundaryConditionSets[0].BoundaryConditions.Clear();
            Assert.IsEmpty(model.SpatialData.InitialFractions.ToList());
            Assert.IsEmpty(model.BoundaryConditionSets[0].BoundaryConditions.ToList());
            Assert.IsEmpty(
                model.BoundaryConditions.OfType<FlowBoundaryCondition>()
                     .Where(bc => bc.FlowQuantity == FlowBoundaryQuantityType.MorphologyBedLevelPrescribed
                                  || bc.FlowQuantity == FlowBoundaryQuantityType.MorphologyBedLevelChangePrescribed
                                  || bc.FlowQuantity == FlowBoundaryQuantityType.MorphologyBedLoadTransport)
                     .ToList());

            model.SedimentFractions.Add(new SedimentFraction() {Name = "Frac1"});
            model.SedimentFractions.Add(new SedimentFraction() {Name = "Frac2"});

            //The fraction does no longer imply an initial value
            Assert.AreEqual(2, model.SpatialData.InitialFractions.Count());

            //add sediment bc
            model.BoundaryConditionSets[0].BoundaryConditions.Add(
                new FlowBoundaryCondition(FlowBoundaryQuantityType.MorphologyBedLevelPrescribed, BoundaryConditionDataType.TimeSeries));
            model.BoundaryConditionSets[0].BoundaryConditions.Add(
                new FlowBoundaryCondition(FlowBoundaryQuantityType.MorphologyBedLevelChangePrescribed, BoundaryConditionDataType.TimeSeries));
            model.BoundaryConditionSets[0].BoundaryConditions.Add(
                new FlowBoundaryCondition(FlowBoundaryQuantityType.MorphologyBedLoadTransport, BoundaryConditionDataType.TimeSeries));

            Assert.IsNotNull(model.BoundaryConditions);
            Assert.AreEqual(model.BoundaryConditions.ToList().Count, 3);
            Assert.AreEqual(model.BoundaryConditions.OfType<FlowBoundaryCondition>()
                                 .Where(bc => bc.FlowQuantity == FlowBoundaryQuantityType.MorphologyBedLevelPrescribed
                                              || bc.FlowQuantity == FlowBoundaryQuantityType.MorphologyBedLevelChangePrescribed
                                              || bc.FlowQuantity == FlowBoundaryQuantityType.MorphologyBedLoadTransport)
                                 .ToList().Count, 3);

            //clearing
            model.SedimentFractions.Clear(); // All boundary conditions with these fractions should be removed.
            Assert.IsEmpty(model.SpatialData.InitialFractions);
            Assert.AreEqual(model.BoundaryConditions.ToList().Count, 2);
            Assert.AreEqual(model.BoundaryConditions.OfType<FlowBoundaryCondition>()
                                 .Where(bc => bc.FlowQuantity == FlowBoundaryQuantityType.MorphologyBedLevelPrescribed
                                              || bc.FlowQuantity == FlowBoundaryQuantityType.MorphologyBedLevelChangePrescribed)
                                 .ToList().Count, 2);
            Assert.IsEmpty(model.BoundaryConditions.OfType<FlowBoundaryCondition>()
                                .Where(bc => bc.FlowQuantity == FlowBoundaryQuantityType.MorphologyBedLoadTransport).ToList());
        }

        [Test]
        public void ChangeSedimentTypeUpdateInitialConditionsTest()
        {
            // setup
            WaterFlowFMModel model = CreateSimpleBoxModel();
            Assert.IsEmpty(model.SpatialData.InitialFractions.ToList());
            var fraction = new SedimentFraction() {Name = "Frac1"};

            // set the sand sediment type to the fraction
            fraction.CurrentSedimentType = fraction.AvailableSedimentTypes.First(sed => sed.Key == "sand");
            model.SedimentFractions.Add(fraction);

            // get the properties, the first property: IsSpaciallyVarying = true
            List<ISpatiallyVaryingSedimentProperty> spatiallyVaryingProperties = fraction.CurrentSedimentType.Properties.OfType<ISpatiallyVaryingSedimentProperty>().ToList();
            Assert.IsTrue(spatiallyVaryingProperties.Any());

            // set all IsSpatiallyVarying to true -> this raises the OnPropertyChanged event
            foreach (ISpatiallyVaryingSedimentProperty sedimentProperty in spatiallyVaryingProperties)
            {
                sedimentProperty.IsSpatiallyVarying = true;
            }

            Assert.AreEqual(spatiallyVaryingProperties.Count, model.SpatialData.InitialFractions.Count());

            // Now change the sediment type to mud and repeat above steps. 
            fraction.CurrentSedimentType = fraction.AvailableSedimentTypes.First(sed => sed.Key == "mud");
            spatiallyVaryingProperties = fraction.CurrentSedimentType.Properties.OfType<ISpatiallyVaryingSedimentProperty>().ToList();

            // set all to false this again raises the OnPropertyChanged, no InitialFractions should be present
            foreach (ISpatiallyVaryingSedimentProperty sedimentProperty in spatiallyVaryingProperties)
            {
                sedimentProperty.IsSpatiallyVarying = false;
            }

            Assert.AreEqual(0, model.SpatialData.InitialFractions.Count());

            // Set all spatially varying to true, this should add as much fractions as present properties
            foreach (ISpatiallyVaryingSedimentProperty sedimentProperty in spatiallyVaryingProperties)
            {
                sedimentProperty.IsSpatiallyVarying = true;
            }

            Assert.AreEqual(spatiallyVaryingProperties.Count, model.SpatialData.InitialFractions.Count());

            // Change the sediment type back to sand, check if the number of fractions is correct 
            fraction.CurrentSedimentType = fraction.AvailableSedimentTypes.First(sed => sed.Key == "sand");
            spatiallyVaryingProperties = fraction.CurrentSedimentType.Properties.OfType<ISpatiallyVaryingSedimentProperty>().ToList();
            Assert.AreEqual(spatiallyVaryingProperties.Count, model.SpatialData.InitialFractions.Count());
        }

        [Test]
        public void ChangeSedimentTypeOrFormulaToSpatiallyVaryingCreatesAnInitialFractionWithName()
        {
            WaterFlowFMModel model = CreateSimpleBoxModel();
            Assert.IsEmpty(model.SpatialData.InitialFractions.ToList());
            var fraction = new SedimentFraction() {Name = "Frac1"};

            model.SedimentFractions.Add(fraction);
            //Get all available sediment types
            foreach (ISedimentType sedType in fraction.AvailableSedimentTypes)
            {
                fraction.CurrentSedimentType = sedType;
                //Set the spatially varying props to true
                List<ISpatiallyVaryingSedimentProperty> typesSVProps = fraction.CurrentSedimentType.Properties.OfType<ISpatiallyVaryingSedimentProperty>().ToList();
                CheckInitialConditionWithName(typesSVProps, model, fraction, typesSVProps.Count);

                //get all formula types
                List<ISedimentFormulaType> suportedFormulas = fraction.SupportedFormulaTypes;
                foreach (ISedimentFormulaType formula in suportedFormulas)
                {
                    fraction.CurrentFormulaType = formula;
                    /* Changing formula type resets Spatially Varying Formula operations */
                    Assert.AreEqual(model.SpatialData.InitialFractions.Count(), typesSVProps.Count);
                    //Set the spatially varying props to true
                    List<ISpatiallyVaryingSedimentProperty> formulaSVProps = fraction.CurrentFormulaType.Properties.OfType<ISpatiallyVaryingSedimentProperty>().ToList();
                    CheckInitialConditionWithName(formulaSVProps, model, fraction, formulaSVProps.Count + typesSVProps.Count); /* SedConc is always SpatiallyVarying*/
                }
            }
        }

        private static WaterFlowFMModel CreateSimpleBoxModel()
        {
            string mduPath = TestHelper.GetTestFilePath(@"simpleBox\simplebox.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var waterFlowFmModel = new WaterFlowFMModel();
            waterFlowFmModel.ImportFromMdu(mduPath);

            return waterFlowFmModel;
        }

        private static void CheckInitialConditionWithName(List<ISpatiallyVaryingSedimentProperty> svProps, WaterFlowFMModel model, SedimentFraction fraction, int svPropsCount)
        {
            if (svProps.Count > 0)
            {
                /*The spatially varying name should be available when changing the current sediment type or formula*/
                svProps.ForEach(f => Assert.NotNull(f.SpatiallyVaryingName));
                svProps.ForEach(f => f.IsSpatiallyVarying = true);

                //assert if the initial condition has not been added.
                Assert.AreEqual(model.SpatialData.InitialFractions.Count(), svPropsCount); /*SedConc is always Spatially varying*/

                //assert if the initial condition does not have the correct name.
                model.SpatialData.InitialFractions.ForEach(iniFrac => Assert.NotNull(iniFrac.Name));
                model.SpatialData.InitialFractions.ForEach(iniFrac => Assert.That(fraction.GetAllActiveSpatiallyVaryingPropertyNames().Contains(iniFrac.Name)));
            }
        }
    }
}