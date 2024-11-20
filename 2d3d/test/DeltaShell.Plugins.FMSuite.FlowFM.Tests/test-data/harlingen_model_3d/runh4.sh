#!/bin/bash
ulimit -s unlimited

exedir=/u/hasselaa/bin

#i7-normal settings:
source /opt/intel/Compiler/11.1/072/bin/ifortvars.sh intel64

PKGROOT=/u/dam_ar/pkg; export PKGROOT

#PATH=$PKGROOT/dflowfm/bin:$PATH
#LD_LIBRARY_PATH=$PKGROOT/dflowfm/current/lib:$LD_LIBRARY_PATH

PATH=$PKGROOT/netcdf/current/bin:$PATH
LD_LIBRARY_PATH=$PKGROOT/netcdf/current/lib:$LD_LIBRARY_PATH

export PATH
export LD_LIBRARY_PATH

$exedir/dflowfm -autostartstop $1
