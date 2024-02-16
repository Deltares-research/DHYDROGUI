using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DelftTools.Utils.Collections;

namespace DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition
{
    public partial class WaterFlowFMModelDefinition
    {
        private readonly Dictionary<string, Action<WaterFlowFMProperty>> waterFlowFmPropertyChangedHandler;

        private bool handlingPropertyChanged;

        private void OnWaterFlowFMCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                object removedOrAddedItem = e.GetRemovedOrAddedItem();
                if (removedOrAddedItem == GetModelProperty(KnownProperties.Temperature))
                {
                    var prop = (WaterFlowFMProperty) removedOrAddedItem;
                    HeatFluxModel.Type = (HeatFluxModelType) (int) prop.Value;
                }
            }
        }

        private void OnWaterFlowFMPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (handlingPropertyChanged)
            {
                return; //prevent recursion in syncing useTemperature with heat flux model type
            }

            handlingPropertyChanged = true;

            try
            {
                var prop = (WaterFlowFMProperty) sender;
                string propName = prop.PropertyDefinition.MduPropertyName.ToLower();
                if (waterFlowFmPropertyChangedHandler.ContainsKey(propName))
                {
                    waterFlowFmPropertyChangedHandler[propName](prop);
                }
            }
            finally
            {
                handlingPropertyChanged = false;
            }
        }

        private void OnIcdTypePropertyChanged(WaterFlowFMProperty icdtypProp)
        {
            var icdtyp = (int) icdtypProp.Value;
            if (icdtyp == 2 || icdtyp == 3)
            {
                WaterFlowFMProperty cdbreakpointsProperty = GetModelProperty(KnownProperties.Cdbreakpoints);
                CorrectWindDragCoefficientBreakpointsCollection(cdbreakpointsProperty, icdtyp);

                WaterFlowFMProperty windspeedbreakpointsProperty =
                    GetModelProperty(KnownProperties.Windspeedbreakpoints);
                CorrectWindDragCoefficientBreakpointsCollection(windspeedbreakpointsProperty, icdtyp);
            }
        }

        private void OnTimePropertyChanged(WaterFlowFMProperty prop)
        {
            UpdateOutputTimes();
        }

        private void OnTemperaturePropertyChanged(WaterFlowFMProperty temperatureProp)
        {
            HeatFluxModel.Type = (HeatFluxModelType) (int) temperatureProp.Value;
        }

        private void OnWriteSnappedFeaturesPropertyChanged(WaterFlowFMProperty prop)
        {
            foreach (string writeProp in KnownWriteOutputSnappedFeatures)
            {
                GetModelProperty(writeProp).Value = WriteSnappedFeatures;
            }
        }

        private void OnMorphologySedimentPropertyChanged(WaterFlowFMProperty prop)
        {
            if (prop.PropertyDefinition.MduPropertyName == GuiProperties.UseMorSed)
            {
                WaterFlowFMProperty sedimentModelNumberProperty = GetModelProperty(KnownProperties.SedimentModelNumber);
                string newValueAsString = UseMorphologySediment ? "4" : "0";
                sedimentModelNumberProperty.SetValueFromString(newValueAsString);

                SetMapFormatPropertyValue();
            }
        }

        private void UpdateOutputTimes()
        {
            UpdateOutputTimesFromSimulationPeriod(GuiProperties.SpecifyHisStart, GuiProperties.HisOutputStartTime,
                                                  GuiProperties.SpecifyHisStop, GuiProperties.HisOutputStopTime);
            UpdateOutputTimesFromSimulationPeriod(GuiProperties.SpecifyMapStart, GuiProperties.MapOutputStartTime,
                                                  GuiProperties.SpecifyMapStop, GuiProperties.MapOutputStopTime);
            UpdateOutputTimesFromSimulationPeriod(GuiProperties.SpecifyRstStart, GuiProperties.RstOutputStartTime,
                                                  GuiProperties.SpecifyRstStop, GuiProperties.RstOutputStopTime);
            UpdateOutputTimesFromSimulationPeriod(GuiProperties.SpecifyWaqOutputStartTime,
                                                  GuiProperties.WaqOutputStartTime,
                                                  GuiProperties.SpecifyWaqOutputStopTime,
                                                  GuiProperties.WaqOutputStopTime); /*rstoutput needs to be replaced */
        }

        private void CorrectWindDragCoefficientBreakpointsCollection(WaterFlowFMProperty breakPointsProperty,
                                                                     int icdtyp)
        {
            var cdbreakpoints = (IList<double>) breakPointsProperty.Value;
            // Append new values:
            if (cdbreakpoints.Count < icdtyp)
            {
                breakPointsProperty.Value =
                    new List<double>(cdbreakpoints.Concat(Enumerable.Repeat(0.0, icdtyp - cdbreakpoints.Count)));
            }

            // Remove obsolete values:
            if (cdbreakpoints.Count > icdtyp)
            {
                breakPointsProperty.Value = new List<double>(cdbreakpoints.Take(icdtyp));
            }
        }

        private void UpdateOutputTimesFromSimulationPeriod(string specifyStartPropName, string startTimePropName,
                                                           string specifyStopPropName, string stopTimePropName)
        {
            if (!(bool) GetModelProperty(specifyStartPropName).Value)
            {
                GetModelProperty(startTimePropName).Value = GetModelProperty(KnownProperties.StartDateTime).Value;
            }

            if (!(bool) GetModelProperty(specifyStopPropName).Value)
            {
                GetModelProperty(stopTimePropName).Value = GetModelProperty(KnownProperties.StopDateTime).Value;
            }
        }
    }
}