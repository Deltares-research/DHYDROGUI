using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core.Workflow;

namespace DeltaShell.Plugins.FMSuite.Common.Gui
{
    public abstract class ModelTreeShortcut
    {
        protected ModelTreeShortcut(string text, Bitmap image, IModel model, object data, ShortCutType shortCutType = ShortCutType.SettingsTab, IEnumerable<object> childObjects = null)
        {
            Text = text;
            Image = image;
            Model = model;
            Data = data;
            ChildObjects = childObjects;
            ShortCutType = shortCutType;
        }

        /// <summary>
        /// Text to show for the node
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// Image to show for the node
        /// </summary>
        public Bitmap Image { get; private set; }

        /// <summary>
        /// Model that this shortcut belongs to
        /// </summary>
        public IModel Model { get; private set; }

        /// <summary>
        /// Data of the shortcut (object being wrapped)
        /// ShortCutType.SettingsTab = > name of the tab
        /// </summary>
        public object Data { get; private set; }

        /// <summary>
        /// Sub-shortcuts for this shortcut
        /// </summary>
        public IEnumerable<object> ChildObjects { get; private set; }

        /// <summary>
        /// Type of shortcut
        /// </summary>
        public ShortCutType ShortCutType { get; private set; }
    }
}