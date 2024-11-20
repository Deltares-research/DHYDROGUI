using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.DataObjects.SubstanceProcessLibrary
{
    [TestFixture]
    public class SubstanceProcessLibraryTest
    {
        [Test]
        public void TestGetDifferentTypeOfSubstances()
        {
            var substanceProcessLibrary = new DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary.SubstanceProcessLibrary()
            {
                Name = "Library",
                Substances =
                {
                    new WaterQualitySubstance
                    {
                        Name = "A",
                        Active = true
                    },
                    new WaterQualitySubstance
                    {
                        Name = "B",
                        Active = false
                    }
                }
            };

            IEnumerable<WaterQualitySubstance> activeSubstances = substanceProcessLibrary.ActiveSubstances;
            Assert.AreEqual(1, activeSubstances.Count());
            Assert.AreEqual("A", activeSubstances.ElementAt(0).Name);

            IEnumerable<WaterQualitySubstance> inActiveSubstances = substanceProcessLibrary.InActiveSubstances;
            Assert.AreEqual(1, inActiveSubstances.Count());
            Assert.AreEqual("B", inActiveSubstances.ElementAt(0).Name);
        }

        [Test]
        public void TestToString()
        {
            var substanceProcessLibrary = new DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary.SubstanceProcessLibrary
            {
                Substances =
                {
                    new WaterQualitySubstance {Name = "A"},
                    new WaterQualitySubstance {Name = "B"},
                    new WaterQualitySubstance {Name = "C"},
                    new WaterQualitySubstance {Name = "D"}
                },
                Processes =
                {
                    new WaterQualityProcess {Name = "E"},
                    new WaterQualityProcess {Name = "F"},
                    new WaterQualityProcess {Name = "G"},
                    new WaterQualityProcess {Name = "H"}
                },
                Parameters =
                {
                    new WaterQualityParameter {Name = "I"},
                    new WaterQualityParameter {Name = "J"},
                    new WaterQualityParameter {Name = "K"}
                },
                OutputParameters =
                {
                    new WaterQualityOutputParameter {Name = "L"},
                    new WaterQualityOutputParameter {Name = "M"},
                    new WaterQualityOutputParameter {Name = "N"},
                    new WaterQualityOutputParameter {Name = "O"},
                    new WaterQualityOutputParameter {Name = "P"}
                }
            };

            const string expectedString = "Substances\n" +
                                          "A\n" +
                                          "B\n" +
                                          "C\n" +
                                          "D\n" +
                                          "\n" +
                                          "Processes\n" +
                                          "E\n" +
                                          "F\n" +
                                          "G\n" +
                                          "H\n" +
                                          "\n" +
                                          "Parameters\n" +
                                          "I\n" +
                                          "J\n" +
                                          "K\n" +
                                          "\n" +
                                          "Output parameters\n" +
                                          "L\n" +
                                          "M\n" +
                                          "N\n" +
                                          "O\n" +
                                          "P\n" +
                                          "";

            Assert.AreEqual(expectedString, substanceProcessLibrary.ToString());
        }

        [Test]
        public void TestClone()
        {
            var substanceProcessLibrary = new DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary.SubstanceProcessLibrary
            {
                Id = 2,
                Name = "Name",
                Substances = {new WaterQualitySubstance()},
                Parameters = {new WaterQualityParameter()},
                Processes = {new WaterQualityProcess()},
                OutputParameters = {new WaterQualityOutputParameter()}
            };

            var substanceProcessLibraryClone = substanceProcessLibrary.Clone() as DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary.SubstanceProcessLibrary;

            Assert.IsNotNull(substanceProcessLibraryClone);
            Assert.AreNotSame(substanceProcessLibrary, substanceProcessLibraryClone);
            Assert.AreEqual(0, substanceProcessLibraryClone.Id);
            Assert.AreEqual(substanceProcessLibrary.Name, substanceProcessLibraryClone.Name);

            // Substances
            Assert.AreEqual(1, substanceProcessLibraryClone.Substances.Count);
            Assert.AreNotSame(substanceProcessLibrary.Substances.First(), substanceProcessLibraryClone.Substances.First());

            // Parameters
            Assert.AreEqual(1, substanceProcessLibraryClone.Parameters.Count);
            Assert.AreNotSame(substanceProcessLibrary.Parameters.First(), substanceProcessLibraryClone.Parameters.First());

            // Processes
            Assert.AreEqual(1, substanceProcessLibraryClone.Processes.Count);
            Assert.AreNotSame(substanceProcessLibrary.Processes.First(), substanceProcessLibraryClone.Processes.First());

            // Output parameters
            Assert.AreEqual(1, substanceProcessLibraryClone.OutputParameters.Count);
            Assert.AreNotSame(substanceProcessLibrary.OutputParameters.First(), substanceProcessLibraryClone.OutputParameters.First());
        }

        [Test]
        public void TestNotifyPropertyChangedAfterOutputParameterPropertyChanged()
        {
            var counter = 0;
            var substanceProcessLibrary = new DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary.SubstanceProcessLibrary
            {
                Name = "Library",
                OutputParameters = {new WaterQualityOutputParameter {Name = "Parameter"}}
            };

            ((INotifyPropertyChange) substanceProcessLibrary).PropertyChanged += (s, e) => counter++;

            substanceProcessLibrary.OutputParameters[0].ShowInHis = true;

            Assert.AreEqual(1, counter);
        }
    }
}