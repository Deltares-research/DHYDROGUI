using System;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Validation.NameValidation;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation
{
    /// <summary>
    /// Unique name validation services for <see cref="INetworkLocation"/>.
    /// </summary>
    public sealed class NetworkLocationsUniqueNameValidationService : IDisposable
    {
        private readonly IDiscretization discretization;
        private readonly UniqueNameValidator uniqueNameValidator;
        private bool disposed;

        /// <summary>
        /// Initialize a new instance of the <see cref="NetworkLocationsUniqueNameValidationService"/> class.
        /// </summary>
        /// <param name="discretization"> The discretization that contain the network locations. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="discretization"/> is <c>null</c>.
        /// </exception>
        public NetworkLocationsUniqueNameValidationService(IDiscretization discretization)
        {
            Ensure.NotNull(discretization, nameof(discretization));

            this.discretization = discretization;
            uniqueNameValidator = new UniqueNameValidator();

            ((INotifyPropertyChange)discretization).PropertyChanging += DiscretizationPropertyChanging;
            ((INotifyPropertyChange)discretization).PropertyChanged += DiscretizationPropertyChanged;
            discretization.Locations.Values.CollectionChanged += NetworkLocationCollectionChanged;
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            ((INotifyPropertyChange)discretization).PropertyChanging -= DiscretizationPropertyChanging;
            ((INotifyPropertyChange)discretization).PropertyChanged -= DiscretizationPropertyChanged;
            discretization.Locations.Values.CollectionChanged -= NetworkLocationCollectionChanged;

            disposed = true;
        }

        private void DiscretizationPropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            if (e.PropertyName == nameof(IHasNameValidation.Name) &&
                sender is INetworkLocation networkLocation)
            {
                uniqueNameValidator.RemoveName(networkLocation.Name);
            }
        }

        private void DiscretizationPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IHasNameValidation.Name) &&
                sender is INetworkLocation networkLocation)
            {
                uniqueNameValidator.AddName(networkLocation.Name);
            }
        }

        private void NetworkLocationCollectionChanged(object sender, MultiDimensionalArrayChangingEventArgs e)
        {
            if (!Equals(sender, discretization.Locations.Values))
            {
                return;
            }

            if (e.Action == NotifyCollectionChangeAction.Add)
            {
                foreach (INetworkLocation addedNetworkLocation in e.Items.OfType<INetworkLocation>())
                {
                    addedNetworkLocation.AttachNameValidator(uniqueNameValidator);
                    uniqueNameValidator.AddName(addedNetworkLocation.Name);
                }
            }

            else if (e.Action == NotifyCollectionChangeAction.Remove)
            {
                foreach (INetworkLocation removedNetworkLocation in e.Items.OfType<INetworkLocation>())
                {
                    removedNetworkLocation.AttachNameValidator(uniqueNameValidator);
                    uniqueNameValidator.AddName(removedNetworkLocation.Name);
                }
            }
        }
    }
}