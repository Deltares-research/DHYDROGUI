using System;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.Spatial;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Spatial
{
    [TestFixture]
    public class SpatialOperationHelperTest
    {
        [Test]
        public void MakeNamesUniquePerSet_OperationSetNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => SpatialOperationHelper.MakeNamesUniquePerSet(null);
            
            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("operationSet"));
        }
        [Test]
        public void MakeNamesUniquePerSet_MakesNamesUnique()
        {
            var operationSet1 = Substitute.For<ISpatialOperationSet>();
            var operationSet2 = Substitute.For<ISpatialOperationSet>();
            var operationSet3 = Substitute.For<ISpatialOperationSet>();
            var operation1 = CreateOperation("operation_name");
            var operation2 = CreateOperation("operation_name");
            var operation3 = CreateOperation("operation_name");
            var operation4 = CreateOperation("operation_name");

            operationSet1.Operations = new EventedList<ISpatialOperation>
            {
                operationSet2,
                operationSet3,
                operation1,
                operation2
            };

            operationSet2.Operations = new EventedList<ISpatialOperation>
            {
                operation3,
                operation4
            };
            
            SpatialOperationHelper.MakeNamesUniquePerSet(operationSet1);
            
            Assert.That(operationSet1.Name, Is.Empty);
            Assert.That(operationSet2.Name, Is.EqualTo("set"));
            Assert.That(operationSet3.Name, Is.EqualTo("set 1"));
            Assert.That(operation1.Name, Is.EqualTo("operation_name"));
            Assert.That(operation2.Name, Is.EqualTo("operation_name 1"));
            Assert.That(operation3.Name, Is.EqualTo("operation_name"));
            Assert.That(operation4.Name, Is.EqualTo("operation_name 1"));

        }

        private ISpatialOperation CreateOperation(string name)
        {
            var operation = Substitute.For<ISpatialOperation>();
            operation.Name = name;

            return operation;
        }
    }
    

}