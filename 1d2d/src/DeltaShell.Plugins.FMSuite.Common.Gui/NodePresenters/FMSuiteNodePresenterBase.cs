using System;
using System.Diagnostics;
using System.Drawing;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Utils;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.NodePresenters
{
    public abstract class FMSuiteNodePresenterBase<T> : TreeViewNodePresenterBaseForPluginGui<T> where T : class 
    {
        private Image lastImageForEmptyData;
        private Image emptyDataImage;

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, T nodeData)
        {
            CheckNodeImageResourceLeak(nodeData);

            node.Text = GetNodeText(nodeData);
            var image = GetNodeImage(nodeData);
            if (IsDataEmpty())
            {
                if (!ReferenceEquals(image, lastImageForEmptyData)) // create new empty image
                {
                    lastImageForEmptyData = image;
                    if (emptyDataImage != null)
                        emptyDataImage.Dispose();
                    emptyDataImage = CreateEmptyDataImage(image);
                }
                image = emptyDataImage;
            }
            node.Image = image;
        }

        private static Image CreateEmptyDataImage(Image image)
        {
            var semiTransparentyImage = new Bitmap(image);
            using (var graphics = Graphics.FromImage(semiTransparentyImage))
            {
                graphics.Clear(Color.Transparent);
                GraphicsUtils.DrawImageTransparent(graphics, image, 0.5f);
            }
            return semiTransparentyImage;
        }

        protected abstract string GetNodeText(T data);
        protected abstract Image GetNodeImage(T data);
        
        protected virtual bool IsDataEmpty()
        {
            return false;
        }

        protected virtual object GetContextMenuData(T data)
        {
            return data;
        }

        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            var menuBase = base.GetContextMenu(sender, nodeData);
            var menu = NodePresenterHelper.GetContextMenuFromPluginGuis(Gui, sender, nodeData);
            if (menuBase != null)
                menu.Add(menuBase);

            var data = GetContextMenuData((T)nodeData);
            menu.Add(new MenuItemContextMenuStripAdapter(ContextMenuFactory.CreateMenuFor(data, Gui, this, sender)));

            return menu;
        }

        private bool firstUpdate = true;

        [Conditional("DEBUG")]
        private void CheckNodeImageResourceLeak(T data)
        {
            if (!firstUpdate)
                return;

            firstUpdate = false;
            var image1 = GetNodeImage(data);
            var image2 = GetNodeImage(data);
            if (!ReferenceEquals(image1, image2))
                throw new InvalidOperationException(
                    string.Format("Apparent resource leak in {0}.GetNodeImage(): a new bitmap is returned each time, please reuse the bitmap",
                        GetType().Name));
        }
    }
}