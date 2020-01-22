using System;
using System.ComponentModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.DataRows;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts
{
    public class NwrwDataRow : RainfallRunoffDataRow<NwrwData>
    {
        [Description("Name")]
        public string Name
        {
            get { return data.Name; }
        }

        [Description("Closed paved area, with a slope (m²)")]
        public double ClosedPavedWithSlope
        {
            get
            {
                return data.SurfaceLevelDict!=null && data.SurfaceLevelDict.TryGetValue(NwrwSurfaceType.ClosedPavedWithSlope, out double result)
                    ? result
                    : 0.0;
            }
            set { data.SurfaceLevelDict[NwrwSurfaceType.ClosedPavedWithSlope] = value; }
        }

        [Description("Closed paved area, flat (m²)")]
        public double ClosedPavedFlat
        {
            get
            {
                return data.SurfaceLevelDict != null && data.SurfaceLevelDict.TryGetValue(NwrwSurfaceType.ClosedPavedFlat, out double result)
                    ? result
                    : 0.0;
            }
            set { data.SurfaceLevelDict[NwrwSurfaceType.ClosedPavedFlat] = value; }
        }

        [Description("Closed paved area, flat stretched (m²)")]
        public double ClosedPavedFlatStretched
        {
            get
            {
                return data.SurfaceLevelDict != null && data.SurfaceLevelDict.TryGetValue(NwrwSurfaceType.ClosedPavedFlatStretch, out double result)
                    ? result
                    : 0.0;
            }
            set { data.SurfaceLevelDict[NwrwSurfaceType.ClosedPavedFlatStretch] = value; }
        }

        [Description("Open paved area, with a slope (m²)")]
        public double OpenPavedWithSlope
        {
            get
            {
                return data.SurfaceLevelDict != null && data.SurfaceLevelDict.TryGetValue(NwrwSurfaceType.OpenPavedWithSlope, out double result)
                    ? result
                    : 0.0;
            }
            set { data.SurfaceLevelDict[NwrwSurfaceType.OpenPavedWithSlope] = value; }
        }


        [Description("Open paved area, flat (m²)")]
        public double OpenPavedFlat
        {
            get
            {
                return data.SurfaceLevelDict != null && data.SurfaceLevelDict.TryGetValue(NwrwSurfaceType.OpenPavedFlat, out double result)
                    ? result
                    : 0.0;
            }
            set { data.SurfaceLevelDict[NwrwSurfaceType.OpenPavedFlat] = value; }
        }

        [Description("Open paved area, flat stretched (m²)")]
        public double OpenPavedFlatStretched
        {
            get
            {
                return data.SurfaceLevelDict != null && data.SurfaceLevelDict.TryGetValue(NwrwSurfaceType.OpenPavedFlatStretched, out double result)
                    ? result
                    : 0.0;
            }
            set { data.SurfaceLevelDict[NwrwSurfaceType.OpenPavedFlatStretched] = value; }
        }

        [Description("Roof area, with a slope (m²)")]
        public double RoofWithSlope
        {
            get
            {
                return data.SurfaceLevelDict != null && data.SurfaceLevelDict.TryGetValue(NwrwSurfaceType.RoofWithSlope, out double result)
                    ? result
                    : 0.0;
            }
            set { data.SurfaceLevelDict[NwrwSurfaceType.RoofWithSlope] = value; }
        }

        [Description("Roof area, flat (m²)")]
        public double RoofFlat
        {
            get
            {
                return data.SurfaceLevelDict != null && data.SurfaceLevelDict.TryGetValue(NwrwSurfaceType.RoofFlat, out double result)
                    ? result
                    : 0.0;
            }
            set { data.SurfaceLevelDict[NwrwSurfaceType.RoofFlat] = value; }
        }

        [Description("Roof area, flat stretched (m²)")]
        public double RoofFlatStretched
        {
            get
            {
                return data.SurfaceLevelDict != null && data.SurfaceLevelDict.TryGetValue(NwrwSurfaceType.RoofFlatStretched, out double result)
                    ? result
                    : 0.0;
            }
            set { data.SurfaceLevelDict[NwrwSurfaceType.RoofFlatStretched] = value; }
        }

        [Description("Unpaved area, with a slope (m²)")]
        public double UnpavedWithSlope
        {
            get
            {
                return data.SurfaceLevelDict != null && data.SurfaceLevelDict.TryGetValue(NwrwSurfaceType.UnpavedWithSlope, out double result)
                    ? result
                    : 0.0;
            }
            set { data.SurfaceLevelDict[NwrwSurfaceType.UnpavedWithSlope] = value; }
        }

        [Description("Unpaved area, flat (m²)")]
        public double UnpavedFlat
        {
            get
            {
                return data.SurfaceLevelDict != null && data.SurfaceLevelDict.TryGetValue(NwrwSurfaceType.UnpavedFlat, out double result)
                    ? result
                    : 0.0;
            }
            set { data.SurfaceLevelDict[NwrwSurfaceType.UnpavedFlat] = value; }
        }

        [Description("Unpaved area, flat stretched (m²)")]
        public double UnpavedFlatStretched
        {
            get
            {
                return data.SurfaceLevelDict != null && data.SurfaceLevelDict.TryGetValue(NwrwSurfaceType.UnpavedFlatStretched, out double result)
                    ? result
                    : 0.0;
            }
            set { data.SurfaceLevelDict[NwrwSurfaceType.UnpavedFlatStretched] = value; }
        }

        [Description("Number of units (dwf1)")]
        public int NumberOfUnitsFirstDwf
        {
            get
            {
                return data.DryWeatherFlows.Count >= 1
                    ? data.DryWeatherFlows[0].NumberOfUnits
                    : 0;
            }
            set { data.DryWeatherFlows[0].NumberOfUnits = value; }
        }

        [Description("DWF definition (dwf1)")]
        public string FirstDryWeatherFlowId
        {
            get
            {
                return data.DryWeatherFlows.Count >= 1
                    ? data.DryWeatherFlows[0].DryWeatherFlowId
                    : String.Empty;
            }
            set { data.DryWeatherFlows[0].DryWeatherFlowId = value; }
        }

        [Description("Number of units (dwf2)")]
        public int NumberOfUnitsLastDwf
        {
            get
            {
                return data.DryWeatherFlows.Count >= 2
                    ? data.DryWeatherFlows[1].NumberOfUnits
                    : 0;
            }
            set { data.DryWeatherFlows[1].NumberOfUnits = value; }
        }

        [Description("DWF definition (dwf2)")]
        public string LastFirstDryWeatherFlowId
        {
            get
            {
                return data.DryWeatherFlows.Count >= 2
                    ? data.DryWeatherFlows[1].DryWeatherFlowId
                    : String.Empty;
            }
            set { data.DryWeatherFlows[1].DryWeatherFlowId = value; }
        }

        [Description("Meteostation identification")]
        public string MeteoStationId
        {
            get { return data.MeteoStationId; }
            set { data.MeteoStationId = value; }
        }



    }
}
