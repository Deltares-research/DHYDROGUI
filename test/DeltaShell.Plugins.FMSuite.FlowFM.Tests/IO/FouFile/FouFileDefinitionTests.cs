using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.FouFile;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.FouFile
{
    [TestFixture]
    public class FouFileDefinitionTests
    {
        [Test]
        public void Variables_IsNotEmpty()
        {
            var fouFileDefinition = new FouFileDefinition();
            Assert.IsTrue(fouFileDefinition.Variables.Any());
        }

        [Test]
        public void ModelPropertyNames_IsNotEmpty()
        {
            var fouFileDefinition = new FouFileDefinition();
            Assert.IsTrue(fouFileDefinition.ModelPropertyNames.Any());
        }

        [Test]
        public void GetModelPropertyName_VariableIsNull_ThrowsArgumentNullException()
        {
            var fouFileDefinition = new FouFileDefinition();
            Assert.Throws<ArgumentNullException>(() => fouFileDefinition.GetModelPropertyName(null));
        }

        [Test]
        public void GetModelPropertyName_UnknownVariable_ReturnsNull()
        {
            var fouFileDefinition = new FouFileDefinition();
            var fouFileVariable = new FouFileVariable();

            string propertyName = fouFileDefinition.GetModelPropertyName(fouFileVariable);

            Assert.IsNull(propertyName);
        }

        [Test]
        public void GetModelPropertyName_SupportedVariables_ReturnsPropertyNames()
        {
            var fouFileDefinition = new FouFileDefinition();

            IEnumerable<FouFileVariable> supportedVariables = fouFileDefinition.Variables.ToArray();
            IEnumerable<string> propertyNames = supportedVariables.Select(x => fouFileDefinition.GetModelPropertyName(x)).Where(x => x != null).ToArray();

            Assert.AreEqual(supportedVariables.Count(), propertyNames.Count());
        }

        [Test]
        public void GetModelPropertyName_VariableWithUpperCase_ReturnsPropertyName()
        {
            var fouFileDefinition = new FouFileDefinition();
            var fouFileVariable = new FouFileVariable { Name = "UC", EllipticParameters = "MAX" };

            string propertyName = fouFileDefinition.GetModelPropertyName(fouFileVariable);

            Assert.AreEqual("WriteUcMaximum", propertyName);
        }
    }
}