using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Reflection;
using DeltaShell.Sobek.Readers.Readers.SobekWaqReaders;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekWaqReaders
{
    [TestFixture]
    public class SobekWaqUserDefinedTypesReaderTest
    {
        # region Sobek212

        [Test]
        public void ReadUserDefinedTypeDefinitionsFromSobek212()
        {
            var userNodeTypes = (IEnumerable<SobekWaqUserDefinedTypesReader.NetterUserDefinedTypeObject>)TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqUserDefinedTypesReader), "ParseUserTypes", new object[] { GetValidTestFile(), true });
            Assert.AreEqual(2, userNodeTypes.Count());

            var nbType1 = userNodeTypes.Where(t => t.TypeId == "SBK_BOUNDARY_BNTYPE1").FirstOrDefault();
            Assert.AreEqual("BNType1", nbType1.TypeName);
            Assert.AreEqual("Boundary Flow", nbType1.Fraction);

            var lsType1 = userNodeTypes.Where(t => t.TypeId == "SBK_LATERALFLOW_LSTYPE1").FirstOrDefault();
            Assert.AreEqual("LSType1", lsType1.TypeName);
            Assert.AreEqual("MyFraction", lsType1.Fraction);

            var userBranchTypes = (IEnumerable<SobekWaqUserDefinedTypesReader.NetterUserDefinedTypeObject>)TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqUserDefinedTypesReader), "ParseUserTypes", new object[] { GetValidTestFile(), false });
            Assert.AreEqual(3, userBranchTypes.Count());

            var branchNormalType = userBranchTypes.Where(t => t.TypeId == "SBK_CHANNEL_BRANCHTYPENORMAL").FirstOrDefault();
            Assert.AreEqual("BranchTypeNormal", branchNormalType.TypeName);
            Assert.AreEqual("Normal", branchNormalType.SurfaceWaterType);

            var branchNotNormalType = userBranchTypes.Where(t => t.TypeId == "SBK_CHANNEL_BRANCHTYPENOTNORMAL").FirstOrDefault();
            Assert.AreEqual("NotSoNormal", branchNotNormalType.SurfaceWaterType);

            var branchTestType = userBranchTypes.Where(t => t.TypeId == "SBK_CHANNEL_BRANCHTYPETEST").FirstOrDefault();
            Assert.AreEqual("BranchTypeTest", branchTestType.TypeName);
            Assert.AreEqual("swuTest", branchTestType.SurfaceWaterType);
            Assert.AreEqual("fractionTest", branchTestType.Fraction);
        }

        [Test]
        public void ReadUserDefinedTypeDefinitionsFromSobek212Section()
        {
            var section = (IEnumerable<string>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqUserDefinedTypesReader), "GetSection", new object[] { GetValidTestFile(), "User Node Types"});
            Assert.AreEqual(8, section.Count());

            section = (IEnumerable<string>) TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqUserDefinedTypesReader), "GetSection", new object[] { GetValidTestFile(), "User Branch Types" });
            Assert.AreEqual(15, section.Count());
        }

        [Test]
        public void ReadUserDefinedTypeDefinitionsFromSobek212MissingNodeSection()
        {
            var error = Assert.Throws<FormatException>(() =>
            {
                try
                {
                    TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqUserDefinedTypesReader), "ParseUserTypes", new object[] { GetTestFileWithoutNodeSection(), true });
                }
                catch (Exception e)
                {
                    throw e.InnerException;
                }
            });
            Assert.AreEqual("Section [User Node Types] was not found", error.Message);
        }

        [Test]
        public void ReadUserDefinedTypeDefinitionsFromSobek212MissingBranchSection()
        {
            var error = Assert.Throws<FormatException>(() =>
            {
                try
                {
                    TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqUserDefinedTypesReader), "ParseUserTypes", new object[] { GetTestFileWithoutBranchSection(), false });
                }
                catch (Exception e)
                {
                    throw e.InnerException;
                }
            });
            Assert.AreEqual("Section [User Branch Types] was not found", error.Message);
        }

        [Test]
        public void ReadUserDefinedTypeDefinitionsFromSobek212MissingNumberOfTypes()
        {
            var error = Assert.Throws<FormatException>(() =>
            {
                try
                {
                    TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqUserDefinedTypesReader), "ParseUserTypes", new object[] { GetTestFileWithoutNumberOfTypesInNodeSection(), true });
                }
                catch (Exception e)
                {
                    throw e.InnerException;
                }
            });
            Assert.AreEqual("'Number of Types=' was not found in section [User Node Types]", error.Message);
        }

        [Test]
        public void ReadUserDefinedTypeDefinitionsFromSobek212MissingId()
        {
            var error = Assert.Throws<FormatException>(() =>
            {
                try
                {
                    TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqUserDefinedTypesReader), "ParseUserTypes", new object[] { GetTestFileWithoutIdInNodeSection(), true });
                }
                catch (Exception e)
                {
                    throw e.InnerException;
                }
            });
            Assert.AreEqual("'1 ID=' was not found in section [User Node Types]", error.Message);
        }

        [Test]
        public void ReadUserDefinedTypeDefinitionsFromSobek212MissingFractionType()
        {
            var error = Assert.Throws<FormatException>(() =>
            {
                try
                {
                    TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqUserDefinedTypesReader), "ParseUserTypes", new object[] { GetTestFileWithoutFractionTypeInNodeSection(), true });
                }
                catch (Exception e)
                {
                    throw e.InnerException;
                }
            });
            Assert.AreEqual("'1 Boundary 1=' was not found in section [User Node Types]", error.Message);
        }

        [Test]
        public void ReadUserDefinedTypeDefinitionsFromSobek212MissingSurfaceWaterType()
        {
            var error = Assert.Throws<FormatException>(() =>
            {
                try
                {
                    TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqUserDefinedTypesReader), "ParseUserTypes", new object[] { GetTestFileWithoutSurfaceWaterTypeInBranchSection(), false });
                }
                catch (Exception e)
                {
                    throw e.InnerException;
                }
            });
            Assert.AreEqual("'2 SurfaceWaterType=' was not found in section [User Branch Types]", error.Message);
        }

        [Test]
        public void ReadUserDefinedTypeDefinitionsFromSobek212MissingNumberOfBoundaries()
        {
            var error = Assert.Throws<FormatException>(() =>
            {
                try
                {
                    TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqUserDefinedTypesReader), "ParseUserTypes", new object[] { GetTestFileWithoutNumberOfBoundariesInBranchSection(), false });
                }
                catch (Exception e)
                {
                    throw e.InnerException;
                }
            });
            Assert.AreEqual("'2 Number of boundaries=' was not found in section [User Branch Types]", error.Message);
        }

        [Test]
        public void ReadUserDefinedTypeDefinitionsFromSobek212MissingFractionTypeInBranchSection()
        {
            var error = Assert.Throws<FormatException>(() =>
            {
                try
                {
                    TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqUserDefinedTypesReader), "ParseUserTypes", new object[] { GetTestFileWithoutFractionTypeInBranchSection(), false });
                }
                catch (Exception e)
                {
                    throw e.InnerException;
                }
            });
            Assert.AreEqual("'2 Boundary 1=' was not found in section [User Branch Types]", error.Message);
        }

        private static string GetValidTestFile()
        {
            return "[User Project Identification]" + Environment.NewLine +
                   "Version=3.10" + Environment.NewLine +
                   "" + Environment.NewLine +
                   "[User Node Types]" + Environment.NewLine +
                   "Number of Types=2" + Environment.NewLine +
                   "1 Type=BNType1" + Environment.NewLine +
                   "1 ID=SBK_BOUNDARY_BNTYPE1" + Environment.NewLine +
                   "1 Boundary 1=Boundary Flow" + Environment.NewLine +
                   "2 Type=LSType1" + Environment.NewLine +
                   "2 ID=SBK_LATERALFLOW_LSTYPE1" + Environment.NewLine +
                   "2 Boundary 1=MyFraction" + Environment.NewLine +
                   "" + Environment.NewLine +
                   "[User Branch Types]" + Environment.NewLine +
                   "Number of Types=3" + Environment.NewLine +
                   "1 Type=BranchTypeNormal" + Environment.NewLine +
                   "1 ID=SBK_CHANNEL_BRANCHTYPENORMAL" + Environment.NewLine +
                   "1 SurfaceWaterType=Normal" + Environment.NewLine +
                   "1 Number of boundaries=0" + Environment.NewLine +
                   "2 Type=BranchTypeNotNormal" + Environment.NewLine +
                   "2 ID=SBK_CHANNEL_BRANCHTYPENOTNORMAL" + Environment.NewLine +
                   "2 SurfaceWaterType=NotSoNormal" + Environment.NewLine +
                   "2 Number of boundaries=0" + Environment.NewLine +
                   "3 Type=BranchTypeTest" + Environment.NewLine +
                   "3 ID=SBK_CHANNEL_BRANCHTYPETEST" + Environment.NewLine +
                   "3 SurfaceWaterType=swuTest" + Environment.NewLine +
                   "3 Number of boundaries=1" + Environment.NewLine +
                   "3 Boundary 1=fractionTest" + Environment.NewLine;

        }

        private static string GetTestFileWithoutNodeSection()
        {
            return "[User Project Identification]" + Environment.NewLine +
                   "Version=3.10" + Environment.NewLine +
                   "" + Environment.NewLine +
                   "[User Branch Types]" + Environment.NewLine +
                   "Number of Types=0" + Environment.NewLine;
        }

        private static string GetTestFileWithoutBranchSection()
        {
            return "[User Project Identification]" + Environment.NewLine +
                   "Version=3.10" + Environment.NewLine +
                   "" + Environment.NewLine +
                   "[User Node Types]" + Environment.NewLine +
                   "Number of Types=0" + Environment.NewLine;
        }

        private static string GetTestFileWithoutNumberOfTypesInNodeSection()
        {
            return "[User Project Identification]" + Environment.NewLine +
                   "Version=3.10" + Environment.NewLine +
                   "" + Environment.NewLine +
                   "[User Node Types]" + Environment.NewLine +
                   "1 Type=BNType1" + Environment.NewLine +
                   "1 Boundary 1=Boundary Flow" + Environment.NewLine +
                   "2 Type=LSType1" + Environment.NewLine +
                   "2 Boundary 1=MyFraction" + Environment.NewLine +
                   "" + Environment.NewLine +
                   "[User Branch Types]" + Environment.NewLine +
                   "Number of Types=0" + Environment.NewLine;
        }

        private static string GetTestFileWithoutIdInNodeSection()
        {
            return "[User Project Identification]" + Environment.NewLine +
                   "Version=3.10" + Environment.NewLine +
                   "" + Environment.NewLine +
                   "[User Node Types]" + Environment.NewLine +
                   "Number of Types=1" + Environment.NewLine +
                   "1 Type=BNType1" + Environment.NewLine +
                   "1 Boundary 1=Boundary Flow" + Environment.NewLine;
        }

        private static string GetTestFileWithoutFractionTypeInNodeSection()
        {
            return "[User Project Identification]" + Environment.NewLine +
                   "Version=3.10" + Environment.NewLine +
                   "" + Environment.NewLine +
                   "[User Node Types]" + Environment.NewLine +
                   "Number of Types=1" + Environment.NewLine +
                   "1 ID=SBK_BOUNDARY_BNTYPE1" + Environment.NewLine +
                   "1 Type=BNType1" + Environment.NewLine;
        }

        private static string GetTestFileWithoutSurfaceWaterTypeInBranchSection()
        {
            return "[User Project Identification]" + Environment.NewLine +
                   "Version=3.10" + Environment.NewLine +
                   "" + Environment.NewLine +
                   "[User Branch Types]" + Environment.NewLine +
                   "Number of Types=2" + Environment.NewLine +
                   "1 Type=BranchTypeNormal" + Environment.NewLine +
                   "1 ID=SBK_CHANNEL_BRANCHTYPENORMAL" + Environment.NewLine +
                   "1 SurfaceWaterType=Normal" + Environment.NewLine +
                   "1 Number of boundaries=0" + Environment.NewLine +
                   "2 Type=BranchTypeNotNormal" + Environment.NewLine +
                   "2 ID=SBK_CHANNEL_BRANCHTYPENOTNORMAL" + Environment.NewLine +
                   "2 Number of boundaries=0" + Environment.NewLine;
        }

        private static string GetTestFileWithoutNumberOfBoundariesInBranchSection()
        {
            return "[User Project Identification]" + Environment.NewLine +
                   "Version=3.10" + Environment.NewLine +
                   "" + Environment.NewLine +
                   "[User Branch Types]" + Environment.NewLine +
                   "Number of Types=2" + Environment.NewLine +
                   "1 Type=BranchTypeNormal" + Environment.NewLine +
                   "1 ID=SBK_CHANNEL_BRANCHTYPENORMAL" + Environment.NewLine +
                   "1 SurfaceWaterType=Normal" + Environment.NewLine +
                   "1 Number of boundaries=0" + Environment.NewLine +
                   "2 Type=BranchTypeNotNormal" + Environment.NewLine +
                   "2 ID=SBK_CHANNEL_BRANCHTYPENOTNORMAL" + Environment.NewLine;
        }

        private static string GetTestFileWithoutFractionTypeInBranchSection()
        {
            return "[User Project Identification]" + Environment.NewLine +
                   "Version=3.10" + Environment.NewLine +
                   "" + Environment.NewLine +
                   "[User Branch Types]" + Environment.NewLine +
                   "Number of Types=2" + Environment.NewLine +
                   "1 Type=BranchTypeNormal" + Environment.NewLine +
                   "1 ID=SBK_CHANNEL_BRANCHTYPENORMAL" + Environment.NewLine +
                   "1 SurfaceWaterType=Normal" + Environment.NewLine +
                   "1 Number of boundaries=0" + Environment.NewLine +
                   "2 Type=BranchTypeNotNormal" + Environment.NewLine +
                   "2 ID=SBK_CHANNEL_BRANCHTYPENOTNORMAL" + Environment.NewLine +
                   "2 Number of boundaries=1" + Environment.NewLine;
        }

        # endregion
    }
}
