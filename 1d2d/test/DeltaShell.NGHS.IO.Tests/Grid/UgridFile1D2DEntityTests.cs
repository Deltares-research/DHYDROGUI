using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Link1d2d;
using Deltares.Infrastructure.API.Logging;
using Deltares.UGrid.Api;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.IO.Grid.DeltaresUGrid;
using DeltaShell.NGHS.IO.Properties;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    [TestFixture]
    public class UgridFile1D2DEntityTests
    {
        [Test]
        public void Read1D2DLinksTest_GeneratedObjectsForLinksIsNull_ThrowsNullException()
        {
            // Arrange 
            using (UGridFile ugridFile = new UGridFile(String.Empty))
            {
                //Act
                void Call() =>
                    ugridFile.Read1D2DLinks(null,
                                          null
                    );

                // Assert
                Assert.That(Call, Throws.ArgumentNullException);
            }
        }
        
        [Test]
        public void Read1D2DLinksTest_LogHandlerIsNull_ThrowsNullException()
        {
            // Arrange 
            using (UGridFile ugridFile = new UGridFile(String.Empty))
            {
                IGeneratedObjectsForLinks generatedObjectsForLinks = Substitute.For<IGeneratedObjectsForLinks>();

                //Act
                void Call() =>
                    ugridFile.Read1D2DLinks(generatedObjectsForLinks,
                                          null
                    );

                // Assert
                Assert.That(Call, Throws.ArgumentNullException);
            }
        }

        [Test]
        public void Read1D2DLinksTest_OpenApiThrowsIoNetCdfNativeError_LogsWarning()
        {
            // Arrange 
            using (UGridFile ugridFile = new UGridFile(String.Empty))
            {
                IGeneratedObjectsForLinks generatedObjectsForLinks = Substitute.For<IGeneratedObjectsForLinks>();
                
                IUGridApi uGridApi = Substitute.For<IUGridApi>();
                uGridApi.When(x => x.Open(Arg.Any<string>())).Do(x => { throw new IoNetCdfNativeError(801, "An error occurred", "NativeCall", "aMember"); });
                ugridFile.Api = uGridApi;

                ILogHandler logHandler = Substitute.For<ILogHandler>();

                //Act
                ugridFile.Read1D2DLinks(generatedObjectsForLinks, logHandler);

                // Assert
                logHandler.Received(1).ReportWarning(Arg.Any<string>());
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void Read1D2DLinksTest_NotIsUGridFileApi_LogsError()
        {
            // Arrange 
            using (UGridFile ugridFile = new UGridFile(String.Empty))
            {
                IGeneratedObjectsForLinks generatedObjectsForLinks = Substitute.For<IGeneratedObjectsForLinks>();
                
                IUGridApi uGridApi = Substitute.For<IUGridApi>();
                uGridApi.IsUGridFile().Returns(false);
                ugridFile.Api = uGridApi;

                ILogHandler logHandler = Substitute.For<ILogHandler>();

                //Act
                ugridFile.Read1D2DLinks(generatedObjectsForLinks, logHandler);

                // Assert
                logHandler.Received(1).ReportError(string.Format(Resources.GeneratedObjectsForLinks_Read1D2DLinks_Could_not_load_links_from__0___This_is_not_a_UGrid_file_, string.Empty));
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void Read1D2DLinksTest_IsUgridApiThrowsIoNetCdfNativeError_LogsWarning()
        {
            // Arrange 
            using (UGridFile ugridFile = new UGridFile(String.Empty))
            {
                IGeneratedObjectsForLinks generatedObjectsForLinks = Substitute.For<IGeneratedObjectsForLinks>();
                IUGridApi uGridApi = Substitute.For<IUGridApi>();

                uGridApi.When(x => x.IsUGridFile()).Do(x => { throw new IoNetCdfNativeError(801, "An error occurred", "NativeCall", "aMember"); });
                ugridFile.Api = uGridApi;

                ILogHandler logHandler = Substitute.For<ILogHandler>();

                //Act
                ugridFile.Read1D2DLinks(generatedObjectsForLinks, logHandler);

                // Assert
                logHandler.Received(1).ReportWarning(Arg.Any<string>());
                logHandler.Received(1).LogReport();
            }
        }
        
        [Test]
        public void Read1D2DLinksTest_Links1D2DListClearThrowsNotSupportedException_LogsError()
        {
            // Arrange 
            using (UGridFile ugridFile = new UGridFile(String.Empty))
            {
                IGeneratedObjectsForLinks generatedObjectsForLinks = Substitute.For<IGeneratedObjectsForLinks>();
                var link1D2Ds = new List<ILink1D2D>();
                generatedObjectsForLinks.Links1D2D.Returns(link1D2Ds);
                generatedObjectsForLinks.When(o => o.Links1D2D?.Clear()).Do(o => { throw new NotSupportedException(); });
                IUGridApi uGridApi = Substitute.For<IUGridApi>();

                uGridApi.IsUGridFile().Returns(false);
                ugridFile.Api = uGridApi;

                ILogHandler logHandler = Substitute.For<ILogHandler>();

                //Act
                ugridFile.Read1D2DLinks(generatedObjectsForLinks, logHandler);

                // Assert
                logHandler.Received(1).ReportError(string.Format(Resources.GeneratedObjectsForLinks_Read1D2DLinks_Could_not_load_links_from__0___This_is_not_a_UGrid_file_, string.Empty));
                logHandler.Received(1).ReportError(string.Format(Resources.UGridFile_Read1D2DLinks_Cannot_clear__0___list_has_no_Clear_support__error___1_, nameof(generatedObjectsForLinks.Links1D2D), "Specified method is not supported."));
                logHandler.Received(1).LogReport();
            }
        }

        [Test]
        public void Read1D2DLinksTest_NothingInFile_ThrowsNothing()
        {
            // Arrange 
            using (UGridFile ugridFile = new UGridFile(String.Empty))
            {
                IGeneratedObjectsForLinks generatedObjectsForLinks = Substitute.For<IGeneratedObjectsForLinks>();

                IUGridApi uGridApi = Substitute.For<IUGridApi>();
                uGridApi.IsUGridFile().Returns(true);
                uGridApi.GetLinksId().Returns(-1);
                ugridFile.Api = uGridApi;

                ILogHandler logHandler = Substitute.For<ILogHandler>();

                //Act
                void Call() => ugridFile.Read1D2DLinks(generatedObjectsForLinks, logHandler);

                // Assert
                Assert.That(Call, Throws.Nothing);
            }
        }
        
        [Test]
        public void Read1D2DLinksTest_NoLinksInFile_ButMesh2DIs_SetInLinksObjectAndThrowsNothing()
        {
            // Arrange 
            using (UGridFile ugridFile = new UGridFile(String.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;

                IGeneratedObjectsForLinks generatedObjectsForLinks = Substitute.For<IGeneratedObjectsForLinks>();
                generatedObjectsForLinks.Grid.Returns(new UnstructuredGrid());
                generatedObjectsForLinks.Mesh2d = null;
                generatedObjectsForLinks.FillValueMesh2DFaceNodes = 8;

                Disposable2DMeshGeometry disposable2DMesh = new Disposable2DMeshGeometry();
                IUGridApi uGridApi = Substitute.For<IUGridApi>();
                uGridApi.IsUGridFile().Returns(true);
                uGridApi.GetMeshIdsByMeshType(UGridMeshType.Mesh2D).Returns(new[] { 1 });
                uGridApi.GetMesh2D(1).Returns(disposable2DMesh);
                uGridApi.GetVariableNoDataValue(Arg.Any<string>(), 1, GridLocationType.Face).Returns(801);
                uGridApi.GetLinksId().Returns(-1);
                ugridFile.Api = uGridApi;

                ILogHandler logHandler = Substitute.For<ILogHandler>();

                //Act
                void Call() => ugridFile.Read1D2DLinks(generatedObjectsForLinks, logHandler);

                // Assert
                Assert.That(Call, Throws.Nothing);
                Assert.That(generatedObjectsForLinks.Mesh2d, Is.EqualTo(disposable2DMesh));
                Assert.That(generatedObjectsForLinks.FillValueMesh2DFaceNodes, Is.EqualTo(801));
            }
        }
        
        [Test]
        public void Read1D2DLinksTest_NoLinksInFile_ButMesh2DAndMesh1DAre_SetInLinksObjectAndThrowsNothing()
        {
            // Arrange 
            using (UGridFile ugridFile = new UGridFile(String.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;

                IGeneratedObjectsForLinks generatedObjectsForLinks = Substitute.For<IGeneratedObjectsForLinks>();
                UnstructuredGrid unstructuredGrid = new UnstructuredGrid();
                generatedObjectsForLinks.Grid.Returns(unstructuredGrid);
                generatedObjectsForLinks.Mesh2d = null;
                generatedObjectsForLinks.FillValueMesh2DFaceNodes = 8;
                
                IDiscretization discretization = Substitute.For<IDiscretization>();
                generatedObjectsForLinks.Discretization.Returns(discretization);
                generatedObjectsForLinks.Mesh1d = null;
                
                Disposable2DMeshGeometry disposable2DMesh = new Disposable2DMeshGeometry();
                Disposable1DMeshGeometry disposable1DMesh = new Disposable1DMeshGeometry { NodeIds = Array.Empty<string>() };
                IUGridApi uGridApi = Substitute.For<IUGridApi>();
                uGridApi.IsUGridFile().Returns(true);
                uGridApi.GetMeshIdsByMeshType(UGridMeshType.Mesh2D).Returns(new[] { 1 });
                uGridApi.GetMesh2D(1).Returns(disposable2DMesh);
                uGridApi.GetVariableNoDataValue(Arg.Any<string>(), 1, GridLocationType.Face).Returns(801);
                uGridApi.GetMeshIdsByMeshType(UGridMeshType.Mesh1D).Returns(new[] { 1 });
                uGridApi.GetMesh1D(1).Returns(disposable1DMesh);
                uGridApi.GetLinksId().Returns(-1);
                ugridFile.Api = uGridApi;

                ILogHandler logHandler = Substitute.For<ILogHandler>();

                //Act
                void Call() => ugridFile.Read1D2DLinks(generatedObjectsForLinks, logHandler);

                // Assert
                Assert.That(Call, Throws.Nothing);
                Assert.That(generatedObjectsForLinks.Mesh2d, Is.EqualTo(disposable2DMesh));
                Assert.That(generatedObjectsForLinks.FillValueMesh2DFaceNodes, Is.EqualTo(801));
                Assert.That(generatedObjectsForLinks.Mesh1d, Is.EqualTo(disposable1DMesh));
            }
        }
        
        [Test]
        public void Read1D2DLinksTest_NoLinksInFile_ButMesh2DAndMesh1DAndNetworkGeometryAre_SetInLinksObjectAndThrowsNothing()
        {
            // Arrange 
            using (UGridFile ugridFile = new UGridFile(String.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;

                IGeneratedObjectsForLinks generatedObjectsForLinks = Substitute.For<IGeneratedObjectsForLinks>();
                UnstructuredGrid unstructuredGrid = new UnstructuredGrid();
                generatedObjectsForLinks.Grid.Returns(unstructuredGrid);
                generatedObjectsForLinks.Mesh2d = null;
                generatedObjectsForLinks.FillValueMesh2DFaceNodes = 8;
                IHydroNetwork hydroNetwork = Substitute.For<IHydroNetwork>();
                IDiscretization discretization = Substitute.For<IDiscretization>();
                discretization.Network.Returns(hydroNetwork);
                generatedObjectsForLinks.Discretization.Returns(discretization);
                generatedObjectsForLinks.Mesh1d = null;
                
                Disposable2DMeshGeometry disposable2DMesh = new Disposable2DMeshGeometry();
                Disposable1DMeshGeometry disposable1DMesh = new Disposable1DMeshGeometry { NodeIds = Array.Empty<string>() };
                DisposableNetworkGeometry networkGeometry = new DisposableNetworkGeometry { NodesX = Array.Empty<double>(), BranchIds = Array.Empty<string>() };
                
                IUGridApi uGridApi = Substitute.For<IUGridApi>();
                uGridApi.IsUGridFile().Returns(true);
                uGridApi.GetMeshIdsByMeshType(UGridMeshType.Mesh2D).Returns(new[] { 1 });
                uGridApi.GetMesh2D(1).Returns(disposable2DMesh);
                uGridApi.GetVariableNoDataValue(Arg.Any<string>(), 1, GridLocationType.Face).Returns(801);
                uGridApi.GetMeshIdsByMeshType(UGridMeshType.Mesh1D).Returns(new[] { 1 });
                uGridApi.GetMesh1D(1).Returns(disposable1DMesh);
                uGridApi.GetNetworkIds().Returns(new[] { 1 });
                uGridApi.GetNetworkGeometry(1).Returns(networkGeometry);
                uGridApi.GetLinksId().Returns(-1);
                ugridFile.Api = uGridApi;

                ILogHandler logHandler = Substitute.For<ILogHandler>();

                //Act
                void Call() => ugridFile.Read1D2DLinks(generatedObjectsForLinks, logHandler);

                // Assert
                Assert.That(Call, Throws.Nothing);
                Assert.That(generatedObjectsForLinks.Mesh2d, Is.EqualTo(disposable2DMesh));
                Assert.That(generatedObjectsForLinks.FillValueMesh2DFaceNodes, Is.EqualTo(801));
                Assert.That(generatedObjectsForLinks.Mesh1d, Is.EqualTo(disposable1DMesh));
                Assert.That(generatedObjectsForLinks.NetworkGeometry, Is.EqualTo(networkGeometry));
            }
        }
        
        [Test]
        public void Read1D2DLinksTest_Mesh2DAndMesh1DAndNetworkGeometryAndLinksAre_HappyFlow_SetInLinksObjectAndThrowsNothing()
        {
            // Arrange 
            using (UGridFile ugridFile = new UGridFile(String.Empty))
            {
                var ugridFileInfo = Substitute.For<IUgridFileInfo>();
                ugridFileInfo.IsValidPath().Returns(true);
                ugridFile.UgridFileInfo = ugridFileInfo;

                IGeneratedObjectsForLinks generatedObjectsForLinks = Substitute.For<IGeneratedObjectsForLinks>();
                UnstructuredGrid unstructuredGrid = new UnstructuredGrid();
                generatedObjectsForLinks.Grid.Returns(unstructuredGrid);
                generatedObjectsForLinks.Mesh2d = null;
                generatedObjectsForLinks.FillValueMesh2DFaceNodes = 8;
                IHydroNetwork hydroNetwork = Substitute.For<IHydroNetwork>();
                IDiscretization discretization = Substitute.For<IDiscretization>();
                discretization.Network.Returns(hydroNetwork);
                generatedObjectsForLinks.Discretization.Returns(discretization);
                generatedObjectsForLinks.Mesh1d = null;
                
                Disposable2DMeshGeometry disposable2DMesh = new Disposable2DMeshGeometry();
                Disposable1DMeshGeometry disposable1DMesh = new Disposable1DMeshGeometry { NodeIds = Array.Empty<string>()};
                DisposableNetworkGeometry networkGeometry = new DisposableNetworkGeometry { NodesX = Array.Empty<double>(), BranchIds = Array.Empty<string>() };
                DisposableLinksGeometry linksGeometry = new DisposableLinksGeometry()
                {
                    LinkId = Array.Empty<string>(),
                };

                IUGridApi uGridApi = Substitute.For<IUGridApi>();
                uGridApi.IsUGridFile().Returns(true);
                uGridApi.GetMeshIdsByMeshType(UGridMeshType.Mesh2D).Returns(new[] { 1 });
                uGridApi.GetMesh2D(1).Returns(disposable2DMesh);
                uGridApi.GetVariableNoDataValue(Arg.Any<string>(), 1, GridLocationType.Face).Returns(801);
                uGridApi.GetMeshIdsByMeshType(UGridMeshType.Mesh1D).Returns(new[] { 1 });
                uGridApi.GetMesh1D(1).Returns(disposable1DMesh);
                uGridApi.GetNetworkIds().Returns(new[] { 1 });
                uGridApi.GetNetworkGeometry(1).Returns(networkGeometry);
                uGridApi.GetLinksId().Returns(1);
                uGridApi.GetLinks(1).Returns(linksGeometry);

                ugridFile.Api = uGridApi;

                ILogHandler logHandler = Substitute.For<ILogHandler>();

                //Act
                void Call() => ugridFile.Read1D2DLinks(generatedObjectsForLinks, logHandler);

                // Assert
                Assert.That(Call, Throws.Nothing);
                Assert.That(generatedObjectsForLinks.Mesh2d, Is.EqualTo(disposable2DMesh));
                Assert.That(generatedObjectsForLinks.FillValueMesh2DFaceNodes, Is.EqualTo(801));
                Assert.That(generatedObjectsForLinks.Mesh1d, Is.EqualTo(disposable1DMesh));
                Assert.That(generatedObjectsForLinks.NetworkGeometry, Is.EqualTo(networkGeometry));
                Assert.That(generatedObjectsForLinks.LinksGeometry, Is.EqualTo(linksGeometry));
                logHandler.Received(3).LogReport();
            }
        }
    }
}