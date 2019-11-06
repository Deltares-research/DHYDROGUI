using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using SharpMap.Data.Providers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries
{
    /// <summary>
    /// <see cref="BoundaryLineMapFeatureProvider"/> is responsible for showing
    /// the appropriate boundary line features given a boundary container.
    /// It provides the appropriate methods such that these features and their
    /// underlying data can be created through the Map.
    /// </summary>
    /// <remarks>
    /// This class leverages the <see cref="MultiIEventedListAdapter{TObserved,TDisplayed}"/>
    /// to create the appropriate lists. These classes are necessary to play
    /// nice with the framework, and ensure a good separation of concerns between
    /// view and data. As such, this feature provider can be seen as a view model
    /// for the line data for the Map.
    /// </remarks>
    public class BoundaryLineMapFeatureProvider : Feature2DCollection
    {
        private readonly IBoundaryContainer boundaryContainer;

        public BoundaryLineMapFeatureProvider(IBoundaryContainer boundaryContainer)
        {
            this.boundaryContainer = boundaryContainer ?? 
                                     throw new ArgumentNullException(nameof(boundaryContainer));
        }
    }
}