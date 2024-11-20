namespace DeltaShell.NGHS.IO.DataObjects.Model1D
{
    public enum QuantityType
    {
        WaterLevel = 1, // BranchNodes, GridpointsOnBranches, HBoundaries, QBoundaries, Measurements
        WaterDepth = 2, // BranchNodes, GridpointsOnBranches, Measurements
        BottomLevel = 3, // BranchNodes, GridpointsOnBranches
        SurfaceArea = 4, // BranchNodes, GridpointsOnBranches, Measurements
        Volume = 5, // BranchNodes, GridpointsOnBranches
        Salinity = 6, // BranchNodes, GridpointsOnBranches
        Dispersion = 7, // ?
        Discharge = 8, // BranchLinks, ReachSegElmSet, Structures, HBoundaries, QBoundaries, Measurements
        Velocity = 9, // BranchLinks, ReachSegElmSet, Structures
        FlowArea = 10, // BranchLinks, ReachSegElmSet, Structures
        FlowPerimeter = 11, // BranchLinks
        FlowHydrad = 12, // BranchLinks
        FlowConv = 13, // BranchLinks
        FlowChezy = 14, // BranchLinks
        TotalArea = 15, // ?
        TotalWidth = 16, // BranchLinks
        Hyddepth = 17, // BranchLinks
        CrestLevel = 18, // Structures
        CrestWidth = 19, // Structures
        GateLowerEdgeLevel = 20, // Structures
        GateOpeningHeight = 21, // Structures
        ValveOpening = 22, // ?
        WaterlevelUp = 23, // Structures, Pumps
        WaterlevelDown = 24, // Structures, Pumps
        Head = 25, // Structures, Pumps
        PressureDifference = 26, // Structures
        PumpCapacity = 27, // Pumps
        Flux = 28, // 
        Xcor = 30, // 
        Ycor = 31, // 
        WindShield = 32, // 
        // Flow analysis quantities
        NoIteration = 33, // BranchNodes, GridpointsOnBranches
        NegativeDepth = 34, // BranchNodes, GridpointsOnBranches
        TimeStepEstimation = 35, // ReachSegElmSet
        // New entities 
        WaterLevelAtCrest = 36, // 
        WaterLevelGradient = 37,
        Froude = 38,
        DischargeMain = 39,
        DischargeFP1 = 40,
        DischargeFP2 = 41,
        ChezyMain = 42,
        ChezyFP1 = 43,
        ChezyFP2 = 44,
        AreaMain = 45,
        AreaFP1 = 46,
        AreaFP2 = 47,
        WidthMain = 48,
        WidthFP1 = 49,
        WidthFP2 = 50,
        HydradMain = 51,
        HydradFP1 = 52,
        HydradFP2 = 53,
        Length = 54, //length of segments
        QLat = 55, //QLat (fluxes: 0 -> many) per branch 
        FiniteVolumeGridIndex = 56, //delwaq segment id
        LateralIndex = 57, //lateral source index
        FiniteGridType = 58, //Not supported in Model Api, but needed for the GUI
        DischargeDemanded = 59, // Discharge demanded on lateral source
        TH_F1 = 60, // Thatcher-Harleman coefficient F1
        TH_F3 = 61, // Thatcher-Harleman coefficient F3
        TH_F4 = 62, // Thatcher-Harleman coefficient F4
        Density = 63, // Density salt
        EnergyLevels = 66, // energy levels on reach segments
        BalBoundariesIn = 67,
        BalBoundariesOut = 68,
        BalBoundariesTot = 69,
        BalError = 70,
        BalLatIn = 71,
        BalLatOut = 72,
        BalLatTot = 73,
        BalStorage = 74,
        BalVolume = 75,
        CrossLevels = 76,
        CrossFlowWidths = 77,
        CrossTotalWidths = 78,
        QZeta_1D2D = 79, // input coefficient for water level dependend flow
        QLat_1D2D = 80, // input coefficient for 1d2d lateral flow
        QTotal_1d2d = 81, // Result of Qzeta_1d2d * s1 - Qlat_1d2d
        Bal2d1dIn = 82,
        Bal2d1dOut = 83,
        Bal2d1dTot = 84,
        LateralAtNodes = 85,
        PumpDischarge = 86,
        SuctionSideLevel = 87,
        DeliverySideLevel = 88,
        PumpHead = 89,
        ActualPumpStage = 90,
        ReductionFactor = 91,
        Temperature = 92,
        TotalHeatFlux = 93,
        RadFluxClearSky = 94,
        HeatLossConv = 95,
        NetSolarRad = 96,
        EffectiveBackRad = 97,
        HeatLossEvap = 98,
        HeatLossForcedEvap = 99,
        HeatLossFreeEvap = 100,
        HeatLossForcedConv = 101,
        HeatLossFreeConv = 102,
        ActualDischarge = 110, // Actual lateral discharge
        DefinedDischarge = 111, // Defined lateral discharge
        LateralDifference = 112, // difference between Actual and Defined lateral discharge
        GateOpeningWidth,
        GateOpeningHorizontalDirection,
        ValveOpeningHeight
    }
}