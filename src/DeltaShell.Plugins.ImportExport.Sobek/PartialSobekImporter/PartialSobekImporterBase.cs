using System;
using System.IO;
using System.Linq;
using System.Reflection;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Sobek.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    public abstract class PartialSobekImporterBase : IPartialSobekImporter
    {
        private string pathSobek;
        private bool isActive = true;
        private bool isVisible = true;

        protected PartialSobekImporterBase()
        {
            SobekFileNames = new SobekFileNames();
            CaseData = new SobekCaseData(Enumerable.Empty<string>());
        }

        public abstract string DisplayName { get; }

        public abstract SobekImporterCategories Category { get; }

        public string PathSobek
        {
            get
            {
                return pathSobek;
            }
            set
            {
                pathSobek = value;

                if (string.IsNullOrEmpty(pathSobek))
                {
                    throw new ArgumentNullException("Path of Sobek");
                }
                
                try
                {
                    SobekFileNames.SobekType = DeltaShell.Sobek.Readers.SobekReaderHelper.GetSobekType(pathSobek);
                    SobekType = DeltaShell.Sobek.Readers.SobekReaderHelper.GetSobekType(pathSobek);
                }
                catch (Exception e)
                {
                    SobekFileNames.SobekType = SobekType.Sobek212;
                    SobekType = SobekType.Sobek212;
                    //gulp   
                }
                
                BaseDir = Path.GetDirectoryName(pathSobek);

                if (BaseDir == null || !Directory.Exists(BaseDir))
                {
                    throw new DirectoryNotFoundException(string.Format("The path {0} doesn't exist", BaseDir));
                }
            }
        }

        public object TargetObject
        {
            get => targetObject;
            set
            {
                targetObject = value;
                if (PartialSobekImporter != null && (targetObject == null || !targetObject.Equals(PartialSobekImporter.TargetObject)))
                {
                    PartialSobekImporter.TargetObject = targetObject;
                }
            }
        }

        public IPartialSobekImporter PartialSobekImporter { get; set; }

        public void Import()
        {
            if (BeforeImport != null)
            {
                BeforeImport(this);
            }

            if (ShouldCancel)
            {
                return;
            }

            if (PartialSobekImporter != null)
            {
                PartialSobekImporter.Import();
            }

            if (ShouldCancel)
            {
                return;
            }

            if (isActive)
            {
                PartialImport();
            }

            if (AfterImport != null)
            {
                AfterImport(this);
            }
        }

        public bool IsActive
        {
            get { return isActive; }
            set { isActive = value; }
        }

        public bool IsVisible
        {
            get { return isVisible; }
            set { isVisible = value; }
        }

        private bool shouldCancel;

        public bool ShouldCancel
        {
            get { return shouldCancel; }
            set
            {
                shouldCancel = value;

                if (PartialSobekImporter != null)
                {
                    PartialSobekImporter.ShouldCancel = true;
                }
            }
        }

        public Action<IPartialSobekImporter> BeforeImport { get; set; }

        public Action<IPartialSobekImporter> AfterImport { get; set; }

        protected DeltaShell.Sobek.Readers.SobekFileNames SobekFileNames { get; private set; }

        protected DeltaShell.Sobek.Readers.SobekType SobekType { get; set; }

        private IHydroNetwork hydroNetwork; // cache hydro network
        private object targetObject;

        protected IHydroNetwork HydroNetwork
        {
            get
            {
                if (hydroNetwork != null)
                {
                    return hydroNetwork;
                }

                if (TargetObject == null)
                {
                    throw new ArgumentException("To object has not been set.");
                }

                if (TargetObject is IHydroNetwork)
                {
                    hydroNetwork = (IHydroNetwork)TargetObject;
                    return hydroNetwork;
                }

                var hydroRegion = TargetObject as HydroRegion;
                if (hydroRegion != null)
                {
                    var network = hydroRegion.SubRegions.OfType<IHydroNetwork>().FirstOrDefault();
                    if (network != null)
                    {
                        hydroNetwork = network;
                        return hydroNetwork;
                    }
                }

                hydroNetwork = GetNetworkFromModels();
                return hydroNetwork;
            }
        }

        protected T TryGetModel<T>() where T : class, IModel
        {
            if (TargetObject == null)
            {
                throw new ArgumentException("To object has not been set.");
            }

            if (TargetObject is T)
            {
                return (T)TargetObject;
            }

            if (TargetObject is ICompositeActivity)
            {
                var returnModel = ((ICompositeActivity)TargetObject).Activities.OfType<T>().FirstOrDefault();

                if (returnModel != null)
                {
                    return returnModel;
                }

                //could be a recursive method, but just 2 layers...
                foreach (var compositeModel in ((ICompositeActivity)TargetObject).Activities.OfType<ICompositeActivity>())
                {
                    returnModel = compositeModel.Activities.OfType<T>().FirstOrDefault();
                    if (returnModel != null)
                    {
                        return returnModel;
                    }
                }

            }
            if (TargetObject is Project)
            {
                return ((Project)TargetObject).RootFolder.GetAllModelsRecursive().OfType<T>().FirstOrDefault();
            }
            return null;
        }

        protected T GetModel<T>() where T : class, IModel
        {
            var model = TryGetModel<T>();
            if (model == null)
            {
                throw new ArgumentException("To object does not have a " + typeof(T));
            }
            return model;
        }

        private IHydroNetwork GetNetworkFromModels()
        {
            if (TargetObject == null)
            {
                return null;
            }

            switch (TargetObject)
            {
                case HydroModel hydroModel:
                {
                    var network = hydroModel.Region.SubRegions.OfType<HydroNetwork>().FirstOrDefault();
                    if (network != null)
                    {
                        return network;
                    }

                    break;
                }
                case WaterFlowFMModel waterFlowFmModel:
                {
                    var network = waterFlowFmModel.Network;
                    if (network != null)
                    {
                        return network;
                    }

                    break;
                }
                case RainfallRunoffModel rainfallRunoffModel when rainfallRunoffModel.Owner is HydroModel integratedModel:
                {
                    var fmModel = integratedModel.Models.OfType<WaterFlowFMModel>().FirstOrDefault();
                    var network = fmModel?.Network;
                    if (network != null)
                    {
                        return network;
                    }

                    break;
                }

                case ICompositeActivity compositeActivity:
                {
                    foreach (var model in compositeActivity.Activities.OfType<IModel>())
                    {
                        var network = GetNetworkOfModel(model);
                        if (network != null)
                        {
                            return network;
                        }
                    }

                    //could be a recursive method, but just 2 layers...
                    foreach (var compositeModel in compositeActivity.Activities.OfType<ICompositeActivity>())
                    {
                        foreach (var model in compositeModel.Activities.OfType<IModel>())
                        {
                            var network = GetNetworkOfModel(model);
                            if (network != null)
                            {
                                return network;
                            }
                        }
                    }

                    break;
                }
                case IModel targetModel:
                    return GetNetworkOfModel(targetModel);

                case Project project:
                {
                    foreach (var model in project.RootFolder.GetAllModelsRecursive())
                    {
                        var network = GetNetworkOfModel(model);
                        if (network != null)
                        {
                            return network;
                        }
                    }

                    break;
                }
            }
            return null;
        }

        private IHydroNetwork GetNetworkOfModel(IModel model)
        {
            Type modelType = model.GetType();

            PropertyInfo[] propertyInfos = modelType.GetProperties();
            var numberPropertyInfo = propertyInfos.FirstOrDefault(pi => pi.Name == "Network");
            if (numberPropertyInfo != null)
            {
                return (IHydroNetwork)numberPropertyInfo.GetValue(model, null);
            }

            return null;
        }

        protected abstract void PartialImport();

        protected static void ThrowWhenFileNotExist(string fileName)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException(string.Format("The file {0} doesn't exist", fileName));
        }

        protected string GetFilePath(string fileName)
        {
            return Path.Combine(BaseDir, fileName);
        }

        protected IDrainageBasin DrainageBasin
        {
            get
            {
                if (TargetObject == null)
                {
                    throw new ArgumentException("To object has not been set.");
                }

                if (TargetObject is IDrainageBasin drainageBasin)
                {
                    return drainageBasin;
                }

                var hydroRegion = TargetObject as IHydroRegion;
                var basin = hydroRegion?.SubRegions.OfType<IDrainageBasin>().FirstOrDefault();
                if (basin != null)
                {
                    return basin;
                }

                return GetDrainageBasinFromModels();
            }
        }

        private IDrainageBasin GetDrainageBasinFromModels()
        {
            if (TargetObject == null)
            {
                return null;
            }

            if (TargetObject is HydroModel)
            {
                var hydroModel = TargetObject as HydroModel;
                var basin = hydroModel.Region.SubRegions.OfType<IDrainageBasin>().FirstOrDefault();
                if (basin != null)
                {
                    return basin;
                }
            }

            if (TargetObject is ICompositeActivity)
            {
                foreach (var model in ((ICompositeActivity)TargetObject).Activities.OfType<IModel>())
                {
                    var basin = GetDrainageBasinOfModel(model);
                    if (basin != null)
                    {
                        return basin;
                    }
                }

                //could be a recursive method, but just 2 layers...
                foreach (var compositeModel in ((ICompositeActivity)TargetObject).Activities.OfType<ICompositeActivity>())
                {
                    foreach (var model in compositeModel.Activities.OfType<IModel>())
                    {
                        var basin = GetDrainageBasinOfModel(model);
                        if (basin != null)
                        {
                            return basin;
                        }
                    }
                }

            }

            if (TargetObject is IModel)
            {
                return GetDrainageBasinOfModel((IModel)TargetObject);
            }

            return null;
        }

        private IDrainageBasin GetDrainageBasinOfModel(IModel model)
        {
            var rrModel = model as RainfallRunoffModel;
            if (rrModel != null)
            {
                return rrModel.Basin;
            }
            return null;
        }

        /// <summary>
        /// Gets the <see cref="SobekCaseData"/> for this case.
        /// </summary>
        protected SobekCaseData CaseData
        {
            get;
            private set;
        }

        private string baseDir;
        
        private string BaseDir
        {
            get => baseDir;
            set
            {
                baseDir = value;
                SetCaseData();
            }
        }

        private void SetCaseData()
        {
            string caseDescriptionFile = GetFilePath(SobekFileNames.SobekCaseDescriptionFile);
            if (!File.Exists(caseDescriptionFile))
            {
                CaseData = new SobekCaseData(Enumerable.Empty<string>());
                return;
            }

            using (var stream = new FileStream(caseDescriptionFile, FileMode.Open))
            {
                CaseData = SobekCaseDataReader.Read(stream, caseDescriptionFile);
            }
        }
    }
}
