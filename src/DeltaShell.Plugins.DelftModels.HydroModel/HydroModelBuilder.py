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
rtcPlugin = HydroAssembly("DeltaShell.Plugins.DelftModels.RealTimeControl", False)
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
from DelftTools.Shell.Core.Extensions import *
from DelftTools.Shell.Core.Workflow import *
from DelftTools.Shell.Core.Workflow.DataItems import *

if rtcPlugin.loaded:
    from DeltaShell.Plugins.DelftModels.RealTimeControl import *

if fmPlugin.loaded:
    from DeltaShell.Plugins.FMSuite.FlowFM import *
    from DeltaShell.Plugins.FMSuite.FlowFM.Model import *
    
if wavePlugin.loaded:
    from DeltaShell.Plugins.FMSuite.Wave import *

class ModelGroups:
    Empty = 0
    FMWaveRtcModels = 1
    All = 2

class HydroModelBuilder(object):
    """Builds pre-configured version of HydroModel containing a set of models and a number of default workflows."""
    RTC_MODEL_NAME = "Real_Time_Control"
    DFLOW_FM_MODEL_NAME = "FlowFM"
    WAVE_MODEL_NAME = "Waves"
    
    def can_create_modelgroup(self, modelGroup):
        if modelGroup == ModelGroups.Empty:
            return True
            
        if modelGroup == ModelGroups.All:
            return True
                
        if modelGroup == ModelGroups.FMWaveRtcModels:
            if fmPlugin.loaded and (wavePlugin.loaded or rtcPlugin.loaded):
                return True
                        
        return False

    def on_activity_added(self, activity):
        # ensure default model names are on adding:
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
        model = HydroModel(Name="Integrated_Model")
                
        if (modelGroup == ModelGroups.FMWaveRtcModels or modelGroup == ModelGroups.All):

            if rtcPlugin.loaded:
                rtc = RealTimeControlModel(Name=self.RTC_MODEL_NAME)
                model.Activities.Add(rtc)

            if hydroPlugin.loaded and fmPlugin.loaded:
                # build area
                area = HydroArea(Name="Area")
                model.Region.SubRegions.Add(area)

            if fmPlugin.loaded:
                flowfm = WaterFlowFMModel(Name=self.DFLOW_FM_MODEL_NAME)
                model.Activities.Add(flowfm)

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
        rtc = None
        if rtcPlugin.loaded:
            rtc = self.get_first_by_type(model.Activities, RealTimeControlModel)

        dflowfm = None
        if fmPlugin.loaded:
            dflowfm = self.get_first_by_type(model.Activities, WaterFlowFMModel)

        wave = None
        if wavePlugin.loaded:
            wave = self.get_first_by_type(model.Activities, WaveModel)

        if dflowfm and rtc:
            w = ParallelActivity(Name="(RTC + FlowFM)")
            w.Activities.AddRange((ActivityWrapper(rtc), ActivityWrapper(dflowfm)))
            model.Workflows.Add(w)

        if dflowfm and wave:
            w = ParallelActivity(Name="(Waves + FlowFM)")
            w.Activities.AddRange((ActivityWrapper(wave), ActivityWrapper(dflowfm)))
            model.Workflows.Add(w)

        if dflowfm and wave and rtc:
            w = ParallelActivity(Name="(Waves + RTC + FlowFM)")
            w.Activities.AddRange((ActivityWrapper(wave), ActivityWrapper(rtc), ActivityWrapper(dflowfm)))
            model.Workflows.Add(w)

        if dflowfm:
            w = ParallelActivity(Name="(FlowFM)")
            w.Activities.Add(ActivityWrapper(dflowfm))
            model.Workflows.Add(w)

        if wave:
            w = ParallelActivity(Name="(Waves)")
            w.Activities.Add(ActivityWrapper(wave))
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
		pass

    # Note: we no longer want to remove the area from a region when fm model is removed from hydro model
    # I've left this function stub here as we may want to use it for other things in future - RP    
    def on_activity_removed(self, model, removedActivity):
        doNothing = True
        
    def rebuild_all_model_links(self, model):
        # get instances of the activities from the model
        # rebuild
        for activity in model.Activities:
            self.auto_add_required_model_links(model, activity, False)

    def links_from_wave_to_flowfm(self, wave, fm, remove=False):
        if remove:           
            wave.ModelDefinition.CommunicationsFilePath = ""
        else:
            fm.SetWaveForcing() 
            wave.IsCoupledToFlow = True
            wave.ModelDefinition.CommunicationsFilePath = '../dflowfm/output/%s_com.nc'%(fm.Name)

    def auto_add_required_model_links(self, model, child, updateRegions=True, relinking=False):
        # query first region
        area = self.get_first_by_type(model.Region.SubRegions, HydroArea)
        
        # query first model
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
            if fm and fm == child and not area:
                area = HydroArea(Name="Area")
                model.Region.SubRegions.Add(area)
            
            # fm area
            if fm and fm == child:
                fm.GetDataItemByValue(fm.Area).LinkTo(model.GetDataItemByValue(area), relinking)
        
        # wave added and fm already or v.v.
        if wave and fm and (wave == child or fm == child):
            self.links_from_wave_to_flowfm(wave, fm)
        
        if(wave and wave == child and not fm):
            wave.ModelDefinition.CommunicationsFilePath = ""