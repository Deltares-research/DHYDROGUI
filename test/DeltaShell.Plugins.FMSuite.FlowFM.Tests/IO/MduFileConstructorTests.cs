using System;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO 
{
    [TestFixture]
    public class MduFileConstructorTests
    {
        private const string staticFieldNameMduFileFMDllVersion = "FMDllVersion"; 
        private const string exceptionMessage = "Testing API";
        private IFlexibleMeshModelApi api;
        
        [SetUp]
        public void Setup()
        {
            api = Substitute.For<IFlexibleMeshModelApi>();
            TypeUtils.SetStaticField(typeof(MduFile), staticFieldNameMduFileFMDllVersion, null);
        }

        [Test]
        public void GivenMduFileConstruct_WhenInjectsCorrectIFlexibleMeshModelApiCallGetMyVersionString_ThenNotThrowAndLogErrorMessage()
        {
            // Arrange
            const string myVersion = "1.0";
            api.GetVersionString().Returns(myVersion);
            
            // Act & Assert
            string message = string.Format(Resources.MduFile_MduFile_Error_retrieving_FM_Dll_version___0_, exceptionMessage);
            
            Assert.That(TestHelper.GetAllRenderedMessages(() => _ = new MduFile(api)), Does.Not.Contain(message).IgnoreCase);
            Assert.That(TypeUtils.GetStaticField<string>(typeof(MduFile), staticFieldNameMduFileFMDllVersion), Is.EqualTo(myVersion));
        }

        [Test]
        public void GivenMduFileConstruct_WhenInjectsIncorrectIFlexibleMeshModelApiCallGetMyVersionString_ThenThrowAndLogErrorMessage()
        {
            // Arrange
            api.GetVersionString().Throws(new Exception(exceptionMessage));
            
            // Act & Assert
            TestHelper.AssertAtLeastOneLogMessagesContains(() => new MduFile(api), string.Format(Resources.MduFile_MduFile_Error_retrieving_FM_Dll_version___0_, exceptionMessage));
            
            Assert.That(TypeUtils.GetStaticField<string>(typeof(MduFile), staticFieldNameMduFileFMDllVersion), Is.EqualTo(Resources.MduFile_MduFile_Unknown_DFlowFMDll_version));
        }

    }
}