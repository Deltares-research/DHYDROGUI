using System.ComponentModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.NWRW;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.DataRows;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts
{
    class NWRWDataRow : RainfallRunoffDataRow<NWRWData>
    {
        [Description("Area Id")]
        public string NWRWDataId
        {
            get { return data.NWRWDataId; }
        }

        [Description("Surface level (m²)")]
        public double SurfaceLevel
        {
            get { return data.SurfaceLevel; }
            set { data.SurfaceLevel = value; }
        }

        [Description("Closed paved area, with a slope (m²)")]
        public int ClosedPavedWithSlope
        {
            get { return data.ClosedPavedWithSlope; }
            set { data.ClosedPavedWithSlope = value; }
        }

        [Description("Closed paved area, flat (m²)")]
        public int ClosedPavedFlat
        {
            get { return data.ClosedPavedFlat; }
            set { data.ClosedPavedFlat = value; }
        }

        [Description("Closed paved area, flat stretched (m²)")]
        public int ClosedPavedFlatStretched
        {
            get { return data.ClosedPavedFlatStretched; }
            set { data.ClosedPavedFlatStretched = value; }
        }

        [Description("Open paved area, with a slope (m²)")]
        public int OpenPavedWithSlope
        {
            get { return data.OpenPavedWithSlope; }
            set { data.OpenPavedWithSlope = value; }
        }

        [Description("Open paved area, flat (m²)")]
        public int OpenPavedFlat
        {
            get { return data.OpenPavedFlat; }
            set { data.OpenPavedFlat = value; }
        }

        [Description("Open paved area, flat stretched (m²)")]
        public int OpenPavedFlatStretched
        {
            get { return data.OpenPavedFlatStretched; }
            set { data.OpenPavedFlatStretched = value; }
        }

        [Description("Roof area, with a slope (m²)")]
        public int RoofWithSlope
        {
            get { return data.RoofWithSlope; }
            set { data.RoofWithSlope = value; }
        }

        [Description("Roof area, flat (m²)")]
        public int RoofFlat
        {
            get { return data.RoofFlat; }
            set { data.RoofFlat = value; }
        }

        [Description("Roof area, flat stretched (m²)")]
        public int RoofFlatStretched
        {
            get { return data.RoofFlatStretched; }
            set { data.RoofFlatStretched = value; }
        }

        [Description("Unpaved area, with a slope (m²)")]
        public int UnpavedWithSlope
        {
            get { return data.UnpavedWithSlope; }
            set { data.UnpavedWithSlope = value; }
        }

        [Description("Unpaved area, flat (m²)")]
        public int UnpavedFlat
        {
            get { return data.UnpavedFlat; }
            set { data.UnpavedFlat = value; }
        }

        [Description("Unpaved area, flat stretched (m²)")]
        public int UnpavedFlatStretched
        {
            get { return data.UnpavedFlatStretched; }
            set { data.UnpavedFlatStretched = value; }
        }

        [Description("Number of people")]
        public int NumberOfPeople
        {
            get { return data.NumberOfPeople; }
            set { data.NumberOfPeople = value; }
        }

        [Description("Dry weather flow identification")]
        public string DryWeatherFlowId
        {
            get { return data.DryWeatherFlowId; }
            set { data.DryWeatherFlowId = value; }
        }

        [Description("Meteostation identification")]
        public string MeteostationId
        {
            get { return data.MeteoStationId; }
            set { data.MeteoStationId = value; }
        }



    }
}
