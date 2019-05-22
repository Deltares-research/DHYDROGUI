using System.Linq;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.DeveloperTools.Commands
{
    public class AddProjectedNetwork : Command, IGuiCommand
    {
        protected override void OnExecute(params object[] arguments)
        {
            var networkCoordinateSystem = OgrCoordinateSystemFactory.SupportedCoordinateSystems.ToList().First(c => c.Name.Contains("WGS_1984_Web_Mercator"));
            var worldCoordinateSystem = OgrCoordinateSystemFactory.SupportedCoordinateSystems.ToList().First(c => c.Name.Contains("World_Sinusoidal"));

            var network = new HydroNetwork { Name = networkCoordinateSystem.Name + "network", CoordinateSystem = networkCoordinateSystem };
            var dataItem = new DataItem(network);

            if (Gui.Application.Project == null)
            {
                Gui.Application.CreateNewProject();
            }

            Gui.Application.Project.RootFolder.Add(dataItem);
            Gui.DocumentViewsResolver.OpenViewForData(dataItem);

            var mapView = ((ProjectItemMapView) Gui.DocumentViews.ActiveView).MapView;
            mapView.Map.CoordinateSystem = worldCoordinateSystem;
            mapView.Map.ShowGrid = true;
            mapView.Map.ZoomToExtents();
        }

        public override bool Enabled
        {
            get { return true; }
        }

        public IGui Gui { get; set; }
    }
}