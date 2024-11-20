using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Utils.Aop;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.Plugins.FMSuite.Common.IO;
using SharpMap;

namespace DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition
{
    [Entity]
    public class SedimentProperty<T> : ISedimentProperty<T>
    {
        private readonly string dataTemplateName = "SedimentPropertyDefaultTemplate";
        private bool isVisible;

        protected virtual Dictionary<Type, string> SedimentPropertiesDataTemplateNames
        {
            get
            {
                return new Dictionary<Type, string>
                {
                    {typeof(bool), "SedimentPropertyBoolTemplate"},
                    {typeof(int), "SedimentPropertyIntegerTemplate"},
                    {typeof(double), "SedimentPropertyDoubleTemplate"},
                };
            }
        }

        public SedimentProperty(string name, T defaultValue, T minValue, bool minIsOpened, T maxValue, bool maxIsOpened, string unit, 
            string description, bool mduOnly, Func<List<ISediment>, bool> enabled = null, Func<List<ISediment>, bool> visible = null)
        {
            Name = name;
            Value = defaultValue;
            DefaultValue = defaultValue;
            MinValue = minValue;
            MaxValue = maxValue;
            MinIsOpened = minIsOpened;
            MaxIsOpened = maxIsOpened;
            Unit = unit;
            MduOnly = mduOnly;
            Description = description;

            if (enabled == null)
                enabled = sediments => true;

            Enabled = enabled;
            
            if (visible == null)
                visible = sediments => true;
            Visible = visible;

            string dataTemplate = string.Empty;
            if (SedimentPropertiesDataTemplateNames.TryGetValue(typeof(T), out dataTemplate))
            {
                dataTemplateName = dataTemplate;
            }
        }

        public virtual void SedimentPropertyWrite(IniSection iniSection)
        {
            iniSection.AddSedimentProperty(Name, string.Format(CultureInfo.InvariantCulture, "{0}", Value), Unit, Description);
        }

        public virtual void SedimentPropertyLoad(IniSection iniSection)
        {
            var prop = iniSection.Properties.FirstOrDefault(p => p.Key == Name);
            if (prop == null) return;
            Value = (T) Convert.ChangeType(prop.Value, typeof(T), CultureInfo.InvariantCulture);
        }

        public string Name { get; set; }
        public T Value { get; set; }
        public T DefaultValue { get; set; }
        public T MinValue { get; set; }
        public T MaxValue { get; set; }
        public bool MinIsOpened { get; set; }
        public bool MaxIsOpened { get; set; }
        public string Unit { get; set; }
        public bool MduOnly { get; set; }
        public string Description { get; set; }
        public Func<List<ISediment>, bool> Visible { get; set; }
        public Func<List<ISediment>, bool> Enabled { get; set; }
        public bool IsEnabled { get; set; }

        public bool IsVisible
        {
            get { return !MduOnly && isVisible; }
            set { isVisible = value; }
        }

        public string DataTemplateName
        {
            get { return dataTemplateName; }
        }

        public override string ToString()
        {
            return Name;
        }
    }

    [Entity]
    public class SpatiallyVaryingSedimentProperty<T> : SedimentProperty<T>, ISpatiallyVaryingSedimentProperty<T>
    {
        protected override Dictionary<Type, string> SedimentPropertiesDataTemplateNames
        {
            get
            {
                return new Dictionary<Type, string>
                {
                    {
                        typeof(double), "SpatiallyVaryingSedimentPropertyDoubleTemplate"
                    }
                };
            }
        }

        public SpatiallyVaryingSedimentProperty(string name, T defaultValue, T minValue, bool minIsOpened, T maxValue, bool maxIsOpened, string unit,
            string description, bool isSpatiallyVarying, bool mduOnly, Func<List<ISediment>, bool> enabled = null, Func<List<ISediment>, bool> visible = null)
            : base(name, defaultValue, minValue, minIsOpened, maxValue, maxIsOpened, unit, description, mduOnly, enabled, visible)
        {
            IsSpatiallyVarying = isSpatiallyVarying;
        }

        #region Overrides of SedimentProperty<T>

        public override void SedimentPropertyWrite(IniSection iniSection)
        {
            if (!IsSpatiallyVarying)
            {
                base.SedimentPropertyWrite(iniSection);
            }
            else
            {
                /* DFlowFM Kernel requires this field to include the extension. */
                iniSection.AddSedimentProperty(Name, string.Format("#{0}#", SpatiallyVaryingName + "." + XyzFile.Extension), Unit, Description);
            }
        }


        public override void SedimentPropertyLoad(IniSection iniSection)
        {
            try
            {
                base.SedimentPropertyLoad(iniSection);
            }
            catch 
            {
                // check if we can cast to string so we can find out if it is a spatially varying property
                var prop = iniSection.Properties.FirstOrDefault(p => p.Key == Name);
                if (prop == null) return;
                var spatVaryingFile = (string)Convert.ChangeType(prop.Value, typeof(string));
                IsSpatiallyVarying = !string.IsNullOrEmpty(spatVaryingFile);
                
                /* DFlowFM Kernel requires this field to include the extension. */
                if (spatVaryingFile != null && spatVaryingFile.EndsWith(XyzFile.Extension))
                    spatVaryingFile = spatVaryingFile.Substring(0, spatVaryingFile.Length - ("." + XyzFile.Extension).Length );

                SpatiallyVaryingName = spatVaryingFile;
            }
            
        }
        
        #endregion

        public bool IsSpatiallyVarying { get; set; }
        
        public string SpatiallyVaryingName { get; set; }
    }
}