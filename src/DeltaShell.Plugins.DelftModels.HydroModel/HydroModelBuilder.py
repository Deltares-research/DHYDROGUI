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
            if i.GetName().Name.Equals(name):
                return # already added
        clr.AddReference(name)

# load dependencies (if necessary)
rrPlugin = HydroAssembly("DeltaShell.Plugins.DelftModels.RainfallRunoff", False)
rtcPlugin = HydroAssembly("DeltaShell.Plugins.DelftModels.RealTimeControl", False)
fmPlugin = HydroAssembly("DeltaShell.Plugins.FMSuite.FlowFM", False)
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

if rrPlugin.loaded:
    from DeltaShell.Plugins.DelftModels.RainfallRunoff import *

if rtcPlugin.loaded:
    from DeltaShell.Plugins.DelftModels.RealTimeControl import *

if fmPlugin.loaded:
    from DeltaShell.Plugins.FMSuite.FlowFM import * 

class ModelGroups:
    Empty = 0
    SobekModels = 1
    FMWaveRtcModels = 2
    OverLandFlow1D2D = 3
    RHUModels = 5
    All = 4

class HydroModelBuilder(object):
    """Builds pre-configured version of HydroModel containing a set of models and a number of default workflows."""
    RR_MODEL_NAME = "Rainfall Runoff"
    RTC_MODEL_NAME = "Real-Time Control"
    DFLOW_FM_MODEL_NAME = "FlowFM"
    
    def can_create_modelgroup(self, modelGroup):
        if modelGroup == ModelGroups.Empty:
            return True
            
        if modelGroup == ModelGroups.All:
            return True
            
        if modelGroup == ModelGroups.SobekModels:
            if fmPlugin.loaded and (rrPlugin.loaded or rtcPlugin.loaded):
                return True
                
        if modelGroup == ModelGroups.RHUModels:
            if fmPlugin.loaded and (rrPlugin.loaded or rtcPlugin.loaded):
                return True
                
        if modelGroup == ModelGroups.FMWaveRtcModels:
            if fmPlugin.loaded and rtcPlugin.loaded:
                return True
                
        if modelGroup == ModelGroups.OverLandFlow1D2D:
            if fmPlugin.loaded:
                return True
        
        return False

    def on_activity_added(self, activity):
        # ensure default model names are on adding:
        if rrPlugin.loaded:
            if isinstance(activity, RainfallRunoffModel):
                activity.Name = self.RR_MODEL_NAME
                return
                
        if rtcPlugin.loaded:
            if isinstance(activity, RealTimeControlModel):
                activity.Name = self.RTC_MODEL_NAME
                return

        if fmPlugin.loaded:
            if isinstance(activity, WaterFlowFMModel):
                activity.Name = self.DFLOW_FM_MODEL_NAME
                return
              
    def build_model(self, modelGroup):
        model = HydroModel(Name="Integrated Model")

        if (modelGroup == ModelGroups.SobekModels or modelGroup == ModelGroups.OverLandFlow1D2D or modelGroup == ModelGroups.All):

            if hydroPlugin.loaded:
                # build network
                network = HydroNetwork(Name="Network")
                model.Region.SubRegions.Add(network)
                            
        if (modelGroup == ModelGroups.SobekModels or modelGroup == ModelGroups.RHUModels or modelGroup == ModelGroups.All):

            if hydroPlugin.loaded and rrPlugin.loaded:
                # build basin
                basin = DrainageBasin(Name="Basin")
                model.Region.SubRegions.Add(basin)
                
            if rrPlugin.loaded:
                rr = RainfallRunoffModel(Name=self.RR_MODEL_NAME)
                model.Activities.Add(rr)
                
        if (modelGroup == ModelGroups.SobekModels or modelGroup == ModelGroups.RHUModels  or modelGroup == ModelGroups.FMWaveRtcModels or modelGroup == ModelGroups.All):

            if rtcPlugin.loaded:
                rtc = RealTimeControlModel(Name=self.RTC_MODEL_NAME)
                model.Activities.Add(rtc)

        if (modelGroup == ModelGroups.SobekModels or modelGroup == ModelGroups.FMWaveRtcModels or modelGroup == ModelGroups.RHUModels  or modelGroup == ModelGroups.OverLandFlow1D2D or modelGroup == ModelGroups.All):

            if hydroPlugin.loaded and fmPlugin.loaded:
                # build area
                area = HydroArea(Name="Area")
                model.Region.SubRegions.Add(area)
                # build network
                network = HydroNetwork(Name="Network")
                model.Region.SubRegions.Add(network)

            if fmPlugin.loaded:
                flowfm = WaterFlowFMModel(Name=self.DFLOW_FM_MODEL_NAME)
                model.Activities.Add(flowfm)

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
                    
        rtc = None
        if rtcPlugin.loaded:
            rtc = self.get_first_by_type(model.Activities, RealTimeControlModel)

        dflowfm = None
        if fmPlugin.loaded:
            dflowfm = self.get_first_by_type(model.Activities, WaterFlowFMModel)

        if rr and dflowfm and rtc:
            w = ParallelActivity(Name="(RR + RTC + FlowFM)")
            w.Activities.AddRange((ActivityWrapper(rr), ActivityWrapper(rtc), ActivityWrapper(dflowfm)))
            model.Workflows.Add(w)

        if dflowfm and rtc:
            w = ParallelActivity(Name="(RTC + FlowFM)")
            w.Activities.AddRange((ActivityWrapper(rtc), ActivityWrapper(dflowfm)))
            model.Workflows.Add(w)
                    
        if rr and dflowfm:
            w = ParallelActivity(Name="(RR + FlowFM)")
            w.Activities.AddRange((ActivityWrapper(rr), ActivityWrapper(dflowfm)))
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

        fm = None
        if fmPlugin.loaded:
            fm = self.get_first_by_type(model.Activities, WaterFlowFMModel)                  
        
    # Note: we no longer want to remove the area from a region when fm model is removed from hydro model
    # I've left this function stub here as we may want to use it for other things in future - RP    
    def on_activity_removed(self, model, removedActivity):
        doNothing = True
        
    def rebuild_all_model_links(self, model):
        # get instances of the activities from the model
        rr = None
        if rrPlugin.loaded:
            rr = self.get_first_by_type(model.Activities, RainfallRunoffModel)

        fm = None
        if fmPlugin.loaded:
            fm = self.get_first_by_type(model.Activities, WaterFlowFMModel)

        # not rtc
                
        # rebuild
        for activity in model.Activities:
            self.auto_add_required_model_links(model, activity, False)                
            
    def auto_add_required_model_links(self, model, child, updateRegions=True, relinking=True):
        # query first region
        network = self.get_first_by_type(model.Region.AllRegions, HydroNetwork)
        basin = self.get_first_by_type(model.Region.SubRegions, DrainageBasin)
        area = self.get_first_by_type(model.Region.AllRegions, HydroArea)
        
        # query first model
        rr = None
        if rrPlugin.loaded:
            rr = self.get_first_by_type(model.Activities, RainfallRunoffModel)
                    
        rtc = None
        if rtcPlugin.loaded:
            rtc = self.get_first_by_type(model.Activities, RealTimeControlModel)
            
        fm = None
        if fmPlugin.loaded:
            fm = self.get_first_by_type(model.Activities, WaterFlowFMModel)
                    
        if updateRegions:
            # rr added - auto-add basin
            if rr and rr == child and not basin:
                basin = DrainageBasin(Name="Basin")
                model.Region.SubRegions.Add(basin)

            if fm and fm == child and not area and not network:
                area = HydroArea(Name="Area")
                model.Region.SubRegions.Add(area)
                network = HydroNetwork(Name="Network")
                model.Region.SubRegions.Add(network)

            # rr basin
            if rr and rr == child: # this is the only rr 0d model
                rr.GetDataItemByValue(rr.Basin).LinkTo(model.GetDataItemByValue(basin), relinking)

            # fm area
            if fm and fm == child:
                fm.GetDataItemByValue(fm.Area).LinkTo(model.GetDataItemByValue(area), relinking)
                fm.GetDataItemByValue(fm.Network).LinkTo(model.GetDataItemByValue(network), relinking)
                        