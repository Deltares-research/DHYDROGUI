using System;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.ImportExport.GWSW.ViewModels
{

    /// <summary>
    /// The GwswFeatureViewItem class is a placeholder for data and actions as a collection for a list/table view
    /// </summary>
    [Entity]
    public class GwswFeatureViewItem
    {
        private bool selected;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="GwswFeatureViewItem"/> is selected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if selected; otherwise, <c>false</c>.
        /// </value>
        public bool Selected
        {
            get { return selected; }
            set
            {
                selected = value;
                AfterSelected?.Invoke();
            }
        }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [file exists].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [file exists]; otherwise, <c>false</c>.
        /// </value>
        public bool FileExists { get; set; }

        /// <summary>
        /// Gets or sets the name of the element.
        /// </summary>
        /// <value>
        /// The name of the element.
        /// </value>
        public string ElementName { get; set; }

        /// <summary>
        /// Gets or sets the type of the feature.
        /// </summary>
        /// <value>
        /// The type of the feature.
        /// </value>
        public string FeatureType { get; set; }

        /// <summary>
        /// Gets or sets the full path.
        /// </summary>
        /// <value>
        /// The full path.
        /// </value>
        public string FullPath { get; set; }

        /// <summary>
        /// Gets or sets the after selected action.
        /// </summary>
        /// <value>
        /// The after selected.
        /// </value>
        public Action AfterSelected { get; set; }

    }
}