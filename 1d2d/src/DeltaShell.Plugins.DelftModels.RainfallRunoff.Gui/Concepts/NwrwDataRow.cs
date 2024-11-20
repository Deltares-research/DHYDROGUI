using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Editors;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.DataRows;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts
{
    public class NwrwDataRow : RainfallRunoffDataRow<NwrwData>
    {
        [Description("Area Id")]
        public string Name
        {
            get { return data.Name; }
        }

        [Description("Closed Sloped area (m²)")]
        public double ClosedPavedWithSlope
        {
            get
            {
                return data.SurfaceLevelDict != null && data.SurfaceLevelDict.TryGetValue(NwrwSurfaceType.ClosedPavedWithSlope, out double result)
                    ? result
                    : 0.0;
            }
            set
            {
                data.SurfaceLevelDict[NwrwSurfaceType.ClosedPavedWithSlope] = value;
                data.UpdateCatchmentAreaSize();
            }
        }

        [Description("Closed Flat area (m²)")]
        public double ClosedPavedFlat
        {
            get
            {
                return data.SurfaceLevelDict != null && data.SurfaceLevelDict.TryGetValue(NwrwSurfaceType.ClosedPavedFlat, out double result)
                    ? result
                    : 0.0;
            }
            set
            {
                data.SurfaceLevelDict[NwrwSurfaceType.ClosedPavedFlat] = value;
                data.UpdateCatchmentAreaSize();
            }
        }

        [Description("Closed Stretch area (m²)")]
        public double ClosedPavedFlatStretched
        {
            get
            {
                return data.SurfaceLevelDict != null && data.SurfaceLevelDict.TryGetValue(NwrwSurfaceType.ClosedPavedFlatStretch, out double result)
                    ? result
                    : 0.0;
            }
            set
            {
                data.SurfaceLevelDict[NwrwSurfaceType.ClosedPavedFlatStretch] = value;
                data.UpdateCatchmentAreaSize();
            }
        }

        [Description("Open Sloped area (m²)")]
        public double OpenPavedWithSlope
        {
            get
            {
                return data.SurfaceLevelDict != null && data.SurfaceLevelDict.TryGetValue(NwrwSurfaceType.OpenPavedWithSlope, out double result)
                    ? result
                    : 0.0;
            }
            set
            {
                data.SurfaceLevelDict[NwrwSurfaceType.OpenPavedWithSlope] = value;
                data.UpdateCatchmentAreaSize();
            }
        }


        [Description("Open Flat area (m²)")]
        public double OpenPavedFlat
        {
            get
            {
                return data.SurfaceLevelDict != null && data.SurfaceLevelDict.TryGetValue(NwrwSurfaceType.OpenPavedFlat, out double result)
                    ? result
                    : 0.0;
            }
            set
            {
                data.SurfaceLevelDict[NwrwSurfaceType.OpenPavedFlat] = value;
                data.UpdateCatchmentAreaSize();
            }
        }

        [Description("Open Stretch area (m²)")]
        public double OpenPavedFlatStretched
        {
            get
            {
                return data.SurfaceLevelDict != null && data.SurfaceLevelDict.TryGetValue(NwrwSurfaceType.OpenPavedFlatStretched, out double result)
                    ? result
                    : 0.0;
            }
            set
            {
                data.SurfaceLevelDict[NwrwSurfaceType.OpenPavedFlatStretched] = value;
                data.UpdateCatchmentAreaSize();
            }
        }

        [Description("Roof Sloped area (m²)")]
        public double RoofWithSlope
        {
            get
            {
                return data.SurfaceLevelDict != null && data.SurfaceLevelDict.TryGetValue(NwrwSurfaceType.RoofWithSlope, out double result)
                    ? result
                    : 0.0;
            }
            set
            {
                data.SurfaceLevelDict[NwrwSurfaceType.RoofWithSlope] = value;
                data.UpdateCatchmentAreaSize();
            }
        }

        [Description("Roof Flat area (m²)")]
        public double RoofFlat
        {
            get
            {
                return data.SurfaceLevelDict != null && data.SurfaceLevelDict.TryGetValue(NwrwSurfaceType.RoofFlat, out double result)
                    ? result
                    : 0.0;
            }
            set
            {
                data.SurfaceLevelDict[NwrwSurfaceType.RoofFlat] = value;
                data.UpdateCatchmentAreaSize();
            }
        }

        [Description("Roof Stretch area (m²)")]
        public double RoofFlatStretched
        {
            get
            {
                return data.SurfaceLevelDict != null && data.SurfaceLevelDict.TryGetValue(NwrwSurfaceType.RoofFlatStretched, out double result)
                    ? result
                    : 0.0;
            }
            set
            {
                data.SurfaceLevelDict[NwrwSurfaceType.RoofFlatStretched] = value;
                data.UpdateCatchmentAreaSize();
            }
        }

        [Description("Unpaved Sloped area (m²)")]
        public double UnpavedWithSlope
        {
            get
            {
                return data.SurfaceLevelDict != null && data.SurfaceLevelDict.TryGetValue(NwrwSurfaceType.UnpavedWithSlope, out double result)
                    ? result
                    : 0.0;
            }
            set
            {
                data.SurfaceLevelDict[NwrwSurfaceType.UnpavedWithSlope] = value;
                data.UpdateCatchmentAreaSize();
            }
        }

        [Description("Unpaved Flat area (m²)")]
        public double UnpavedFlat
        {
            get
            {
                return data.SurfaceLevelDict != null && data.SurfaceLevelDict.TryGetValue(NwrwSurfaceType.UnpavedFlat, out double result)
                    ? result
                    : 0.0;
            }
            set
            {
                data.SurfaceLevelDict[NwrwSurfaceType.UnpavedFlat] = value;
                data.UpdateCatchmentAreaSize();
            }
        }

        [Description("Unpaved Stretch area (m²)")]
        public double UnpavedFlatStretched
        {
            get
            {
                return data.SurfaceLevelDict != null && data.SurfaceLevelDict.TryGetValue(NwrwSurfaceType.UnpavedFlatStretched, out double result)
                    ? result
                    : 0.0;
            }
            set
            {
                data.SurfaceLevelDict[NwrwSurfaceType.UnpavedFlatStretched] = value;
                data.UpdateCatchmentAreaSize();
            }
        }

        [Description("Number of units (inhabitant)")]
        public double NumberOfUnitsFirstDwf
        {
            get
            {
                return data.DryWeatherFlows.Count >= 1
                    ? data.DryWeatherFlows[0].NumberOfUnits
                    : 0;
            }
            set { data.DryWeatherFlows[0].NumberOfUnits = value; }
        }

        [Description("DWF definition (inhabitant)")]
        public string FirstDryWeatherFlowId
        {
            get
            {
                return data.DryWeatherFlows.Count >= 1
                    ? data.DryWeatherFlows[0].DryWeatherFlowId
                    : string.Empty;
            }
            set { data.DryWeatherFlows[0].DryWeatherFlowId = value; }
        }

        [Description("Number of units (company)")]
        public double NumberOfUnitsLastDwf
        {
            get
            {
                return data.DryWeatherFlows.Count >= 2
                    ? data.DryWeatherFlows[1].NumberOfUnits
                    : 0;
            }
            set { data.DryWeatherFlows[1].NumberOfUnits = value; }
        }

        [Description("DWF definition (company)")]
        public string LastDryWeatherFlowId
        {
            get
            {
                return data.DryWeatherFlows.Count >= 2
                    ? data.DryWeatherFlows[1].DryWeatherFlowId
                    : string.Empty;
            }
            set { data.DryWeatherFlows[1].DryWeatherFlowId = value; }
        }

        [Description("Meteostation identification")]
        public string MeteoStationId
        {
            get { return data.MeteoStationId; }
            set { data.MeteoStationId = value; }
        }

        public override void SetColumnEditorForDataWithModel(IRainfallRunoffModel model,
            IEnumerable<ITableViewColumn> tableViewColumns)
        {
            var dwfidcolumn = tableViewColumns.FirstOrDefault(c =>
                c.Caption.Equals(TypeUtils.GetMemberDescription(() => new NwrwDataRow().FirstDryWeatherFlowId)));
            if (dwfidcolumn != null)
            {
                dwfidcolumn.Editor = new ComboBoxTypeEditor
                {
                    Items = model?.NwrwDryWeatherFlowDefinitions
                            .Select(dwfd => dwfd.Name)
                };
            }
            dwfidcolumn = tableViewColumns.FirstOrDefault(c =>
                c.Caption.Equals(TypeUtils.GetMemberDescription(() => new NwrwDataRow().LastDryWeatherFlowId)));
            if (dwfidcolumn != null)
            {
                dwfidcolumn.Editor = new ComboBoxTypeEditor
                {
                    Items = model?.NwrwDryWeatherFlowDefinitions
                            .Select(dwfd => dwfd.Name)
                };
            }
        }
    }
}
