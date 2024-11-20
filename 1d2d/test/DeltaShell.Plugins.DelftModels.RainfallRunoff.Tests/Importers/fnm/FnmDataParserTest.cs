using System;
using System.IO;
using System.Text;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Importers.fnm;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Importers.fnm
{
    [TestFixture]
    public class FnmDataParserTest
    {
        [Test]
        public void Parse_StreamNull_ThrowsArgumentNullException()
        {
            void Call() => FnmDataParser.Parse(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("stream"));
        }

        [Test]
        public void Parse_ValidStream_CorrectResults()
        {
            string content = @"'delft_3b.ini'                            * 1. Control file                                             I
'3b_nod.tp'                                      * 2. Knoop data                                               I
'3b_link.tp'                                     * 3. Tak data                                                      I
'3brunoff.tp'                                    * 4. Open water data                                               I
'paved.3b'                                       * 5. Verhard gebied algemeen                                       I
'paved.sto'                                      * 6. Verhard gebied storage                                        I
'paved.dwa'                                      * 7. Verhard gebied DWA                                            I
'paved.tbl'                                      * 8. Verhard gebied sewer pump capacity                            I
'pluvius.dwa'                                    * 9. Boundaries                                                    I
'pluvius.3b'                                     *10. Pluvius                                                       I
'pluvius.alg'                                    *11. Pluvius algemeen                                              I
'kasklass'                    *12. Kasklasse                                                     I
'T1.bui'                                        *13. buifile                                                  I
'T1.evp'                                       *14. verdampingsfile                                          I
'unpaved.3b'                                     *15. unpaved algemeen                                              I
'unpaved.sto'                                    *16. unpaved storage                                               I
'kasinit'                     *17. kasgebied initialisatie (SC)                                  I
'kasgebr'                     *18. kasgebied verbruiksdata (SC)                                  I
'cropfact'                    *19. crop factors gewassen                                         I
'bergcoef'                    *20. tabel bergingscoef=f(ontw.diepte,grondsoort)                  I
'unpaved.alf'                                    *21. Unpaved - alfa factor definities                                I
'sobek_3b.log'                            *22. Run messages                                                  O
'3b_gener.out'                            *23. Overzicht van schematisatie, algemene gegevens                O
'paved.out'                               *24. Output results verhard                                        O
'unpaved.out'                             *25. Output results onverhard                                      O
'grnhous.out'                             *26. Output results kas                                            O
'openwate.out'                            *27. Output results open water                                     O
'struct3b.out'                            *28. Output results kunstwerk                                      O
'bound3b.out'                             *29. Output results boundaries                                     O
'pluvius.out'                             *30. Output results Pluvius                                        O
'unpaved.inf'                                    *31. Unpaved infiltratie definities                                  I
'sobek_3b.dbg'                                    *32. Debugfile                                                     O
'unpaved.sep'                                    *33. Unpaved seepage                                                 I
'unpaved.tbl'                                    *34. Unpaved tabels initial gwl and Scurve                           I
'greenhse.3b'                                    *35. Kassen general data                                             I
'greenhse.rf'                                    *36. Kassen roof storage                                             I
'runoff.out'                                      *37. Pluvius rioolinloop ASCII file                                O
'sbk_rtc.his'                             *38. Invoerfile met variabele peilen op randknopen                 I
'salt.3b'                                 *39. Invoerfile met zoutgegevens                                   I
'crop_ow.prn'                 *40. Invoerfile met cropfactors open water                         I
'RSRR_IN'                                      *41. Restart file input                                            I
'RSRR_OUT'                                     *42. Restart file output                                           O
'3b_input.bin'                            *43. Binary file input                                             I
'sacrmnto.3b'                                    *44. Sacramento input I
'aanvoer.abr'                             *45. Uitvoer ASCII file met debieten van/naar randknopen           O
'saltbnd.out'                             *46. Uitvoer ASCII file met zoutconcentratie op rand               O
'salt.out'                                *47. Zout uitvoer in ASCII file                                    O
'greenhse.sil'                                   *48. Greenhouse silo definitions                                     I
'openwate.3b'                                    *49. Open water general data                                         I
'openwate.sep'                                   *50. Open water seepage definitions                                  I
'openwate.tbl'                                   *51. Open water tables target levels                                 I
'struct3b.dat'                                   *52. General structure data                                          I
'struct3b.def'                                   *53. Structure definitions                                           I
'contr3b.def'                                    *54. Controller definitions                                          I
'struct3b.tbl'                                   *55. Tabellen structures                                             I
'bound3b.3b'                                     *56. Boundary data                                                   I
'bound3b.tbl'                                    *57. Boundary tables                                                 I
'sbk_loc.rtc'                                    *58.                                                                 I
'wwtp.3b'                                        *59. Wwtp data                                                       I
'wwtp.tbl'                                       *60. Wwtp tabellen                                                   I
'industry.3b'                                    *61. Industry general data                                           I
'pvstordt.his'                            *62. Mappix output file detail berging riool verhard gebied per tijdstap  O
'pvflowdt.his'                            *63. Mappix output file detail debiet verhard gebied        per tijdstap  O
'upflowdt.his'                            *64. Mappix output file detail debiet onverhard gebied      per tijdstap  O
'upgwlvdt.his'                            *65. Mappix output file detail grondwaterstand              per tijdstap  O
'grnstrdt.his'                            *66. Mappix output file detail bergingsgraad kasbassins     per tijdstap  O
'grnflodt.his'                            *67. Mappix output file detail uitslag kasbassins           per tijdstap  O
'ow_lvldt.his'                            *68. Mappix output file detail open water peil              per tijdstap  O
'ow_excdt.his'                            *69  Mappix output file detail overschrijdingsduur ref.peil per tijdstap  O
'strflodt.his'                            *70. Mappix output file detail debiet over kunstwerk        per tijdstap  O
'bndflodt.his'                            *71. Mappix output file detail debiet naar rand             per tijdstap  O
'plvstrdt.his'                            *72. Mappix output file max.berging riool Pluvius           per tijdstap  O
'plvflodt.his'                            *73. Mappix output file max.debiet Pluvius                  per tijdstap  O
'balansdt.his'                            *74. Mappix output file detail balans                       per tijdstap  O
'cumbaldt.his'                            *75. Mappix output file detail balans cumulatief            per tijdstap  O
'saltdt.his'                            *76. Mappix output file detail zoutconcentraties            per tijdstap  O
'industry.tbl'                                   *77. Industry tabellen                                               I
'rtc_3b.his'                              *78. Maalstop                                                 I
'default.tmp'                                         *79. Temperature time series I
'rnff.#'                                         *80. Runoff time series
'bndfltot.his'                            *81. Totalen/lozingen op randknopen                           O
'sobek_3b.lng'           *82. Language file                                                   I
'ow_vol.his'                              *83. OW-volume                                                O
'ow_level.his'                            *84. OW_peilen                                                O
'3b_bal.out'                              *85. Balans file                                              O
'3bareas.his'                             *86. 3B-arealen in HIS file                                   O
'3bstrdim.his'                            *87. 3B-structure data in HIS file                            O
'rrrunoff.his'                            *88. RR Runoff his file
'sacrmnto.his'                            *89. Sacramento HIS file
'wwtpdt.his'                              *90. rwzi HIS file                                            O
'industdt.his'                            *91. Industry HIS file                                        O
'ctrl.ini'                                        *92. CTRL.INI                                                 I
'root_sim.inp'                *93. CAPSIM input file                                I
'unsa_sim.inp'                *94. CAPSIM input file                                I
'capsim.msg'                                      *95. CAPSIM message file                                      O
'capsim.dbg'                                      *96. CAPSIM debug file                                        O
'restart1.out'                                    *97. Restart file na 1 uur                                    O
'restart12.out'                                   *98. Restart file na 12 uur                                   O
'RR-ready'                                        *99. Ready                                                    O
'NwrwArea.His'                            *100. NWRW detailed areas                                     O
'3blinks.his'                             *101. Link flows                                              O
'modflow_rr.His'                          *102. Modflow-RR                                              O
'rr_modflow.His'                          *103. RR-Modflow                                              O
'rr_wlmbal.His'                           *104. RR-balance for WLM
'sacrmnto.out'                            *105. Sacramento ASCII output
'pluvius.tbl'                                    *106. Additional NWRW input file with DWA table                      I
'rrbalans.his'                            *107. RR balans
'KasKlasData.dat'             *108. Kasklasse, new format I
'KasInitData.dat'             *109. KasInit, new format I
'KasGebrData.dat'             *110. KasGebr, new format I
'CropData.dat'                *111. CropFact, new format I
'CropOWData.dat'              *112. CropOW, new format I
'SoilData.dat'                *113. Soildata, new format I
'dioconfig.ini'             *114. DioConfig Ini file
'NWRWCONT.#'                                     *115. Buifile voor continue berekening Reeksen
'NwrwSys.His'                                    *116. NWRW output
'3b_rout.3b'                                     *117. RR Routing link definitions I
'3b_cel.3b'                                    *118. Cel input file
'3b_cel.his'                              *119. Cel output file
'sobek3b_progress.txt'                            *120. RR Log file for Simulate
'wqrtc.his'                               *121. coupling WQ salt RTC
'BoundaryConditions.bc'                           *122. RR Boundary conditions file for SOBEK3
'ASCIIRestartOpenDA.txt'                          *123. Optional RR ASCII restart (test) for OpenDA";

            byte[] byteArray = Encoding.ASCII.GetBytes(content);
            using (var stream = new MemoryStream(byteArray))
            using (var streamReader = new StreamReader(stream))
            {
                FnmData data = FnmDataParser.Parse(streamReader);


                Assert.That(data.ControlFile, Is.EqualTo("delft_3b.ini"));
                Assert.That(data.NodeData, Is.EqualTo("3b_nod.tp"));
                Assert.That(data.LinkData, Is.EqualTo("3b_link.tp"));
                Assert.That(data.OpenWaterData, Is.EqualTo("3brunoff.tp"));
                Assert.That(data.PavedAreaGeneral, Is.EqualTo("paved.3b"));
                Assert.That(data.PavedAreaStorage, Is.EqualTo("paved.sto"));
                Assert.That(data.PavedAreaDwa, Is.EqualTo("paved.dwa"));
                Assert.That(data.PavedAreaSewerPumpCapacity, Is.EqualTo("paved.tbl"));
                Assert.That(data.Boundaries, Is.EqualTo("pluvius.dwa"));
                Assert.That(data.Pluvius, Is.EqualTo("pluvius.3b"));
                Assert.That(data.PluviusGeneral, Is.EqualTo("pluvius.alg"));
                Assert.That(data.Kasklasse, Is.EqualTo("kasklass"));
                Assert.That(data.BuiFile, Is.EqualTo("T1.bui"));
                Assert.That(data.VerdampingsFile, Is.EqualTo("T1.evp"));
                Assert.That(data.UnpavedAreaGeneral, Is.EqualTo("unpaved.3b"));
                Assert.That(data.UnpavedAreaStorage, Is.EqualTo("unpaved.sto"));
                Assert.That(data.GreenHouseAreaInitialisation, Is.EqualTo("kasinit"));
                Assert.That(data.GreenHouseAreaUsageData, Is.EqualTo("kasgebr"));
                Assert.That(data.CropFactors, Is.EqualTo("cropfact"));
                Assert.That(data.TableBergingscoef, Is.EqualTo("bergcoef"));
                Assert.That(data.UnpavedAlphaFactorDefinitions, Is.EqualTo("unpaved.alf"));
                Assert.That(data.RunLogFile, Is.EqualTo("sobek_3b.log"));
                Assert.That(data.SchemaOverview, Is.EqualTo("3b_gener.out"));
                Assert.That(data.OutputResultsPavedArea, Is.EqualTo("paved.out"));
                Assert.That(data.OutputResultsUnpavedArea, Is.EqualTo("unpaved.out"));
                Assert.That(data.OutputResultsGreenHouses, Is.EqualTo("grnhous.out"));
                Assert.That(data.OutputResultsOpenWater, Is.EqualTo("openwate.out"));
                Assert.That(data.OutputResultsStructures, Is.EqualTo("struct3b.out"));
                Assert.That(data.OutputResultsBoundaries, Is.EqualTo("bound3b.out"));
                Assert.That(data.OutputResultsPluvius, Is.EqualTo("pluvius.out"));
                Assert.That(data.InfiltrationDefinitionsUnpaved, Is.EqualTo("unpaved.inf"));
                Assert.That(data.RunDebugFile, Is.EqualTo("sobek_3b.dbg"));
                Assert.That(data.UnpavedSeepage, Is.EqualTo("unpaved.sep"));
                Assert.That(data.UnpavedInitialValuesTables, Is.EqualTo("unpaved.tbl"));
                Assert.That(data.GreenHouseGeneral, Is.EqualTo("greenhse.3b"));
                Assert.That(data.GreenHouseRoofStorage, Is.EqualTo("greenhse.rf"));
                Assert.That(data.PluviusSewageEntry, Is.EqualTo("runoff.out"));
                Assert.That(data.InputVariableGaugesOnEdgeNodes, Is.EqualTo("sbk_rtc.his"));
                Assert.That(data.InputSaltData, Is.EqualTo("salt.3b"));
                Assert.That(data.InputCropFactorsOpenWater, Is.EqualTo("crop_ow.prn"));
                Assert.That(data.RestartFileInput, Is.EqualTo("RSRR_IN"));
                Assert.That(data.RestartFileOutput, Is.EqualTo("RSRR_OUT"));
                Assert.That(data.BinaryFileInput, Is.EqualTo("3b_input.bin"));
                Assert.That(data.SacramentoInput, Is.EqualTo("sacrmnto.3b"));
                Assert.That(data.OutputFlowRatesEdgeNodes, Is.EqualTo("aanvoer.abr"));
                Assert.That(data.OutputSaltConcentrationEdge, Is.EqualTo("saltbnd.out"));
                Assert.That(data.OutputSaltExportation, Is.EqualTo("salt.out"));
                Assert.That(data.GreenhouseSiloDefinitions, Is.EqualTo("greenhse.sil"));
                Assert.That(data.OpenWaterGeneral, Is.EqualTo("openwate.3b"));
                Assert.That(data.OpenWaterSeepageDefinitions, Is.EqualTo("openwate.sep"));
                Assert.That(data.OpenWaterTablesTargetLevels, Is.EqualTo("openwate.tbl"));
                Assert.That(data.StructureGeneral, Is.EqualTo("struct3b.dat"));
                Assert.That(data.StructureDefinitions, Is.EqualTo("struct3b.def"));
                Assert.That(data.ControllerDefinitions, Is.EqualTo("contr3b.def"));
                Assert.That(data.StructureTables, Is.EqualTo("struct3b.tbl"));
                Assert.That(data.BoundaryData, Is.EqualTo("bound3b.3b"));
                Assert.That(data.BoundaryTables, Is.EqualTo("bound3b.tbl"));
                Assert.That(data.SobekLocationRtc, Is.EqualTo("sbk_loc.rtc"));
                Assert.That(data.WwtpData, Is.EqualTo("wwtp.3b"));
                Assert.That(data.WwtpTables, Is.EqualTo("wwtp.tbl"));
                Assert.That(data.IndustryGeneral, Is.EqualTo("industry.3b"));
                Assert.That(data.MappixPavedAreaSewageStorageName, Is.EqualTo("pvstordt.his"));
                Assert.That(data.MappixPavedAreaFlowRatesName, Is.EqualTo("pvflowdt.his"));
                Assert.That(data.MappixUnpavedAreaFlowRatesName, Is.EqualTo("upflowdt.his"));
                Assert.That(data.MappixGroundWaterLevelsName, Is.EqualTo("upgwlvdt.his"));
                Assert.That(data.MappixGreenHouseBassinsStorageName, Is.EqualTo("grnstrdt.his"));
                Assert.That(data.MappixGreenHouseBassinsResultsName, Is.EqualTo("grnflodt.his"));
                Assert.That(data.MappixOpenWaterDetailsName, Is.EqualTo("ow_lvldt.his"));
                Assert.That(data.MappixExceedanceTimeReferenceLevelsName, Is.EqualTo("ow_excdt.his"));
                Assert.That(data.MappixFlowRatesOverStructuresName, Is.EqualTo("strflodt.his"));
                Assert.That(data.MappixFlowRatesToEdgeName, Is.EqualTo("bndflodt.his"));
                Assert.That(data.MappixPluviusMaxSewageStorageName, Is.EqualTo("plvstrdt.his"));
                Assert.That(data.MappixPluviusMaxFlowRatesName, Is.EqualTo("plvflodt.his"));
                Assert.That(data.MappixBalanceName, Is.EqualTo("balansdt.his"));
                Assert.That(data.MappixCumulativeBalanceName, Is.EqualTo("cumbaldt.his"));
                Assert.That(data.MappixSaltConcentrationsName, Is.EqualTo("saltdt.his"));
                Assert.That(data.IndustryTables, Is.EqualTo("industry.tbl"));
                Assert.That(data.Maalstop, Is.EqualTo("rtc_3b.his"));
                Assert.That(data.TimeSeriesTemperature, Is.EqualTo("default.tmp"));
                Assert.That(data.TimeSeriesRunoff, Is.EqualTo("rnff.#"));
                Assert.That(data.DischargesTotalsAtEdgeNodes, Is.EqualTo("bndfltot.his"));
                Assert.That(data.LanguageFile, Is.EqualTo("sobek_3b.lng"));
                Assert.That(data.OwVolume, Is.EqualTo("ow_vol.his"));
                Assert.That(data.OwLevels, Is.EqualTo("ow_level.his"));
                Assert.That(data.BalanceFile, Is.EqualTo("3b_bal.out"));
                Assert.That(data.His3BAreaLength, Is.EqualTo("3bareas.his"));
                Assert.That(data.His3BStructureData, Is.EqualTo("3bstrdim.his"));
                Assert.That(data.HisRRRunoff, Is.EqualTo("rrrunoff.his"));
                Assert.That(data.HisSacramento, Is.EqualTo("sacrmnto.his"));
                Assert.That(data.HisRwzi, Is.EqualTo("wwtpdt.his"));
                Assert.That(data.HisIndustry, Is.EqualTo("industdt.his"));
                Assert.That(data.CtrlIni, Is.EqualTo("ctrl.ini"));
                Assert.That(data.RootCapsimInputFile, Is.EqualTo("root_sim.inp"));
                Assert.That(data.UnsaCapsimInputFile, Is.EqualTo("unsa_sim.inp"));
                Assert.That(data.CapsimMessageFile, Is.EqualTo("capsim.msg"));
                Assert.That(data.CapsimDebugFile, Is.EqualTo("capsim.dbg"));
                Assert.That(data.Restart1Hour, Is.EqualTo("restart1.out"));
                Assert.That(data.Restart12Hours, Is.EqualTo("restart12.out"));
                Assert.That(data.RRReady, Is.EqualTo("RR-ready"));
                Assert.That(data.NwrwAreas, Is.EqualTo("NwrwArea.His"));
                Assert.That(data.LinkFlows, Is.EqualTo("3blinks.his"));
                Assert.That(data.ModflowRR, Is.EqualTo("modflow_rr.His"));
                Assert.That(data.RRModflow, Is.EqualTo("rr_modflow.His"));
                Assert.That(data.RRWlmBalance, Is.EqualTo("rr_wlmbal.His"));
                Assert.That(data.SacramentoAsciiOutput, Is.EqualTo("sacrmnto.out"));
                Assert.That(data.NwrwInputDwaTable, Is.EqualTo("pluvius.tbl"));
                Assert.That(data.RRBalance, Is.EqualTo("rrbalans.his"));
                Assert.That(data.GreenHouseClasses, Is.EqualTo("KasKlasData.dat"));
                Assert.That(data.GreenHouseInit, Is.EqualTo("KasInitData.dat"));
                Assert.That(data.GreenHouseUsage, Is.EqualTo("KasGebrData.dat"));
                Assert.That(data.CropFactor, Is.EqualTo("CropData.dat"));
                Assert.That(data.CropOw, Is.EqualTo("CropOWData.dat"));
                Assert.That(data.SoilData, Is.EqualTo("SoilData.dat"));
                Assert.That(data.DioConfigIniFile, Is.EqualTo("dioconfig.ini"));
                Assert.That(data.BuiFileForContinuousCalculationSeries, Is.EqualTo("NWRWCONT.#"));
                Assert.That(data.NwrwOutput, Is.EqualTo("NwrwSys.His"));
                Assert.That(data.RRRoutingLinkDefinitions, Is.EqualTo("3b_rout.3b"));
                Assert.That(data.CellInputFile, Is.EqualTo("3b_cel.3b"));
                Assert.That(data.CellOutputFile, Is.EqualTo("3b_cel.his"));
                Assert.That(data.RRSimulateLogFile, Is.EqualTo("sobek3b_progress.txt"));
                Assert.That(data.RtcCouplingWqSalt, Is.EqualTo("wqrtc.his"));
                Assert.That(data.RRBoundaryConditionsSobek3, Is.EqualTo("BoundaryConditions.bc"));
                Assert.That(data.RRAsciiRestartOpenda, Is.EqualTo("ASCIIRestartOpenDA.txt"));
            }
        }
    }
}