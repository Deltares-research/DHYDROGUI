using System;
using System.Collections.Generic;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers
{
    /// <summary>
    /// (this part runs in DeltaShell)
    /// This wrapper exists, in combination with the IRemoteModelApiWrapper, to aggregate set/get calls to the cf_dll, when 
    /// it runs in another process. This aggregation / batch-fetching significantly reduces the number of calls which 
    /// have to be done between processes, which is beneficial for the performance.
    /// </summary>
    public class LocalModelApiWrapper : IModelApi
    {
        // READ THIS: if the IModelApi interface changes, just use ReSharper Alt-Ins -> Delegating Members and select 
        // the newly added members to update this class automatically.

        private readonly IRemoteModelApiWrapper remoteApi; // remote instance
        
        private readonly List<int> networkSetBoundaryValueIRefs = new List<int>();
        private double networkSetBoundaryValueTime;
        private readonly List<double> networkSetBoundaryValueValues = new List<double>();

        public void NetworkSetBoundaryValue(int iref, double time, double valueDouble)
        {
            networkSetBoundaryValueIRefs.Add(iref);
            networkSetBoundaryValueTime = time;
            networkSetBoundaryValueValues.Add(valueDouble);
        }

        public bool ModelInitializeUserTimeStep()
        {
            return RemoteApi.ModelInitializeUserTimeStep();
        }

        public bool ModelFinalizeUserTimeStep()
        {
            return RemoteApi.ModelFinalizeUserTimeStep();
        }

        public double ModelInitializeComputationalTimeStep(double newTime, double dt)
        {
            return RemoteApi.ModelInitializeComputationalTimeStep(newTime, dt);
        }

        public double ModelRunComputationalTimeStep(double dt)
        {
            return RemoteApi.ModelRunComputationalTimeStep(dt);
        }

        public bool ModelFinalizeComputationalTimeStep()
        {
            return RemoteApi.ModelFinalizeComputationalTimeStep();
        }

        private readonly List<int> setStrucControlValueIRefs = new List<int>();
        private readonly List<int> setStrucControlValueTypes = new List<int>();
        private readonly List<double> setStrucControlValueValues = new List<double>();

        public void SetStrucControlValue(int istru, QuantityType type, double numValue)
        {
            setStrucControlValueIRefs.Add(istru);
            setStrucControlValueTypes.Add((int)type);
            setStrucControlValueValues.Add(numValue);
        }

        public void ReadFiles(string filename)
        {
            remoteApi.ReadFiles(filename);
        }

        private readonly Dictionary<int, double> prefetchedValues = new Dictionary<int, double>();
        private readonly List<int> keysToPrefetch = new List<int>(); 

        public double GetValue(QuantityType quantity, ElementSet elmSet, int location)
        {
            // encode the parameters into a key (lossless!) to keep the dictionary & api simple
            var key = location + ((int)elmSet << 16) + ((int)quantity << 24);

            double value;
            if (prefetchedValues.TryGetValue(key, out value))
                return value;

            if (!keysToPrefetch.Contains(key)) // double requests
                keysToPrefetch.Add(key);
            return remoteApi.GetValue(quantity, elmSet, location); // retrieve immediately (slow)
        }

        public LocalModelApiWrapper(IRemoteModelApiWrapper remoteApi)
        {
            this.remoteApi = remoteApi;
        }
        
        private void FlushSetValueCalls()
        {
            if (networkSetBoundaryValueIRefs.Count > 0)
            {
                remoteApi.NetworkSetBoundaryValues(networkSetBoundaryValueIRefs.ToArray(), networkSetBoundaryValueTime,
                                                   networkSetBoundaryValueValues.ToArray());
                networkSetBoundaryValueIRefs.Clear();
                networkSetBoundaryValueValues.Clear();
            }
            if (setStrucControlValueIRefs.Count > 0)
            {
                remoteApi.SetStrucControlValues(setStrucControlValueIRefs.ToArray(), setStrucControlValueTypes.ToArray(),
                                                setStrucControlValueValues.ToArray());
                setStrucControlValueIRefs.Clear();
                setStrucControlValueTypes.Clear();
                setStrucControlValueValues.Clear();
            }
        }

        public bool ModelPerformTimeStep()
        {
            FlushSetValueCalls();
            
            var res = remoteApi.ModelPerformTimeStep();

            PrefetchGetValuesCalls();

            return res;
        }

        private void PrefetchGetValuesCalls()
        {
            // prefetch values in batch we expect will be retrieved by GetValue();
            var values = remoteApi.GetAllValues(keysToPrefetch.ToArray());
            prefetchedValues.Clear();
            for (var i = 0; i < keysToPrefetch.Count; i++)
            {
                var key = keysToPrefetch[i];
                prefetchedValues.Add(key, values[i]);
            }
        }

        public bool Finalize()
        {
            return remoteApi.Finalize();
        }

        public IModelApi RemoteApi
        {
            get { return remoteApi; }
        }
        
        #region Delegating members

        public void get_var(string variable, ref Array array)
        {
            remoteApi.get_var(variable, ref array);
        }

        public void get_var_shape(string variable, ref int[] shape)
        {
            remoteApi.get_var_shape(variable, ref shape);
        }

        public void get_var_rank(string variable, out int rank)
        {
            remoteApi.get_var_rank(variable, out rank);
        }

        public void get_var_type(string variable, out string value)
        {
            remoteApi.get_var_type(variable, out value);
        }

        public void set_var(string variable, double[] values)
        {
            remoteApi.set_var(variable, values);
        }

        public void initialize(string path)
        {
            remoteApi.initialize(path); 
        }

        public int NetworkSetBoundary(int nodeId, double[] discharge, double[] waterLevel)
        {
            return remoteApi.NetworkSetBoundary(nodeId, discharge, waterLevel);
        }

        public void setParameter(ModelApiParameter parameter)
        {
            remoteApi.setParameter(parameter);
        }

        public string GetMessage(int messageId, out ErrorLevels errorLevel)
        {
            return remoteApi.GetMessage(messageId, out errorLevel);
        }

        public void InitialiseStrucMappingArray(int count)
        {
            remoteApi.InitialiseStrucMappingArray(count);
        }

        public int setStrucBridge(string id, int ibranch2, double dist, int icompound, double bottomlevel, double pillarwidth,
                                  double formfactor, int crosssectionnr, double length, double inletlosscoef, double outletlosscoef,
                                  int allowedFlowDir, int bedFrictionType, double bedFriction, double groundFriction)
        {
            return remoteApi.setStrucBridge(id, ibranch2, dist, icompound, bottomlevel, pillarwidth, formfactor, crosssectionnr, length, inletlosscoef, outletlosscoef, allowedFlowDir, bedFrictionType, bedFriction, groundFriction);
        }

        public int setStrucBridge(string id, int ibranch2, double dist, int icompound, double pillarwidth, double formfactor,
                                  int allowedFlowDir)
        {
            return remoteApi.setStrucBridge(id, ibranch2, dist, icompound, pillarwidth, formfactor, allowedFlowDir);
        }

        public int setStrucPump(string Id, int ibranch2, double dist, int icompound, int direction, int nrstages, int icapo,
                                double computed_capacity, double oldcapacity, double capacitySetpoint, bool isControlled,
                                double[] capacity, double[] onlevelup, double[] offlevelup, double[] onleveldown, double[] offleveldown,
                                int tabsize, int interpoltype, int storedcounter, double[] leveldiff, double[] redfact)
        {
            return remoteApi.setStrucPump(Id, ibranch2, dist, icompound, direction, nrstages, icapo, computed_capacity, oldcapacity, capacitySetpoint, isControlled, capacity, onlevelup, offlevelup, onleveldown, offleveldown, tabsize, interpoltype, storedcounter, leveldiff, redfact);
        }

        public int setStrucGeneralst(string Id, int ibranch2, double dist, int icompound, double widthleftW1, double levelleftZb1,
                                     double widthleftWsdl, double levelleftZbsl, double widthcenter, double levelcenter,
                                     double widthrightWsdr, double levelrightZbsr, double widthrightW2, double levelrightZb2,
                                     double gateheight, double pos_freegateflowcoef, double pos_drowngateflowcoef,
                                     double pos_freeweirflowcoef, double pos_drownweirflowcoef, double pos_contrcoeffreegate,
                                     double neg_freegateflowcoef, double neg_drowngateflowcoef, double neg_freeweirflowcoef,
                                     double neg_drownweirflowcoef, double neg_contrcoeffreegate, double extraresistance)
        {
            return remoteApi.setStrucGeneralst(Id, ibranch2, dist, icompound, widthleftW1, levelleftZb1, widthleftWsdl, levelleftZbsl, widthcenter, levelcenter, widthrightWsdr, levelrightZbsr, widthrightW2, levelrightZb2, gateheight, pos_freegateflowcoef, pos_drowngateflowcoef, pos_freeweirflowcoef, pos_drownweirflowcoef, pos_contrcoeffreegate, neg_freegateflowcoef, neg_drowngateflowcoef, neg_freeweirflowcoef, neg_drownweirflowcoef, neg_contrcoeffreegate, extraresistance);
        }

        public int setStrucUniWeir(string Id, int ibranch2, double dist, int icompound, double[] y, double[] z, double crestlevel, double dischargecoef,
                                   int allowedflowdir, double freesubmergedfactor)
        {
            return remoteApi.setStrucUniWeir(Id, ibranch2, dist, icompound, y, z, crestlevel, dischargecoef, allowedflowdir, freesubmergedfactor);
        }

        public int setStrucInvSiphon(string Id, int ibranch2, double dist, int icompound, double leftlevel, double rightlevel,
                                     int crosssectionnr, int allowedflowdir, double length, double inletlosscoef, double outletlosscoef,
                                     double bendlosscoef, int valve_onoff, double inivalveopen, int bedFrictionType, double bedFriction,
                                     double groundFriction, double[] relativeOpening, double[] lossCoefficient)
        {
            return remoteApi.setStrucInvSiphon(Id, ibranch2, dist, icompound, leftlevel, rightlevel, crosssectionnr, allowedflowdir, length, inletlosscoef, outletlosscoef, bendlosscoef, valve_onoff, inivalveopen, bedFrictionType, bedFriction, groundFriction, relativeOpening, lossCoefficient);
        }

        public int setStrucSiphon(string Id, int ibranch2, double dist, int icompound, double leftlevel, double rightlevel,
                                  int crosssectionnr, int allowedflowdir, double length, double inletlosscoef, double outletlosscoef,
                                  double bendlosscoef, int valve_onoff, double inivalveopen, int bedFrictionType, double bedFriction,
                                  double groundFriction, double[] relativeOpening, double[] lossCoefficient, double turnonlevel,
                                  double turnofflevel, int siphon_onoff)
        {
            return remoteApi.setStrucSiphon(Id, ibranch2, dist, icompound, leftlevel, rightlevel, crosssectionnr, allowedflowdir, length, inletlosscoef, outletlosscoef, bendlosscoef, valve_onoff, inivalveopen, bedFrictionType, bedFriction, groundFriction, relativeOpening, lossCoefficient, turnonlevel, turnofflevel, siphon_onoff);
        }

        public int setStrucCulvert(string Id, int ibranch2, double dist, int icompound, double leftlevel, double rightlevel,
                                   int crosssectionnr, int allowedflowdir, double length, double inletlosscoef, double outletlosscoef,
                                   int valve_onoff, double inivalveopen, int bedFrictionType, double bedFriction, double groundFriction,
                                   double[] relativeOpening, double[] lossCoefficient)
        {
            return remoteApi.setStrucCulvert(Id, ibranch2, dist, icompound, leftlevel, rightlevel, crosssectionnr, allowedflowdir, length, inletlosscoef, outletlosscoef, valve_onoff, inivalveopen, bedFrictionType, bedFriction, groundFriction, relativeOpening, lossCoefficient);
        }

        public int setStrucOrifice(string Id, int ibranch2, double dist, int icompound, double crestlevel, double crestwidth,
                                   double contrcoef, double latcontrcoef, int allowedflowdir, double openlevel, int uselimitflowpos,
                                   double limitflowpos, int uselimitflowneg, double limitflowneg)
        {
            return remoteApi.setStrucOrifice(Id, ibranch2, dist, icompound, crestlevel, crestwidth, contrcoef, latcontrcoef, allowedflowdir, openlevel, uselimitflowpos, limitflowpos, uselimitflowneg, limitflowneg);
        }

        public int setStrucAdvWeir(string Id, int ibranch2, double dist, int icompound, double crestlevel, double totwidth, int npiers,
                                   double pos_height, double pos_designhead, double pos_piercontractcoef, double pos_abutcontractcoef,
                                   double neg_height, double neg_designhead, double neg_piercontractcoef, double neg_abutcontractcoef)
        {
            return remoteApi.setStrucAdvWeir(Id, ibranch2, dist, icompound, crestlevel, totwidth, npiers, pos_height, pos_designhead, pos_piercontractcoef, pos_abutcontractcoef, neg_height, neg_designhead, neg_piercontractcoef, neg_abutcontractcoef);
        }

        public int setStrucRiverWeir(string Id, int ibranch2, double dist, int icompound, double crestlevel, double crestwidth,
                                     double pos_cwcoef, double pos_slimlimit, double[] pos_sf, double[] pos_red, double neg_cwcoef,
                                     double neg_slimlimit, double[] neg_sf, double[] neg_red)
        {
            return remoteApi.setStrucRiverWeir(Id, ibranch2, dist, icompound, crestlevel, crestwidth, pos_cwcoef, pos_slimlimit, pos_sf, pos_red, neg_cwcoef, neg_slimlimit, neg_sf, neg_red);
        }

        public int setStrucWeir(string Id, int ibranch2, double dist, int icompound, double crestlevel, double crestwidth,
                                double dischargecoef, double latdiscoef, int allowflowdir)
        {
            return remoteApi.setStrucWeir(Id, ibranch2, dist, icompound, crestlevel, crestwidth, dischargecoef, latdiscoef, allowflowdir);
        }

        public void WriteWaqOutput(WaqVolType volType)
        {
            remoteApi.WriteWaqOutput(volType);
        }

        public bool SetMissingValue(double missingValue)
        {
            return remoteApi.SetMissingValue(missingValue);
        }

        public double[] GetStatisticalOutput(ElementSet elementsetId, QuantityType quantityId, AggregationOptions operation,
                                             double outputTimeStep)
        {
            return remoteApi.GetStatisticalOutput(elementsetId, quantityId, operation, outputTimeStep);
        }

        public int SetStatisticalOutput(ElementSet elementsetId, QuantityType quantityId, AggregationOptions operation,
                                        double outputInterval)
        {
            return remoteApi.SetStatisticalOutput(elementsetId, quantityId, operation, outputInterval);
        }

        public void SetValues(QuantityType type, double[] values)
        {
            remoteApi.SetValues(type, values);
        }

        public int GetSize(ElementSet ielmSet)
        {
            return remoteApi.GetSize(ielmSet);
        }

        public double[] GetValues(QuantityType iquant, ElementSet ielmSet)
        {
            return remoteApi.GetValues(iquant, ielmSet);
        }

        public int NetworkAddStorage(string id)
        {
            return remoteApi.NetworkAddStorage(id);
        }

        public int NetworkSetBoundary(int nodeId, int interpolationType, BoundaryType type, double value, double returnTime)
        {
            return remoteApi.NetworkSetBoundary(nodeId, interpolationType, type, value, returnTime);
        }

        public int NetworkSetBoundary(int nodeId, int interpolationType, BoundaryType type, double value)
        {
            return remoteApi.NetworkSetBoundary(nodeId, interpolationType, type, value);
        }

        public int NetworkAddObservationPoint(string id, int branchId, double offset)
        {
            return remoteApi.NetworkAddObservationPoint(id, branchId, offset);
        }

        public int NetworkSetYZCrossSection(double[] y, double[] z, double[] frictionSectionFrom, double[] frictionSectionTo,
                                            int[] frictionTypePos, double[] frictionValuePos, int[] frictionTypeNeg,
                                            double[] frictionValueNeg, double[] storageLevels, double[] storage)
        {
            return remoteApi.NetworkSetYZCrossSection(y, z, frictionSectionFrom, frictionSectionTo, frictionTypePos, frictionValuePos, frictionTypeNeg, frictionValueNeg, storageLevels, storage);
        }

        public int NetworkSetTabCrossSection(double[] levels, double[] flowWidth, double[] totalWidth, double[] plains,
                                             double levelCrest, double levelBottom, double flowArea, double totalArea, bool closed,
                                             bool groundlayerUsed, double groundlayer)
        {
            return remoteApi.NetworkSetTabCrossSection(levels, flowWidth, totalWidth, plains, levelCrest, levelBottom, flowArea, totalArea, closed, groundlayerUsed, groundlayer);
        }

        public int NetworkSetTabCrossSection(double[] levels, double[] flowWidth, double[] totalWidth, double[] plains, bool closed,
                                             bool groundlayerUsed, double groundlayer)
        {
            return remoteApi.NetworkSetTabCrossSection(levels, flowWidth, totalWidth, plains, closed, groundlayerUsed, groundlayer);
        }

        public int ResetMessageCount()
        {
            return remoteApi.ResetMessageCount();
        }

        public int MessageCount()
        {
            return remoteApi.MessageCount();
        }

        public void LogMessages()
        {
            remoteApi.LogMessages();
        }

        public bool LoggingEnabled
        {
            get { return remoteApi.LoggingEnabled; }
            set { remoteApi.LoggingEnabled = value; }
        }

        #endregion
    }
}