using System.Drawing;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Coverages;
using SharpMap.Api.Layers;
using SharpMap.Layers;

namespace DeltaShell.Plugins.FMSuite.Common.Gui
{
    public class SpatialOperationCoverageTreeShortcut<TModel, TModelView> : TreeShortcut<TModel, TModelView>
        where TModel : IModel
        where TModelView : ITabbedModelView
    {
        private ICoverage Coverage { get; set; }

        public SpatialOperationCoverageTreeShortcut(string text, Bitmap image, TModel model, ICoverage coverage, string tabText)
            : base(text, image, model, null, null)
        {
            Coverage = coverage;
            TabText = tabText;
        }

        public void FocusSpatialEditor(IGui gui)
        {
            var projectItemMapView =
                gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault(mv => ContainsModel(mv.Data));
            if (projectItemMapView != null)
            {
                var layer = projectItemMapView.MapView.Map.GetAllLayers(true).FirstOrDefault(l => MatchLayerForCoverage(l, Coverage));

                if (layer != null)
                {
                    projectItemMapView.SetSpatialOperationLayer(layer, true);
                    SharpMapGisGuiPlugin.Instance.FocusSpatialOperationView();
                    SharpMapGisGuiPlugin.Instance.EnableColorScaleLegend();
                }
            }
        }

        private bool ContainsModel(object model)
        {
            var compositeActivity = model as ICompositeActivity;
            if (compositeActivity != null)
            {
                return compositeActivity.Activities.Contains(Model);
            }
            return Equals(model, Model);
        }
        
        /// <summary>
        /// Find the layer that contains the coverage, but return the spatial operation set layer when spatial operations
        /// have been used on the original layer.
        /// </summary>
        private static bool MatchLayerForCoverage(ILayer l, ICoverage coverage)
        {
            var covLayer = l as ICoverageLayer;
            if (covLayer != null)
            {
                // for spatialoperationsetlayer, one will be a clone of the other and
                // we should not compare references. Therefore we will check on unique 
                // properties, in this case: Coverage.Name..
                return Equals(covLayer.Coverage.Name, coverage.Name);
            }

            var setLayer = l as SpatialOperationSetLayer;
            if (setLayer != null)
            {
                return MatchLayerForCoverage(setLayer.InputLayer, coverage);
            }

            return false;
        }
    }
}
