using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Controls;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Data;

namespace DeltaShell.Plugins.FMSuite.Common.Gui
{
    public class FMTreeModelWrapper
    {
        public IModel TargetModel { get; set; }
    }

    public class TreeShortcut<TModel,TModelView> : Unique<long>, IProjectItem, IProjectItemOwned
        where TModel : IModel
    {
        public TModel Model { get; private set; }

        public IEnumerable<object> SubItems { get; private set; }

        public string Text { get; private set; }

        public Bitmap Image { get; private set; }

        public object TargetData { get; private set; }

        public Func<object,object> ContextMenuDataGetter { get; set; }

        public object ContextMenuData 
        {
            get
            {
                if (ContextMenuDataGetter == null)
                {
                    return TargetData;
                }
                var modelWrapper = TargetData as FMTreeModelWrapper;
                return modelWrapper == null
                    ? ContextMenuDataGetter(TargetData)
                    : ContextMenuDataGetter(modelWrapper.TargetModel);
            }
        }

        public string TabText { private get; set; }

        protected TreeShortcut(string text, Bitmap image, TModel model, object data = null,
            IEnumerable<object> subItems = null)
        {
            Text = text;
            Image = image;
            Model = model;
            TargetData = data ?? new FMTreeModelWrapper {TargetModel = model};
            TabText = Text;
            SubItems = subItems ?? new object[0];
        }

        public void NavigateToInView(IView modelView)
        {
            if (CanSwitchToTab && modelView != null)
            {
                modelView.EnsureVisible(TabText);
            }
        }

        public bool CanSwitchToTab
        {
            get { return TargetData is FMTreeModelWrapper; }
        }

        public IProjectItem Owner { get { return Model; } }
        
        public IEnumerable<object> GetDirectChildren()
        {
            yield break;
        }

        public string Name { get; set; }

        public object DeepClone()
        {
            throw new NotImplementedException();
        }
    }
}