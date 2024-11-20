using System;
using Deltares.Infrastructure.API.Logging;
using Deltares.UGrid.Api;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.IO.Properties;
using NetTopologySuite.Extensions.Grids;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    [TestFixture]
    public class UgridFileMesh2DEntityTests
    {
        private readonly string defaultIoNetCdfNativeOpenErrorMessage = string.Format(Resources.IoNetCdfNativeError_Api_open_error___0______1___in_method__2__, 801, "An error occurred", "NativeCall");
        private readonly string defaultIoNetCdfNativeCallErrorMessage = string.Format(Resources.IoNetCdfNativeError_Api_call_error___0______1___in_method__2, 801, "An error occurred", "NativeCall");
        private readonly IoNetCdfNativeError ioNetCdfNativeError = new IoNetCdfNativeError(801, "An error occurred", "NativeCall", "aMember");

        [Test]
        public void ApplyMesh2DTest_LogHandlerIsNull_ThrowsNullException()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                // Act
                Disposable2DMeshGeometry Call() =>
                    ugridFile.ApplyMesh2D(null,
                                          null
                    );

                // Assert
                Assert.That(Call, Throws.ArgumentNullException);
            }
        }

        [Test]
        public void ApplyMesh2DTest_UnstructuredGridIsNull_LogsWarningAndReturnsNull()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var logHandler = Substitute.For<ILogHandler>();

                // Act
                Disposable2DMeshGeometry disposable2DMeshGeometry = ugridFile.ApplyMesh2D(null,
                                                                                          logHandler
                );

                // Assert
                Assert.That(disposable2DMeshGeometry, Is.Null);
                logHandler.Received(1).ReportWarning(string.Format(Resources.ApplyMesh2D_Could_not_find_grid_file_at___0____this_is_because_you_maybe_just_created_this_model__If_this_is_not_the_case_please_check_if_the_file_with_the_grid_in_it_exists_, string.Empty));
            }
        }

        [Test]
        public void ApplyMesh2DTest_InValidFile_LogsWarningAndReturnsNull()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(false);
                ugridFile.UgridFileInfo = ugridFileInfo;
                var grid = Substitute.For<UnstructuredGrid>();
                var logHandler = Substitute.For<ILogHandler>();

                // Act
                Disposable2DMeshGeometry disposable2DMeshGeometry = ugridFile.ApplyMesh2D(grid,
                                                                                          logHandler
                );

                // Assert
                Assert.That(disposable2DMeshGeometry, Is.Null);
                logHandler.Received(1).ReportWarning(string.Format(Resources.ApplyMesh2D_Could_not_find_grid_file_at___0____this_is_because_you_maybe_just_created_this_model__If_this_is_not_the_case_please_check_if_the_file_with_the_grid_in_it_exists_, string.Empty));
            }
        }

        [Test]
        public void ApplyMesh2DTest_NonUgridFile_LogsWarningAndReturnsPlainReadDisposableMesh2DGeometry()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;
                var grid = Substitute.For<UnstructuredGrid>();
                var uGridApi = Substitute.For<IUGridApi>();

                uGridApi.IsUGridFile().Returns(false);
                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                Disposable2DMeshGeometry disposable2DMeshGeometry = ugridFile.ApplyMesh2D(grid,
                                                                                          logHandler
                );

                // Assert
                Assert.That(disposable2DMeshGeometry, Is.Not.Null);
                Assert.That(disposable2DMeshGeometry, Is.TypeOf<Disposable2DMeshGeometry>());
                logHandler.Received(1).ReportWarning(string.Format(Resources.UGridFile_ApplyMesh2D_Could_not_create_mesh2d_from_file__0__because_it_is_not_a_ugrid_type_file__Trying_plain_netcdf_2d_grid_file_reading, string.Empty));
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void ApplyMesh2DTest_OpenApiThrowsIoNetCdfNativeError_LogsWarningAndReturnsNull()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;
                var grid = Substitute.For<UnstructuredGrid>();
                var uGridApi = Substitute.For<IUGridApi>();

                uGridApi.IsUGridFile().Returns(false);
                uGridApi.When(x => x.Open(Arg.Any<string>())).Do(x => { throw ioNetCdfNativeError; });
                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                Disposable2DMeshGeometry disposable2DMeshGeometry = ugridFile.ApplyMesh2D(grid,
                                                                                          logHandler
                );

                // Assert
                Assert.That(disposable2DMeshGeometry, Is.Null);
                logHandler.Received(1).ReportWarning(defaultIoNetCdfNativeOpenErrorMessage);
            }
        }

        [Test]
        public void ApplyMesh2DTest_IsUgridFile_ReportProgressError_LogsWarningAndErrorAndReturnsNull()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;
                var grid = Substitute.For<UnstructuredGrid>();
                var uGridApi = Substitute.For<IUGridApi>();

                uGridApi.IsUGridFile().Returns(true);
                ugridFile.Api = uGridApi;
                void ReportProgressRaiseException(string progressText) => throw new Exception("An error occurred");
                var logHandler = Substitute.For<ILogHandler>();

                // Act
                Disposable2DMeshGeometry disposable2DMeshGeometry = ugridFile.ApplyMesh2D(grid,
                                                                                          logHandler,
                                                                                          ReportProgressRaiseException
                );

                // Assert
                Assert.That(disposable2DMeshGeometry, Is.Null);
                logHandler.Received(1).ReportWarning(string.Format(Resources.ReportProgressWithException_Could_not_report_progress_because, "An error occurred"));
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void ApplyMesh2DTest_IsUgridApiThrowsIoNetCdfNativeError_LogsWarningAndReturnsNull()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;
                var grid = Substitute.For<UnstructuredGrid>();
                var uGridApi = Substitute.For<IUGridApi>();

                uGridApi.IsUGridFile().Returns(false);
                uGridApi.When(x => x.IsUGridFile()).Do(x => { throw ioNetCdfNativeError; });
                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                Disposable2DMeshGeometry disposable2DMeshGeometry = ugridFile.ApplyMesh2D(grid,
                                                                                          logHandler
                );

                // Assert
                Assert.That(disposable2DMeshGeometry, Is.Null);
                logHandler.Received(1).ReportWarning(defaultIoNetCdfNativeCallErrorMessage);
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void ApplyMesh2DTest_ApiGetMeshIdsReturnsValidValue_ReturnsNull()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;
                var grid = Substitute.For<UnstructuredGrid>();
                var uGridApi = Substitute.For<IUGridApi>();

                uGridApi.IsUGridFile().Returns(true);
                uGridApi.GetMeshIdsByMeshType(UGridMeshType.Mesh2D).Returns(new[] { 1 });
                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                Disposable2DMeshGeometry disposable2DMeshGeometry = ugridFile.ApplyMesh2D(grid,
                                                                                          logHandler
                );

                // Assert
                Assert.That(disposable2DMeshGeometry, Is.Null);
                logHandler.Received(1).LogReport();
                uGridApi.Received(2).IsUGridFile();
                uGridApi.Received(1).GetMeshIdsByMeshType(UGridMeshType.Mesh2D);
            }
        }

        [Test]
        public void ApplyMesh2DTest_ApiGetMesh2DThrowsIoNetCdfNativeError_LogsWarningAndReturnsNull()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;
                var grid = Substitute.For<UnstructuredGrid>();
                var uGridApi = Substitute.For<IUGridApi>();

                uGridApi.IsUGridFile().Returns(true);
                uGridApi.GetMeshIdsByMeshType(UGridMeshType.Mesh2D).Returns(new[] { 1 });
                uGridApi.When(x => x.GetMesh2D(1)).Do(x => { throw ioNetCdfNativeError; });
                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                Disposable2DMeshGeometry disposable2DMeshGeometry = ugridFile.ApplyMesh2D(grid,
                                                                                          logHandler
                );

                // Assert
                Assert.That(disposable2DMeshGeometry, Is.Null);
                logHandler.Received(1).ReportWarning(defaultIoNetCdfNativeCallErrorMessage);
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void ApplyMesh2DTest_ApiGetMesh2DReturnsExpectedDisposableMesh2D_ReturnsExpectedMesh2D()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;
                var grid = Substitute.For<UnstructuredGrid>();
                grid.CoordinateSystem = null;
                var uGridApi = Substitute.For<IUGridApi>();

                var disposable2DMesh = new Disposable2DMeshGeometry();
                uGridApi.IsUGridFile().Returns(true);
                uGridApi.GetMeshIdsByMeshType(UGridMeshType.Mesh2D).Returns(new[] { 1 });
                uGridApi.GetMesh2D(1).Returns(disposable2DMesh);

                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                Disposable2DMeshGeometry disposable2DMeshGeometry = ugridFile.ApplyMesh2D(grid,
                                                                                          logHandler
                );

                // Assert
                Assert.That(disposable2DMeshGeometry, Is.EqualTo(disposable2DMesh));
                ugridFileInfo.Received(2).IsValidPath();
                uGridApi.Received(2).Open(string.Empty, OpenMode.Reading);
                uGridApi.Received(2).IsUGridFile();
                uGridApi.Received(1).GetMeshIdsByMeshType(UGridMeshType.Mesh2D);
                uGridApi.Received(1).GetMesh2D(1);
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void ApplyMesh2DTest_ApiGetMesh2DReturnsExpectedDisposableMesh2D_SetCoordinateSystem_ReturnsExpectedMesh2D()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;
                var grid = Substitute.For<UnstructuredGrid>();
                grid.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(28991);
                var uGridApi = Substitute.For<IUGridApi>();

                var disposable2DMesh = new Disposable2DMeshGeometry();
                uGridApi.IsUGridFile().Returns(true);
                uGridApi.GetMeshIdsByMeshType(UGridMeshType.Mesh2D).Returns(new[] { 1 });
                uGridApi.GetMesh2D(1).Returns(disposable2DMesh);
                uGridApi.GetCoordinateSystemCode().Returns(28992);

                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                Disposable2DMeshGeometry disposable2DMeshGeometry = ugridFile.ApplyMesh2D(grid,
                                                                                          logHandler
                );

                // Assert
                Assert.That(disposable2DMeshGeometry, Is.EqualTo(disposable2DMesh));
                Assert.That(grid.CoordinateSystem.AuthorityCode, Is.EqualTo(28992));
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void ApplyMesh2D_Read2DMeshTest_IncorrectFile_LogsWarningAndReturnsNull()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true, false);
                ugridFile.UgridFileInfo = ugridFileInfo;
                var grid = Substitute.For<UnstructuredGrid>();
                var uGridApi = Substitute.For<IUGridApi>();

                uGridApi.IsUGridFile().Returns(true);
                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                Disposable2DMeshGeometry disposable2DMeshGeometry = ugridFile.ApplyMesh2D(grid,
                                                                                          logHandler
                );

                // Assert
                Assert.That(disposable2DMeshGeometry, Is.Null);
                logHandler.Received(1).ReportWarning(string.Format(Resources.ApplyMesh2D_Could_not_find_grid_file_at___0____this_is_because_you_maybe_just_created_this_model__If_this_is_not_the_case_please_check_if_the_file_with_the_grid_in_it_exists_, string.Empty));
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void ApplyMesh2D_Read2DMeshTest_NonUgridApi_ReturnsNull()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;
                var grid = Substitute.For<UnstructuredGrid>();
                var uGridApi = Substitute.For<IUGridApi>();

                uGridApi.IsUGridFile().Returns(true, false);
                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                Disposable2DMeshGeometry disposable2DMeshGeometry = ugridFile.ApplyMesh2D(grid,
                                                                                          logHandler
                );

                // Assert
                Assert.That(disposable2DMeshGeometry, Is.Null);
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void ApplyMesh2D_Read2DMeshTest_OpenFileApiReturnsIoNetCdfNativeError_ReportsWarningAndReturnsNull()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;
                var grid = Substitute.For<UnstructuredGrid>();
                var uGridApi = Substitute.For<IUGridApi>();

                uGridApi.IsUGridFile().Returns(true);
                var callCount = 0;
                uGridApi.When(x => x.Open(Arg.Any<string>())).Do(x =>
                {
                    callCount++;
                    if (callCount == 2)
                    {
                        throw ioNetCdfNativeError;
                    }
                });
                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                Disposable2DMeshGeometry disposable2DMeshGeometry = ugridFile.ApplyMesh2D(grid,
                                                                                          logHandler
                );

                // Assert
                Assert.That(disposable2DMeshGeometry, Is.Null);
                logHandler.Received(1).ReportWarning(defaultIoNetCdfNativeOpenErrorMessage);
                logHandler.Received(2).LogReport();
                uGridApi.Received(1).IsUGridFile();
                uGridApi.Received(2).Open(string.Empty, OpenMode.Reading);
            }
        }
    }
}