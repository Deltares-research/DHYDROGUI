#
# Test case configuration file for testbench: config_testcase.tcl
#
set testcase_engine "unstruc"
#
set runid           "index4_1d2d"
#
set netcdf_check_args {
    {cross_section_discharge index4_1d2d_his.nc}
    {cross_section_area      index4_1d2d_his.nc}
    {cross_section_velocity  index4_1d2d_his.nc}
}
