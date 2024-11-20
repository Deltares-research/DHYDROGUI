# Generated on 15:06:07, 14-09-2010
# @(#)Deltares, UNSTRUC Version 1.0.11.12771M, Sep 13 2010, 16:24:46

[model]
Program          = UNSTRUC
Version          = 1.0.11
AutoStart        = 0                   # Autostart simulation after loading MDU or not (1/0).

[geometry]
NetFile          = rot45_3_net.nc      # *_net.nc
BathymetryFile   =                     # *.xyb
LandBoundaryFile =                     # Only for plotting
ThinDamFile      =                     # *_tdm.pli, Polyline(s) for tracing thin dams.
FixedWeirFile    =                     # *.pliz, Polyline(s) x,y,z, z = fixed weir top levels
WaterLevIni      = 0.                  # Initial water level
BotLevUni        = -5.                 # Uniform bottom level, (only if Botlevtype>=3, used at missing z values in netfile
BotLevType       = 3                   # 1 : Bottom levels at waterlevel cells (=flow nodes), like tiles xz, yz, bl , bob = max(bl left, bl right)
                                       # 2 : Bottom levels at velocity points  (=flow links),            xu, yu, blu, bob = blu,    bl = lowest connected link
                                       # 3 : Bottom levels at velocity points  (=flow links), using mean network levels xk, yk, zk  bl = lowest connected link
                                       # 4 : Bottom levels at velocity points  (=flow links), using min  network levels xk, yk, zk  bl = lowest connected link
                                       # 5 : Bottom levels at velocity points  (=flow links), using mean network levels xk, yk, zk  bl = lowest connected link
                                       # 5 : above -10 m, but average values of link depths if average value < -10 m
Keepbobsonbnd    = 1                   # 1 : Keep original bob values on open boundaries.
AngLat           = 0.                  # Angle of latitude (deg), 0=no Coriolis

[numerics]
CFLMax           = 0.3                 # Max. Courant nr.
CFLWaveFrac      = 0.2                 # Wave velocity fraction, total courant vel = u + cflw*wavevelocity
AdvecType        = 0                   # Adv type, 0=no, 1= Wenneker, qu-udzt, 2=1, q(uio-u), 3=Perot q(uio-u), 4=Perot q(ui-u), 5=Perot q(ui-u) without itself
Limtyphu         = 0                   # Limiter type for waterdepth in continuity eq., 0=no, 1=minmod,2=vanLeer,3=Kooren,4=Monotone Central
Limtypsa         = 0                   # Limiter type for salinity transport,           0=no, 1=minmod,2=vanLeer,3=Kooren,4=Monotone Central
Hkad             = 0.                  # Threshold for minimum bottomlevel step at which to apply energy conservation factor i.c. flow contraction

[physics]
UnifFrictCoef    = 2.4d-2              # Uniform friction coefficient, 0=no friction
UnifFrictType    = 1                   # 0=Chezy, 1=Manning, 2=White Colebrook, 3=z0 etc
Vicouv           = 1.000000000000001E-008# Uniform horizontal eddy viscosity
irov             = 0                   # 0=free slip, 1 = partial slip using wall_ks
wall_ks          = 0.                  # Nikuradse roughness for side walls, wall_z0=wall_ks/30
Vicoww           = 0.                  # Uniform vertical eddy viscosity
TidalForcing     = 1                   # Tidal forcing (0=no, 1=yes) (only for jsferic == 1)

[time]
RefDate          = 19920831            # Reference date (yyyymmdd)
Tunit            = Time                # Time units in MDU (H, M or S)
DtUser           = 30.                 # User timestep in seconds (interval for external forcing update)
DtMax            = 6.                  # Max timestep in seconds
AutoTimestep     = 2                   # Use CFL timestep limit or not (1/0)
TStart           = 0.                  # Start time w.r.t. RefDate (in TUnit)
TStop            = 1800.               # Stop  time w.r.t. RefDate (in TUnit)

[external forcing]
ExtForceFile     = ROT3.ext            # *.ext


[output]
ObsFile          =                     # *.xyn
CrsFile          = straight_crs.pli    #
HisFile          =                     # *_his.nc History file in NetCDF format.
HisInterval      = 120.                # Interval (in s) between history outputs
MapFile          =                     # *_map.nc Map file in NetCDF format.
MapInterval      = 1200.               # Interval (in s) between map file outputs
RstInterval      = 86400.              # Interval (in s) between map file outputs
SnapshotDir      =                     # Directory where snapshots/screendumps are saved.
