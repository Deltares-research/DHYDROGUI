using System;
using System.Runtime.InteropServices;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi
{
    public interface IModelApi
    {

        #region Logging

        bool LoggingEnabled { get;  set; }

        int ResetMessageCount();

        void LogMessages();

        /// <summary>
        /// Returns the number of messages the engine has available. Use GetMessage to request the message.
        /// </summary>
        /// <returns></returns>
        int MessageCount();

        /// <summary>
        /// Returns the message stored in the model engine.
        /// </summary>
        /// <param name="messageId"></param>
        /// The id of the message. The id is a one based index.
        /// <param name="errorLevel"></param>
        /// error/warning level of the message.
        /// <returns></returns>
        string GetMessage(int messageId, out ErrorLevels errorLevel);

        #endregion

        #region Network
        int NetworkSetTabCrossSection([In] double[] levels, [In] double[] flowWidth,
                                      [In] double[] totalWidth, [In] double[] plains, [In] bool closed,
                                      [In] bool groundlayerUsed, [In] double groundlayer);

        int NetworkSetTabCrossSection([In] double[] levels, [In] double[] flowWidth, [In] double[] totalWidth,
                                      [In] double[] plains, [In] double levelCrest, [In] double levelBottom,
                                      [In] double flowArea, [In] double totalArea, [In] bool closed,
                                      [In] bool groundlayerUsed, [In] double groundlayer);
        int NetworkSetYZCrossSection(double[] y, double[] z, double[] frictionSectionFrom, double[] frictionSectionTo,
                            int[] frictionTypePos, double[] frictionValuePos, int[] frictionTypeNeg, double[] frictionValueNeg,
                            double[] storageLevels, double[] storage);
 
        int NetworkAddObservationPoint(string id, int branchId, double offset);
       
        int NetworkSetCS(int branch, double location, int iref, double bottomLevel);

        int NetworkSetBoundary(int nodeId, int interpolationType, BoundaryType type, double value);
        int NetworkSetBoundary(int nodeId, int interpolationType, BoundaryType type, double value, double returnTime);
        int NetworkSetBoundary(int nodeId, double[] discharge, double[] waterLevel);
        
        void NetworkSetBoundaryValue(int iref, double time, double valueDouble);

        int NetworkAddStorage(string id);

        #endregion

        #region coupling on computational timestep

        bool ModelPerformTimeStep();

        bool ModelInitializeUserTimeStep();
        bool ModelFinalizeUserTimeStep();
        
        double ModelInitializeComputationalTimeStep(double newTime, double dt);
        double ModelRunComputationalTimeStep(double dt);
        bool ModelFinalizeComputationalTimeStep();

        bool Finalize();

        #endregion

        #region Structures

        int setStrucWeir([In] string Id, [In] int ibranch2, [In] double dist, [In] int icompound,
            [In] double crestlevel,
            [In] double crestwidth, 
            [In] double dischargecoef, 
            [In] double latdiscoef, 
            [In] int allowflowdir);

        
        int setStrucRiverWeir(
            [In] string Id, 
            [In] int ibranch2, [In] double dist, [In] int icompound,
            [In] double crestlevel,
            [In] double crestwidth,
            [In] double pos_cwcoef,
            [In] double pos_slimlimit,
            [In] double [] pos_sf, 
            [In] double [] pos_red,
            [In] double neg_cwcoef,
            [In] double neg_slimlimit,
            [In] double[] neg_sf,
            [In] double[] neg_red);
        
        int setStrucAdvWeir(
            [In] string Id,
            [In] int ibranch2, [In] double dist, [In] int icompound,
            [In] double crestlevel,
            [In] double totwidth,
            [In] int npiers,
            [In] double pos_height,
            [In] double pos_designhead,
            [In] double pos_piercontractcoef,
            [In] double pos_abutcontractcoef,
            [In] double neg_height,
            [In] double neg_designhead,
            [In] double neg_piercontractcoef,
            [In] double neg_abutcontractcoef);
        
        int setStrucOrifice(
            [In] string Id, 
            [In] int ibranch2, [In] double dist, [In] int icompound,
            [In] double crestlevel,
            [In] double crestwidth,
            [In] double contrcoef,
            [In] double latcontrcoef,
            [In] int allowedflowdir,
            [In] double openlevel,
            [In] int uselimitflowpos,
            [In] double limitflowpos,
            [In] int uselimitflowneg,
            [In] double limitflowneg);
        
        int setStrucCulvert(
            [In] string Id, 
            [In] int ibranch2, [In] double dist, [In] int icompound,
            [In] double leftlevel,
            [In] double rightlevel,
            [In] int crosssectionnr,
            [In] int allowedflowdir,
            [In] double length,
            [In] double inletlosscoef,
            [In] double outletlosscoef,
            [In] int valve_onoff,
            [In] double inivalveopen,
            [In] int bedFrictionType, 
            [In] double bedFriction, 
            [In] double groundFriction,
            [In] double [] relativeOpening,
            [In] double [] lossCoefficient);
        
        int setStrucSiphon(
            [In] string Id, 
            [In] int ibranch2, [In] double dist, [In] int icompound,
            [In] double leftlevel,
            [In] double rightlevel,
            [In] int crosssectionnr,
            [In] int allowedflowdir,
            [In] double length,
            [In] double inletlosscoef,
            [In] double outletlosscoef,
            [In] double bendlosscoef,
            [In] int valve_onoff,
            [In] double inivalveopen,
            [In] int bedFrictionType,
            [In] double bedFriction,
            [In] double groundFriction, 
            [In] double [] relativeOpening,
            [In] double [] lossCoefficient,
            [In] double turnonlevel,
            [In] double turnofflevel,
            [In] int siphon_onoff);
        
        int setStrucInvSiphon(
            [In] string Id, 
            [In] int ibranch2, [In] double dist, [In] int icompound,
            [In] double leftlevel,
            [In] double rightlevel,
            [In] int crosssectionnr,
            [In] int allowedflowdir,
            [In] double length,
            [In] double inletlosscoef,
            [In] double outletlosscoef,
            [In] double bendlosscoef,
            [In] int valve_onoff,
            [In] double inivalveopen,
            [In] int bedFrictionType,
            [In] double bedFriction,
            [In] double groundFriction,
            [In] double[] relativeOpening,
            [In] double[] lossCoefficient);
        
        int setStrucUniWeir(
            [In] string Id, 
            [In] int ibranch2,
            [In] double dist,
            [In] int icompound,
            [In] double[] y, 
            [In] double[] z,
            [In] double crestlevel,
            [In] double dischargecoef,
            [In] int allowedflowdir,
            [In] double freesubmergedfactor);
               
        int setStrucGeneralst(
            [In] string Id, 
            [In] int ibranch2, [In] double dist, [In] int icompound,
            [In] double widthleftW1,
            [In] double levelleftZb1,
            [In] double widthleftWsdl,
            [In] double levelleftZbsl,
            [In] double widthcenter,
            [In] double levelcenter,
            [In] double widthrightWsdr,
            [In] double levelrightZbsr,
            [In] double widthrightW2,
            [In] double levelrightZb2,
            [In] double gateheight,
            [In] double pos_freegateflowcoef,
            [In] double pos_drowngateflowcoef,
            [In] double pos_freeweirflowcoef,
            [In] double pos_drownweirflowcoef,
            [In] double pos_contrcoeffreegate,
            [In] double neg_freegateflowcoef,
            [In] double neg_drowngateflowcoef,
            [In] double neg_freeweirflowcoef,
            [In] double neg_drownweirflowcoef,
            [In] double neg_contrcoeffreegate,
            [In] double extraresistance);
        
        int setStrucPump(
            [In] string Id, 
            [In] int ibranch2, [In] double dist, [In] int icompound,
            [In] int direction,
            [In] int nrstages, 
            [In] int icapo,
            [In] double computed_capacity,
            [In] double oldcapacity,
            [In] double capacitySetpoint,
            [In] bool isControlled,
            [In] double[] capacity,
            [In] double[] onlevelup,
            [In] double[] offlevelup,
            [In] double[] onleveldown,
            [In] double[] offleveldown,
            [In] int tabsize,
            [In] int interpoltype,
            [In] int storedcounter,
            [In] double[] leveldiff,
            [In] double[] redfact
            );

        int setStrucBridge([In] string id, [In] int ibranch2, [In] double dist, [In] int icompound,
                           [In] double pillarwidth, [In] double formfactor, [In] int allowedFlowDir);

        int setStrucBridge([In] string id, [In] int ibranch2, [In] double dist, [In] int icompound,
                           [In] double bottomlevel, [In] double pillarwidth, [In] double formfactor, [In] int crosssectionnr,
                           [In] double length, [In] double inletlosscoef, [In] double outletlosscoef,
                           [In] int allowedFlowDir, [In] int bedFrictionType, [In] double bedFriction,
                           [In] double groundFriction);

        
        void SetStrucControlValue([In] int istru, [In] QuantityType type, [In] double numValue);

        void InitialiseStrucMappingArray([In] int count);

        #endregion

        #region Conveyance tables and interpolation

        void GetConveyanceTable(int crossSectionNr, ref double[] levels, ref double[] flowArea, ref double[] flowWidth, ref double[] perimeter, ref double[] hydraulicRadius, ref double[] totalWidth, ref double[] conveyancePos, ref double[] conveyanceNeg);

        int GetInterpolatedZWCrossSection(int crossSectionNr1, int crossSectionNr2, double distanceBetweenCrossSections,
            double distanceToCrossSectionNr1, out int levelsCount, out double bottomLevelShift, ref double[] levels,
            ref double[] flowWidth, ref double[] totalWidth, ref double[] plains, [In, Out] ref double levelCrest,
            [In, Out] ref double levelBottom, [In, Out] ref double flowArea, [In, Out] ref double totalArea,
            [In, Out] ref bool groundlayerUsed, [In, Out] ref double groundlayer);

        int GetInterpolatedYZCrossSection(int crossSectionNr1, int crossSectionNr2, double distanceBetweenCrossSections,
            double distanceToCrossSectionNr1, ref double[] y, ref double[] z);

        #endregion

        #region Other

        // TODO to be replaced by BMI get_Var
        double[] GetValues(QuantityType iquant, ElementSet ielmSet);
        double GetValue(QuantityType quantity, ElementSet elmSet, int location);
        int GetSize(ElementSet ielmSet);

        void SetValues(QuantityType type, double[] values);



        int SetStatisticalOutput(ElementSet elementsetId, QuantityType quantityId,
                                 AggregationOptions operation, double outputInterval);


        double[] GetStatisticalOutput(ElementSet elementsetId, QuantityType quantityId,
                                      AggregationOptions operation, double outputTimeStep);

        void WriteWaqOutput(WaqVolType volType);

        bool SetMissingValue(double missingValue);

        void ReadFiles(string configfile);
        
        void setParameter(ModelApiParameter parameter);
        
        void get_var(string variable, ref Array result);

        void get_var_shape(string variable, ref int[] shape);

        void get_var_rank(string variable, out int rank);

        void get_var_type(string variable, out string value);

        void set_var(string variable, double[] values);

        void initialize(string path);

        #endregion
    }
}