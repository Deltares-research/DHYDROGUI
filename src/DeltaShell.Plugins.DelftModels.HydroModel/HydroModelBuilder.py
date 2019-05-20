import clr

class HydroAssembly:
    """Defines an assembly with a name and if it is loaded"""
    loaded = False
    
    def __init__(self, plugin, throwError):
        try:
            HydroAssembly._HydroAssembly__add_reference(plugin, throwError)
            self.loaded = True
        except IOError:
            if throwError:
                raise
    
    @staticmethod
    def __add_reference(name, throwError):
        """Adds references to the specified assembly if it is not added."""
        for i in clr.References:
            if i.ToString().Contains(name):
                return # already added
        clr.AddReference(name)

# load dependencies (if necessary)
rrPlugin = HydroAssembly("DeltaShell.Plugins.DelftModels.RainfallRunoff", False)
rtcPlugin = HydroAssembly("DeltaShell.Plugins.DelftModels.RealTimeControl", False)
flow1DPlugin = HydroAssembly("DeltaShell.Plugins.DelftModels.WaterFlowModel", False)
fmPlugin = HydroAssembly("DeltaShell.Plugins.FMSuite.FlowFM", False)
wavePlugin = HydroAssembly("DeltaShell.Plugins.FMSuite.Wave", False)
hydroModelPlugin = HydroAssembly("DeltaShell.Plugins.DelftModels.HydroModel", True)
geoApiPlugin = HydroAssembly("GeoAPI.Extensions", True)
hydroPlugin = HydroAssembly("DelftTools.Hydro", True)
corePlugin = HydroAssembly("DelftTools.Shell.Core", True)

# throwError was set to 
from DelftTools.Hydro import *
from GeoAPI.Extensions.Coverages import *
from DeltaShell.Plugins.DelftModels.HydroModel import *
from DeltaShell.Plugins.DelftModels.HydroModel.ValueConverters import *
from DelftTools.Shell.Core.Extensions import *
from DelftTools.Shell.Core.Workflow import *
from DelftTools.Shell.Core.Workflow.DataItems import *

if flow1DPlugin.loaded:
    from DeltaShell.Plugins.DelftModels.WaterFlowModel import *

if rrPlugin.loaded:
    from DeltaShell.Plugins.DelftModels.RainfallRunoff import *

if rtcPlugin.loaded:
    from DeltaShell.Plugins.DelftModels.RealTimeControl import *

if fmPlugin.loaded:
    from DeltaShell.Plugins.FMSuite.FlowFM import *
    from DeltaShell.Plugins.FMSuite.FlowFM.WaterFlowFMModel import *
    
if wavePlugin.loaded:
    from DeltaShell.Plugins.FMSuite.Wave import *

class ModelGroups:
    Empty = 0
    SobekModels = 1
    FMWaveRtcModels = 2
    OverLandFlow1D2D = 3
    All = 4

class HydroModelBuilder(object):
    """Builds pre-configured version of HydroModel containing a set of models and a number of default workflows."""
    RR_MODEL_NAME = "Rainfall Runoff"
    FLOW_MODEL_NAME = "Flow1D"
    RTC_MODEL_NAME = "Real-Time Control"
    DFLOW_FM_MODEL_NAME = "FlowFM"
    WAVE_MODEL_NAME = "Waves"
    
    def can_create_modelgroup(self, modelGroup):
        if modelGroup == ModelGroups.Empty:
            return True
            
        if modelGroup == ModelGroups.All:
            return True
            
        if modelGroup == ModelGroups.SobekModels:
            if flow1DPlugin.loaded and (rrPlugin.loaded or rtcPlugin.loaded):
                return True
                
        if modelGroup == ModelGroups.FMWaveRtcModels:
            if fmPlugin.loaded and (wavePlugin.loaded or rtcPlugin.loaded):
                return True
                
        if modelGroup == ModelGroups.OverLandFlow1D2D:
            if fmPlugin.loaded and flow1DPlugin.loaded:
                return True
        
        return False

    def on_activity_added(self, activity):
        # ensure default model names are on adding:
        if rrPlugin.loaded:
            if isinstance(activity, RainfallRunoffModel):
                activity.Name = self.RR_MODEL_NAME
                return
        
        if flow1DPlugin.loaded:
            if isinstance(activity, WaterFlowModel1D):
                activity.Name = self.FLOW_MODEL_NAME
                return

        if rtcPlugin.loaded:
            if isinstance(activity, RealTimeControlModel):
                activity.Name = self.RTC_MODEL_NAME
                return

        if fmPlugin.loaded:
            if isinstance(activity, WaterFlowFMModel):
                activity.Name = self.DFLOW_FM_MODEL_NAME
                return
            
        if wavePlugin.loaded:
            if isinstance(activity, WaveModel):
                activity.Name = self.WAVE_MODEL_NAME
                return

    def build_model(self, modelGroup):
        model = HydroModel(Name="Integrated Model")

        if (modelGroup == ModelGroups.SobekModels or modelGroup == ModelGroups.OverLandFlow1D2D or modelGroup == ModelGroups.All):

            if hydroPlugin.loaded and flow1DPlugin.loaded:
                # build network
                network = HydroNetwork(Name="Network")
                model.Region.SubRegions.Add(network)

            if flow1DPlugin.loaded:
                flow = WaterFlowModel1D(Name=self.FLOW_MODEL_NAME)
                model.Activities.Add(flow)

        if (modelGroup == ModelGroups.SobekModels or modelGroup == ModelGroups.All):

            if hydroPlugin.loaded and rrPlugin.loaded:
                # build basin
                basin = DrainageBasin(Name="Basin")
                model.Region.SubRegions.Add(basin)
                
            if rrPlugin.loaded:
                rr = RainfallRunoffModel(Name=self.RR_MODEL_NAME)
                model.Activities.Add(rr)
                
        if (modelGroup == ModelGroups.SobekModels or modelGroup == ModelGroups.FMWaveRtcModels or modelGroup == ModelGroups.All):

            if rtcPlugin.loaded:
                rtc = RealTimeControlModel(Name=self.RTC_MODEL_NAME)
                model.Activities.Add(rtc)

        if (modelGroup == ModelGroups.FMWaveRtcModels or modelGroup == ModelGroups.OverLandFlow1D2D or modelGroup == ModelGroups.All):

            if hydroPlugin.loaded and fmPlugin.loaded:
                # build area
                area = HydroArea(Name="Area")
                model.Region.SubRegions.Add(area)

            if fmPlugin.loaded:
                flowfm = WaterFlowFMModel(Name=self.DFLOW_FM_MODEL_NAME)
                model.Activities.Add(flowfm)

        if (modelGroup == ModelGroups.FMWaveRtcModels or modelGroup == ModelGroups.All):
            if wavePlugin.loaded:
                wave = WaveModel(Name=self.WAVE_MODEL_NAME)
                model.Activities.Add(wave)

        self.refresh_default_model_workflows(model)
        if model.Workflows.Count > 0 : 
            model.CurrentWorkflow = model.Workflows[0]

        for a in model.Activities:
            self.auto_add_required_model_links(model, a)

        return model

    def refresh_default_model_workflows(self, model):
        
        if model.CurrentWorkflow != None :
            lastModelWorkflowName = model.CurrentWorkflow.Name 
        else : 
            lastModelWorkflowName = "___undefined___"
        
        model.Workflows.Clear()

        # take first instances of models
        rr = None
        if rrPlugin.loaded:
            rr = self.get_first_by_type(model.Activities, RainfallRunoffModel)

        flow = None
        if flow1DPlugin.loaded:
            flow = self.get_first_by_type(model.Activities, WaterFlowModel1D)

        rtc = None
        if rtcPlugin.loaded:
            rtc = self.get_first_by_type(model.Activities, RealTimeControlModel)

        dflowfm = None
        if fmPlugin.loaded:
            dflowfm = self.get_first_by_type(model.Activities, WaterFlowFMModel)

        wave = None
        if wavePlugin.loaded:
            wave = self.get_first_by_type(model.Activities, WaveModel)

        if rr and flow and rtc:
            w = SequentialActivity(Name="RR + (RTC + Flow1D)")
            w1 = ParallelActivity(Name="Parallel")
            w1.Activities.AddRange((ActivityWrapper(rtc), ActivityWrapper(flow)))
            w.Activities.AddRange((ActivityWrapper(rr), w1))
            model.Workflows.Add(w)

        if rr and flow:
            w = ParallelActivity(Name="(RR + Flow1D)")
            w.Activities.AddRange((ActivityWrapper(rr), ActivityWrapper(flow)))
            model.Workflows.Add(w)
            w = SequentialActivity(Name="RR + Flow1D")
            w.Activities.AddRange((ActivityWrapper(rr), ActivityWrapper(flow)))
            model.Workflows.Add(w)

        if rr and flow and rtc:
            w = ParallelActivity(Name="(RR + RTC + Flow1D)")
            w.Activities.AddRange((ActivityWrapper(rr), ActivityWrapper(rtc), ActivityWrapper(flow)))
            model.Workflows.Add(w)

        if flow and rtc:
            w = ParallelActivity(Name="(RTC + Flow1D)")
            w.Activities.AddRange((ActivityWrapper(rtc), ActivityWrapper(flow)))
            model.Workflows.Add(w)

        if dflowfm and rtc:
            w = ParallelActivity(Name="(RTC + FlowFM)")
            w.Activities.AddRange((ActivityWrapper(rtc), ActivityWrapper(dflowfm)))
            model.Workflows.Add(w)

        if dflowfm and flow:
            w = Iterative1D2DCoupler(Name="(FlowFM + Flow1D)")
            w.Flow1DModel = flow
            w.Flow2DModel = dflowfm
            w.Activities.AddRange((ActivityWrapper(dflowfm), ActivityWrapper(flow)))
            model.Workflows.Add(w)

        if dflowfm and flow and rtc:            
            w = Iterative1D2DCoupler(Name="(FlowFM + Flow1D)")
            w.Flow1DModel = flow
            w.Flow2DModel = dflowfm
            w.Activities.AddRange((ActivityWrapper(dflowfm), ActivityWrapper(flow)))
            p = ParallelActivity(Name="(RTC + (FlowFM + Flow1D))")
            p.Activities.AddRange((ActivityWrapper(rtc), ActivityWrapper(w)))
            model.Workflows.Add(p)

        if dflowfm and wave:
            w = ParallelActivity(Name="(Waves + FlowFM)")
            w.Activities.AddRange((ActivityWrapper(wave), ActivityWrapper(dflowfm)))
            model.Workflows.Add(w)

        if dflowfm and wave and rtc:
            w = ParallelActivity(Name="(Waves + RTC + FlowFM)")
            w.Activities.AddRange((ActivityWrapper(wave), ActivityWrapper(rtc), ActivityWrapper(dflowfm)))
            model.Workflows.Add(w)

        if flow:
            w = ParallelActivity(Name="(Flow1D)")
            w.Activities.Add(ActivityWrapper(flow))
            model.Workflows.Add(w)

        if rr:
            w = ParallelActivity(Name="(RR)")
            w.Activities.Add(ActivityWrapper(rr))
            model.Workflows.Add(w)

        if dflowfm:
            w = ParallelActivity(Name="(FlowFM)")
            w.Activities.Add(ActivityWrapper(dflowfm))
            model.Workflows.Add(w)

        model.CurrentWorkflow = None
        for workflow in model.Workflows : 
            if workflow.Name == lastModelWorkflowName : 
                model.CurrentWorkflow = workflow
                break
        if model.CurrentWorkflow == None and model.Workflows.Count > 0:
            model.CurrentWorkflow = model.Workflows[0]

    def get_first_by_type(self, items, t):
        # find the first of type t in items
        for i in items:
            if isinstance(i, t):
                return i

        # found nothing
        return None
        
    def on_activity_removing(self, model, activityBeingRemoved):
        # removing an activity, so remove them from the model builder
        # get instances of the activities from the model
        rr = None
        if rrPlugin.loaded:
            rr = self.get_first_by_type(model.Activities, RainfallRunoffModel)

        flow = None
        if flow1DPlugin.loaded:
            flow = self.get_first_by_type(model.Activities, WaterFlowModel1D)

        # remove rr -> flow
        # remove flow -> rr
        if ((activityBeingRemoved == rr and flow) or (activityBeingRemoved == flow and rr)):
            self.links_from_rr_to_flow(model, rr, flow, remove=True)
            self.links_from_flow_to_rr(model, flow, rr, remove=True)
        
    # Note: we no longer want to remove the area from a region when fm model is removed from hydro model
    # I've left this function stub here as we may want to use it for other things in future - RP    
    def on_activity_removed(self, model, removedActivity):
        doNothing = True
        
    def rebuild_all_model_links(self, model):
        # get instances of the activities from the model
        rr = None
        if rrPlugin.loaded:
            rr = self.get_first_by_type(model.Activities, RainfallRunoffModel)

        flow = None
        if flow1DPlugin.loaded:
            flow = self.get_first_by_type(model.Activities, WaterFlowModel1D)

        # not rtc

        # remove rr -> flow
        # remove flow -> rr
        if (rr and flow):
            self.links_from_rr_to_flow(model, rr, flow, remove=True)
            self.links_from_flow_to_rr(model, flow, rr, remove=True)

        # rebuild
        for activity in model.Activities:
            self.auto_add_required_model_links(model, activity, False)

    def links_from_rr_to_flow(self, model, rr, flow, remove=False):
        # flow <- rr
        #                
        # Ql(laterals/nodes, t) 
        #     Ql(c, t)  ----------------> Ql(c, t) 
        #      + value converter
        #         
        rrOutflow  = rr.GetDataItemByValue(rr.BoundaryDischarge)
        flowInflow = flow.GetDataItemByValue(flow.Inflows)

        if remove:
            if flowInflow.Children.Count > 0:
                convertedDischargeDataItem = flowInflow.Children[0]
                convertedDischargeDataItem.Unlink()
                flowInflow.Children.Remove(convertedDischargeDataItem)
        else:
            if flowInflow.Children.Count == 0:
                dischargeValueConverter = HydroLinksFeatureCoverageValueConverter(HydroRegion = model.Region, OriginalValue = flowInflow.Value)

                convertedDischargeDataItem = DataItem(ValueType = IFeatureCoverage, ShouldBeRemovedAfterUnlink = True, ValueConverter = dischargeValueConverter, Name="discharge (from rr 0d)")
                convertedDischargeDataItem.LinkTo(rrOutflow)

                flowInflow.Children.Add(convertedDischargeDataItem)

    def links_from_flow_to_rr(self, model, flow, rr, remove=False):
        # flow -> rr
        #
        #                           DI_H(c, t)
        # DI_H(nl, t) <---------------  DI_H(nl, t)
        #                                 + value converter
        flowWaterLevel = flow.GetDataItemByValue(flow.OutputWaterLevel)
        rrWaterLevel = rr.GetDataItemByValue(rr.InputWaterLevel)

        if remove:
            if rrWaterLevel.Children.Count > 0:
                convertedWaterLevelDataItem = rrWaterLevel.Children[0]
                convertedWaterLevelDataItem.Unlink()
                rrWaterLevel.Children.Remove(convertedWaterLevelDataItem)
        else:
            if rrWaterLevel.Children.Count == 0:
                waterLevelValueConverter = HydroRegionFeatureCoverageFromNetworkCoverageValueConverter(HydroRegion = model.Region, OriginalValue = rrWaterLevel.Value)

                convertedWaterLevelDataItem = DataItem(ValueType = INetworkCoverage, ShouldBeRemovedAfterUnlink = True, ValueConverter = waterLevelValueConverter, Name="water depth (from flow 1d)")
                convertedWaterLevelDataItem.LinkTo(flowWaterLevel)

                rrWaterLevel.Children.Add(convertedWaterLevelDataItem)

    def links_from_flow1d_to_flowfm(self, flow, fm, remove=False):
        print "linking overland flow stuff"

    def links_from_wave_to_flowfm(self, wave, fm, remove=False):
        if remove:
            wave.IsCoupledToFlow = False
            wave.GetFlowComFilePath = None
        else:
            fm.SetWaveForcing()
            wave.IsCoupledToFlow = True
            wave.GetFlowComFilePath = lambda: fm.ComFilePath;

    def auto_add_required_model_links(self, model, child, updateRegions=True, relinking=False):
        # query first region
        network = self.get_first_by_type(model.Region.SubRegions, HydroNetwork)
        basin = self.get_first_by_type(model.Region.SubRegions, DrainageBasin)
        area = self.get_first_by_type(model.Region.SubRegions, HydroArea)
        
        # query first model
        rr = None
        if rrPlugin.loaded:
            rr = self.get_first_by_type(model.Activities, RainfallRunoffModel)

        flow = None
        if flow1DPlugin.loaded:
            flow = self.get_first_by_type(model.Activities, WaterFlowModel1D)

        rtc = None
        if rtcPlugin.loaded:
            rtc = self.get_first_by_type(model.Activities, RealTimeControlModel)
            
        fm = None
        if fmPlugin.loaded:
            fm = self.get_first_by_type(model.Activities, WaterFlowFMModel)

        wave = None
        if wavePlugin.loaded:
            wave = self.get_first_by_type(model.Activities, WaveModel)

        if updateRegions:
            # rr added - auto-add basin
            if rr and rr == child and not basin:
                basin = DrainageBasin(Name="Basin")
                model.Region.SubRegions.Add(basin)

            # flow added - auto-add network
            if flow and flow == child and not network:
                network = HydroNetwork(Name="Network")
                model.Region.SubRegions.Add(network)
        
            if fm and fm == child and not area:
                area = HydroArea(Name="Area")
                model.Region.SubRegions.Add(area)

            # rr basin
            if rr and rr == child: # this is the only rr 0d model
                rr.GetDataItemByValue(rr.Basin).LinkTo(model.GetDataItemByValue(basin), relinking)

            # flow network
            if flow and flow == child: # this is the only flow 1d model
                flow.GetDataItemByValue(flow.Network).LinkTo(model.GetDataItemByValue(network), relinking)

            # fm area
            if fm and fm == child:
                fm.GetDataItemByValue(fm.Area).LinkTo(model.GetDataItemByValue(area), relinking)

        # rr or flow
        if flow and rr and (rr == child or flow == child): # rr or flow is added and flow or rr exists
            self.links_from_rr_to_flow(model, rr, flow)

            if (rr in CompositeActivityExtensions.GetActivitiesRunningSimultaneous(model, flow)):
                self.links_from_flow_to_rr(model, flow, rr)

        # wave added and fm already or v.v.
        if wave and fm and (wave == child or fm == child):
            self.links_from_wave_to_flowfm(wave, fm)