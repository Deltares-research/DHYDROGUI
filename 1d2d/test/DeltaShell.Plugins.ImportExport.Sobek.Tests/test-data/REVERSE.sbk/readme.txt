Modeler: Rolf Hulsbergen, Deltares, august 2011
Testmodels created to test DeltaShell Sobek RE reverse roughness import.
Model result testing not documented yet, but the cases should be useable for that too.

**Case description "Reverse Roughness test land-sea":
**A single channel with a "land" boundary at the left (Q=0) and a "sea" boundary at the right (Q=daily sine, repeating).
Smooth roughness from left to right (normal direction):
 Main = Chezy 60
 FP1 = manning 0.02
Coarse roughness from right to left (reverse direction).
 Main = Chezy 2.5
 FP1 = manning 0.35

**Case description "Reverse Roughness test 5 channels":
**5 Channels, A through E.
For all channels:
 Discharge of 500 m3/s from left to right, with H=0 at right boundary.
 
*Channel A:
*Base channel, large difference in rougness between directions, but always flow from left to right.
Smooth roughness from left to right (normal direction):
 Main = Chezy 60
 FP1 = manning 0.02
Coarse roughness from right to left (reverse direction).
 Main = Chezy 2.5
 FP1 = manning 0.35
 
Channel B through E:
Defined in opposite direction of channel A (so normal flow = right to left, actual flow = left to right)

*Channel B:
*Like channel A, only reversed channel definition direction
Smooth roughness from right to left (normal direction):
 Main = Chezy 60
 FP1 = manning 0.02
Coarse roughness from left to right (reverse direction).
 Main = Chezy 2.5
 FP1 = manning 0.35

*Channel C:
*Like channel B, but with F(place) for main and As main for FP1
Smooth roughness from right to left (normal direction):
 Main = Chezy 30 - 60
 FP1 = As main
Coarse roughness from left to right (reverse direction).
 Main = Chezy 2.5 - 5 [5 - 2.5]
 FP1 = As main

*Channel D:
*Like Channel C, but with negative using normal rougness and constant manning rougness for FP1
Smooth roughness from right to left (normal direction):
 Main = Chezy 30 - 60
 FP1 = Manning 0.02
Smooth roughness from left to right (reverse direction).
 Main & FP1 = As normal direction

*Channel E:
*Like Channel D, but with reverse roughness leading and FP1 = main
Smooth roughness from right to left (normal direction):
 Main & FP1 = As reverse direction
Smooth roughness from left to right (reverse direction).
 Main = Chezy 30 - 60
 FP1 = Main
