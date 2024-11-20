using System.Linq;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Editing;
using GeoAPI.Extensions.Feature;
using SharpMap.Api.Layers;
using SharpMap.Editors.Interactors.Network;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors
{
    public class SewerConnectionInteractor : BranchInteractor
    {
        public SewerConnectionInteractor(ILayer layer, IFeature feature, VectorStyle vectorStyle, IEditableObject editableObject) : base(layer, feature, vectorStyle, editableObject)
        {
        }

        public override void Add(IFeature feature)
        {
            if(feature is IPipe pipe)
                SewerFactory.AddDefaultPipeToNetwork(pipe, Network);
            else if(feature is SewerConnection sewerConnection)
                SewerFactory.AddDefaultSewerConnectionToNetwork(sewerConnection, Network);
        }

        public override void Delete()
        {
            base.Delete();

            if (!(SourceFeature is ISewerConnection connection))
            {
                return;
            }
            
            var nodes = new[] {connection.Source, connection.Target};

            // check for pipe connections if non then remove nodes
            var nodesToRemove = nodes.OfType<IManhole>()
                .Where(n => !n.IncomingBranches.OfType<IPipe>().Concat(n.OutgoingBranches.OfType<IPipe>()).Any())
                .ToList();

            // check for internal branches to remove
            var internalBranches = nodesToRemove
                .SelectMany(n => n.IncomingBranches.Concat(n.OutgoingBranches))
                .OfType<ISewerConnection>()
                .Distinct()
                .ToList();
            
            nodesToRemove.ForEach(n => Network.Nodes.Remove(n));
            internalBranches.ForEach(b => Network.Branches.Remove(b));
        }
    }
}