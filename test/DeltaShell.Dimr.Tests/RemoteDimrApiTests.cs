using DelftTools.Utils.Reflection;
using DelftTools.Utils.Remoting;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Dimr.Tests
{
    [TestFixture]
    internal class RemoteDimrApiTests
    {
        /// <summary>
        /// GIVEN
        /// A DimrApi mock which throws a InvalidOperationException when
        /// disposed is called AND
        /// A RemoteDimrApi containing this mocked DimrApi
        /// WHEN
        /// Disposed is called on this RemoteDimrApi
        /// THEN
        /// An debug message is logged AND
        /// The RemoteDimrApi is properly disposed off
        /// </summary>
        /// <remarks>
        /// Currently, this test observes quite a bit of the private space
        /// of the RemoteDimrApi, which is not good test design.
        /// </remarks>
        [Test]
        public void GivenADimrApiWhichThrowsAnInvalidOperationExceptionAndARemoteDimrApiContainingThisMockedDimrApi_WhenDisposedIsCalledOnThisRemoteDimrApi_ThenADebugMessageIsLoggedAndTheRemoteDimrApiIsProperlyDisposedOff()
        {
            // Given
            var mocks = new MockRepository();
            var dimrApi = mocks.StrictMock<IDimrApi>();

            var remoteDimrApi = new RemoteDimrApi();

            // clean up api object 
            const string apiFieldName = "api";
            Assert.That(TypeUtils.HasField(typeof(RemoteDimrApi), apiFieldName));

            object privateApi = TypeUtils.GetField(remoteDimrApi, apiFieldName);
            RemoteInstanceContainer.RemoveInstance(privateApi);

            // Set the variable inside the remoteDimrApi and 
            TypeUtils.SetField(remoteDimrApi, apiFieldName, dimrApi);

            mocks.ReplayAll();
            // When
            remoteDimrApi.Dispose();

            // Then
            mocks.VerifyAll();

            // Verify private variables
            const string disposedFieldName = "disposed";
            Assert.That(TypeUtils.HasField(typeof(RemoteDimrApi), disposedFieldName));

            object disposedVal = TypeUtils.GetField(remoteDimrApi, disposedFieldName);
            Assert.That(disposedVal, Is.True);

            object privateApiAfterDispose = TypeUtils.GetField(remoteDimrApi, apiFieldName);
            Assert.That(privateApiAfterDispose, Is.Null);
        }
    }
}