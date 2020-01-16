using System;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class InitalSaltConditionsReaderTest
    {
        [Test]
        public void ReadSaltInitialConditionConstant()
        {
            const string saltBoundaryText = @"STIN id '25' nm '(null)' ci '10' ty 0 co co 0 56 9.9999e+009 stin";

            var saltIc = InitalSaltConditionsReader.GetFlowInitialCondition(saltBoundaryText);

            Assert.IsFalse(saltIc.IsGlobalDefinition);
            Assert.AreEqual("25", saltIc.Id);
            Assert.AreEqual("10", saltIc.BranchId);
            // ty 0 = salt (1 = chloride)
            Assert.AreEqual(SaltConcentrationType.Salt, saltIc.SaltConcentrationType);
            Assert.IsTrue(saltIc.Salt.IsConstant);
            Assert.AreEqual(56.0, saltIc.Salt.Constant, 1.0e-6);
        }

        [Test]
        public void ReadSaltInitialFunctionOfLocationOnBranch()
        {
            var saltBoundaryText =
                @"STIN id '24' nm '(null)' ci '11' ty 0 co co 2 9.9999e+009 9.9999e+009 'Initial Salt Concentration on Branch <omhoog> with length: 70.7' PDIN 0 0 '' pdin CLTT 'Location' 'Concentration' cltt CLID '(null)' '(null)' clid TBLE " +
                Environment.NewLine +
                @"10 11 < " + Environment.NewLine +
                @"50 51 < " + Environment.NewLine +
                @"55 10 < " + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @" stin";

            var saltIc = InitalSaltConditionsReader.GetFlowInitialCondition(saltBoundaryText);

            Assert.IsFalse(saltIc.IsGlobalDefinition);
            Assert.AreEqual("24", saltIc.Id);
            Assert.AreEqual("11", saltIc.BranchId);
            // ty 0 = salt (1 = chloride)
            Assert.AreEqual(SaltConcentrationType.Salt, saltIc.SaltConcentrationType);
            Assert.IsFalse(saltIc.Salt.IsConstant);
            Assert.AreEqual(3, saltIc.Salt.Data.Rows.Count);
            Assert.AreEqual(10.0, saltIc.Salt.Data.Rows[0][0]);
            Assert.AreEqual(11.0, saltIc.Salt.Data.Rows[0][1]);
            Assert.AreEqual(55.0, saltIc.Salt.Data.Rows[2][0]);
            Assert.AreEqual(10.0, saltIc.Salt.Data.Rows[2][1]);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ReadSaltBoundaryFile()
        {
            var path = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowFMModelImporterTest).Assembly, @"ReModels\NatSobek.sbk\6\DEFICN.4");
            var saltInitialConditions = new InitalSaltConditionsReader().Read(path);
            Assert.AreEqual(126, saltInitialConditions.Count());
            var initialCondition = saltInitialConditions.FirstOrDefault(sb => sb.Id == "P_4");
            Assert.IsNotNull(initialCondition);
            Assert.IsFalse(initialCondition.Salt.IsConstant);
            Assert.AreEqual(2, initialCondition.Salt.Data.Rows.Count);
        }

    }
}
