using System.ComponentModel;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls
{
    public class MeteoDataProperties : ObjectProperties<MeteoData>
    {
        [Category("General")]
        [PropertyOrder(0)]
        public string Name
        {
            get { return data.Name; }
        }

        [Category("General")]
        [PropertyOrder(1)]
        public MeteoDataDistributionType MeteoDataDistributionType
        {
            get { return data.DataDistributionType; }
            set { data.DataDistributionType = value; }
        }

        [Category("Data")]
        [PropertyOrder(2)]
        public ExtrapolationTypeMeteo Extrapolation
        {
            get { return (ExtrapolationTypeMeteo) data.Data.Arguments[0].ExtrapolationType; }
            set { data.Data.Arguments[0].ExtrapolationType = (ExtrapolationType)value; }
        }

        [Category("Data")]
        [PropertyOrder(3)]
        public InterpolationType Interpolation
        {
            get { return data.Data.Arguments[0].InterpolationType; }
            set { data.Data.Arguments[0].InterpolationType = value; }
        }

        # region ExtrapolationTypeMeteo enum

        public enum ExtrapolationTypeMeteo
        {
            None = ExtrapolationType.None,
            Periodic = ExtrapolationType.Periodic
        }

        # endregion
    }
}