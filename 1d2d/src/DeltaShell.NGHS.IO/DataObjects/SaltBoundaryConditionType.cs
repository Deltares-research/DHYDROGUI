namespace DeltaShell.NGHS.IO.DataObjects
{
    /// <summary>
    /// What kind of salt condition is defined on the boundary?
    /// </summary>
    public enum SaltBoundaryConditionType
    {
        None,            /* No salt defined */
        Constant,        /* A constant concentration of salt */
        TimeDependent    /* Salt concentration in a time series. */
    }

    public enum SaltLateralDischargeType
    {
        Default,            /* No salt defined */
        ConcentrationConstant,  /* Constant concentration  5 ppt */
        ConcentrationTimeSeries, /* Concentration of salt changes over time (ppt) (t) */
        MassConstant,
        MassTimeSeries
    }
}