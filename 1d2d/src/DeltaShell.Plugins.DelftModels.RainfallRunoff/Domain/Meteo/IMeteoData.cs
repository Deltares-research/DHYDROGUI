using System;
using System.ComponentModel;
using DelftTools.Functions;
using DelftTools.Utils;
using IEditableObject = DelftTools.Utils.Editing.IEditableObject;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo
{
    /// <summary>
    /// <see cref="IMeteoData"/> defines the MeteoData used in a <see cref="IRainfallRunoffModel"/>.
    /// </summary>
    public interface IMeteoData : INotifyPropertyChanged, INameable, IEditableObject, ICloneable
    {
        /// <summary>
        /// The type of data distribution used within this <see cref="IMeteoData"/>.
        /// </summary>
        MeteoDataDistributionType DataDistributionType { get; set; }

        /// <summary>
        /// The current data backing this <see cref="IMeteoData"/>.
        /// </summary>
        IFunction Data { get; }

        /// <summary>
        /// Event fired when the underlying catchments have changed.
        /// </summary>
        event EventHandler<EventArgs> CatchmentsChanged;
    }
}