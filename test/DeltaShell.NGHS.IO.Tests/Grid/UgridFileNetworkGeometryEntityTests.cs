using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using Deltares.Infrastructure.API.Logging;
using Deltares.UGrid.Api;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.IO.Properties;
using GeoAPI.Extensions.Networks;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    [TestFixture]
    public class UgridFileNetworkGeometryEntityTests
    {
        private readonly string defaultIoNetCdfNativeOpenErrorMessage = string.Format(Resources.IoNetCdfNativeError_Api_open_error___0______1___in_method__2__, 801, "An error occurred", "NativeCall");
        private readonly string defaultIoNetCdfNativeCallErrorMessage = string.Format(Resources.IoNetCdfNativeError_Api_call_error___0______1___in_method__2, 801, "An error occurred", "NativeCall");
        private readonly IoNetCdfNativeError ioNetCdfNativeError = new IoNetCdfNativeError(801, "An error occurred", "NativeCall", "aMember");

        [Test]
        public void ApplyNetworkGeometryTest_CompartmentPropertiesIsNull_ThrowsNullException()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;
                var logHandler = Substitute.For<ILogHandler>();

                //Act
                DisposableNetworkGeometry Call() =>
                    ugridFile.ApplyNetworkGeometry(null,
                                                   null,
                                                   Enumerable.Empty<BranchProperties>(),
                                                   logHandler: logHandler
                    );

                // Assert
                Assert.That(Call, Throws.ArgumentNullException);
            }
        }

        [Test]
        public void ApplyNetworkGeometryTest_BranchPropertiesIsNull_ThrowsNullException()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;
                var logHandler = Substitute.For<ILogHandler>();

                //Act
                DisposableNetworkGeometry Call() =>
                    ugridFile.ApplyNetworkGeometry(null,
                                                   Enumerable.Empty<CompartmentProperties>(),
                                                   null,
                                                   logHandler: logHandler
                    );

                // Assert
                Assert.That(Call, Throws.ArgumentNullException);
            }
        }

        [Test]
        public void ApplyNetworkGeometryTest_NetworkIsNull_LogsError()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;
                var logHandler = Substitute.For<ILogHandler>();

                //Act
                DisposableNetworkGeometry disposableNetworkGeometry =
                    ugridFile.ApplyNetworkGeometry(null,
                                                   Enumerable.Empty<CompartmentProperties>(),
                                                   Enumerable.Empty<BranchProperties>(),
                                                   logHandler: logHandler
                    );

                // Assert
                Assert.That(disposableNetworkGeometry, Is.Null);
                logHandler.Received(1).ReportError(string.Format(Resources.ApplyNetworkGeometry_Could_not_load_network_from__0_, string.Empty));
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void ApplyNetworkGeometryTest_InvalidPath_LogsError()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(false);
                ugridFile.UgridFileInfo = ugridFileInfo;
                var hydroNetwork = Substitute.For<IHydroNetwork>();
                var logHandler = Substitute.For<ILogHandler>();

                //Act
                DisposableNetworkGeometry disposableNetworkGeometry =
                    ugridFile.ApplyNetworkGeometry(hydroNetwork,
                                                   Enumerable.Empty<CompartmentProperties>(),
                                                   Enumerable.Empty<BranchProperties>(),
                                                   logHandler: logHandler
                    );

                // Assert
                Assert.That(disposableNetworkGeometry, Is.Null);
                logHandler.Received(1).ReportError(string.Format(Resources.ApplyNetworkGeometry_Could_not_load_network_from__0_, string.Empty));
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void ApplyNetworkGeometryTest_OpenApiFails_ReturnsNull()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;

                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.When(x => x.Open(Arg.Any<string>())).Do(x => { throw ioNetCdfNativeError; });
                ugridFile.Api = uGridApi;

                var hydroNetwork = Substitute.For<IHydroNetwork>();

                var logHandler = Substitute.For<ILogHandler>();

                //Act
                DisposableNetworkGeometry disposableNetworkGeometry =
                    ugridFile.ApplyNetworkGeometry(hydroNetwork,
                                                   Enumerable.Empty<CompartmentProperties>(),
                                                   Enumerable.Empty<BranchProperties>(),
                                                   logHandler: logHandler
                    );

                // Assert
                Assert.That(disposableNetworkGeometry, Is.Null);
                logHandler.Received(1).ReportWarning(defaultIoNetCdfNativeOpenErrorMessage);
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void ApplyNetworkGeometryTest_NonUgridFile_LogsError()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;
                var hydroNetwork = Substitute.For<IHydroNetwork>();
                var logHandler = Substitute.For<ILogHandler>();

                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.IsUGridFile().Returns(false);
                ugridFile.Api = uGridApi;

                //Act
                DisposableNetworkGeometry disposableNetworkGeometry =
                    ugridFile.ApplyNetworkGeometry(hydroNetwork,
                                                   Enumerable.Empty<CompartmentProperties>(),
                                                   Enumerable.Empty<BranchProperties>(),
                                                   logHandler: logHandler
                    );

                // Assert
                Assert.That(disposableNetworkGeometry, Is.Null);
                //logHandler.Received(1).ReportError(defaultIoNetCdfNativeOpenErrorMessage);
                logHandler.Received(1).ReportError(string.Format(Resources.ApplyNetworkGeometry_Could_not_load_network_from__0_, string.Empty));
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void ApplyNetworkGeometry_UgridFile_ReadNetworkTest_NoNetworkIds_CallLogReportAndReturnNull()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;
                var hydroNetwork = Substitute.For<IHydroNetwork>();
                var logHandler = Substitute.For<ILogHandler>();

                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.IsUGridFile().Returns(true);
                ugridFile.Api = uGridApi;

                //Act
                DisposableNetworkGeometry disposableNetworkGeometry =
                    ugridFile.ApplyNetworkGeometry(hydroNetwork,
                                                   Enumerable.Empty<CompartmentProperties>(),
                                                   Enumerable.Empty<BranchProperties>(),
                                                   logHandler: logHandler
                    );

                // Assert
                Assert.That(disposableNetworkGeometry, Is.Null);
                logHandler.Received(1).ReportError(Resources.ReadNetwork_No_network_geometries_in_file_detected);
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void ApplyNetworkGeometry_UgridFile_ReadNetworkTest_WithNetworkIds_ReturnsNull()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;
                var hydroNetwork = Substitute.For<IHydroNetwork>();
                var logHandler = Substitute.For<ILogHandler>();

                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.IsUGridFile().Returns(true);
                uGridApi.GetNetworkIds().Returns(new[] { 1 });
                ugridFile.Api = uGridApi;

                // Act
                DisposableNetworkGeometry disposableNetworkGeometry =
                    ugridFile.ApplyNetworkGeometry(hydroNetwork,
                                                   Enumerable.Empty<CompartmentProperties>(),
                                                   Enumerable.Empty<BranchProperties>(),
                                                   logHandler: logHandler
                    );

                // Assert
                Assert.That(disposableNetworkGeometry, Is.Null);
                logHandler.Received(1).LogReport();
                uGridApi.Received(1).Open(string.Empty, OpenMode.Reading);
                uGridApi.Received(1).IsUGridFile();
                uGridApi.Received(1).GetNetworkIds();
                uGridApi.Received(1).GetNetworkGeometry(1);
            }
        }

        [Test]
        public void ApplyNetworkGeometry_UgridFile_ReadNetworkTest_WithNetworkGeometry_HappyFlow_CallsLogReportAndReturnsExpectedDisposableNetworkGeometry()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;
                var hydroNetwork = Substitute.For<IHydroNetwork>();

                var logHandler = Substitute.For<ILogHandler>();

                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.IsUGridFile().Returns(true);
                uGridApi.GetNetworkIds().Returns(new[] { 1 });
                var networkGeometry = new DisposableNetworkGeometry()
                {
                    NodesX = Array.Empty<double>(),
                    BranchIds = Array.Empty<string>()
                };

                uGridApi.GetNetworkGeometry(1).Returns(networkGeometry);
                ugridFile.Api = uGridApi;

                // Act
                DisposableNetworkGeometry disposableNetworkGeometry =
                    ugridFile.ApplyNetworkGeometry(hydroNetwork,
                                                   Enumerable.Empty<CompartmentProperties>(),
                                                   Enumerable.Empty<BranchProperties>(),
                                                   logHandler: logHandler
                    );

                // Assert
                Assert.That(disposableNetworkGeometry, Is.EqualTo(networkGeometry));
                ugridFileInfo.Received(1).IsValidPath();
                uGridApi.Received(1).Open(string.Empty, OpenMode.Reading);
                uGridApi.Received(1).IsUGridFile();
                uGridApi.Received(1).GetNetworkIds();
                uGridApi.Received(1).GetNetworkGeometry(1);

                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void ApplyNetworkGeometry_UgridFile_ReportProgressRaisesException_ReadNetworkTest_WithNetworkGeometry_LogsWarningContinueAndReturnsExpectedDisposableNetworkGeometry()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;
                var hydroNetwork = Substitute.For<IHydroNetwork>();

                var logHandler = Substitute.For<ILogHandler>();

                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.IsUGridFile().Returns(true);
                uGridApi.GetNetworkIds().Returns(new[] { 1 });
                var networkGeometry = new DisposableNetworkGeometry()
                {
                    NodesX = Array.Empty<double>(),
                    BranchIds = Array.Empty<string>()
                };

                uGridApi.GetNetworkGeometry(1).Returns(networkGeometry);
                ugridFile.Api = uGridApi;

                void ReportProgressRaiseException(string progressText) => throw new Exception("An error occurred");
                // Act
                DisposableNetworkGeometry disposableNetworkGeometry =
                    ugridFile.ApplyNetworkGeometry(hydroNetwork,
                                                   Enumerable.Empty<CompartmentProperties>(),
                                                   Enumerable.Empty<BranchProperties>(),
                                                   logHandler: logHandler,
                                                   reportProgress: ReportProgressRaiseException
                    );

                // Assert
                Assert.That(disposableNetworkGeometry, Is.EqualTo(networkGeometry));
                logHandler.Received(1).ReportWarning(string.Format(Resources.ReportProgressWithException_Could_not_report_progress_because, "An error occurred"));
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void ApplyNetworkGeometry_UgridFile_ReadNetworkTest_ApiFailsWithIoNetCdfNativeError_LogsWarningAndReturnsNull()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;

                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.IsUGridFile().Returns(true);
                uGridApi.GetNetworkIds().Returns(new[] { 1 });
                uGridApi.When(x => x.GetNetworkIds()).Do(x => { throw ioNetCdfNativeError; });
                ugridFile.Api = uGridApi;

                var hydroNetwork = Substitute.For<IHydroNetwork>();

                var logHandler = Substitute.For<ILogHandler>();

                //Act
                DisposableNetworkGeometry disposableNetworkGeometry =
                    ugridFile.ApplyNetworkGeometry(hydroNetwork,
                                                   Enumerable.Empty<CompartmentProperties>(),
                                                   Enumerable.Empty<BranchProperties>(),
                                                   logHandler: logHandler
                    );

                // Assert
                Assert.That(disposableNetworkGeometry, Is.Null);
                logHandler.Received(1).ReportWarning(defaultIoNetCdfNativeCallErrorMessage);
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void ApplyNetworkGeometry_UgridFile_UpdateBranchLengthIfNotMatchesFileBranchLengthTest_WithNetworkGeometryBranchLengthsDifferentThanLoadedInHydroNetwork_CallsLogReportAndReturnsExpectedDisposableNetworkGeometryAndSetBranchLengthFromDisposableNetworkGeometry()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;
                var branch = Substitute.For<IBranch>();
                branch.IsLengthCustom = false;
                branch.Length = 8.01;
                IEventedList<IBranch> branches = new EventedList<IBranch> { branch };
                var hydroNetwork = Substitute.For<IHydroNetwork>();
                hydroNetwork.Branches.Returns(branches);

                var logHandler = Substitute.For<ILogHandler>();

                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.IsUGridFile().Returns(true);
                uGridApi.GetNetworkIds().Returns(new[] { 1 });
                var networkGeometry = new DisposableNetworkGeometry()
                {
                    NodesX = Array.Empty<double>(),
                    BranchIds = Array.Empty<string>(),
                    BranchLengths = new[] { 80.1 }
                };

                uGridApi.GetNetworkGeometry(1).Returns(networkGeometry);
                ugridFile.Api = uGridApi;

                // Act
                DisposableNetworkGeometry disposableNetworkGeometry =
                    ugridFile.ApplyNetworkGeometry(hydroNetwork,
                                                   Enumerable.Empty<CompartmentProperties>(),
                                                   Enumerable.Empty<BranchProperties>(),
                                                   logHandler: logHandler
                    );

                // Assert
                Assert.That(disposableNetworkGeometry, Is.EqualTo(networkGeometry));
                logHandler.Received(1).LogReport();
                Assert.That(disposableNetworkGeometry.BranchLengths[0], Is.EqualTo(hydroNetwork.Branches[0].Length));
                Assert.That(hydroNetwork.Branches[0].IsLengthCustom, Is.True);
            }
        }

        [Test]
        public void GetNumberOfNetworksTest_InvalidPath_Returns0()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(false);
                ugridFile.UgridFileInfo = ugridFileInfo;

                var logHandler = Substitute.For<ILogHandler>();

                //Act
                int nrOfNetworks = ugridFile.GetNumberOfNetworks(logHandler);

                // Assert
                Assert.That(nrOfNetworks, Is.EqualTo(0));
            }
        }

        [Test]
        public void GetNumberOfNetworksTest_ValidPathAndFieldsAreAlreadySetOnce_Returns1()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;
                TypeUtils.SetField<UGridFile>(ugridFile, "lastCheckedUGridPath", string.Empty);
                TypeUtils.SetField<UGridFile>(ugridFile, "numberOfNetworks", 1);
                var logHandler = Substitute.For<ILogHandler>();

                //Act
                int nrOfNetworks = ugridFile.GetNumberOfNetworks(logHandler);

                // Assert
                Assert.That(nrOfNetworks, Is.EqualTo(1));
            }
        }

        [Test]
        public void GetNumberOfNetworksTest_WithUGridFileOpenIoNetCdfNativeErrorException_Returns0()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;

                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.When(x => x.Open(Arg.Any<string>())).Do(x => { throw ioNetCdfNativeError; });
                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                //Act
                int nrOfNetworks = ugridFile.GetNumberOfNetworks(logHandler);

                // Assert
                Assert.That(nrOfNetworks, Is.EqualTo(0));
                logHandler.ReceivedWithAnyArgs(1).ReportWarning(defaultIoNetCdfNativeOpenErrorMessage);
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void GetNumberOfNetworksTest_ApiCallFailsWithIoNetCdfNativeErrorException_Returns0()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;

                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.When(x => x.GetNumberOfNetworks()).Do(x => { throw ioNetCdfNativeError; });
                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                //Act
                int nrOfNetworks = ugridFile.GetNumberOfNetworks(logHandler);

                // Assert
                Assert.That(nrOfNetworks, Is.EqualTo(0));
                logHandler.ReceivedWithAnyArgs(1).ReportWarning(defaultIoNetCdfNativeCallErrorMessage);
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void GetNumberOfNetworksTest_ApiCall_HappyFlow_ReturnsExpectedValue()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;

                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.GetNumberOfNetworks().Returns(801);
                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                //Act
                int nrOfNetworks = ugridFile.GetNumberOfNetworks(logHandler);

                // Assert
                Assert.That(nrOfNetworks, Is.EqualTo(801));
                ugridFileInfo.Received(1).IsValidPath();
                uGridApi.Received(1).Open(string.Empty, OpenMode.Reading);
                uGridApi.Received(1).GetNumberOfNetworks();
            }
        }
    }
}