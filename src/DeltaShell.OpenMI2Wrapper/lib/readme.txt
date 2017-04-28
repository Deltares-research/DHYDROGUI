This folder contains the Deltares version of the OpenMI2 SDK model wrapping utilities.

To avoid conflicts in version of SDK utilities (e.g. of OATC.OpenMI.Sdk.Backbone),
every OpenMI component provider takes the source of the OATC OpenMI2 SDK,
and replaces the OATC.OpenMI.Sdk name space by his own name space.
For Deltares, this is Deltares.OpenMI.Oatc.Sdk.

The binaries are the ones Deltares also uses for the OpenMI2 wrappers
for Sobek212 (RR, CF, RTC), Delft3D-flow and Waqua. They are copied from:
https://repos.deltares.nl/repos/ds/trunk/src/third_party/openmi2

The sources for these Deltares.SDK binaries are stored in:
https://repos.deltares.nl/repos/ds/trunk/third_party_src/openmi2

The OATC.OpenMI.Sdk sources that serve as a base for the
Deltares.OpenMI.Oatc.Sdk sources can be found on the OpenMI sourceforge project:
https://openmi.svn.sourceforge.net/svnroot/openmi/trunk
