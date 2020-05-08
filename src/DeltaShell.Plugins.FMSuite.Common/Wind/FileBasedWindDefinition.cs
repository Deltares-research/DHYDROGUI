using System.Collections.Generic;
using System.ComponentModel;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.FMSuite.Common.Wind
{
    [Entity(FireOnCollectionChange = false)]
    public class FileBasedWindDefinition
    {
        public enum FileBasedWindQuantity
        {
            [Description("X-component")]
            VelocityX,

            [Description("Y-component")]
            VelocityY,

            [Description("Velocity vector")]
            VelocityVector,

            [Description("Air pressure")]
            AirPressure,

            [Description("Wind vector and air pressure")]
            VelocityVectorAirPressure,

            [Description("Spider web grid")]
            SpiderWeb
        }

        public static readonly IDictionary<string, int> WindFileTypes = new Dictionary<string, int>
        {
            {UniformSeriesFilter, 1},
            {UniformXSeriesFilter, 1},
            {UniformYSeriesFilter, 1},
            {UniformXYSeriesFilter, 1},
            {UniformPolarSeriesFilter, 2},
            {XComponentArcInfoFilter, 4},
            {YComponentArcInfoFilter, 4},
            {PressureComponentArcInfoFilter, 4},
            {MeteoCurviGridFilter, 6},
            {SpiderWebFileFilter, 5}
        };

        public static readonly IDictionary<string, int> WindFileMethods = new Dictionary<string, int>
        {
            {UniformSeriesFilter, 1},
            {UniformXSeriesFilter, 1},
            {UniformYSeriesFilter, 1},
            {UniformXYSeriesFilter, 1},
            {UniformPolarSeriesFilter, 1},
            {XComponentArcInfoFilter, 2},
            {YComponentArcInfoFilter, 2},
            {PressureComponentArcInfoFilter, 2},
            {MeteoCurviGridFilter, 3},
            {SpiderWebFileFilter, 1}
        };

        public static readonly IDictionary<FileBasedWindQuantity, string[]> WindQuantityFileExtensions =
            new Dictionary<FileBasedWindQuantity, string[]>
            {
                {
                    FileBasedWindQuantity.VelocityX, new[]
                    {
                        UniformXSeriesFilter,
                        XComponentArcInfoFilter
                    }
                },
                {
                    FileBasedWindQuantity.VelocityY, new[]
                    {
                        UniformYSeriesFilter,
                        YComponentArcInfoFilter
                    }
                },
                {
                    FileBasedWindQuantity.VelocityVector, new[]
                    {
                        UniformXYSeriesFilter,
                        UniformPolarSeriesFilter
                    }
                },
                {
                    FileBasedWindQuantity.AirPressure, new[]
                    {
                        PressureComponentArcInfoFilter
                    }
                },
                {
                    FileBasedWindQuantity.VelocityVectorAirPressure, new[]
                    {
                        MeteoCurviGridFilter
                    }
                },
                {
                    FileBasedWindQuantity.SpiderWeb, new[]
                    {
                        SpiderWebFileFilter
                    }
                }
            };

        private const string UniformSeriesFilter = "uniform series (*.wnd)|*.wnd";
        private const string UniformXSeriesFilter = "uniform x series (*.wnd)|*.wnd";
        private const string UniformYSeriesFilter = "uniform y series (*.wnd)|*.wnd";
        private const string UniformXYSeriesFilter = "uniform xy series (*.wnd)|*.wnd";
        private const string UniformPolarSeriesFilter = "uniform polar series (*.wnd)|*.wnd";
        private const string XComponentArcInfoFilter = "x-component arcinfo (*.amu)|*.amu";
        private const string YComponentArcInfoFilter = "y-component arcinfo (*.amv)|*.amv";
        private const string PressureComponentArcInfoFilter = "pressure arcinfo (*.amp)|*.amp";
        private const string MeteoCurviGridFilter = "meteo curvilinear grid (*.apwxwy)|*.apwxwy";
        private const string SpiderWebFileFilter = "spider web wind file (*.spw)|*.spw";

        private WindDefinitionType type;

        public FileBasedWindDefinition()
        {
            WindFiles = new Dictionary<FileBasedWindQuantity, ExtForceValue>();
            Type = WindDefinitionType.WindXWindY;
        }

        public FileBasedWindDefinition(WindDefinitionType type)
        {
            WindFiles = new Dictionary<FileBasedWindQuantity, ExtForceValue>();
            Type = type;
        }

        public WindDefinitionType Type
        {
            get => type;
            set
            {
                string additionalSpiderWeb = AdditionalSpiderWeb;
                bool spiderWebAdded = CanRemoveSpiderWeb;

                WindFiles.Clear();

                type = value;

                foreach (FileBasedWindQuantity quantity in ExpectedQuantities[type])
                {
                    WindFiles.Add(quantity, new ExtForceValue());
                }

                if (spiderWebAdded && CanAddSpiderWeb)
                {
                    AddSpiderWeb(additionalSpiderWeb);
                }
            }
        }

        // Dummy property for events...
        public string WindFile { get; set; }

        public IDictionary<FileBasedWindQuantity, ExtForceValue> WindFiles { get; private set; }

        public bool CanAddSpiderWeb => Type != WindDefinitionType.SpiderWebGrid &&
                                       !WindFiles.ContainsKey(FileBasedWindQuantity.SpiderWeb);

        public bool CanRemoveSpiderWeb => Type != WindDefinitionType.SpiderWebGrid &&
                                          WindFiles.ContainsKey(FileBasedWindQuantity.SpiderWeb);

        public string AdditionalSpiderWeb
        {
            get
            {
                ExtForceValue result = null;
                if (Type != WindDefinitionType.SpiderWebGrid)
                {
                    WindFiles.TryGetValue(FileBasedWindQuantity.SpiderWeb, out result);
                }

                return result == null ? null : result.FilePathHandler.FilePath;
            }
        }

        public void AddQuantityKey(FileBasedWindQuantity quantity, string file, string fileFilter)
        {
            if (WindFileTypes.ContainsKey(fileFilter) && WindFileMethods.ContainsKey(fileFilter))
            {
                WindFiles[quantity] = new ExtForceValue
                {
                    FilePathHandler = FilePathHandler.Create(file),
                    FileType = WindFileTypes[fileFilter],
                    Method = WindFileMethods[fileFilter]
                };
                WindFile = file;
            }
        }

        public void AddSpiderWeb(string file = null)
        {
            if (CanAddSpiderWeb)
            {
                const string key = SpiderWebFileFilter;
                WindFiles.Add(FileBasedWindQuantity.SpiderWeb,
                              new ExtForceValue
                              {
                                  FilePathHandler = FilePathHandler.Create(file),
                                  FileType = WindFileTypes[key],
                                  Method = WindFileMethods[key]
                              });
                WindFile = file;
            }
        }

        public void AddSpiderWeb(string directory, string file)
        {
            if (CanAddSpiderWeb)
            {
                const string key = SpiderWebFileFilter;
                WindFiles.Add(FileBasedWindQuantity.SpiderWeb,
                              new ExtForceValue
                              {
                                  FilePathHandler = FilePathHandler.Create(directory, file),
                                  FileType = WindFileTypes[key],
                                  Method = WindFileMethods[key]
                              });
                WindFile = file;
            }
        }

        public void RemoveSpiderWeb()
        {
            if (Type == WindDefinitionType.SpiderWebGrid)
            {
                return;
            }

            FilePathHandler spiderWebFile = WindFiles[FileBasedWindQuantity.SpiderWeb].FilePathHandler;

            WindFiles.Remove(FileBasedWindQuantity.SpiderWeb);

            WindFile = spiderWebFile.FilePath;
        }

        private static IDictionary<WindDefinitionType, IList<FileBasedWindQuantity>> ExpectedQuantities { get; } =
            new Dictionary<WindDefinitionType, IList<FileBasedWindQuantity>>
            {
                {
                    WindDefinitionType.WindXWindY, new[]
                    {
                        FileBasedWindQuantity.VelocityX,
                        FileBasedWindQuantity.VelocityY,
                        FileBasedWindQuantity.AirPressure
                    }
                },
                {
                    WindDefinitionType.WindXY, new[]
                    {
                        FileBasedWindQuantity.VelocityVector,
                        FileBasedWindQuantity.AirPressure
                    }
                },
                {
                    WindDefinitionType.WindXYP, new[]
                    {
                        FileBasedWindQuantity.VelocityVectorAirPressure
                    }
                },
                {
                    WindDefinitionType.SpiderWebGrid, new[]
                    {
                        FileBasedWindQuantity.SpiderWeb
                    }
                }
            };

        public class ExtForceValue
        {
            public ExtForceValue()
            {
                FilePathHandler = new FilePathHandler();
                FileType = -1;
                Method = -1;
            }

            public FilePathHandler FilePathHandler { get; set; }

            public int FileType { get; set; }

            public int Method { get; set; }
        }
    }
}