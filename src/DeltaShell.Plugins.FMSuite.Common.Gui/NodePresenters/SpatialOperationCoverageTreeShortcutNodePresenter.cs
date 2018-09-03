using System.Drawing;
using DelftTools.Shell.Core.Workflow;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.NodePresenters
{
    public class SpatialOperationCoverageTreeShortcutNodePresenter<TModel, TModelView> :
        FMSuiteNodePresenterBase<SpatialOperationCoverageTreeShortcut<TModel, TModelView>> 
        where TModel : IModel
    {
        protected override string GetNodeText(SpatialOperationCoverageTreeShortcut<TModel, TModelView> data)
        {
            return data.Text;
        }

        protected override Image GetNodeImage(SpatialOperationCoverageTreeShortcut<TModel, TModelView> data)
        {
            return data.Image;
        }

        protected override object GetContextMenuData(SpatialOperationCoverageTreeShortcut<TModel, TModelView> data)
        {
            return data.ContextMenuData;
        }
    }
}
