using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Utils.Collections;
using DeltaShell.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Editors;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap.UI.Tools;

namespace Sobek.IntegrationTests
{
    public class UndoRedoHydroRegionTestBase
    {
        #region Fields
        protected DeltaShellGui gui;
        protected Project project;
        protected IHydroRegion region;
        protected DrainageBasin basin;
        protected IHydroNetwork network;
        protected Window mainWindow;
        protected Action onMainWindowShown;
        protected ProjectItemMapView ProjectItemMapView;
        private MouseEventArgs args = new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0);
        private MouseEventArgs argsMove = new MouseEventArgs(MouseButtons.None, 1, 0, 0, 0);

        #endregion

        protected void AssertNetworkAsExpected(string context, int numNodes, int numBranches, int numbridges = -1)
        {
            Assert.AreEqual(numNodes, network.HydroNodes.Count(), "# nodes " + context);
            Assert.AreEqual(numBranches, network.Branches.Count, "# branches " + context);
            if (numbridges >= 0)
                Assert.AreEqual(numbridges, network.Bridges.Count(), "# bridges " + context);
        }

        protected void AssertNumUndoableActions(string context, int numUndoableActions)
        {
            Assert.AreEqual(numUndoableActions, gui.UndoRedoManager.UndoStack.Count(), "# undoable actions " + context);
        }

        protected void DeleteFeature(IFeature feature)
        {
            var mapControl = ProjectItemMapView.MapView.MapControl;
            mapControl.SelectTool.Select(feature);
            mapControl.DeleteTool.DeleteSelection();
        }

        protected void MoveFeature(IFeature feature, Coordinate coordinate)
        {
            ProjectItemMapView.MapView.MapControl.SelectTool.Select(feature);

            var moveTool = ProjectItemMapView.MapView.MapControl.MoveTool;

            moveTool.OnMouseDown(feature.Geometry.Coordinate, new MouseEventArgs(MouseButtons.Left, 1, -1, -1, -1));
            moveTool.OnMouseMove(coordinate, args);
            moveTool.OnMouseUp(coordinate, args);
        }

        protected IChannel AddBranchToNetwork(Coordinate[] coordinates)
        {
            var branchLayer = ProjectItemMapView.MapView.GetLayerForData(network.Channels);

            var branchGeom = new LineString(coordinates);

            return (IChannel)branchLayer.DataSource.Add(branchGeom);
        }

        protected IBridge AddBridge(Coordinate coordinate)
        {
            var newBridgeTool = ProjectItemMapView.MapView.MapControl.Tools.OfType<NewPointFeatureTool>().First(t => t.Name == HydroRegionEditorMapTool.AddBridgeToolName);
            newBridgeTool.OnMouseDown(coordinate, args);
            newBridgeTool.OnMouseMove(coordinate, args);
            newBridgeTool.OnMouseUp(coordinate, null);
            return (IBridge)newBridgeTool.Layers.First().DataSource.Features.Cast<IFeature>().Last();
        }

        protected IWeir AddWeir(Coordinate coordinate)
        {
            var newWeirTool = ProjectItemMapView.MapView.MapControl.Tools.OfType<NewPointFeatureTool>().First(t => t.Name == HydroRegionEditorMapTool.AddWeirToolName);
            newWeirTool.OnMouseDown(coordinate, args);
            newWeirTool.OnMouseMove(coordinate, args);
            newWeirTool.OnMouseUp(coordinate, null);
            return (IWeir)newWeirTool.Layers.First().DataSource.Features.Cast<IFeature>().Last();
        }

        protected WasteWaterTreatmentPlant AddWasteWaterTreatmentPlant(Coordinate coordinate)
        {
            var newWWTPTool = ProjectItemMapView.MapView.MapControl.Tools.OfType<NewPointFeatureTool>().First(t => t.Name == HydroRegionEditorMapTool.AddWasteWaterTreatmentPlantToolName);
            newWWTPTool.OnMouseDown(coordinate, args);
            newWWTPTool.OnMouseMove(coordinate, args);
            newWWTPTool.OnMouseUp(coordinate, null);
            return (WasteWaterTreatmentPlant)newWWTPTool.Layers.First().DataSource.Features.Cast<IFeature>().Last();
        }

        protected RunoffBoundary AddRunoffBoundary(Coordinate coordinate)
        {
            var newRunoffBoundaryTool = ProjectItemMapView.MapView.MapControl.Tools.OfType<NewPointFeatureTool>().First(t => t.Name == HydroRegionEditorMapTool.AddRunoffBoundaryToolName);
            newRunoffBoundaryTool.OnMouseDown(coordinate, args);
            newRunoffBoundaryTool.OnMouseMove(coordinate, args);
            newRunoffBoundaryTool.OnMouseUp(coordinate, null);
            return (RunoffBoundary)newRunoffBoundaryTool.Layers.First().DataSource.Features.Cast<IFeature>().Last();
        }

        protected Catchment AddCatchment(Coordinate center, CatchmentType newCatchmentType = null, bool point=false, bool line=false)
        {
            if (newCatchmentType == null)
                newCatchmentType = CatchmentType.Unpaved;

            var newCatchmentTool = ProjectItemMapView.MapView.MapControl.Tools.OfType<NewLineTool>().First(t => t.Name == HydroRegionEditorMapTool.AddCatchmentToolName);
            
            newCatchmentTool.Layers.Select(l => l.FeatureEditor)
                            .OfType<CatchmentFeatureEditor>()
                            .ForEach(cfe => cfe.NewCatchmentType = newCatchmentType);

            var startCoord = new Coordinate(center.X - 5, center.Y - 5);

            if (line)
            {
                newCatchmentTool.OnMouseDown(startCoord, args);
                newCatchmentTool.OnMouseUp(new Coordinate(center.X + 4, center.Y), args);
            }
            else
            {
                newCatchmentTool.OnMouseDown(startCoord, args);
                if (!point)
                {
                    newCatchmentTool.OnMouseDown(startCoord, args);
                    newCatchmentTool.OnMouseDown(new Coordinate(center.X + 5, center.Y - 5), args);
                    newCatchmentTool.OnMouseDown(new Coordinate(center.X + 5, center.Y + 5), args);
                    newCatchmentTool.OnMouseDown(new Coordinate(center.X - 5, center.Y + 5), args);
                }
                newCatchmentTool.OnMouseUp(startCoord, args);
            }

            return (Catchment) newCatchmentTool.Layers.First().DataSource.Features.Cast<IFeature>().LastOrDefault();
        }

        protected ICulvert AddCulvert(Coordinate coordinate)
        {
            var newCulvertTool = ProjectItemMapView.MapView.MapControl.Tools.OfType<NewPointFeatureTool>().First(t => t.Name == HydroRegionEditorMapTool.AddCulvertToolName);
            newCulvertTool.OnMouseDown(coordinate, args);
            newCulvertTool.OnMouseMove(coordinate, args);
            newCulvertTool.OnMouseUp(coordinate, null);
            return (ICulvert)newCulvertTool.Layers.First().DataSource.Features.Cast<IFeature>().Last();
        }

        protected ICrossSection AddCrossSection(Coordinate coordinate, ICrossSection crossSection)
        {
            var mapControl = ProjectItemMapView.MapView.MapControl;
            var crossSectionLayer = ProjectItemMapView.MapView.GetLayerForData(network.CrossSections);
            crossSectionLayer.FeatureEditor.CreateNewFeature = (l => crossSection);
            
            var newCSTool = mapControl.Tools.OfType<NewPointFeatureTool>().First(t => t.Name == HydroRegionEditorMapTool.AddPointCrossSectionToolName);
            newCSTool.OnMouseDown(coordinate, args);
            newCSTool.OnMouseMove(coordinate, args);
            newCSTool.OnMouseUp(coordinate, null);
            return (ICrossSection)newCSTool.Layers.First().DataSource.Features.Cast<IFeature>().Last();
        }
        
        protected ILateralSource AddLateral(Coordinate coordinate)
        {
            var newLateralTool = ProjectItemMapView.MapView.MapControl.Tools.OfType<NewPointFeatureTool>().First(t => t.Name == HydroRegionEditorMapTool.AddLateralSourceToolName);
            newLateralTool.OnMouseDown(coordinate, args);
            newLateralTool.OnMouseMove(coordinate, args);
            newLateralTool.OnMouseUp(coordinate, null);
            return (ILateralSource)newLateralTool.Layers.First().DataSource.Features.Cast<IFeature>().Last();
        }

        protected void RemoveCoordinateFromFeatureGeometry(IFeature feature, Coordinate coordinate)
        {
            ProjectItemMapView.MapView.MapControl.SelectTool.Select(feature);

            var removePointTool = ProjectItemMapView.MapView.MapControl.Tools.OfType<CurvePointTool>().First();
            removePointTool.Mode = CurvePointTool.EditMode.Remove;
            removePointTool.OnMouseMove(coordinate, argsMove);
            removePointTool.OnMouseDown(coordinate, args);
        }
    }
}