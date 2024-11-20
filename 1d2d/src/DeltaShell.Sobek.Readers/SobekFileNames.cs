namespace DeltaShell.Sobek.Readers
{
    public class SobekFileNames
    {
        public SobekType SobekType { get; set; }

        public string SobekNetworkFileName { get { return SobekType == SobekType.Sobek212 ? "NETWORK.TP" : "DEFTOP.1"; } }
        public string SobekNetworkGridFileName { get { return SobekType == SobekType.Sobek212 ? "NETWORK.GR" : "DEFGRD.1"; } }
        public string SobekNetworkBrancheGeometryFileName { get { return SobekType == SobekType.Sobek212 ? "NETWORK.CP" : "DEFTOP.2"; } }
        public string SobekNetworkStructuresFileName { get { return SobekType == SobekType.Sobek212 ? "NETWORK.ST" : "DEFSTR.1"; } }
        public string SobekStructureDefinitionFileName { get { return SobekType == SobekType.Sobek212 ? "STRUCT.DEF" : "DEFSTR.3"; } }
        public string SobekStructureDatFileName { get { return SobekType == SobekType.Sobek212 ? "STRUCT.DAT" : "DEFSTR.2"; } }
        public string SobekCompoundStructureFileName { get { return SobekType == SobekType.Sobek212 ? "STRUCT.CMP" : "DEFSTR.7"; } }
        public string SobekBoundaryConditionsLocationsFileName { get { return SobekType == SobekType.Sobek212 ? "NETWORK.CN" : "DEFCND.1"; } }
        public string SobekFrictionFileName { get { return SobekType == SobekType.Sobek212 ? "FRICTION.DAT" : "DEFFRC.1"; } }
        public string SobekExtraFrictionFileName { get { return SobekType == SobekType.Sobek212 ? "FRICTION.DAT" : "DEFFRC.3"; } }
        public string SobekGlobalFrictionFileName { get { return SobekType == SobekType.Sobek212 ? "" : "DEFFRC.4"; } }
        public string SobekValveDataFileName { get { return SobekType == SobekType.Sobek212 ? "VALVE.TAB" : null; } }
        public string SobekProfileDefinitionsFileName { get { return SobekType == SobekType.Sobek212 ? "PROFILE.DEF" : "DEFCRS.2"; } }
        public string SobekProfileDataFileName { get { return SobekType == SobekType.Sobek212 ? "PROFILE.DAT" : "DEFCRS.3"; } }
        public string SobekNetworkLocationsFileName { get { return SobekType == SobekType.Sobek212 ? "NETWORK.CR" : "DEFCRS.1"; } }
        public string SobekMeasurementStationsFileName { get { return SobekType == SobekType.Sobek212 ? "NETWORK.ME" : "DEFSTR.4|DEFSTR.5"; } }
        public string SobekNodeFileName { get { return SobekType == SobekType.Sobek212 ? "NODES.DAT" : "DEFCND.3"; } }
        public string SobekObjectTypeFileName { get { return SobekType == SobekType.Sobek212 ? "NETWORK.OBI" : ""; } }

        public string SobekTopolgyFileName { get { return SobekType == SobekType.Sobek212 ? "NETWORK.TP" : "DEFTOP.1"; } }

        public string SobekBoundaryFileName { get { return SobekType == SobekType.Sobek212 ? "BOUNDARY.DAT" : "DEFCND.2"; } }
        public string SobekLaterSourcesFileName { get { return SobekType == SobekType.Sobek212 ? "LATERAL.DAT" : "DEFCND.3"; } }
        public string SobekSaltNodeBoundaryFileName { get { return SobekType == SobekType.Sobek212 ? "BOUNDARY.SAL" : "DEFCND.6"; } }
        public string SobekSaltLateralBoundaryFileName { get { return SobekType == SobekType.Sobek212 ? "LATERAL.SAL" : "DEFCND.7"; } }

        public string SobekCaseSettingFileName { get { return SobekType == SobekType.Sobek212 ? "SETTINGS.DAT" : "PARSEN.INI"; } }
        public string SobekInitialConditionsFileName { get { return SobekType == SobekType.Sobek212 ? "INITIAL.DAT" : "DEFICN.2"; } }
        public string SobekSaltInitialConditionsFileName { get { return SobekType == SobekType.Sobek212 ? "INITIAL.SAL" : "DEFICN.4"; } }
        public string SobekCaseDescriptionFileName { get { return "CASEDESC.CMT"; } }

        public string SobekSaltGlobalDispersionFileName { get { return SobekType == SobekType.Sobek212 ? "GLOBDISP.DAT" : "DEFDIS.1"; } }
        public string SobekSaltLocalDispersionFileName { get { return SobekType == SobekType.Sobek212 ? "LOKDISP.DAT" : "DEFDIS.2"; } }
        public string SobekRetentionLateralsFileName { get { return SobekType == SobekType.Sobek212 ? "LATERAL.DAT" : "DEFCND.3"; } }

        public string SobekStructuresFileName { get { return SobekType == SobekType.Sobek212 ? "STRUCT.DAT" : "DEFSTR.2"; } }
        public string SobekTriggersFileName { get { return SobekType == SobekType.Sobek212 ? "TRIGGER.DEF" : "DEFSTR.5"; } }
        public string SobekControllersFileName { get { return SobekType == SobekType.Sobek212 ? "CONTROL.DEF" : "DEFSTR.4"; } }

        public string SobekNetworkNetterFileName { get { return SobekType == SobekType.Sobek212 ? "NETWORK.NTW" : ""; } }
        public string SobekUserDefinedTypesFileName { get { return SobekType == SobekType.Sobek212 ? "NTRPLUV.OBJ" : ""; } }

        public string SobekRRIniFileName { get { return SobekType == SobekType.Sobek212 ? "DELFT_3B.INI" : ""; } }
        public string SobekRRNodeFileName { get { return SobekType == SobekType.Sobek212 ? "3B_NOD.TP" : ""; } }
        public string SobekRRLinkFileName { get { return SobekType == SobekType.Sobek212 ? "3B_LINK.TP" : ""; } }
        public string SobekRRPavedFileName { get { return SobekType == SobekType.Sobek212 ? "PAVED.3B" : ""; } }
        public string SobekRRUnpavedFileName { get { return SobekType == SobekType.Sobek212 ? "UNPAVED.3B" : ""; } }
        public string SobekRRRunoffNodesFileName { get { return SobekType == SobekType.Sobek212 ? "3BRUNOFF.TP" : ""; } }

        public string SobekRRNwrwFileName { get { return SobekType == SobekType.Sobek212 ? "PLUVIUS.3B" : ""; } }
        public string SobekRRNwrwSettingsFileName { get { return SobekType == SobekType.Sobek212 ? "PLUVIUS.ALG" : ""; } }
        public string SobekRRNwrwDwaFileName { get { return SobekType == SobekType.Sobek212 ? "PLUVIUS.DWA" : ""; } }
        public string SobekRRNwrwTableFileName { get { return SobekType == SobekType.Sobek212 ? "PLUVIUS.TBL" : ""; } }

        public string SobekRRGreenhouseFileName { get { return SobekType == SobekType.Sobek212 ? "GREENHSE.3B" : ""; } }
        public string SobekRRWasteWaterTreatmentPlantFileName { get { return SobekType == SobekType.Sobek212 ? "WWTP.3B" : ""; } }
        public string SobekRROpenWaterFileName { get { return SobekType == SobekType.Sobek212 ? "OPENWATE.3B" : ""; } }
        public string SobekRRSacramentoFileName { get { return SobekType == SobekType.Sobek212 ? "SACRMNTO.3B" : ""; } }

        public string SobekCaseDescriptionFile { get { return SobekType == SobekType.Sobek212 ? "CASEDESC.CMT" : ""; } }
    }
}
