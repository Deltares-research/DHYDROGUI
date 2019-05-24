using System.ComponentModel;
using DelftTools.Utils;
using DelftTools.Utils.Reflection;
using GeoAPI.Extensions.CoordinateSystems;
using SharpMap.Data.Providers;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui
{
    /// <summary>
    /// WaqModelFeatureCollection listens to coordinate system changing.
    /// If so, the event is thrown as if it would come from the collection itself.
    /// This should be something that is more common than only in waq,
    /// but we've implemented it here fore now.
    /// </summary>
    public class WaqModelFeatureCollection : Feature2DCollection
    {
        private readonly WaterQualityModel model;

        public WaqModelFeatureCollection(WaterQualityModel parentModel)
        {
            model = parentModel;
            ((INotifyPropertyChange) model).PropertyChanged += Model_PropertyChanged;
        }

        public override ICoordinateSystem CoordinateSystem
        {
            get => model.CoordinateSystem;
            set
            {
                // do nothing, it is derived from the model.
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            ((INotifyPropertyChange) model).PropertyChanged -= Model_PropertyChanged;
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Equals(e.PropertyName, TypeUtils.GetMemberName<WaterQualityModel>(x => x.CoordinateSystem)))
            {
                OnCoordinateSystemChanged();
            }
        }
    }
}