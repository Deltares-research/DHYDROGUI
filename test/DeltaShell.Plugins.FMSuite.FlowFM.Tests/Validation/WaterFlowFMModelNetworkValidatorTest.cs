using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Validation;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation
{
    [TestFixture]
    public class WaterFlowFMModelNetworkValidatorTest
    {

        private static IEnumerable<InvalidCompartmentTestCaseData> InvalidCompartmentTestCaseSource
        {
            get
            {
                yield return new InvalidCompartmentTestCaseData
                {
                    ConfigureCompartments = hydroNetwork =>
                    {
                        var manhole = new Manhole();
                        var compartment = new Compartment
                        {
                            ManholeLength = -10,
                            ManholeWidth = 11,
                            FloodableArea = 12
                        };

                        hydroNetwork.Nodes.Add(manhole);
                        manhole.Compartments.Add(compartment);
                    },
                    ExpectedMessage = "Length must be larger than 0",
                    ExpectedSubject = hydroNetwork => hydroNetwork.Compartments.First(),
                    Severity = ValidationSeverity.Error
                    
                };
                yield return new InvalidCompartmentTestCaseData
                {
                    ConfigureCompartments = hydroNetwork =>
                    {
                        var manhole = new Manhole();
                        var compartment = new Compartment
                        {
                            ManholeLength = 0,
                            ManholeWidth = 11,
                            FloodableArea = 12
                        };

                        hydroNetwork.Nodes.Add(manhole);
                        manhole.Compartments.Add(compartment);
                    },
                    ExpectedMessage = "Length must be larger than 0",
                    ExpectedSubject = hydroNetwork => hydroNetwork.Compartments.First(),
                    Severity = ValidationSeverity.Error
                };
                yield return new InvalidCompartmentTestCaseData
                {
                    ConfigureCompartments = hydroNetwork =>
                    {
                        var manhole = new Manhole();
                        var compartment = new Compartment
                        {
                            ManholeLength = 10,
                            ManholeWidth = -11,
                            FloodableArea = 12
                        };

                        hydroNetwork.Nodes.Add(manhole);
                        manhole.Compartments.Add(compartment);
                    },
                    ExpectedMessage = "Width / diameter must be larger than 0",
                    ExpectedSubject = hydroNetwork => hydroNetwork.Compartments.First(),
                    Severity = ValidationSeverity.Error
                };
                yield return new InvalidCompartmentTestCaseData
                {
                    ConfigureCompartments = hydroNetwork =>
                    {
                        var manhole = new Manhole();
                        var compartment = new Compartment
                        {
                            ManholeLength = 10,
                            ManholeWidth = 0,
                            FloodableArea = 12
                        };

                        hydroNetwork.Nodes.Add(manhole);
                        manhole.Compartments.Add(compartment);
                    },
                    ExpectedMessage = "Width / diameter must be larger than 0",
                    ExpectedSubject = hydroNetwork => hydroNetwork.Compartments.First(),
                    Severity = ValidationSeverity.Error
                };
                yield return new InvalidCompartmentTestCaseData
                {
                    ConfigureCompartments = hydroNetwork =>
                    {
                        var manhole = new Manhole();
                        var compartment = new Compartment
                        {
                            ManholeLength = 10,
                            ManholeWidth = 11,
                            FloodableArea = -12
                        };

                        hydroNetwork.Nodes.Add(manhole);
                        manhole.Compartments.Add(compartment);
                    },
                    ExpectedMessage = "Street storage area is set to 0. Recommended to use storage type closed instead of reservoir",
                    ExpectedSubject = hydroNetwork => hydroNetwork.Compartments.First(),
                    Severity = ValidationSeverity.Warning
                };
                yield return new InvalidCompartmentTestCaseData
                {
                    ConfigureCompartments = hydroNetwork =>
                    {
                        var manhole = new Manhole();
                        var compartment = new Compartment
                        {
                            ManholeLength = 10,
                            ManholeWidth = 11,
                            FloodableArea = 0
                        };
 
                        hydroNetwork.Nodes.Add(manhole);
                        manhole.Compartments.Add(compartment);
                    },
                    ExpectedMessage = "Street storage area is set to 0. Recommended to use storage type closed instead of reservoir",
                    ExpectedSubject = hydroNetwork => hydroNetwork.Compartments.First(),
                    Severity =  ValidationSeverity.Warning
                };
            }
        }

        public class InvalidCompartmentTestCaseData
        {
            public Action<IHydroNetwork> ConfigureCompartments { get; set; }

            public string ExpectedMessage { get; set; }

            public Func<IHydroNetwork, object> ExpectedSubject { get; set; }
            public ValidationSeverity Severity { get; set; }
        }
    }
}