using System;
using System.Collections.Generic;
using System.ComponentModel;
using DelftTools.Controls;
using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.DataRows
{
    public abstract class RainfallRunoffDataRow<T> : IDataRow, INotifyPropertyChanged where T : CatchmentModelData
    {
        protected T data;

        public void Initialize(T data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            this.data = data;

            // use weak events to prevent event leaks
            var notifyPropertyChanged = (INotifyPropertyChanged)data;
            notifyPropertyChanged.PropertyChanged += new PropertyChangedEventHandler(DataRowPropertyChanged)
                .MakeWeak(e => notifyPropertyChanged.PropertyChanged -= e);
        }

        void DataRowPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                // always send dummy property change; just to trigger table refresh
                PropertyChanged(this, new PropertyChangedEventArgs("Dummy"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void SetColumnEditorForDataWithModel(IRainfallRunoffModel model,
            IEnumerable<ITableViewColumn> tableViewColumns)
        {
            //use default column editors
        }
    }
}