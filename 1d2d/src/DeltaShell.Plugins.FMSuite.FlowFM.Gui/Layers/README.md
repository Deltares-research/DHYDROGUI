# DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers

The layers namespace defines the logic to generate the map layers of the FlowFM GUI 
plugin. It is organised using the `ILayerSubProvider` architecture. Each layer should
and is created by a corresponding implementation of the `ILayerSubProvider`. These 
can be found in the `Providers` folder. This folder is organized based on the hierarchical
structure of the layers.

The actual creation of the layers is done in the [`FlowFMLayerInstanceCreator`](FlowFMLayerInstanceCreator.cs), which is injected as a [`IFlowFMLayerInstanceCreator`](IFlowFMLayerInstanceCreator.cs) into the providers. This class holds all the creation methods, but none of the logic when it shoud be created. 

A single overarching `IMapLayerProvider` is created in the [`FlowFMMapLayerProviderFactory`](FlowFMMapLayerProviderFactory.cs). 
Each of the providers defined in the providers folder should be defined in this factory.
If done so, their logic will be included in the actual creation of the map layers.

## Notes and Observations

### Child layer objects are used to determine which layers to refresh.

Currently, DeltaShell determines which layers to update based on which child layer objects,
retrieved with the `GenerateChildLayerObjects` methods, should be added and removed. Any
child layer objects that are no longer present in the new list, will be removed. 

This observation is important when dealing with Data Transfer Objects (DTO) generated as
children, for example the [`InputLayerData`](Providers/InputLayerData.cs). These objects are
newed whenever the child layer objects method is called. As such, two DTOs need to be equal
if their fields are equal, and not only when their references are equal. Otherwise, any update
will trigger a complete refresh of the layers. This can be achieved by implementing the 
`IEquatable` interface.