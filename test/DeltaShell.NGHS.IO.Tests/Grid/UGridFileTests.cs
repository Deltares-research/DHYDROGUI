using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Link1d2d;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using Deltares.Infrastructure.API.Logging;
using Deltares.UGrid.Api;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.IO.Grid.DeltaresUGrid;
using DeltaShell.NGHS.IO.Properties;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;
using SharpMap;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    [TestFixture]
    public class UGridFileTests
    {
        private readonly string defaultIoNetCdfNativeOpenErrorMessage = string.Format(Resources.IoNetCdfNativeError_Api_open_error___0______1___in_method__2__, 801, "An error occurred", "NativeCall");
        private readonly string defaultIoNetCdfNativeCallErrorMessage = string.Format(Resources.IoNetCdfNativeError_Api_call_error___0______1___in_method__2, 801, "An error occurred", "NativeCall");
        private IoNetCdfNativeError ioNetCdfNativeError = new IoNetCdfNativeError(801, "An error occurred", "NativeCall", "aMember");

        [Test]
        public void UGridFileTest_Constructor()
        {
            using (var ugridFile = new UGridFile(string.Empty))
            {
                // Assert
                Assert.That(ugridFile.Api, Is.Not.Null);
                Assert.That(ugridFile.UgridFileInfo, Is.InstanceOf<IUgridFileInfo>());
                Assert.That(ugridFile.Api, Is.InstanceOf<IUGridApi>());
            }
        }

        [Test]
        public void ReadZValuesTest_WithInvalidFile_LogWarning()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(false);
                ugridFile.UgridFileInfo = ugridFileInfo;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                ugridFile.ReadZValues(BedLevelLocation.CellEdges, logHandler);

                // Assert
                logHandler.Received(1).ReportWarningFormat(Resources.Could_not_read_file_0_properly_it_doesnt_exist, string.Empty);
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void ReadZValuesTest_WithUGridFileOpenFileNotFoundException_LogWarning()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;

                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.When(x => x.Open(Arg.Any<string>())).Do(x => { throw new FileNotFoundException("An error occurred"); });
                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                ugridFile.ReadZValues(BedLevelLocation.CellEdges, logHandler);

                // Assert
                logHandler.ReceivedWithAnyArgs(1).ReportWarningFormat(Resources.Could_not_read_file_0_properly_it_doesnt_exist, string.Empty + "An error occurred");
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void ReadZValuesTest_WithUGridFileOpenIoNetCdfNativeErrorException_LogWarning()
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

                // Act
                ugridFile.ReadZValues(BedLevelLocation.CellEdges, logHandler);

                // Assert
                logHandler.ReceivedWithAnyArgs(1).ReportWarning(defaultIoNetCdfNativeOpenErrorMessage);
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void ReadZValuesTest_WithValidUGridAndInIsUGridFileIoNetCdfNativeErrorException_LogWarning()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;

                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.When(x => x.IsUGridFile()).Do(x => { throw ioNetCdfNativeError; });
                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                ugridFile.ReadZValues(BedLevelLocation.CellEdges, logHandler);

                // Assert
                logHandler.ReceivedWithAnyArgs(1).ReportWarning(defaultIoNetCdfNativeOpenErrorMessage);
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void ReadZValuesTest_WithNonUGridFile_LogWarning()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;

                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.IsUGridFile().Returns(false);

                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                ugridFile.ReadZValues(BedLevelLocation.CellEdges, logHandler);

                // Assert
                logHandler.ReceivedWithAnyArgs(1).ReportWarningFormat(Resources.ReadZValues_Unable_to_read_z_values_from_file___0___file_is_not_UGrid_convention, string.Empty);
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void ReadZValuesTest_WithNoMesh2DInFile_LogWarning()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;

                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.IsUGridFile().Returns(true);
                uGridApi.GetMeshIdsByMeshType(UGridMeshType.Mesh2D).Returns(Array.Empty<int>());

                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                ugridFile.ReadZValues(BedLevelLocation.CellEdges, logHandler);

                // Assert
                logHandler.ReceivedWithAnyArgs(1).ReportWarningFormat(Resources.ReadZValues_No_2D_mesh_found_in_file__0, string.Empty);
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void ReadZValuesTest_WithUnknownLocationTypeInFile_LogWarning()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;

                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.IsUGridFile().Returns(true);
                uGridApi.GetMeshIdsByMeshType(UGridMeshType.Mesh2D).Returns(new[] { 1 });

                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                ugridFile.ReadZValues((BedLevelLocation)801, logHandler);

                // Assert
                logHandler.ReceivedWithAnyArgs(1).ReportWarningFormat(Resources.ReadZValues_The_expected_location_type__0__is_not_found_in_file__1, (BedLevelLocation)801, string.Empty);
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void ReadZValuesTest_HappyFlow_ReturnExpectedValues()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;

                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.IsUGridFile().Returns(true);
                uGridApi.GetMeshIdsByMeshType(UGridMeshType.Mesh2D).Returns(new[] { 1 });
                uGridApi.GetVariableValues(UGridConstants.Naming.FaceZ, 1, GridLocationType.Face).Returns(new[] { 8.01, 80.1 });

                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                double[] readValues = ugridFile.ReadZValues(BedLevelLocation.Faces, logHandler);

                // Assert
                logHandler.Received(1).LogReport();
                Assert.That(readValues[0], Is.EqualTo(8.01));
                Assert.That(readValues[1], Is.EqualTo(80.1));
            }
        }

        [Test]
        public void DisposeTest()
        {
            var ugridFile = new UGridFile(string.Empty);

            // Act
            ugridFile.Dispose();

            // Assert
            Assert.That(ugridFile.Api, Is.Null);
        }

        [Test]
        public void DestructorTest()
        {
            // Arrange
            UGridFile ugridFile = CreateAndReleaseUGridFile();
            var weakReference = new WeakReference(ugridFile);

            // Act
            ugridFile = null;              // Make the object eligible for GC
            GC.Collect();                  // Force garbage collection
            GC.WaitForPendingFinalizers(); // Wait for the finalizer to run
            GC.Collect();                  // Collect the object

            // Assert
            Assert.IsFalse(weakReference.IsAlive); // Check if the object was collected
        }

        private UGridFile CreateAndReleaseUGridFile()
        {
            var ugridFile = new UGridFile(string.Empty);
            ugridFile = null; // Make the object eligible for GC
            return ugridFile;
        }

        [Test]
        public void GetZCoordinateNoDataValueTest_WithInvalidFile_ReturnsDefaultNoDataValue()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(false);
                ugridFile.UgridFileInfo = ugridFileInfo;

                // Act
                double zCoordinateNoDataValue = ugridFile.GetZCoordinateNoDataValue(BedLevelLocation.CellEdges);

                // Assert
                Assert.That(zCoordinateNoDataValue, Is.EqualTo(UGridFile.DEFAULT_NO_DATA_VALUE));
            }
        }

        [Test]
        public void GetZCoordinateNoDataValueTest_WithUGridFileOpenFileNotFoundException_LogWarningAndReturnDefaultNoDataValue()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;

                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.When(x => x.Open(Arg.Any<string>())).Do(x => { throw new FileNotFoundException("An error occurred"); });
                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                double zCoordinateNoDataValue = ugridFile.GetZCoordinateNoDataValue(BedLevelLocation.CellEdges, logHandler);

                // Assert
                logHandler.ReceivedWithAnyArgs(1).ReportWarningFormat(Resources.Could_not_read_file_0_properly_it_doesnt_exist, string.Empty + "An error occurred");
                logHandler.Received(1).LogReport();
                Assert.That(zCoordinateNoDataValue, Is.EqualTo(UGridFile.DEFAULT_NO_DATA_VALUE));
            }
        }

        [Test]
        public void GetZCoordinateNoDataValueTest_WithUGridFileOpenIoNetCdfNativeErrorException_LogWarningReturnDefaultNoDataValue()
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

                // Act
                double zCoordinateNoDataValue = ugridFile.GetZCoordinateNoDataValue(BedLevelLocation.CellEdges, logHandler);

                // Assert
                logHandler.ReceivedWithAnyArgs(1).ReportWarning(defaultIoNetCdfNativeOpenErrorMessage);
                logHandler.Received(1).LogReport();
                Assert.That(zCoordinateNoDataValue, Is.EqualTo(UGridFile.DEFAULT_NO_DATA_VALUE));
            }
        }

        [Test]
        public void GetZCoordinateNoDataValueTest_WithValidUGridAndInIsUGridFileIoNetCdfNativeErrorException_LogWarningReturnDefaultNoDataValue()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;

                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.When(x => x.IsUGridFile()).Do(x => { throw ioNetCdfNativeError; });
                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                double zCoordinateNoDataValue = ugridFile.GetZCoordinateNoDataValue(BedLevelLocation.CellEdges, logHandler);

                // Assert
                logHandler.ReceivedWithAnyArgs(1).ReportWarning(defaultIoNetCdfNativeOpenErrorMessage);
                logHandler.Received(1).LogReport();
                Assert.That(zCoordinateNoDataValue, Is.EqualTo(UGridFile.DEFAULT_NO_DATA_VALUE));
            }
        }

        [Test]
        public void GetZCoordinateNoDataValueTest_WithNonUGridFile_ReturnDefaultNoDataValue()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;

                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.IsUGridFile().Returns(false);

                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                double zCoordinateNoDataValue = ugridFile.GetZCoordinateNoDataValue(BedLevelLocation.CellEdges, logHandler);

                // Assert
                Assert.That(zCoordinateNoDataValue, Is.EqualTo(UGridFile.DEFAULT_NO_DATA_VALUE));
            }
        }

        [Test]
        public void GetZCoordinateNoDataValueTest_WithNoMesh2DInFile_ReturnDefaultNoDataValue()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;

                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.IsUGridFile().Returns(true);
                uGridApi.GetMeshIdsByMeshType(UGridMeshType.Mesh2D).Returns(Array.Empty<int>());

                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                double zCoordinateNoDataValue = ugridFile.GetZCoordinateNoDataValue(BedLevelLocation.CellEdges, logHandler);

                // Assert
                Assert.That(zCoordinateNoDataValue, Is.EqualTo(UGridFile.DEFAULT_NO_DATA_VALUE));
            }
        }

        [Test]
        public void GetZCoordinateNoDataValueTest_WithUnknownLocationTypeInFile_ReturnDefaultNoDataValue()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;

                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.IsUGridFile().Returns(true);
                uGridApi.GetMeshIdsByMeshType(UGridMeshType.Mesh2D).Returns(new[] { 1 });

                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                double zCoordinateNoDataValue = ugridFile.GetZCoordinateNoDataValue((BedLevelLocation)801, logHandler);

                // Assert
                Assert.That(zCoordinateNoDataValue, Is.EqualTo(UGridFile.DEFAULT_NO_DATA_VALUE));
            }
        }

        [Test]
        public void GetZCoordinateNoDataValueTest_HappyFlow_ReturnExpectedValues()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;

                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.IsUGridFile().Returns(true);
                uGridApi.GetMeshIdsByMeshType(UGridMeshType.Mesh2D).Returns(new[] { 1 });
                uGridApi.GetVariableNoDataValue(UGridConstants.Naming.FaceZ, 1, GridLocationType.Face).Returns(80.1);

                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                double zCoordinateNoDataValue = ugridFile.GetZCoordinateNoDataValue(BedLevelLocation.Faces, logHandler);

                // Assert
                uGridApi.Received(1).GetMeshIdsByMeshType(UGridMeshType.Mesh2D);
                uGridApi.Received(1).GetVariableNoDataValue(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<GridLocationType>());
                logHandler.Received(1).LogReport();
                Assert.That(zCoordinateNoDataValue, Is.EqualTo(80.1));
            }
        }

        [Test]
        public void ReadCoordinateSystemTest_WithInvalidFile_ReturnsDefaultNull()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(false);
                ugridFile.UgridFileInfo = ugridFileInfo;

                // Act
                ICoordinateSystem coordinateSystem = ugridFile.ReadCoordinateSystem();

                // Assert
                Assert.That(coordinateSystem, Is.Null);
            }
        }

        [Test]
        public void ReadCoordinateSystemTest_WithUGridFileOpenFileNotFoundException_LogWarningAndReturnDefaultNull()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;

                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.When(x => x.Open(Arg.Any<string>())).Do(x => { throw new FileNotFoundException("An error occurred"); });
                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                ICoordinateSystem coordinateSystem = ugridFile.ReadCoordinateSystem(logHandler);

                // Assert
                logHandler.ReceivedWithAnyArgs(1).ReportWarningFormat(Resources.Could_not_read_file_0_properly_it_doesnt_exist, string.Empty + "An error occurred");
                logHandler.Received(1).LogReport();
                Assert.That(coordinateSystem, Is.Null);
            }
        }

        [Test]
        public void ReadCoordinateSystemTest_WithUGridFileOpenIoNetCdfNativeErrorException_LogWarningReturnNull()
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

                // Act
                ICoordinateSystem coordinateSystem = ugridFile.ReadCoordinateSystem(logHandler);

                // Assert
                logHandler.ReceivedWithAnyArgs(1).ReportWarning(defaultIoNetCdfNativeOpenErrorMessage);
                logHandler.Received(1).LogReport();
                Assert.That(coordinateSystem, Is.Null);
            }
        }

        [Test]
        public void ReadCoordinateSystemTest_WithValidUGridAndInIsUGridFileIoNetCdfNativeErrorException_LogWarningReturnNull()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;

                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.When(x => x.IsUGridFile()).Do(x => { throw ioNetCdfNativeError; });
                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                ICoordinateSystem coordinateSystem = ugridFile.ReadCoordinateSystem(logHandler);

                // Assert
                logHandler.ReceivedWithAnyArgs(1).ReportWarning(defaultIoNetCdfNativeOpenErrorMessage);
                logHandler.Received(1).LogReport();
                Assert.That(coordinateSystem, Is.Null);
            }
        }

        [Test]
        public void ReadCoordinateSystemTest_WithNonUGridFile_ReturnNull()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;

                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.IsUGridFile().Returns(false);

                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                ICoordinateSystem coordinateSystem = ugridFile.ReadCoordinateSystem(logHandler);

                // Assert
                Assert.That(coordinateSystem, Is.Null);
            }
        }

        [Test]
        public void ReadCoordinateSystemTest_WithValidUGridAndInGetCoordinateSystemCodeIoNetCdfNativeErrorException_LogWarningReturnNull()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;

                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.IsUGridFile().Returns(true);
                uGridApi.When(x => x.GetCoordinateSystemCode()).Do(x => { throw ioNetCdfNativeError; });
                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                ICoordinateSystem coordinateSystem = ugridFile.ReadCoordinateSystem(logHandler);

                // Assert
                logHandler.ReceivedWithAnyArgs(1).ReportWarning(defaultIoNetCdfNativeOpenErrorMessage);
                logHandler.Received(1).LogReport();
                Assert.That(coordinateSystem, Is.Null);
            }
        }

        [Test]
        public void ReadCoordinateSystemTest_WithGetCoordinateSystemCodeIs0_ReturnNull()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;

                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.IsUGridFile().Returns(true);
                uGridApi.GetCoordinateSystemCode().Returns(0);

                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                ICoordinateSystem coordinateSystem = ugridFile.ReadCoordinateSystem(logHandler);

                // Assert
                Assert.That(coordinateSystem, Is.Null);
            }
        }

        [Test]
        public void ReadCoordinateSystemTest_WithGetCoordinateSystemCodeIsRdNew28992_ReturnRdNew()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;

                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.IsUGridFile().Returns(true);
                uGridApi.GetCoordinateSystemCode().Returns(28992);

                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                ICoordinateSystem coordinateSystem = ugridFile.ReadCoordinateSystem(logHandler);

                // Assert
                Assert.That(coordinateSystem, Is.Not.Null);
                Assert.That(coordinateSystem.AuthorityCode, Is.EqualTo(28992));
                Assert.That(coordinateSystem.Name, Is.EqualTo("Amersfoort / RD New"));
            }
        }

        [Test]
        public void WriteZValuesTest_WithUGridFileOpenFileNotFoundException_LogWarningAndReturnDefaultNull()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.When(x => x.Open(Arg.Any<string>(), OpenMode.Appending)).Do(x => { throw new FileNotFoundException("An error occurred"); });
                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                ugridFile.WriteZValues(BedLevelLocation.Faces, Enumerable.Repeat(80.1, 10).ToArray(), logHandler);

                // Assert
                logHandler.ReceivedWithAnyArgs(1).ReportWarningFormat(Resources.Could_not_read_file_0_properly_it_doesnt_exist, string.Empty + "An error occurred");
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void WriteZValuesTest_WithUGridFileOpenIoNetCdfNativeErrorException_LogWarningReturnNull()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.When(x => x.Open(Arg.Any<string>(), OpenMode.Appending)).Do(x => { throw ioNetCdfNativeError; });
                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                ugridFile.WriteZValues(BedLevelLocation.Faces, Enumerable.Repeat(80.1, 10).ToArray(), logHandler);

                // Assert
                logHandler.ReceivedWithAnyArgs(1).ReportWarning(defaultIoNetCdfNativeOpenErrorMessage);
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void WriteZValuesTest_WithValidUGridAndInIsUGridFileIoNetCdfNativeErrorException_LogWarningReturnNull()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.When(x => x.IsUGridFile()).Do(x => { throw ioNetCdfNativeError; });
                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                ugridFile.WriteZValues(BedLevelLocation.Faces, Enumerable.Repeat(80.1, 10).ToArray(), logHandler);

                // Assert
                logHandler.ReceivedWithAnyArgs(1).ReportWarningFormat(defaultIoNetCdfNativeOpenErrorMessage);
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void WriteZValuesTest_WithNonUGridFileTriesToWriteWithPlainNetCDFCalls_ThrowsFileNotFoundException()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.IsUGridFile().Returns(false);

                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                void Call() => ugridFile.WriteZValues(BedLevelLocation.Faces, Enumerable.Repeat(80.1, 10).ToArray(), logHandler);

                // Assert
                Assert.That(Call, Throws.TypeOf<FileNotFoundException>());
            }
        }

        [Test]
        public void WriteZValuesTest_WithUGridFileButNoMesh2dIds_LogsWarning()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.IsUGridFile().Returns(true);
                uGridApi.GetMeshIdsByMeshType(UGridMeshType.Mesh2D).Returns(Array.Empty<int>());

                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                ugridFile.WriteZValues(BedLevelLocation.Faces, Enumerable.Repeat(80.1, 10).ToArray(), logHandler);

                // Assert
                uGridApi.Received(1).Open(string.Empty, OpenMode.Appending);
                uGridApi.Received(1).IsUGridFile();
                uGridApi.Received(1).GetMeshIdsByMeshType(UGridMeshType.Mesh2D);

                logHandler.ReceivedWithAnyArgs(1).ReportWarning(string.Format(Resources.WriteZValuesWithApi_Unable_to_write_z_values_to_file____0____no_2d_mesh_found, string.Empty));
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void WriteZValuesTest_WithUGridFileAndLocationTypeCellEdges_LogsWarning()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.IsUGridFile().Returns(true);
                uGridApi.GetMeshIdsByMeshType(UGridMeshType.Mesh2D).Returns(new[] { 1 });

                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                ugridFile.WriteZValues(BedLevelLocation.CellEdges, Enumerable.Repeat(80.1, 10).ToArray(), logHandler);

                // Assert
                logHandler.ReceivedWithAnyArgs(1).ReportWarning(Resources.ReadZValues_Unable_to_read_z_values_at_this_location__CellEdges_are_not_currently_supported);
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void WriteZValuesTest_WithUGridFileAndInvalidLocationTypeCellEdges_LogsWarning()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.IsUGridFile().Returns(true);
                uGridApi.GetMeshIdsByMeshType(UGridMeshType.Mesh2D).Returns(new[] { 1 });

                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                ugridFile.WriteZValues((BedLevelLocation)801, Enumerable.Repeat(80.1, 10).ToArray(), logHandler);

                // Assert
                logHandler.ReceivedWithAnyArgs(1).ReportWarning(string.Format(Resources.UGridFile_GetLocationType_Unsupported_bed_level_location___0_, 801));
                logHandler.Received(1).LogReport();
            }
        }

        [TestCase(BedLevelLocation.Faces)]
        [TestCase(BedLevelLocation.FacesMeanLevFromNodes)]
        [TestCase(BedLevelLocation.NodesMaxLev)]
        [TestCase(BedLevelLocation.NodesMeanLev)]
        [TestCase(BedLevelLocation.NodesMinLev)]
        public void WriteZValuesTest_WithUGridFileAndValidBedLevelLocation_LogsNothing(BedLevelLocation location)
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.IsUGridFile().Returns(true);
                uGridApi.GetMeshIdsByMeshType(UGridMeshType.Mesh2D).Returns(new[] { 1 });

                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                ugridFile.WriteZValues(location, Enumerable.Repeat(80.1, 10).ToArray(), logHandler);

                // Assert
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void WriteZValuesTest_WithUGridFileAndValidBedLevelLocationButApiSetVariableValues_LogsWarning()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                double[] values = Enumerable.Repeat(80.1, 10).ToArray();

                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.IsUGridFile().Returns(true);
                uGridApi.GetMeshIdsByMeshType(UGridMeshType.Mesh2D).Returns(new[] { 1 });
                uGridApi.When(x =>
                                  x.SetVariableValues(Arg.Any<string>(),
                                                      Arg.Any<string>(),
                                                      Arg.Any<string>(),
                                                      Arg.Any<string>(),
                                                      Arg.Any<int>(),
                                                      GridLocationType.Face,
                                                      values))
                        .Do(x =>
                        {
                            throw ioNetCdfNativeError;
                        });

                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                ugridFile.WriteZValues(BedLevelLocation.Faces, values, logHandler);

                // Assert
                logHandler.Received(1).ReportWarning(defaultIoNetCdfNativeCallErrorMessage);
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void WriteCoordinateSystemTest_WithNonUGridFileTriesToWriteWithPlainNetCDFCalls_ThrowsFileNotFoundException()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.IsUGridFile().Returns(false);

                ugridFile.Api = uGridApi;

                var coordinateSystem = Substitute.For<ICoordinateSystem>();

                // Act
                void Call() => ugridFile.WriteCoordinateSystem(coordinateSystem);

                // Assert
                Assert.That(Call, Throws.TypeOf<FileNotFoundException>());
            }
        }

        [Test]
        public void WriteCoordinateSystemTest_WithUGridFileTriesToWriteWithPlainNetCDFCalls_ThrowsNothing()
        {
            string filePath = TestHelper.CreateLocalCopySingleFile(Path.Combine(TestHelper.GetTestDataDirectory(), "ugrid", "Custom_Ugrid.nc"));
            // Arrange 
            using (var ugridFile = new UGridFile(filePath))
            {
                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.IsUGridFile().Returns(true);

                ugridFile.Api = uGridApi;

                var coordinateSystem = Substitute.For<ICoordinateSystem>();

                // Act
                void Call() => ugridFile.WriteCoordinateSystem(coordinateSystem);

                // Assert
                Assert.That(Call, Throws.Nothing);
            }

            FileUtils.DeleteIfExists(filePath);
        }

        [Test]
        public void IsUGridFileTest_WithInvalidFile_ReturnsFalse()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(false);
                ugridFile.UgridFileInfo = ugridFileInfo;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                bool isUgridFile = ugridFile.IsUGridFile(logHandler);

                // Assert
                Assert.That(isUgridFile, Is.False);
            }
        }

        [Test]
        public void IsUGridFileTest_WithUGridFileOpenIoNetCdfNativeErrorException_LogWarningAndIsFalse()
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

                // Act
                bool isUgridFile = ugridFile.IsUGridFile(logHandler);

                // Assert
                Assert.That(isUgridFile, Is.False);

                logHandler.ReceivedWithAnyArgs(1).ReportWarning(defaultIoNetCdfNativeOpenErrorMessage);
                logHandler.Received(1).LogReport();
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void IsUGridFileTest_WithUGridFileOpenAndApiReturnValue_ReturnExpectedValue(bool expectation)
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;

                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.IsUGridFile().Returns(expectation);

                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                bool isUgridFile = ugridFile.IsUGridFile(logHandler);

                // Assert
                Assert.That(isUgridFile, Is.EqualTo(expectation));
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void IsUGridFileTest_WithUGridFileOpenAndApiReturnValueRunTwiceUseCachedResult_ReturnExpectedValue(bool expectation)
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;

                var uGridApi = Substitute.For<IUGridApi>();
                uGridApi.IsUGridFile().Returns(expectation);

                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                bool isUgridFile = ugridFile.IsUGridFile(logHandler);
                isUgridFile = ugridFile.IsUGridFile(logHandler);

                // Assert
                Assert.That(isUgridFile, Is.EqualTo(expectation));
            }
        }

        [Test]
        public void IsUGridFileTest_WithUGridFileOpenAndApiReturnValueThrowsIoNetCdfNativeError_LogsWarningAndReturnFalse()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;

                var uGridApi = Substitute.For<IUGridApi>();

                uGridApi.When(x =>
                                  x.IsUGridFile())
                        .Do(x =>
                        {
                            throw ioNetCdfNativeError;
                        });
                ugridFile.Api = uGridApi;

                var logHandler = Substitute.For<ILogHandler>();

                // Act
                bool isUgridFile = ugridFile.IsUGridFile(logHandler);

                // Assert
                Assert.That(isUgridFile, Is.False);
                logHandler.Received(1).ReportWarning(defaultIoNetCdfNativeOpenErrorMessage);
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void WriteGridToFileTest_AllEmptyModelEntities_ThrowsNothing()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var uGridApi = Substitute.For<IUGridApi>();
                ugridFile.Api = uGridApi;

                var grid = Substitute.For<UnstructuredGrid>();
                var hydroNetwork = Substitute.For<IHydroNetwork>();
                var discretization = Substitute.For<IDiscretization>();
                var links = Substitute.For<IEnumerable<ILink1D2D>>();
                string name = TestHelper.GetCurrentMethodName();
                string pluginName = null;
                string pluginVersion = null;
                var location = BedLevelLocation.Faces;
                double[] zValues = Array.Empty<double>();
                var logHandler = Substitute.For<ILogHandler>();

                // Act
                void WriteGridToFile()
                {
                    ugridFile.InitializeMetaData(name, pluginName, pluginVersion);
                    ugridFile.WriteGridToFile(grid, hydroNetwork, discretization, links, location, zValues, logHandler);
                }

                // Assert
                Assert.That(WriteGridToFile, Throws.Nothing);
                uGridApi.Received(1).CreateFile(string.Empty, Arg.Any<FileMetaData>());
                uGridApi.Received(1).WriteMesh2D(Arg.Any<Disposable2DMeshGeometry>());
                uGridApi.Received(1).GetMeshIdsByMeshType(UGridMeshType.Mesh2D);
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void WriteGridToFileTest_WithNetworkModelEntity_ThrowsNothingAndRelevantApiCallsAreCalled()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var uGridApi = Substitute.For<IUGridApi>();
                ugridFile.Api = uGridApi;

                var grid = Substitute.For<UnstructuredGrid>();
                var hydroNetwork = Substitute.For<IHydroNetwork>();
                var nodes = Substitute.For<IEventedList<INode>>();
                nodes.Count.Returns(1, 0);
                hydroNetwork.Nodes.Returns(nodes);
                var discretization = Substitute.For<IDiscretization>();
                var links = Substitute.For<IEnumerable<ILink1D2D>>();
                string name = TestHelper.GetCurrentMethodName();
                string pluginName = null;
                string pluginVersion = null;
                var location = BedLevelLocation.Faces;
                double[] zValues = Array.Empty<double>();
                var logHandler = Substitute.For<ILogHandler>();

                // Act
                void WriteGridToFile()
                {
                    ugridFile.InitializeMetaData(name, pluginName, pluginVersion);
                    ugridFile.WriteGridToFile(grid, hydroNetwork, discretization, links, location, zValues, logHandler);
                }

                // Assert
                Assert.That(WriteGridToFile, Throws.Nothing);
                uGridApi.Received(1).CreateFile(string.Empty, Arg.Any<FileMetaData>());
                uGridApi.Received(1).WriteMesh2D(Arg.Any<Disposable2DMeshGeometry>());
                uGridApi.Received(1).GetMeshIdsByMeshType(UGridMeshType.Mesh2D);
                uGridApi.Received(1).WriteNetworkGeometry(Arg.Any<DisposableNetworkGeometry>());
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void WriteGridToFileTest_WithNetworkAndDiscretizationModelEntities_ThrowsNothing()
        {
            // Arrange 
            using (var ugridFile = new UGridFile(string.Empty))
            {
                var uGridApi = Substitute.For<IUGridApi>();
                ugridFile.Api = uGridApi;

                var grid = Substitute.For<UnstructuredGrid>();
                var hydroNetwork = Substitute.For<IHydroNetwork>();
                var nodes = Substitute.For<IEventedList<INode>>();
                nodes.Count.Returns(1, 0);
                hydroNetwork.Nodes.Returns(nodes);

                var discretization = Substitute.For<IDiscretization>();
                var locationValues = Substitute.For<IMultiDimensionalArray<INetworkLocation>>();
                locationValues.Count.Returns(1, 0);
                var locations = Substitute.For<IVariable<INetworkLocation>>();
                locations.Values.Returns(locationValues);
                discretization.Locations.Returns(locations);
                var links = Substitute.For<IEnumerable<ILink1D2D>>();
                string name = TestHelper.GetCurrentMethodName();
                string pluginName = null;
                string pluginVersion = null;
                var location = BedLevelLocation.Faces;
                double[] zValues = Array.Empty<double>();
                var logHandler = Substitute.For<ILogHandler>();

                // Act
                void WriteGridToFile()
                {
                    ugridFile.InitializeMetaData(name, pluginName, pluginVersion);
                    ugridFile.WriteGridToFile(grid, hydroNetwork, discretization, links, location, zValues, logHandler);
                }

                // Assert
                Assert.That(WriteGridToFile, Throws.Nothing);
                uGridApi.Received(1).CreateFile(string.Empty, Arg.Any<FileMetaData>());
                uGridApi.Received(1).WriteMesh2D(Arg.Any<Disposable2DMeshGeometry>());
                uGridApi.Received(1).GetMeshIdsByMeshType(UGridMeshType.Mesh2D);
                uGridApi.Received(1).WriteNetworkGeometry(Arg.Any<DisposableNetworkGeometry>());
                uGridApi.Received(1).WriteMesh1D(Arg.Any<Disposable1DMeshGeometry>(), Arg.Any<int>());
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void GivenUGridMeshAdapterWithSnakeBranchWithGeometryInRdNew_DoingCreateDisposableNetworkGeometry_ShouldGiveValidDisposableNetworkGeometry()
        {
            //Arrange
            Channel channel = GenerateSnakeChannelInANetworkWithRdNewCoordinateSystem();
            var ugridFileInfo = Substitute.For<IUgridFileInfo>();
            ugridFileInfo.IsValidPath().Returns(true);
            var api = Substitute.For<IUGridApi>();
            api.IsUGridFile().Returns(true);
            api.GetNetworkIds().Returns(new[] { 1 });
            api.GetCoordinateSystemCode().Returns(0); //return nothing, user has removed crs

            // Act
            DisposableNetworkGeometry networkGeometry = ((HydroNetwork)channel.Network).CreateDisposableNetworkGeometry();
            api.GetNetworkGeometry(1).Returns(networkGeometry);
            HydroNetwork recreatedNetworkFromDisposableNetworkGeometry = new HydroNetwork();

            using (var ugridFile = new UGridFile(string.Empty))
            {
                ugridFile.Api = api;
                ugridFile.UgridFileInfo = ugridFileInfo;
                ugridFile.ApplyNetworkGeometry(recreatedNetworkFromDisposableNetworkGeometry, Enumerable.Empty<CompartmentProperties>(), Enumerable.Empty<BranchProperties>());
            }

            // Assert
            Assert.That(channel.Length, Is.Not.EqualTo(channel.GeometryLength));
            Assert.That(channel.Length, Is.EqualTo(channel.GeodeticLength));

            Assert.AreEqual(2, networkGeometry.NodeIds.Length);
            Assert.AreEqual(1, networkGeometry.BranchIds.Length);

            Assert.AreEqual(2, recreatedNetworkFromDisposableNetworkGeometry.Nodes.Count);
            Assert.AreEqual(1, recreatedNetworkFromDisposableNetworkGeometry.Branches.Count);
            Channel recreatedBranch = (Channel)recreatedNetworkFromDisposableNetworkGeometry.Branches[0];

            Assert.That(channel.Length, Is.EqualTo(recreatedBranch.Length));
            Assert.That(channel.IsLengthCustom, Is.False);
            Assert.That(recreatedBranch.IsLengthCustom, Is.True);
            Assert.That(recreatedBranch.Network.CoordinateSystem, Is.Null);
        }

        [Test]
        public void GivenUGridMeshAdapterWithSnakeBranchWithGeometryInRdNewAndTargetToo_DoingCreateDisposableNetworkGeometry_ShouldGiveValidDisposableNetworkGeometry()
        {
            //Arrange
            Channel channel = GenerateSnakeChannelInANetworkWithRdNewCoordinateSystem();
            var ugridFileInfo = Substitute.For<IUgridFileInfo>();
            ugridFileInfo.IsValidPath().Returns(true);
            var api = Substitute.For<IUGridApi>();
            api.IsUGridFile().Returns(true);
            api.GetNetworkIds().Returns(new[] { 1 });
            api.GetCoordinateSystemCode().Returns(28992);//return rd new, saved data has same crs as before saving.

            // Act
            DisposableNetworkGeometry networkGeometry = ((HydroNetwork)channel.Network).CreateDisposableNetworkGeometry();
            api.GetNetworkGeometry(1).Returns(networkGeometry);
            HydroNetwork recreatedNetworkFromDisposableNetworkGeometry = new HydroNetwork();

            using (var ugridFile = new UGridFile(string.Empty))
            {
                ugridFile.Api = api;
                ugridFile.UgridFileInfo = ugridFileInfo;
                ugridFile.ApplyNetworkGeometry(recreatedNetworkFromDisposableNetworkGeometry, Enumerable.Empty<CompartmentProperties>(), Enumerable.Empty<BranchProperties>());
            }

            // Assert
            Assert.That(channel.Length, Is.Not.EqualTo(channel.GeometryLength));
            Assert.That(channel.Length, Is.EqualTo(channel.GeodeticLength));

            Assert.AreEqual(2, networkGeometry.NodeIds.Length);
            Assert.AreEqual(1, networkGeometry.BranchIds.Length);

            Assert.AreEqual(2, recreatedNetworkFromDisposableNetworkGeometry.Nodes.Count);
            Assert.AreEqual(1, recreatedNetworkFromDisposableNetworkGeometry.Branches.Count);
            Channel recreatedBranch = (Channel)recreatedNetworkFromDisposableNetworkGeometry.Branches[0];

            Assert.That(channel.Length, Is.EqualTo(recreatedBranch.Length));
            Assert.That(channel.IsLengthCustom, Is.False);
            Assert.That(recreatedBranch.IsLengthCustom, Is.False);
            Assert.That(recreatedBranch.Network.CoordinateSystem, Is.EqualTo(channel.HydroNetwork.CoordinateSystem));
        }
        
        private Channel GenerateSnakeChannelInANetworkWithRdNewCoordinateSystem()
        {
            var network = new HydroNetwork();
            var hydroNodeLB = new HydroNode("LeftBottom");
            var hydroNodeTR = new HydroNode("TopRight");
            if (Map.CoordinateSystemFactory == null) Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
            var rdCoordinateSystem = Map.CoordinateSystemFactory.CreateFromEPSG(28992);
            network.CoordinateSystem = rdCoordinateSystem;

            hydroNodeLB.Geometry = new Point(0, 300000);
            hydroNodeTR.Geometry = new Point(300000, 620000);

            var channel = new Channel(hydroNodeLB, hydroNodeTR);
            var coordinates = CreateWaveGeometryPoints(hydroNodeLB.Geometry.Centroid, hydroNodeTR.Geometry.Centroid).ToArray();
            Coordinate lastCoordinate = coordinates.Last();
            hydroNodeTR.Geometry = new Point(lastCoordinate.X, lastCoordinate.Y);
            channel.Geometry = new LineString(coordinates);
            channel.GeodeticLength = GeodeticDistance.Length(network.CoordinateSystem, channel.Geometry);

            network.Nodes.Add(hydroNodeLB);
            network.Nodes.Add(hydroNodeTR);
            network.Branches.Add(channel);
            return channel;
        }

        private IEnumerable<Coordinate> CreateWaveGeometryPoints(IPoint start, IPoint boundPoint)
        {
            double x = start.X;
            double y = start.Y;
            yield return new Coordinate(x, y);
            for (int i = 0; i < boundPoint.X; i++)
            {
                x += i;
                y = CalculateWaveY(x, start.X, start.Y);
                if (y > boundPoint.Y)
                    yield break;
                yield return new Coordinate(x, y);
            }
        }

        private double CalculateWaveY(double x, double offsetX, double offsetY)
        {
            return (x - offsetX) + 5000 * (Math.Sin((2 * Math.PI) / 10000 * (x - offsetX))) + offsetY;
        }

    }
}