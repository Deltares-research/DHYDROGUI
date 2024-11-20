namespace DeltaShell.NGHS.IO.DataObjects
{
    /// <summary>
    /// What kind of temperature condition is defined on the boundary?
    /// </summary>
    public enum TemperatureBoundaryConditionType
    {
        None,            /* No temperature defined */
        Constant,        /* A constant temperature */
        TimeDependent    /* Temperature in a time series. */
    }

    public enum TemperatureLateralDischargeType
    {
        None,            /* No temperature defined */
        Constant,        /* A constant temperature */
        TimeDependent    /* Temperature in a time series. */
    }
}