using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Commands
{
    public class NetworkEditorHelper
    {
        /// <summary>
        /// Returns a list with all models in the project
        /// </summary>
        /// <returns></returns>
        public static IList<IModel> GetAllModelsContainingHydroNetwork(INetwork network, Project project)
        {
            IList<IModel> models = new List<IModel>();
            IEnumerable<IModel> modelsInProject = project.RootFolder.GetAllItemsRecursive().OfType<IModel>();
            foreach (IModel model in modelsInProject)
            {
                IEnumerable<INetwork> networks = model.GetAllItemsRecursive().OfType<INetwork>();
                if (networks.Any(modelNetwork => modelNetwork == network))
                {
                    models.Add(model);
                }
            }

            return models;
        }
    }
}