using System;
using System.Collections.Generic;
using DelftTools.Utils.Reflection;
using Deltares.Infrastructure.API.Logging;
using Deltares.UGrid.Api;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.IO.Properties;
using GeoAPI.Extensions.Coverages;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    [TestFixture]
    public class UgridFileMesh1DEntityTests
    {
        private readonly string defaultIoNetCdfNativeOpenErrorMessage = string.Format(Resources.IoNetCdfNativeError_Api_open_error___0______1___in_method__2__, 801, "An error occurred", "NativeCall");
        private readonly string defaultIoNetCdfNativeCallErrorMessage = string.Format(Resources.IoNetCdfNativeError_Api_call_error___0______1___in_method__2, 801, "An error occurred", "NativeCall");

        private IoNetCdfNativeError ioNetCdfNativeError = new IoNetCdfNativeError(801, "An error occurred", "NativeCall", "aMember");

        [Test]
        public void ApplyMesh1DTest_LogHandlerIsNull_ThrowsNullException()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                // Act
                Disposable1DMeshGeometry Call() =>
                    ugridFile.ApplyMesh1D(null,
                                          null,
                                          null,
                                          null
                    );

                // Assert
                Assert.That(Call, Throws.ArgumentNullException);
            }
        }

        [Test]
        public void ApplyMesh1DTest_DiscretizationIsNull_LogsErrorAndReturnsNull()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var logHandler = Substitute.For<ILogHandler>();

                // Act
                Disposable1DMeshGeometry disposable1DMeshGeometry = ugridFile.ApplyMesh1D(null,
                                                                                          null,
                                                                                          logHandler,
                                                                                          null
                );

                // Assert
                Assert.That(disposable1DMeshGeometry, Is.Null);
                logHandler.Received(1).ReportError(string.Format(Resources.UGridFile_ApplyMesh1D_Could_not_load_computational_grid_from__0_, string.Empty));
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void ApplyMesh1DTest_InValidFile_LogsErrorAndReturnsNull()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(false);
                ugridFile.UgridFileInfo = ugridFileInfo;
                var discretization = Substitute.For<IDiscretization>();
                var logHandler = Substitute.For<ILogHandler>();

                // Act
                Disposable1DMeshGeometry disposable1DMeshGeometry = ugridFile.ApplyMesh1D(discretization,
                                                                                          null,
                                                                                          logHandler,
                                                                                          null
                );

                // Assert
                Assert.That(disposable1DMeshGeometry, Is.Null);
                logHandler.Received(1).ReportError(string.Format(Resources.UGridFile_ApplyMesh1D_Could_not_load_computational_grid_from__0_, string.Empty));
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void ApplyMesh1DTest_CannotUseXYFromFileBecauseNotUgridFile_LogsErrorAndReturnsNull()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;
                var discretization = Substitute.For<IDiscretization>();
                var uGridApi = Substitute.For<IUGridApi>();

                uGridApi.IsUGridFile().Returns(false);
                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                Disposable1DMeshGeometry disposable1DMeshGeometry = ugridFile.ApplyMesh1D(discretization,
                                                                                          null,
                                                                                          logHandler,
                                                                                          null
                );

                // Assert
                Assert.That(disposable1DMeshGeometry, Is.Null);
                logHandler.Received(1).ReportError(string.Format(Resources.UGridFile_ApplyMesh1D_Could_not_load_computational_grid_from__0_, string.Empty));
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void ApplyMesh1DTest_CannotUseXYFromFileBecauseNotUgridFile_OpenApiThrowsIoNetCdfNativeError_LogsWarningAndReturnsNull()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;
                var discretization = Substitute.For<IDiscretization>();
                var uGridApi = Substitute.For<IUGridApi>();

                uGridApi.IsUGridFile().Returns(false);
                uGridApi.When(x => x.Open(Arg.Any<string>())).Do(x => { throw ioNetCdfNativeError; });
                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                Disposable1DMeshGeometry disposable1DMeshGeometry = ugridFile.ApplyMesh1D(discretization,
                                                                                          null,
                                                                                          logHandler,
                                                                                          null
                );

                // Assert
                Assert.That(disposable1DMeshGeometry, Is.Null);
                logHandler.Received(2).ReportWarning(defaultIoNetCdfNativeOpenErrorMessage); // 1x in IsUgridFile and 1x in OpenFileApi
                logHandler.Received(2).LogReport();
            }
        }

        [Test]
        public void ApplyMesh1DTest_CannotUseXYFromFileBecauseNotUgridFile_BeforeReadMeshIsCorrectUgridFile_ReportProgressError_LogsWarningAndReturnsNull()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;
                var discretization = Substitute.For<IDiscretization>();
                var uGridApi = Substitute.For<IUGridApi>();

                uGridApi.IsUGridFile().Returns(false, true);
                ugridFile.Api = uGridApi;
                void ReportProgressRaiseException(string progressText) => throw new Exception("An error occurred");
                var logHandler = Substitute.For<ILogHandler>();

                // Act
                Disposable1DMeshGeometry disposable1DMeshGeometry = ugridFile.ApplyMesh1D(discretization,
                                                                                          null,
                                                                                          logHandler,
                                                                                          ReportProgressRaiseException
                );

                // Assert
                Assert.That(disposable1DMeshGeometry, Is.Null);
                logHandler.Received(1).ReportWarning(string.Format(Resources.ReportProgressWithException_Could_not_report_progress_because, "An error occurred"));
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void ApplyMesh1DTest_CannotUseXYFromFileBecauseNotUgridFile_IsUgridApiThrowsIoNetCdfNativeError_LogsWarningAndReturnsNull()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;
                var discretization = Substitute.For<IDiscretization>();
                var uGridApi = Substitute.For<IUGridApi>();

                uGridApi.IsUGridFile().Returns(false);
                uGridApi.When(x => x.IsUGridFile()).Do(x => { throw ioNetCdfNativeError; });
                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                Disposable1DMeshGeometry disposable1DMeshGeometry = ugridFile.ApplyMesh1D(discretization,
                                                                                          null,
                                                                                          logHandler,
                                                                                          null
                );

                // Assert
                Assert.That(disposable1DMeshGeometry, Is.Null);
                logHandler.Received(1).ReportWarning(defaultIoNetCdfNativeOpenErrorMessage);
                logHandler.Received(1).ReportWarning(defaultIoNetCdfNativeCallErrorMessage);
                logHandler.Received(2).LogReport();
            }
        }

        [Test]
        public void ApplyMesh1DTest_CannotUseXYFromFileBecauseNotUgridFile_ApiGetMeshIdsReturnsValidValue_ReturnsNull()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;
                var discretization = Substitute.For<IDiscretization>();
                var uGridApi = Substitute.For<IUGridApi>();

                uGridApi.IsUGridFile().Returns(false, true);
                uGridApi.GetMeshIdsByMeshType(UGridMeshType.Mesh1D).Returns(new[] { 1 });
                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                Disposable1DMeshGeometry disposable1DMeshGeometry = ugridFile.ApplyMesh1D(discretization,
                                                                                          null,
                                                                                          logHandler,
                                                                                          null
                );

                // Assert
                Assert.That(disposable1DMeshGeometry, Is.Null);
                logHandler.Received(1).LogReport();
                uGridApi.Received(2).IsUGridFile();
                uGridApi.Received(1).GetMeshIdsByMeshType(UGridMeshType.Mesh1D);
                uGridApi.Received(1).GetMesh1D(1);

            }
        }

        [Test]
        public void ApplyMesh1DTest_CannotUseXYFromFileBecauseNotUgridFile_ApiGetMesh1DThrowsIoNetCdfNativeError_LogsWarningAndReturnsNull()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;
                var discretization = Substitute.For<IDiscretization>();
                var uGridApi = Substitute.For<IUGridApi>();

                uGridApi.IsUGridFile().Returns(false, true);
                uGridApi.GetMeshIdsByMeshType(UGridMeshType.Mesh1D).Returns(new[] { 1 });
                uGridApi.When(x => x.GetMesh1D(1)).Do(x => { throw ioNetCdfNativeError; });
                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                Disposable1DMeshGeometry disposable1DMeshGeometry = ugridFile.ApplyMesh1D(discretization,
                                                                                          null,
                                                                                          logHandler,
                                                                                          null
                );

                // Assert
                Assert.That(disposable1DMeshGeometry, Is.Null);
                logHandler.Received(1).ReportWarning(defaultIoNetCdfNativeCallErrorMessage);
                logHandler.Received(2).LogReport();
            }
        }

        [Test]
        public void ApplyMesh1DTest_CannotUseXYFromFileBecauseNotUgridFile_ApiGetMesh1DReturnsExpectedDisposableMesh1D_ReturnsExpectedMesh1D()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;
                var discretization = Substitute.For<IDiscretization>();
                var uGridApi = Substitute.For<IUGridApi>();

                var disposable1DMesh = new Disposable1DMeshGeometry() { NodeIds = Array.Empty<string>() };
                uGridApi.IsUGridFile().Returns(false, true);
                uGridApi.GetMeshIdsByMeshType(UGridMeshType.Mesh1D).Returns(new[] { 1 });
                uGridApi.GetMesh1D(1).Returns(disposable1DMesh);

                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                Disposable1DMeshGeometry disposable1DMeshGeometry = ugridFile.ApplyMesh1D(discretization,
                                                                                          null,
                                                                                          logHandler,
                                                                                          null
                );

                // Assert
                Assert.That(disposable1DMeshGeometry, Is.EqualTo(disposable1DMesh));
                ugridFileInfo.Received(2).IsValidPath();
                uGridApi.Received(2).Open(string.Empty, OpenMode.Reading);
                uGridApi.Received(2).IsUGridFile();
                uGridApi.Received(1).GetMeshIdsByMeshType(UGridMeshType.Mesh1D);
                uGridApi.Received(1).GetMesh1D(1);
                logHandler.Received(2).LogReport();
            }
        }

        [Test]
        public void GetNumberOfNetworkDiscretizationsTest_InvalidFile_Returns0()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(false);
                ugridFile.UgridFileInfo = ugridFileInfo;
                var logHandler = Substitute.For<ILogHandler>();

                // Act
                int nrOfNetworkDiscretizations = ugridFile.GetNumberOfNetworkDiscretizations(logHandler);

                // Assert
                Assert.That(nrOfNetworkDiscretizations, Is.EqualTo(0));
            }
        }

        [Test]
        public void GetNumberOfNetworkDiscretizationsTest_ValidFileAndCachedMesh1DType_ReturnsExpectedValue()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;

                var logHandler = Substitute.For<ILogHandler>();

                var numberOfMeshByType = new Dictionary<UGridMeshType, int>();
                numberOfMeshByType[UGridMeshType.Mesh1D] = 801;
                TypeUtils.SetField<UGridFile>(ugridFile, "lastCheckedUGridPath", string.Empty);
                TypeUtils.SetField<UGridFile>(ugridFile, "numberOfMeshByType", numberOfMeshByType);

                // Act
                int nrOfNetworkDiscretizations = ugridFile.GetNumberOfNetworkDiscretizations(logHandler);

                // Assert
                Assert.That(nrOfNetworkDiscretizations, Is.EqualTo(801));
            }
        }

        [Test]
        public void GetNumberOfNetworkDiscretizationsTest_OpenFileApiThrowsIoNetCdfNativeError_Returns0()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;

                var logHandler = Substitute.For<ILogHandler>();

                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.When(x => x.Open(Arg.Any<string>())).Do(x => { throw ioNetCdfNativeError; });
                ugridFile.Api = uGridApi;

                // Act
                int nrOfNetworkDiscretizations = ugridFile.GetNumberOfNetworkDiscretizations(logHandler);

                // Assert
                Assert.That(nrOfNetworkDiscretizations, Is.EqualTo(0));
                logHandler.Received(1).ReportWarning(defaultIoNetCdfNativeOpenErrorMessage);
            }
        }

        [Test]
        public void GetNumberOfNetworkDiscretizationsTest_GetNumberOfMeshByTypeApiValue_ReturnsApiValue()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;

                var logHandler = Substitute.For<ILogHandler>();

                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.GetNumberOfMeshByType(UGridMeshType.Mesh1D).Returns(801);
                ugridFile.Api = uGridApi;

                // Act
                int nrOfNetworkDiscretizations = ugridFile.GetNumberOfNetworkDiscretizations(logHandler);

                // Assert
                Assert.That(nrOfNetworkDiscretizations, Is.EqualTo(801));
            }
        }
        
        [Test]
        public void GetNumberOfNetworkDiscretizationsTest_GetNumberOfMeshByTypeApiValueThrowsIonetCdfError_LogsWarning()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;

                var logHandler = Substitute.For<ILogHandler>();

                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.When(x => x.GetNumberOfMeshByType(UGridMeshType.Mesh1D)).Do(x => { throw ioNetCdfNativeError; });

                ugridFile.Api = uGridApi;

                // Act
                ugridFile.GetNumberOfNetworkDiscretizations(logHandler);

                // Assert
                logHandler.Received(1).ReportWarning(defaultIoNetCdfNativeCallErrorMessage);
            }
        }
    }
}