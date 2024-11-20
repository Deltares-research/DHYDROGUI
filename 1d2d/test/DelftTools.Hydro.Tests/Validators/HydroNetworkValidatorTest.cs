using System.Linq;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Validators;
using DelftTools.Utils.Validation;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.Validators
{
    [TestFixture]
    public class HydroNetworkValidatorTest
    {
        [Test]
        public void ValidateWhenTargetIsNullThrowsArgumentNullException()
        {
            // Call
            void Call() { HydroNetworkValidator.Validate(null); }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }
        
        [Test]
        public void Two_Compartments_With_The_Same_Name_Gives_Validation_Error()
        {
            // Setup
            const string nonUniqueName = "NotUnique";
            IHydroNetwork network = CreateSimpleNetworkWithTwoCompartmentsWithTheSameName(nonUniqueName);

            // Call
            ValidationReport report = HydroNetworkValidator.Validate(network);

            // Assert
            const string expectedMessage = "Several compartments with the same id exist";
            Assert.That(report.AllErrors.Any(error => error.Message.Equals(expectedMessage)), Is.True);
        }

        [Test]
        public void Two_Manholes_With_The_Same_Name_Gives_Validation_Error()
        {
            // Setup
            const string nonUniqueName = "NotUnique";
            IHydroNetwork network = CreateSimpleNetworkWithTwoManholesWithTheSameName(nonUniqueName);
            
            // Call
            ValidationReport report = HydroNetworkValidator.Validate(network);
            
            // Assert
            const string expectedMessage = "Several manholes with the same id exist";
            Assert.That(report.AllErrors.Any(error => error.Message.Equals(expectedMessage)), Is.True);
        }

        [Test]
        public void Two_SewerConnections_With_The_Same_Name_Gives_Validation_Error()
        {
            // Setup
            const string nonUniqueName = "NotUnique";
            IHydroNetwork network = CreateSimpleNetworkWithTwoSewerConnectionsWithTheSameName(nonUniqueName);

            // Call
            ValidationReport report = HydroNetworkValidator.Validate(network);
            
            // Assert
            const string expectedMessage = "Several sewer connections with the same id exist";
            Assert.That(report.AllErrors.Any(error => error.Message.Equals(expectedMessage)), Is.True);
        }
        
        [Test]
        public void Two_Routes_With_The_Same_Name_Gives_Validation_Error()
        {
            // Setup
            const string nonUniqueName = "NotUnique";
            IHydroNetwork network = new HydroNetwork();
            network.Routes.Add(new Route {Name = nonUniqueName});
            network.Routes.Add(new Route {Name = nonUniqueName});

            // Call
            ValidationReport report = HydroNetworkValidator.Validate(network);

            // Assert
            const string expectedMessage = "Several routes with the same id exist";
            Assert.That(report.AllErrors.Any(error => error.Message.Equals(expectedMessage)), Is.True);
        }

        private static IHydroNetwork CreateSimpleNetworkWithTwoSewerConnectionsWithTheSameName(string nonUniqueName)
        {
            var manhole1 = new Manhole();
            var manhole2 = new Manhole();
            var sewerConnection1 = new SewerConnection(nonUniqueName) 
            { 
                Source = manhole1,
                Target = manhole2
            };

            
            var manhole3 = new Manhole();
            var manhole4 = new Manhole();
            var sewerConnection2 = new SewerConnection(nonUniqueName) 
            { 
                Source = manhole3,
                Target = manhole4
            };

            var network = new HydroNetwork();
            network.Branches.Add(sewerConnection1);
            network.Branches.Add(sewerConnection2);
            network.Nodes.Add(manhole1);
            network.Nodes.Add(manhole2);
            network.Nodes.Add(manhole3);
            network.Nodes.Add(manhole4);

            return network;
        }

        private static IHydroNetwork CreateSimpleNetworkWithTwoManholesWithTheSameName(string nonUniqueName)
        {
            var manhole1 = new Manhole(nonUniqueName);
            var manhole2 = new Manhole(nonUniqueName);
            
            var pipe = new Pipe();
            pipe.Source = manhole1;
            pipe.Target = manhole2;
            
            var network = new HydroNetwork();
            network.Branches.Add(pipe);
            network.Nodes.Add(manhole1);
            network.Nodes.Add(manhole2);
            
            return network;
        }

        private static IHydroNetwork CreateSimpleNetworkWithTwoCompartmentsWithTheSameName(string nonUniqueName)
        {
            var manhole1 = new Manhole();
            manhole1.Compartments.Add(new Compartment(nonUniqueName));
            
            var manhole2 = new Manhole();
            manhole2.Compartments.Add(new Compartment(nonUniqueName));
            
            var pipe = new Pipe();
            pipe.Source = manhole1;
            pipe.Target = manhole2;

            var network = new HydroNetwork();
            network.Branches.Add(pipe);
            network.Nodes.Add(manhole1);
            network.Nodes.Add(manhole2);
            
            return network;
        }
    }
}