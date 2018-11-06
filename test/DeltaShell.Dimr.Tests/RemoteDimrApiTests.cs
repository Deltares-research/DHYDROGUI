using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DelftTools.Utils.Remoting;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Dimr.Tests
{
    [TestFixture()]
    class RemoteDimrApiTests
    {
        /// <summary>
        /// GIVEN
        ///   A DimrApi mock which throws a InvalidOperationException when
        ///   disposed is called AND
        ///   A RemoteDimrApi containing this mocked DimrApi
        /// WHEN
        ///   Disposed is called on this RemoteDimrApi
        /// THEN
        ///   An debug message is logged AND
        ///   The RemoteDimrApi is properly disposed off
        /// </summary>
        /// <remarks>
        /// Currently, this test observes quite a bit of the private space
        /// of the RemoteDimrApi, which is not good test design.
        /// </remarks>
        [Test]
        public void GivenADimrApiWhichThrowsAnInvalidOperationExceptionWhenDisposedIsCalledAndARemoteDimrApiContainingThisDimrApi_WhenDisposedIsCalledOnThisRemoteDimrApi_ThenADebugMessageIsLoggedAndTheRemoteDimrApiIsProperlyDisposedOff()
        {
            // Given
            var mocks = new MockRepository();
            var dimrApi = mocks.StrictMock<IDimrApi>();

            const string msg = "Remote process not available / terminated during call Dispose";
            dimrApi.Expect(m => m.Dispose())
                .Throw(new InvalidOperationException(msg))
                .Repeat.Any();

            var remoteDimrApi = new RemoteDimrApi();

            // clean up api object 
            const string apiFieldName = "api";
            Assert.That(TypeUtils.HasField(typeof(RemoteDimrApi), apiFieldName));

            var privateApi = TypeUtils.GetField(remoteDimrApi, apiFieldName);
            RemoteInstanceContainer.RemoveInstance(privateApi);

            // Set the variable inside the remoteDimrApi and 
            TypeUtils.SetField(remoteDimrApi, apiFieldName, dimrApi);

            mocks.ReplayAll();
            // When
            TestHelper.AssertAtLeastOneLogMessagesContains(() => remoteDimrApi.Dispose(), msg);
            
            // Then
            mocks.VerifyAll();
            
            // Verify private variables
            const string disposedFieldName = "disposed";
            Assert.That(TypeUtils.HasField(typeof(RemoteDimrApi), disposedFieldName));

            var disposedVal = TypeUtils.GetField(remoteDimrApi, disposedFieldName);
            Assert.That(disposedVal, Is.True);

            var privateApiAfterDispose = TypeUtils.GetField(remoteDimrApi, apiFieldName);
            Assert.That(privateApiAfterDispose, Is.Null);
        }
    }
}
