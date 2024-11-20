How to update to new version RTCTools?

Get source from 
	https://repos.deltares.nl/repos/RtcModule


copy xsd's 
{
 pi_sharedtypes.xsd
 pi_timeseries.xsd
 rtcDataConfig.xsd
 rtcRuntimeConfig.xsd
 rtcSharedTypes.xsd
 rtcToolsConfig.xsd
 treeVector.xsd
}
from 
  [rtc]\RTCTools\xsd
to
  [delft-tools]\src\src\Plugins\DelftModels\DeltaShell.Plugins.DelftModels.RealTimeControl\Resources\

copy
from
  [rtc]\RTCTools\x86-windows-vc-9.0\bin\release\RTCTools.dll
to
  [delft-tools]src\lib\Plugins\DelftModels\DeltaShell.Plugins.DelftModels.RealTimeControl\

rebuild DeltaShell project
run tests from
<DeltaShell.Plugins.DelftModels.RealTimeControl.Tests>